using CoreArchV2.Core.Entity.Common;
using CoreArchV2.Core.Entity.Logistics;
using CoreArchV2.Core.Enum;
using CoreArchV2.Data.GenericRepository;
using CoreArchV2.Data.UnitOfWork;
using CoreArchV2.Services.Interfaces;
using CoreArchV2.Utilies;
using Microsoft.Extensions.Options;
using ServiceReference1;
using System.Globalization;
using System.Transactions;

namespace CoreArchV2.Services.PO
{
    public class PoService : IPoService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMobileService _mobileService;
        private readonly POSetting _pOSetting;
        private readonly IGenericRepository<FuelLog> _fuelLogRepository;
        private readonly IGenericRepository<Vehicle> _vehicleRepository;
        private readonly IGenericRepository<TaskScheduler_> _jobRepository;
        private readonly IMailService _mailService;
        AutomaticOperationsClient client = new AutomaticOperationsClient();
        public PoService(IUnitOfWork uow, IMobileService mobileService, IOptions<POSetting> pOSetting, IMailService mailService)
        {
            _uow = uow;
            _mobileService = mobileService;
            _fuelLogRepository = uow.GetRepository<FuelLog>();
            _vehicleRepository = uow.GetRepository<Vehicle>();
            _jobRepository = uow.GetRepository<TaskScheduler_>();
            _pOSetting = pOSetting.Value;
            _mailService = mailService;
        }

        public async Task FuelInsert()
        {
            if (new ModeDetector().IsDebug)
                return;

            var taskScheduler = await _jobRepository.FirstOrDefaultAsync(f => f.TypeId == 120 && f.Name == "PetrolOfisi");
            try
            {
                if (taskScheduler != null)
                {
                    var guid = Guid.NewGuid().ToString();
                    var user = new AutomaticRequestInfo()
                    {
                        TransactionId = guid,
                        UserName = _pOSetting.UserName,
                        UserPassword = _pOSetting.Password
                    };
                    var paged = new AutomaticPagedRequestInfo()
                    {
                        PageIndex = 1,
                        PageSize = int.MaxValue
                    };

                    DateTime startDate = taskScheduler.EndDate;
                    DateTime endDate = DateTime.Now;
                    var detailReport = new InvoiceDetailReportRequest()
                    {
                        AutomaticPagedRequestInfo = paged,
                        AutomaticRequestInfo = user,
                        FleetId = _pOSetting.FleetId,
                        StartDate = startDate,
                        EndDate = endDate,
                    };
                    var result = await client.InvoiceDetailReportAsync(detailReport);//Fatura detay raporu
                    taskScheduler.LastRunDate = DateTime.Now;
                    if (result.AutomaticResponseInfo.ResponseCode == "0000")
                    {
                        var fuelList = result.InvoiceDetailInfoList;
                        if (!fuelList.Any())
                            return;

                        var report = result.InvoiceDetailProductList;
                        var dbNoPlateList = new List<string>();
                        var manyRecorPlate = new List<string>();//birden fazla plaka ekliyse
                        var insertFuelList = new List<FuelLog>();
                        var allVehicle = _vehicleRepository.Where(w => w.Status).ToList();
                        foreach (var item in allVehicle)
                        {
                            if (item.Plate.Length == 5)
                                item.Plate = "000" + item.Plate;
                            else if (item.Plate.Length == 6)
                                item.Plate = "00" + item.Plate;
                            else if (item.Plate.Length == 7)
                                item.Plate = "0" + item.Plate;
                        }
                        var discountPercent = Math.Round((1 - (report.TotalDiscountedAmount / fuelList.Sum(s => s.TotalQuantity))) * 100, 5);
                        foreach (InvoiceDetailInfo item in fuelList)
                        {
                            var vehicle = allVehicle.Where(f => f.Plate.Contains(item.Plate.Replace(" ", ""))).ToList();
                            if (vehicle.Count > 1)
                                manyRecorPlate.Add(item.Plate);
                            else if (vehicle.Count == 1)//veritabanında plaka varsa
                            {
                                var vehicleSingle = vehicle[0];
                                var beforeInsert = _fuelLogRepository.Any(a => a.Status && a.VehicleId == vehicleSingle.Id//Daha önce aynı tarihte kayıt atıldıysa, ekleme
                                && a.TransactionDate == item.ProcessDate && a.TotalAmount == item.TotalQuantity);
                                if (!beforeInsert)
                                    insertFuelList.Add(SetFuelTable(vehicleSingle, item, discountPercent));
                            }
                            else
                                dbNoPlateList.Add(item.Plate);
                        }

                        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            await _fuelLogRepository.InsertRangeAsync(insertFuelList);
                            await _uow.SaveChangesAsync();
                            scope.Complete();
                        }

                        taskScheduler.StartDate = startDate;
                        taskScheduler.EndDate = endDate;
                        taskScheduler.ErrorMessage = null;
                        await UpdateJob(taskScheduler);

                        var message = "<b>Eklenen Yakıt Bilgileri</b> (" + startDate.ToString("dd/MM/yyyy HH:mm") + "-" + endDate.AddDays(1).ToString("dd/MM/yyyy HH:mm") + ")";
                        message += "<br/>Araç Sayısı: " + insertFuelList.GroupBy(g => g.VehicleId).ToList().Count + " Adet";
                        message += "<br/>Toplam Litre: " + report.TotalLiter.ToString("N2", CultureInfo.CreateSpecificCulture("tr-TR"));
                        message += "<br/><br/>Tutar: " + report.TotalAmount.ToString("C", CultureInfo.CreateSpecificCulture("tr-TR"));
                        message += "<br/>Kdv: " + report.TotalKdv.ToString("C", CultureInfo.CreateSpecificCulture("tr-TR"));
                        message += "<br/>Genel Toplam: " + (report.TotalAmount + report.TotalKdv).ToString("C", CultureInfo.CreateSpecificCulture("tr-TR"));
                        message += "<br/>İndirim: " + report.TotalDiscount.ToString("C", CultureInfo.CreateSpecificCulture("tr-TR"));
                        message += "<br/>İndirimli Tutar: " + report.TotalDiscountedAmount.ToString("C", CultureInfo.CreateSpecificCulture("tr-TR"));
                        message += "<br/>İndirim Oranı: %" + discountPercent;
                        message += "<br/><br/>Yakıt verileri başarılı şekilde sisteme aktarılmıştır. <br/>Kontrol edip yayınlayabilirsiniz.";
                        if (dbNoPlateList.Any())
                        {
                            message += "<br/><br/><b>Sisteme Eklenemeyen Plakalar</b> (Sistemde Bulunamadı)<br/>";
                            message += string.Join("<br/>", dbNoPlateList.Distinct());
                            message += "<br/>Not: Bu plakaları manuel eklemeniz gerekmektedir";
                        }

                        if (manyRecorPlate.Any())
                        {
                            message += "<br/><br/><b>Sisteme Eklenemeyen Plakalar</b> (Aynı plakaya sahip araç listesi)<br/>";
                            message += string.Join("<br/>", manyRecorPlate.Distinct());
                            message += "<br/><br/>Not: Bu plakaları manuel eklemeniz gerekmektedir.";
                        }

                        if (dbNoPlateList.Any() || manyRecorPlate.Any())
                            await FuelMailSender("Otomatik Petrol Ofisi Yakıt Aktarımı Hk.", message);
                    }
                    else
                    {
                        taskScheduler.ErrorMessage = result.AutomaticResponseInfo.ResponseCode + "-" + result.AutomaticResponseInfo.ResponseMessage;
                        await UpdateJob(taskScheduler);
                        var message = startDate + "-" + endDate + " tarihleri arasındaki yakıtları çekerken <b>hata</b> oluştu!!";
                        await FuelMailSender("Petrol Ofisi Yakıt Aktarımı Hk.", message);
                    }

                    #region Yakıt alım raporu                
                    //var getFuelReport = new ServiceReference1.FuelPurchaseInfoReportRequest()
                    //{
                    //    FleetId = _pOSetting.FleetId,
                    //    AutomaticPagedRequestInfo = paged,
                    //    AutomaticRequestInfo = user,
                    //    StartDate = System.DateTime.Now.AddDays(-1).Date,
                    //    EndDate = System.DateTime.Now.Date
                    //};
                    //var v2 = await client.FuelPurchaseInfoReport_v2Async(getFuelReport);//Yakıt alım raporu
                    #endregion
                }
            }
            catch (Exception ex)
            {
                taskScheduler.ErrorMessage = ex.Message;
                await UpdateJob(taskScheduler);
                await FuelMailSender("Petrol Ofisi Yakıt Aktarımı Hk.", "Hata İçeriği: " + ex.Message);
            }
        }

        public async Task UpdateJob(TaskScheduler_ entity)
        {
            try
            {
                _jobRepository.Update(entity);
                await _uow.SaveChangesAsync();
            }
            catch (Exception)
            {
            }
        }

        public FuelLog SetFuelTable(Vehicle vehicle, InvoiceDetailInfo item, decimal discountPercent)
        {
            return new FuelLog()
            {
                CreatedBy = 1,
                VehicleId = vehicle.Id,
                TransactionDate = item.ProcessDate,
                TotalAmount = item.TotalQuantity,
                DiscountPercent = discountPercent,
                Liter = item.Liter,
                Description = "Otomatik eklendi",
                FuelStationId = 120,
                IsPublisher = false,
                InsertType = (int)FuelInsertType.Auto
            };
        }

        public async Task FuelMailSender(string subject, string body)
        {
            try
            {
                //var adminMailList = new AdminMailList();
                //var mailList = adminMailList.GetAdminMailList();
                //_mailService.SendMail(mailList, subject, body);

                var adminMailList = await _mobileService.GetParameterByKey(ParameterEnum.AdminMailList.ToString());
                var mailList = adminMailList?.ValueP;
                if (!string.IsNullOrEmpty(mailList))
                    _mailService.SendMail(mailList, subject, body);
            }
            catch (Exception ex) { }
        }
    }
}
