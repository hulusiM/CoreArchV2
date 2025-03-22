using ClosedXML.Excel;
using CoreArchV2.Core.Entity.Common;
using CoreArchV2.Core.Entity.TripVehicle;
using CoreArchV2.Core.Enum;
using CoreArchV2.Data.GenericRepository;
using CoreArchV2.Data.UnitOfWork;
using CoreArchV2.Dto.ELogisticsDto;
using CoreArchV2.Dto.ENoticeVehicleDto.ENoticeDto_;
using CoreArchV2.Dto.EReportDto;
using CoreArchV2.Dto.ETripDto;
using CoreArchV2.Services.Interfaces;
using CoreArchV2.Utilies;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace CoreArchV2.Web.Controllers
{
    public class ExportController : AdminController
    {
        private readonly IUnitOfWork _uow;
        private readonly IWebHostEnvironment _env;
        private readonly IVehicleService _vehicleService;
        private readonly IMaintenanceService _maintenanceService;
        private readonly IReportService _reportService;
        private readonly ITenderService _tenderService;
        private readonly ITripService _tripService;
        private readonly IOutOfHourService _outOfHourService;
        private readonly INoticeService _noticeService;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<Unit> _unitRepository;
        private readonly IGenericRepository<Trip> _tripRepository;


        public ExportController(
            IVehicleService vehicleService,
            IMaintenanceService maintenanceService,
            ITenderService tenderService,
            IReportService reportService,
            INoticeService noticeService,
            ITripService tripService,
             IOutOfHourService outOfHourService,
             IUnitOfWork uow,
        IWebHostEnvironment env)
        {
            _env = env;

            _userRepository = uow.GetRepository<User>();
            _tripRepository = uow.GetRepository<Trip>();
            _unitRepository = uow.GetRepository<Unit>();
            _vehicleService = vehicleService;
            _maintenanceService = maintenanceService;
            _reportService = reportService;
            _tenderService = tenderService;
            _tripService = tripService;
            _outOfHourService = outOfHourService;
            _noticeService = noticeService;
        }

        #region Logistics
        //Araç listesi(aktif/pasif tümünü ve detayları listeler)
        public IActionResult VehicleInfo(EVehicleDto filterModel)
        {
            var list = _vehicleService.GetAllVehicleList(filterModel);
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Araç Listesi");
            var currentRow = 1;
            worksheet.Cell(currentRow, 1).Value = "Durumu";
            worksheet.Cell(currentRow, 2).Value = "Plaka";
            worksheet.Cell(currentRow, 3).Value = "Şaşi No";
            worksheet.Cell(currentRow, 4).Value = "Renk";
            worksheet.Cell(currentRow, 5).Value = "Motor No";
            worksheet.Cell(currentRow, 6).Value = "Marka/Model";
            worksheet.Cell(currentRow, 7).Value = "Lisans Seri No";
            worksheet.Cell(currentRow, 8).Value = "Model Yıl";
            worksheet.Cell(currentRow, 9).Value = "Kullanım Tipi";
            worksheet.Cell(currentRow, 10).Value = "Yakıt Tipi";
            worksheet.Cell(currentRow, 11).Value = "Demirbaş";
            worksheet.Cell(currentRow, 12).Value = "Kullanım Amacı";
            worksheet.Cell(currentRow, 13).Value = "Araç Tipi";
            worksheet.Cell(currentRow, 14).Value = "Birim/Proje";
            worksheet.Cell(currentRow, 15).Value = "Arvento No";
            worksheet.Cell(currentRow, 16).Value = "TTS";
            worksheet.Cell(currentRow, 17).Value = "Kiralama Firma Adı";
            worksheet.Cell(currentRow, 18).Value = "Vites Türü";
            worksheet.Cell(currentRow, 19).Value = "Ortaklı Kiralık";
            worksheet.Cell(currentRow, 20).Value = "Leasing";
            worksheet.Cell(currentRow, 21).Value = "Hgs";
            worksheet.Cell(currentRow, 22).Value = "Sözleşme Tarihi";
            worksheet.Cell(currentRow, 23).Value = "Zimmetli Bilgileri";
            worksheet.Cell(currentRow, 24).Value = "Son Km";
            worksheet.Cell(currentRow, 25).Value = "Görev Yaptığı Son Şehir";


            worksheet.Cell(currentRow, 26).Value = "K Belgesi Bitiş Tarihi";
            worksheet.Cell(currentRow, 27).Value = "Kasko Bitiş Tarihi";
            worksheet.Cell(currentRow, 28).Value = "Trafik Sigortası Bitiş Tarihi";
            worksheet.Cell(currentRow, 29).Value = "Muayene Bitiş Tarihi";
            //Sütun Kolonu
            var rangeCol = "A1:A" + (list.Count + 1);
            var col1 = worksheet.Range(rangeCol);
            col1.Style.Fill.BackgroundColor = XLColor.Gold;
            col1.Style.Font.SetBold(true);
            //col1.Width = 5;

            //Sıra Kolonu
            var rangeRow = "A1:" + GetRowName(worksheet.Columns().Count()) + "1";
            var row1 = worksheet.Range(rangeRow);
            row1.Style.Font.SetBold(true);
            row1.Style.Fill.BackgroundColor = XLColor.Gold;

            foreach (var item in list)
            {
                currentRow++;
                worksheet.Cell(currentRow, 1).Value = item.StatusName;
                worksheet.Cell(currentRow, 2).Value = item.Plate;
                worksheet.Cell(currentRow, 3).Value = item.ChassisNo;
                worksheet.Cell(currentRow, 4).Value = item.ColorName;
                worksheet.Cell(currentRow, 5).Style.NumberFormat.Format = "@";  // Metin formatı
                worksheet.Cell(currentRow, 5).Value = item.EngineNo?.ToString();
                worksheet.Cell(currentRow, 6).Value = item.VehicleModelName;
                worksheet.Cell(currentRow, 7).Value = item.LicenceSeri + " " + item.LicenceNo;
                worksheet.Cell(currentRow, 8).Value = item.ModelYear;
                worksheet.Cell(currentRow, 9).Value = item.UsageTypeName;
                worksheet.Cell(currentRow, 10).Value = item.FuelTypeName;
                worksheet.Cell(currentRow, 11).Value = item.FixtureName;
                worksheet.Cell(currentRow, 12).Value = item.UsageTypeName;
                worksheet.Cell(currentRow, 13).Value = item.VehicleTypeName;
                worksheet.Cell(currentRow, 14).Value = item.UnitName;
                worksheet.Cell(currentRow, 15).Value = item.ArventoNo;
                worksheet.Cell(currentRow, 16).Value = item.IsTtsName;
                worksheet.Cell(currentRow, 17).Value = item.RentFirmName;
                worksheet.Cell(currentRow, 18).Value = item.GearTypeName;
                worksheet.Cell(currentRow, 19).Value = item.PartnerShipName;
                worksheet.Cell(currentRow, 20).Value = item.LeasingName;
                worksheet.Cell(currentRow, 21).Value = item.IsHgsName;
                worksheet.Cell(currentRow, 22).Value = item.ContractDateRange;
                worksheet.Cell(currentRow, 23).Value = item.DebitNameSurname;
                worksheet.Cell(currentRow, 24).Value = item.LastKm;
                worksheet.Cell(currentRow, 25).Value = item.LastCityName;


                worksheet.Cell(currentRow, 26).Value = item.KDocumentEndDate;
                worksheet.Cell(currentRow, 27).Value = item.KaskoEndDate;
                worksheet.Cell(currentRow, 28).Value = item.TrafficEndDate;
                worksheet.Cell(currentRow, 29).Value = item.ExaminationEndDate;
            }

            //Otomatik genişlik
            worksheet.Columns().AdjustToContents();
            worksheet.Columns().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            worksheet.Rows().AdjustToContents();
            worksheet.Rows().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);


            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            return File(
                content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Araç Listesi.xlsx");
        }
        //Müdürlük bazında aktif,plaka ve zimmetli
        public IActionResult VehicleInfo2(RFilterModelDto filterModel)
        {
            if (!_loginUserInfo.IsAdmin)
                filterModel = ForAutVehicleSetUnitId(filterModel);

            var list = _vehicleService.GetActiveVehicleForExcel(filterModel);
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Araç Listesi");
            var currentRow = 1;
            worksheet.Cell(currentRow, 1).Value = "S.No";
            worksheet.Cell(currentRow, 2).Value = "Plaka";
            worksheet.Cell(currentRow, 3).Value = "Marka/Model";
            worksheet.Cell(currentRow, 4).Value = "Müdürlük";
            worksheet.Cell(currentRow, 5).Value = "Proje";
            worksheet.Cell(currentRow, 6).Value = "Kiralama Firma Adı";
            worksheet.Cell(currentRow, 7).Value = "Demirbaş";
            worksheet.Cell(currentRow, 8).Value = "Kullanım Amacı";
            worksheet.Cell(currentRow, 9).Value = "Zimmetli Bilgileri";

            worksheet.Cell(currentRow, 10).Value = "Son Km";
            worksheet.Cell(currentRow, 11).Value = "Görev Yaptığı Son Şehir";

            worksheet.Cell(currentRow, 12).Value = "Sözleşme Tarihleri";
            worksheet.Cell(currentRow, 13).Value = "Kiralık Tutar";
            worksheet.Cell(currentRow, 14).Value = "Toplam Tutar";

            //Sütun Kolonu
            var rangeCol = "A1:A" + (list.Count + 1);
            var col1 = worksheet.Range(rangeCol);
            col1.Style.Font.SetBold(true);
            //col1.Width = 5;

            //Sıra Kolonu
            var rangeRow = "A1:" + GetRowName(worksheet.Columns().Count()) + "1";
            var row1 = worksheet.Range(rangeRow);
            row1.Style.Font.SetBold(true);
            row1.Style.Fill.BackgroundColor = XLColor.Gold;

            foreach (var item in list)
            {
                currentRow++;
                worksheet.Cell(currentRow, 1).Value = currentRow - 1;
                worksheet.Cell(currentRow, 2).Value = item.Plate;
                worksheet.Cell(currentRow, 3).Value = item.VehicleModelName;
                worksheet.Cell(currentRow, 4).Value = item.UnitParentName;
                worksheet.Cell(currentRow, 5).Value = item.UnitName;
                worksheet.Cell(currentRow, 6).Value = item.RentFirmName;
                worksheet.Cell(currentRow, 7).Value = item.FixtureName;
                worksheet.Cell(currentRow, 8).Value = item.UsageTypeName;
                worksheet.Cell(currentRow, 9).Value = item.DebitNameSurname;
                worksheet.Cell(currentRow, 10).Value = item.LastKm;
                worksheet.Cell(currentRow, 11).Value = item.LastCityName;


                worksheet.Cell(currentRow, 12).Value = item.ContractDateRange;
                worksheet.Cell(currentRow, 13).Value = item.ContractPrice;
                worksheet.Cell(currentRow, 14).Value = item.TotalPrice;
            }
            currentRow++;
            SettingAllColumn(worksheet);

            //Kiralık Toplam
            var rentCost = GetRowName(worksheet.Columns().Count() - 1);
            var totalFormul1 = "SUM(" + rentCost + "2:" + rentCost + (currentRow - 1) + ")";
            worksheet.Cell(currentRow, 13).FormulaA1 = totalFormul1;
            SetAmountColumnSetting(worksheet, currentRow, 13);

            //Genel Toplam
            var totalCost = GetRowName(worksheet.Columns().Count());
            var totalFormul2 = "SUM(" + totalCost + "2:" + totalCost + (currentRow - 1) + ")";
            worksheet.Cell(currentRow, 14).FormulaA1 = totalFormul2;
            SetAmountColumnSetting(worksheet, currentRow, 14);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Araç Listesi.xlsx");
        }
        public async Task<IActionResult> MaintenanceInfo(RFilterModelDto filterModel)
        {
            var mainList = await _reportService.GetMaintenanceListRange(filterModel);
            var debitList = await _reportService.GetDebitListRange(ForAutVehicleSetUnitId(filterModel));

            var result = new List<RVehicleMaintenanceDto>();
            foreach (var item in debitList)
            {
                var unitName = item.ProjectName;
                if (item.State == (int)DebitState.Pool)
                    unitName = "Havuz";
                else if (item.State == (int)DebitState.InService)
                    unitName = "Servis";

                var model = (from m in mainList
                             where m.InvoiceDate >= item.StartDate && m.InvoiceDate < item.EndDate && m.Plate == item.Plate
                             select new RVehicleMaintenanceDto()
                             {
                                 InvoiceDate = m.InvoiceDate,
                                 Plate = m.Plate,
                                 UserNameSurname = m.UserNameSurname,
                                 SupplierName = m.SupplierName,
                                 UnitName = unitName,
                                 Amount = m.Amount,
                                 UserFaultAmount = m.UserFaultAmount,
                                 UserFaultDescription = m.UserFaultDescription
                             }).ToList();
                result.AddRange(model);
            }

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Bakım Onarım");
            var currentRow = 1;
            worksheet.Cell(currentRow, 1).Value = "S.No";
            worksheet.Cell(currentRow, 2).Value = "Tarih";
            worksheet.Cell(currentRow, 3).Value = "Araç Plaka";
            worksheet.Cell(currentRow, 4).Value = "Proje";
            worksheet.Cell(currentRow, 5).Value = "Talep Eden";
            worksheet.Cell(currentRow, 6).Value = "Firma Bilgi";
            worksheet.Cell(currentRow, 7).Value = "Kullanıcı Hata Açıklama";
            worksheet.Cell(currentRow, 8).Value = "Kullanıcı Hata Tutarı";
            worksheet.Cell(currentRow, 9).Value = "Bakım/Onarım Tutarı";


            SettingFirstColumn(worksheet, result.Count);
            result = result.OrderBy(o => o.UnitName).ToList();
            foreach (var item in result)
            {
                currentRow++;
                worksheet.Cell(currentRow, 1).Value = currentRow - 1;
                worksheet.Cell(currentRow, 2).Value = item.InvoiceDate;
                worksheet.Cell(currentRow, 3).Value = item.Plate;
                worksheet.Cell(currentRow, 4).Value = item.UnitName;
                worksheet.Cell(currentRow, 5).Value = item.UserNameSurname;
                worksheet.Cell(currentRow, 6).Value = item.SupplierName;
                worksheet.Cell(currentRow, 7).Value = item.UserFaultAmount > 0 ? item.UserFaultDescription : "";

                if (item.UserFaultAmount > 0)
                    worksheet.Cell(currentRow, 8).Value = item.UserFaultAmount.Value;
                else
                    worksheet.Cell(currentRow, 8).Value = "";

                worksheet.Cell(currentRow, 9).Value = item.Amount;
            }
            currentRow++;
            SettingAllColumn(worksheet);

            //Kullanıcı hatası toplam tutar
            var totalUserFaultAmount = GetRowName(worksheet.Columns().Count() - 1);
            var totalFormul1 = "SUM(" + totalUserFaultAmount + "2:" + totalUserFaultAmount + (currentRow - 1) + ")"; //SUM(J2:J218)
            worksheet.Cell(currentRow, 8).FormulaA1 = totalFormul1;
            SetAmountColumnSetting(worksheet, currentRow, 8);

            //Genel toplam tutar
            var totalTotalAmount = GetRowName(worksheet.Columns().Count());
            var totalFormul2 = "SUM(" + totalTotalAmount + "2:" + totalTotalAmount + (currentRow - 1) + ")"; //SUM(J2:J218)
            worksheet.Cell(currentRow, 9).FormulaA1 = totalFormul2;
            SetAmountColumnSetting(worksheet, currentRow, 9);

            await using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();
            var folderName = "Bakım Onarım Raporu " + filterModel.StartDate.ToString("yyyy MMMM dd") + "-" + filterModel.EndDate.ToString("yyyy MMMM dd") + ".xlsx";
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", folderName);
        }
        public async Task<IActionResult> VehicleCostInfo(RFilterModelDto filterModel)
        {
            var list = await _reportService.GetVehicleCostWithDebitList(ForAutVehicleSetUnitId(filterModel));
            int daysInMonth = 30;// DateTime.DaysInMonth(filterModel.StartDate.Year, filterModel.StartDate.Month);
            var costCalc = (from r in list
                            select new RVehicleCostDto()
                            {
                                CostType = r.CostType,
                                CostTypeName = r.CostTypeName,
                                RentFirmName = r.RentFirmName,
                                VehicleId = r.VehicleId,
                                Plate = r.Plate,
                                Amount = r.Amount,
                                ExtraAmount = r.ExtraAmount,
                                UnitName = r.UnitName,
                                ProjectName = r.ProjectName,
                                UnitId = r.UnitId,
                                DayCount = (int)(r.EndDate - r.StartDate).TotalDays,
                                DatesRangeCost = (decimal)(r.Amount / daysInMonth) * (int)(r.EndDate - r.StartDate).TotalDays,
                            }).ToList();

            var result = costCalc.GroupBy(g => g.VehicleId).Select(s => new RVehicleCostDto()
            {
                Plate = s.First().Plate,
                ProjectName = s.First().ProjectName,
                Amount = costCalc.Where(w => w.VehicleId == s.First().VehicleId && w.CostType == (int)VehicleAmountType.KiraBedeli).Sum(s => s.DatesRangeCost),
                ArventoAmount = costCalc.Where(w => w.VehicleId == s.First().VehicleId && (w.CostType == (int)VehicleAmountType.ArventoMaliyet || w.CostType == (int)VehicleAmountType.SimKartMaliyet)).Sum(s => s.DatesRangeCost),
                ExtraAmount = s.Sum(s => s.ExtraAmount),
            }).ToList();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Araç Kiralama Raporu");
            var currentRow = 1;
            worksheet.Cell(currentRow, 1).Value = "S.No";
            worksheet.Cell(currentRow, 2).Value = "Araç Plaka";
            worksheet.Cell(currentRow, 3).Value = "Proje";
            worksheet.Cell(currentRow, 4).Value = "Kira Bedeli";
            worksheet.Cell(currentRow, 5).Value = "Arvento ve Sim kart";
            worksheet.Cell(currentRow, 6).Value = "Extra Tutar";
            worksheet.Cell(currentRow, 7).Value = "Toplam Tutar";

            SettingFirstColumn(worksheet, result.Count);
            result = result.OrderBy(o => o.UnitName).ToList();
            foreach (var item in result)
            {
                currentRow++;
                worksheet.Cell(currentRow, 1).Value = currentRow - 1;
                worksheet.Cell(currentRow, 2).Value = item.Plate;
                worksheet.Cell(currentRow, 3).Value = item.ProjectName;
                worksheet.Cell(currentRow, 4).Value = item.Amount;
                worksheet.Cell(currentRow, 5).Value = (XLCellValue)(item.ArventoAmount > 0 ? (object)item.ArventoAmount : "");
                worksheet.Cell(currentRow, 6).Value = (XLCellValue)(item.ExtraAmount > 0 ? (object)item.ExtraAmount : "");
                worksheet.Cell(currentRow, 7).Value = (item.Amount + item.ArventoAmount + item.ExtraAmount);
            }
            currentRow++;
            SettingAllColumn(worksheet);

            //Kira bedeli toplam
            var totalTotalAmount1 = GetRowName(worksheet.Columns().Count() - 3);
            var totalFormul1 = "SUM(" + totalTotalAmount1 + "2:" + totalTotalAmount1 + (currentRow - 1) + ")"; //SUM(J2:J218)
            worksheet.Cell(currentRow, 4).FormulaA1 = totalFormul1;
            SetAmountColumnSetting(worksheet, currentRow, 4);

            //Arvento-Sim kart toplam
            var totalTotalAmount2 = GetRowName(worksheet.Columns().Count() - 2);
            var totalFormul2 = "SUM(" + totalTotalAmount2 + "2:" + totalTotalAmount2 + (currentRow - 1) + ")"; //SUM(J2:J218)
            worksheet.Cell(currentRow, 5).FormulaA1 = totalFormul2;
            SetAmountColumnSetting(worksheet, currentRow, 5);

            //Extra Tutar toplam
            var totalTotalAmount3 = GetRowName(worksheet.Columns().Count() - 1);
            var totalFormul3 = "SUM(" + totalTotalAmount3 + "2:" + totalTotalAmount3 + (currentRow - 1) + ")"; //SUM(J2:J218)
            worksheet.Cell(currentRow, 6).FormulaA1 = totalFormul3;
            SetAmountColumnSetting(worksheet, currentRow, 6);

            //Genel toplam tutar
            var totalTotalAmount4 = GetRowName(worksheet.Columns().Count());
            var totalFormul4 = "SUM(" + totalTotalAmount4 + "2:" + totalTotalAmount4 + (currentRow - 1) + ")"; //SUM(J2:J218)
            worksheet.Cell(currentRow, 7).FormulaA1 = totalFormul4;
            SetAmountColumnSetting(worksheet, currentRow, 7);

            await using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();
            var folderName = "Araç Kiralama Raporu " + filterModel.StartDate.ToString("yyyy MMMM dd") + "-" + filterModel.EndDate.ToString("yyyy MMMM dd") + ".xlsx";
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", folderName);
        }
        public async Task<IActionResult> FuelInfo(RFilterModelDto filterModel)
        {
            var fuelList = await _reportService.GetFuelListRange(ForAutVehicleSetUnitId(filterModel));
            var debitList = await _reportService.GetDebitListRangeForExcel(ForAutVehicleSetUnitId(filterModel));
            var result = new List<RVehicleFuelDto>();
            foreach (var item in debitList)
            {
                var unitName = item.ProjectName;
                if (item.State == (int)DebitState.Pool)
                    unitName = "Havuz";
                else if (item.State == (int)DebitState.InService)
                    unitName = "Servis";

                var model = (from m in fuelList
                             where m.FuelDate >= item.StartDate && m.FuelDate < item.EndDate && m.Plate == item.Plate
                             select new RVehicleFuelDto()
                             {
                                 FuelDate = m.FuelDate,
                                 Plate = m.Plate,
                                 UserNameSurname = item.UserNameSurname,
                                 UnitName = unitName,
                                 Liter = m.Liter,
                                 SupplierName = m.SupplierName,
                                 DiscountAmount = m.DiscountAmount,
                                 Amount = m.Amount,
                                 ParentUnitId = item.ParentUnitId,
                                 VehicleModelName = item.VehicleModelName,
                                 VehicleTypeName = item.VehicleTypeName
                             }).ToList();
                result.AddRange(model);
            }
            result = result.OrderBy(o => o.FuelDate).ThenBy(t => t.UnitName).ToList();

            var folderName = "Yakıt Raporu " + filterModel.StartDate.ToString("yyyy MMMM dd") + "-" + filterModel.EndDate.ToString("yyyy MMMM dd") + ".xlsx";
            var monthCount = ((filterModel.EndDate.Year - filterModel.StartDate.Year) * 12) + filterModel.EndDate.Month - filterModel.StartDate.Month;
            var totalGroupPlate = result.GroupBy(g => g.Plate).Select(s => new RVehicleFuelDto()
            {
                FuelDate = s.First().FuelDate,
                Plate = s.First().Plate,
                UserNameSurname = s.First().UserNameSurname,
                UnitName = s.First().UnitName,
                SupplierName = s.First().SupplierName,
                DiscountAmount = s.Sum(s => s.DiscountAmount),
                Amount = s.Sum(s => s.Amount),
                Liter = s.Sum(s => s.Liter),
                ParentUnitId = s.First().ParentUnitId,
                VehicleModelName = s.First().VehicleModelName,
                VehicleTypeName = s.First().VehicleTypeName
            }).ToList();

            using var workbook = new XLWorkbook();
            var worksheetTotal = workbook.Worksheets.Add("Genel Toplam");
            worksheetTotal = ExcelToWorkSheet(worksheetTotal, totalGroupPlate);

            var worksheetDetail = workbook.Worksheets.Add("Plaka Bazlı Detay");
            worksheetTotal = ExcelToWorkSheet(worksheetDetail, result, false);

            if (monthCount <= 1) //1 ay 
            {
                var startDate_1 = filterModel.StartDate;
                var endDate_1 = new DateTime(filterModel.StartDate.Year, filterModel.StartDate.Month, 16);
                var result_1Donem = result.Where(w => w.FuelDate >= startDate_1 && w.FuelDate < endDate_1).ToList();//1.dönem
                var result_2Donem = result.Where(w => w.FuelDate >= endDate_1 && w.FuelDate < filterModel.EndDate).ToList();//2.dönem

                var worksheet = workbook.Worksheets.Add("1.Dönem");
                worksheet = ExcelToWorkSheet(worksheet, result_1Donem);

                var worksheet2 = workbook.AddWorksheet("2.Dönem");
                worksheet2 = ExcelToWorkSheet(worksheet2, result_2Donem);
            }
            else//çoklu ay
            {
                var months = DateDiff.TwoDateRangeMonthSplitAddDays_1(filterModel.StartDate, filterModel.EndDate);
                foreach (var item in months)
                {
                    var tempResult = result.Where(w => w.FuelDate >= item.StartDate && w.FuelDate < item.EndDate)
                        .GroupBy(g => g.Plate)
                        .Select(s => new RVehicleFuelDto()
                        {
                            FuelDate = s.First().FuelDate,
                            Plate = s.First().Plate,
                            UserNameSurname = s.First().UserNameSurname,
                            UnitName = s.First().UnitName,
                            SupplierName = s.First().SupplierName,
                            DiscountAmount = s.Sum(s => s.DiscountAmount),
                            Amount = s.Sum(s => s.Amount),
                            Liter = s.Sum(s => s.Liter),
                            ParentUnitId = s.First().ParentUnitId,
                            VehicleModelName = s.First().VehicleModelName,
                            VehicleTypeName = s.First().VehicleTypeName
                        }).ToList();

                    if (tempResult.Count > 0)
                    {
                        var worksheet = workbook.AddWorksheet(item.StartDate.ToString("Y"));
                        worksheet = ExcelToWorkSheet(worksheet, tempResult);
                    }
                }
            }

            await using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", folderName);
        }
        public async Task<IActionResult> HgsInfo(RFilterModelDto filterModel)
        {
            var result = await _reportService.HgsWithDebitlist(ForAutVehicleSetUnitId(filterModel), true);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Araç HGS Raporu");
            var currentRow = 1;
            worksheet.Cell(currentRow, 1).Value = "S.No";
            worksheet.Cell(currentRow, 2).Value = "İşlem Tarihi";
            worksheet.Cell(currentRow, 3).Value = "Araç Plaka";
            worksheet.Cell(currentRow, 4).Value = "Proje";
            worksheet.Cell(currentRow, 5).Value = "Zimmetli Adı Soyadı";
            worksheet.Cell(currentRow, 6).Value = "İşlem Türü";
            worksheet.Cell(currentRow, 7).Value = "İşlem Tutarı";

            SettingFirstColumn(worksheet, result.Count);
            foreach (var item in result)
            {
                currentRow++;
                worksheet.Cell(currentRow, 1).Value = currentRow - 1;
                worksheet.Cell(currentRow, 2).Value = item.InvoiceDate;
                worksheet.Cell(currentRow, 3).Value = item.Plate;
                worksheet.Cell(currentRow, 4).Value = item.UnitName;
                worksheet.Cell(currentRow, 5).Value = item.DebitNameSurname;
                worksheet.Cell(currentRow, 6).Value = item.AllMaintenanceTypeWithJoin;
                worksheet.Cell(currentRow, 7).Value = item.Amount;//.ToString("C", CultureInfo.CreateSpecificCulture("tr-TR"));
            }
            currentRow++;
            SettingAllColumn(worksheet);

            //Genel toplam tutar
            var totalTotalAmount = GetRowName(worksheet.Columns().Count());
            var totalFormul = "SUM(" + totalTotalAmount + "2:" + totalTotalAmount + (currentRow - 1) + ")"; //SUM(J2:J218)
            worksheet.Cell(currentRow, 7).FormulaA1 = totalFormul;
            SetAmountColumnSetting(worksheet, currentRow, 7);

            await using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();
            var folderName = "HGS Raporu " + filterModel.StartDate.ToString("yyyy MMMM dd") + "-" + filterModel.EndDate.ToString("yyyy MMMM dd") + ".xlsx";
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", folderName);
        }
        public async Task<IActionResult> TripInfo(ETripDto filterModel)
        {
            var result = _tripService.GetAllTrip(filterModel);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Araç Görev Raporu");
            var currentRow = 1;
            worksheet.Cell(currentRow, 1).Value = "S.No";
            worksheet.Cell(currentRow, 2).Value = "Araç Plaka";
            worksheet.Cell(currentRow, 3).Value = "Görev Açan Kullanıcı";
            worksheet.Cell(currentRow, 4).Value = "Yönetici Onay";
            worksheet.Cell(currentRow, 5).Value = "Görev Adı";
            worksheet.Cell(currentRow, 6).Value = "Görev Tipi";
            worksheet.Cell(currentRow, 7).Value = "Baş. Bitiş İl";
            worksheet.Cell(currentRow, 8).Value = "Baş. Bitiş Tarih";
            worksheet.Cell(currentRow, 9).Value = "Baş. Bitiş Km";
            worksheet.Cell(currentRow, 10).Value = "Görev Top. Km";
            worksheet.Cell(currentRow, 11).Value = "Görev Durumu";

            SettingFirstColumn(worksheet, result.Count);
            foreach (var item in result)
            {
                currentRow++;
                worksheet.Cell(currentRow, 1).Value = currentRow - 1;
                worksheet.Cell(currentRow, 2).Value = item.Plate;
                worksheet.Cell(currentRow, 3).Value = item.NameSurname;

                if (item.IsManagerAllowed == null)
                    worksheet.Cell(currentRow, 4).Value = "Yönetici Onayı Bekliyor";
                else if (item.IsManagerAllowed == true)
                    worksheet.Cell(currentRow, 4).Value = "Onaylı";
                else if (item.IsManagerAllowed == false)
                    worksheet.Cell(currentRow, 4).Value = "Onaysız";

                worksheet.Cell(currentRow, 5).Value = item.MissionName;
                worksheet.Cell(currentRow, 6).Value = _tripService.GetTripType(item.Type);
                worksheet.Cell(currentRow, 7).Value = item.StartCityName + " / " + item.EndCityName;
                worksheet.Cell(currentRow, 8).Value = item.StartDate.ToString("yyyy MMMM dd HH:mm") + " / " + (item.EndDate != null ? item.EndDate.Value.ToString("yyyy MMMM dd HH:mm") : "-");
                worksheet.Cell(currentRow, 9).Value = item.StartKm + " / " + (item.EndKm > 0 ? item.EndKm.ToString() : "-") + " Km";
                worksheet.Cell(currentRow, 10).Value = item.EndKm > 0 ? (Convert.ToInt32(item.EndKm.Value - item.StartKm.Value) + " Km") : "0";
                worksheet.Cell(currentRow, 11).Value = _tripService.SetStateTrip(item.State);
            }
            currentRow++;
            SettingAllColumn(worksheet);

            ////Genel toplam tutar
            //var totalKm = GetRowName(worksheet.Columns().Count());
            //var totalFormul = "SUM(" + totalKm + "2:" + totalKm + (currentRow - 1) + ")"; //SUM(J2:J218)
            //worksheet.Cell(currentRow, 9).FormulaA1 = totalFormul;
            //SetAmountColumnSettingNotMoney(worksheet, currentRow, 9);

            await using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();
            var folderName = "Araç Görev Raporu " + filterModel.StartDate.ToString("yyyy MMMM dd") + "-" + filterModel.EndDate.Value.ToString("yyyy MMMM dd") + ".xlsx";
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", folderName);
        }
        public async Task<IActionResult> OutOfHourInfo(EOutOfHourDto filterModel)
        {
            if (filterModel.StartDate == null || filterModel.EndDate == null)
            {
                var dateNow = DateTime.Now;
                filterModel.EndDate = dateNow;
                filterModel.StartDate = dateNow.AddDays(-2);
            }

            filterModel.CreatedBy = _loginUserInfo.Id;
            filterModel.IsAdmin = _loginUserInfo.IsAdmin;
            var result = await _outOfHourService.GetAllList(filterModel);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Mesai Raporu");
            var currentRow = 1;
            worksheet.Cell(currentRow, 1).Value = "S.No";
            worksheet.Cell(currentRow, 2).Value = "Arvento/Zimmetli Plaka";
            worksheet.Cell(currentRow, 3).Value = "Zimmetli/Görev Kullanıcı";
            worksheet.Cell(currentRow, 4).Value = "Görev Açıldı Mı?";
            worksheet.Cell(currentRow, 5).Value = "Mesai Türü";
            worksheet.Cell(currentRow, 6).Value = "Rapor Baş-Bit. Tarihi";
            worksheet.Cell(currentRow, 7).Value = "Kontak Açma/Kapama Tarihi";
            worksheet.Cell(currentRow, 8).Value = "Kontak Açık Kalma Süresi";
            worksheet.Cell(currentRow, 9).Value = "Arvento Km";
            worksheet.Cell(currentRow, 10).Value = "Görev Km";
            worksheet.Cell(currentRow, 11).Value = "Hareket Süresi";
            worksheet.Cell(currentRow, 12).Value = "Rolanti Süresi";
            worksheet.Cell(currentRow, 13).Value = "Duraklama Süresi";
            worksheet.Cell(currentRow, 14).Value = "Max Hız";

            SettingFirstColumn(worksheet, result.Count);

            foreach (var item in result)
            {
                if (item.LastDebitId > 0)
                {
                    var debitUser = _userRepository.Find(item.LastDebitId.Value);
                    item.DebitTripUser += "Zimmetli: " + debitUser.Name + " " + debitUser.Surname + "/" + debitUser.MobilePhone;
                }
                else
                {
                    item.DebitTripUser += "Zimmetli: ✘";
                    if (item.LastDebitStatus == (int)DebitState.Pool)
                        item.DebitTripUser = "Zimmetli: Havuzda";
                    else if (item.LastDebitStatus == (int)DebitState.InService)
                        item.DebitTripUser = "Zimmetli: Serviste";
                    else if (item.LastDebitStatus == (int)DebitState.Deleted)
                        item.DebitTripUser = "Zimmetli: Silinmiş";
                }

                var tripList = _tripRepository.Where(f => f.Status && f.VehicleId == item.VehicleId && item.StartDate <= f.StartDate && f.StartDate <= item.EndDate).OrderByDescending(o => o.Id).ToList();
                if (tripList.Any())
                {
                    bool isMultipTrip = tripList.Count > 1;
                    int tripCount = 1;
                    foreach (var trip in tripList)
                    {
                        if (trip.EndDate == null)
                            item.TripDescription += $"{trip.StartDate.ToString("dd/MM/yyyy HH:mm")}-Görev Kapatılmayı Bekliyor \r\n";
                        else
                        {
                            item.TripDescription += $"{trip.StartDate.ToString("dd/MM/yyyy HH:mm")}-{trip.EndDate.Value.ToString("dd/MM/yyyy HH:mm")} \r\n";
                            item.TripKm = (Convert.ToInt32(item.TripKm) + Convert.ToInt32(trip.EndKm.Value - trip.StartKm)).ToString();
                        }

                        var tripUser = _userRepository.Find(trip.DriverId);
                        if (trip.DriverId != item.LastDebitId)
                            item.DebitTripUser += "\r\n Görev Açan: " + (isMultipTrip ? "-" + (tripCount + " ") : "") + (tripUser.Name + " " + tripUser.Surname + "/" + tripUser.MobilePhone);
                        else
                            item.DebitTripUser += "\r\n Görev Açan: " + (isMultipTrip ? "-" + (tripCount + " ") : "") + (tripUser.Name + " " + tripUser.Surname + "/" + tripUser.MobilePhone);

                        tripCount++;
                    }

                    item.TripKm += Convert.ToInt32(item.TripKm) > 0 ? " Km/h" : "";
                }
                else
                {
                    item.TripDescription = "✘";
                    item.DebitTripUser += "\r\n Görev Açan: ✘";
                    item.TripKm = "-";
                }

                #region Excel
                currentRow++;
                worksheet.Cell(currentRow, 1).Value = currentRow - 1;
                worksheet.Cell(currentRow, 2).Value = item.ArventoDebitPlateNo;
                worksheet.Cell(currentRow, 3).Value = item.DebitTripUser;
                worksheet.Cell(currentRow, 4).Value = item.TripDescription;
                worksheet.Cell(currentRow, 5).Value = item.TypeName;
                worksheet.Cell(currentRow, 6).Value = item.ArventoStartEndDate;
                worksheet.Cell(currentRow, 7).Value = item.IlkSonKontakAcildi;
                worksheet.Cell(currentRow, 8).Value = item.KontakAcikKalmaSuresi;
                worksheet.Cell(currentRow, 9).Value = item.MesafeKm;
                worksheet.Cell(currentRow, 10).Value = item.TripKm;
                worksheet.Cell(currentRow, 11).Value = item.HareketSuresi;
                worksheet.Cell(currentRow, 12).Value = item.RolantiSuresi;
                worksheet.Cell(currentRow, 13).Value = item.DuraklamaSuresi;
                worksheet.Cell(currentRow, 14).Value = item.MaxHiz;

                #endregion
            }

            currentRow++;
            SettingAllColumn(worksheet);

            await using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();
            var folderName = "Araç Mesai İçi/Dışı Raporu " + filterModel.StartDate.Value.ToString("yyyy MMMM dd") + "-" + filterModel.EndDate.Value.ToString("yyyy MMMM dd") + ".xlsx";
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", folderName);
        }

        public async Task<IActionResult> VehicleSpeedInfo(ENoticeDto filterModel)
        {
            if (filterModel.StartDate == null || filterModel.EndDate == null || filterModel.StartDate == DateTime.MinValue || filterModel.EndDate == DateTime.MinValue)
            {
                var dateNow = DateTime.Now;
                filterModel.EndDate = dateNow;
                filterModel.StartDate = dateNow.AddDays(-2);
            }

            filterModel.CreatedBy = _loginUserInfo.Id;
            filterModel.IsAdmin = _loginUserInfo.IsAdmin;
            var result = await _noticeService.GetAllSpeed(filterModel);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Araç Hız İhlal Raporu");
            var currentRow = 1;
            worksheet.Cell(currentRow, 1).Value = "S.No";
            worksheet.Cell(currentRow, 2).Value = "Plaka";
            worksheet.Cell(currentRow, 3).Value = "İşlem Tarihi";
            worksheet.Cell(currentRow, 4).Value = "Zimmetli/Görev Kullanıcı";
            worksheet.Cell(currentRow, 5).Value = "Görev Açıldı Mı?";
            worksheet.Cell(currentRow, 6).Value = "Hız";
            worksheet.Cell(currentRow, 7).Value = "Birim/Proje";
            worksheet.Cell(currentRow, 8).Value = "Şehir";
            worksheet.Cell(currentRow, 9).Value = "İhlal Adresi";

            SettingFirstColumn(worksheet, result.Count);

            foreach (var item in result)
            {
                if (item.LastDebitUserId > 0)
                {
                    var debitUser = _userRepository.Find(item.LastDebitUserId.Value);
                    item.DebitTripUser += "Zimmetli: " + debitUser.Name + " " + debitUser.Surname + "/" + debitUser.MobilePhone;
                }
                else
                {
                    item.DebitTripUser += "Zimmetli: ✘";
                    if (item.LastDebitStatus == (int)DebitState.Pool)
                        item.DebitTripUser = "Zimmetli: Havuzda";
                    else if (item.LastDebitStatus == (int)DebitState.InService)
                        item.DebitTripUser = "Zimmetli: Serviste";
                    else if (item.LastDebitStatus == (int)DebitState.Deleted)
                        item.DebitTripUser = "Zimmetli: Silinmiş";
                }

                var tripList = _tripRepository.Where(f => f.Status && f.VehicleId == item.VehicleId && item.StartDate <= f.StartDate && f.StartDate <= item.EndDate).OrderByDescending(o => o.Id).ToList();
                if (tripList.Any())
                {
                    bool isMultipTrip = tripList.Count > 1;
                    int tripCount = 1;
                    foreach (var trip in tripList)
                    {
                        if (trip.EndDate == null)
                            item.TripDescription += $"{trip.StartDate.ToString("dd/MM/yyyy HH:mm")}-Görev Kapatılmayı Bekliyor \r\n";
                        else
                        {
                            item.TripDescription += $"{trip.StartDate.ToString("dd/MM/yyyy HH:mm")}-{trip.EndDate.Value.ToString("dd/MM/yyyy HH:mm")} \r\n";
                            item.TripKm = (Convert.ToInt32(item.TripKm) + Convert.ToInt32(trip.EndKm.Value - trip.StartKm)).ToString();
                        }

                        var tripUser = _userRepository.Find(trip.DriverId);
                        if (trip.DriverId != item.LastDebitUserId)
                            item.DebitTripUser += "\r\n Görev Açan: " + (isMultipTrip ? "-" + (tripCount + " ") : "") + (tripUser.Name + " " + tripUser.Surname + "/" + tripUser.MobilePhone);
                        else
                            item.DebitTripUser += "\r\n Görev Açan: " + (isMultipTrip ? "-" + (tripCount + " ") : "") + (tripUser.Name + " " + tripUser.Surname + "/" + tripUser.MobilePhone);

                        tripCount++;
                    }

                    item.TripKm += Convert.ToInt32(item.TripKm) > 0 ? " Km/h" : "";
                }
                else
                {
                    item.TripDescription = "✘";
                    item.DebitTripUser += "\r\n Görev Açan: ✘";
                    item.TripKm = "-";
                }

                #region Excel
                currentRow++;
                worksheet.Cell(currentRow, 1).Value = currentRow - 1;
                worksheet.Cell(currentRow, 2).Value = item.Plate;
                worksheet.Cell(currentRow, 3).Value = item.TransactionDate;
                worksheet.Cell(currentRow, 4).Value = item.DebitTripUser;
                worksheet.Cell(currentRow, 5).Value = item.TripDescription;
                worksheet.Cell(currentRow, 6).Value = item.Speed2;
                worksheet.Cell(currentRow, 7).Value = item.UnitName;
                worksheet.Cell(currentRow, 8).Value = item.CityName;
                worksheet.Cell(currentRow, 9).Value = item.Address;

                #endregion
            }

            currentRow++;
            SettingAllColumn(worksheet);

            await using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();
            var folderName = "Araç Hız İhlal Raporu " + filterModel.StartDate.ToString("yyyy MMMM dd") + "-" + filterModel.EndDate.ToString("yyyy MMMM dd") + ".xlsx";
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", folderName);
        }

        public async Task<IActionResult> VehicleImageLoadInfo(EVehicleDto filterModel)
        {
            filterModel.CreatedBy = _loginUserInfo.Id;
            filterModel.IsAdmin = _loginUserInfo.IsAdmin;
            var result = await _vehicleService.GetNotLoadVehicleImage(filterModel);

            result.RemoveAt(0);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Araç Resim Yükleme Raporu");
            var currentRow = 1;
            worksheet.Cell(currentRow, 1).Value = "S.No";
            worksheet.Cell(currentRow, 2).Value = "Plaka";
            worksheet.Cell(currentRow, 3).Value = "Yükleyen Kullanıcı";
            worksheet.Cell(currentRow, 4).Value = "Yükleme Tarihi";
            worksheet.Cell(currentRow, 5).Value = "Zimmetli Adı Soyadı";
            worksheet.Cell(currentRow, 6).Value = "Müdürlük/Proje";

            SettingFirstColumn(worksheet, result.Count);

            foreach (var item in result)
            {
                currentRow++;
                worksheet.Cell(currentRow, 1).Value = currentRow - 1;
                worksheet.Cell(currentRow, 2).Value = item.TempPlateNo2;
                worksheet.Cell(currentRow, 3).Value = item.NameSurname;
                worksheet.Cell(currentRow, 4).Value = item.DebitCreatedDate;
                worksheet.Cell(currentRow, 5).Value = item.DebitNameSurname;
                worksheet.Cell(currentRow, 6).Value = item.UnitName;
            }

            currentRow++;
            SettingAllColumn(worksheet);

            await using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();
            var folderName = "Araç Resim Yükleme Raporu " + filterModel.TransactionDate.ToString("yyyy MMMM dd") + ".xlsx";
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", folderName);
        }

        public RFilterModelDto ForAutVehicleSetUnitId(RFilterModelDto filterModel)
        {
            if (_loginUserInfo.IsAdmin) //adminse tüm birimleri listeleyebilir
            {
                filterModel.IsAdmin = _loginUserInfo.IsAdmin;
                filterModel.UnitId = filterModel.UnitId;
                filterModel.ParentUnitId = filterModel.ParentUnitId;
            }
            else //değilse sadece yetkili oldu birimi listeleyebilir
            {
                filterModel.UnitId = _loginUserInfo.UnitId == null ? filterModel.UnitId : _loginUserInfo.UnitId;
                filterModel.ParentUnitId = _loginUserInfo.ParentUnitId;
            }
            return filterModel;
        }
        #endregion

        #region Common
        public IXLWorksheet ExcelToWorkSheet(IXLWorksheet worksheet, List<RVehicleFuelDto> result, bool isDateVisible = true)
        {
            var currentRow = 1;
            worksheet.Cell(currentRow, 1).Value = "S.No";
            worksheet.Cell(currentRow, 2).Value = "Tarih";
            worksheet.Cell(currentRow, 3).Value = "Araç Plaka";
            worksheet.Cell(currentRow, 4).Value = "Araç Marka/Model";
            worksheet.Cell(currentRow, 5).Value = "Araç Türü";
            worksheet.Cell(currentRow, 6).Value = "Araç Tipi";
            worksheet.Cell(currentRow, 7).Value = "Proje";
            worksheet.Cell(currentRow, 8).Value = "Zimmetli Adı Soyadı";
            worksheet.Cell(currentRow, 9).Value = "Firma Bilgi";

            worksheet.Cell(currentRow, 10).Value = "Toplam Litre";

            worksheet.Cell(currentRow, 11).Value = "Toplam Tutar (İskontosuz)";
            worksheet.Cell(currentRow, 12).Value = "Toplam Tutar (İskontolu)";

            SettingFirstColumn(worksheet, result.Count);
            foreach (var item in result)
            {
                currentRow++;
                worksheet.Cell(currentRow, 1).Value = currentRow - 1;
                worksheet.Cell(currentRow, 2).Value = item.FuelDate;
                worksheet.Cell(currentRow, 3).Value = item.Plate;
                worksheet.Cell(currentRow, 4).Value = item.VehicleModelName;
                worksheet.Cell(currentRow, 5).Value = item.VehicleTypeName;
                worksheet.Cell(currentRow, 6).Value = item.VehicleTypeName == "Otomobil" ? "Binek" : "Ticari";
                worksheet.Cell(currentRow, 7).Value = item.UnitName;
                worksheet.Cell(currentRow, 8).Value = item.UserNameSurname;
                worksheet.Cell(currentRow, 9).Value = item.SupplierName;

                worksheet.Cell(currentRow, 10).Value = item.Liter;

                worksheet.Cell(currentRow, 11).Value = item.Amount;
                worksheet.Cell(currentRow, 12).Value = item.DiscountAmount;
            }
            currentRow++;

            //Otomatik genişlik
            //worksheet.Columns().AdjustToContents();
            //worksheet.Columns().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            //worksheet.Rows().AdjustToContents();
            //worksheet.Rows().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            ////Tutar sağa yaslama
            //var rangeColAmount = GetRowName(worksheet.Columns().Count()) + "2:J" + (result.Count + 2);
            //var colAmount = worksheet.Range(rangeColAmount);
            //colAmount.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
            SettingAllColumn(worksheet);
            //10.satır
            var totalColumnLiter = GetRowName(worksheet.Columns().Count() - 2);
            var totalFormul0 = "SUM(" + totalColumnLiter + "2:" + totalColumnLiter + (currentRow - 1) + ")"; //SUM(J2:J218)
            worksheet.Cell(currentRow, 10).FormulaA1 = totalFormul0;
            SetAmountColumnSettingNotMoney(worksheet, currentRow, 10);

            //11.satır
            var totalColumnNameAmount = GetRowName(worksheet.Columns().Count() - 1);
            var totalFormul1 = "SUM(" + totalColumnNameAmount + "2:" + totalColumnNameAmount + (currentRow - 1) + ")"; //SUM(J2:J218)
            worksheet.Cell(currentRow, 11).FormulaA1 = totalFormul1;
            SetAmountColumnSetting(worksheet, currentRow, 11);
            //worksheet.Cell(currentRow, 10).Style.Font.SetBold(true);
            //worksheet.Cell(currentRow, 10).Style.Fill.BackgroundColor = XLColor.Gold;
            //worksheet.Column(10).Style.Font.SetBold(true);
            //worksheet.Column(10).Style.NumberFormat.Format = "#,##0.00₺";


            //12.satır
            var totalColumnNameDiscountAmount = GetRowName(worksheet.Columns().Count());
            var totalFormul2 = "SUM(" + totalColumnNameDiscountAmount + "2:" + totalColumnNameDiscountAmount + (currentRow - 1) + ")"; //SUM(K2:K218)
            worksheet.Cell(currentRow, 12).FormulaA1 = totalFormul2;
            SetAmountColumnSetting(worksheet, currentRow, 12);

            //worksheet.Cell(currentRow, 11).Style.Font.SetBold(true);
            //worksheet.Cell(currentRow, 11).Style.Fill.BackgroundColor = XLColor.Gold;
            //worksheet.Column(11).Style.Font.SetBold(true);
            //worksheet.Column(11).Style.NumberFormat.Format = "#,##0.00₺";

            if (isDateVisible)//Tarih kolonu siliniyor
                worksheet.Columns(2, 2).Delete();
            return worksheet;
        }
        //Sıra no kolonu
        public IXLWorksheet SettingFirstColumn(IXLWorksheet worksheet, int columnCount)
        {
            //Sütun Kolonu
            var rangeCol = "A1:A" + (columnCount + 1);
            var col1 = worksheet.Range(rangeCol);
            col1.Style.Fill.BackgroundColor = XLColor.Gold;
            col1.Style.Font.SetBold(true);

            //Sıra Kolonu
            var rangeRow = "A1:" + GetRowName(worksheet.Columns().Count()) + "1";
            var row1 = worksheet.Range(rangeRow);
            row1.Style.Font.SetBold(true);
            row1.Style.Fill.BackgroundColor = XLColor.Gold;
            return worksheet;
        }
        public string GetRowName(int rowNumber)
        {
            string[] arr =
            {
                "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U",
                "V", "W", "X", "Y", "Z", // 1-26
                "AA", "AB", "AC", "AD", "AE", "AF", "AG", "AH", "AI", "AJ", "AK", "AL", "AM", "AN", "AO", "AP", "AQ", "AR", "AS", "AT", "AU",
                "AV", "AW", "AX", "AY", "AZ", // 27-52
                "BA", "BB", "BC", "BD", "BE", "BF", "BG", "BH", "BI", "BJ", "BK", "BL", "BM", "BN", "BO", "BP", "BQ", "BR", "BS", "BT", "BU",
                "BV", "BW", "BX", "BY", "BZ", // 53-78
                "CA", "CB", "CC", "CD", "CE", "CF", "CG", "CH", "CI", "CJ", "CK", "CL", "CM", "CN", "CO", "CP", "CQ", "CR", "CS", "CT", "CU",
                "CV", "CW", "CX", "CY", "CZ", // 79-104
            };

            return arr[rowNumber - 1];
        }
        public IXLWorksheet SettingAllColumn(IXLWorksheet worksheet)
        {
            //Otomatik genişlik
            worksheet.Columns().AdjustToContents();
            worksheet.Columns().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            worksheet.Rows().AdjustToContents();
            worksheet.Rows().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            return worksheet;
        }
        public IXLWorksheet SetAmountColumnSetting(IXLWorksheet worksheet, int currentRow, int column)
        {
            //ilgili satır ayarları
            worksheet.Cell(currentRow, column).Style.Font.SetBold(true);
            worksheet.Cell(currentRow, column).Style.Fill.BackgroundColor = XLColor.Gold;
            worksheet.Column(column).Style.Font.SetBold(true);
            worksheet.Column(column).Style.NumberFormat.Format = "#,##0.00₺";

            //Tutar sağa yaslama
            var rangeColAmount = GetRowName(column) + "2:" + GetRowName(column) + (currentRow + 2);
            var colAmount = worksheet.Range(rangeColAmount);
            colAmount.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
            return worksheet;
        }
        public IXLWorksheet SetAmountColumnSettingNotMoney(IXLWorksheet worksheet, int currentRow, int column)
        {
            //ilgili satır ayarları
            worksheet.Cell(currentRow, column).Style.Font.SetBold(true);
            worksheet.Cell(currentRow, column).Style.Fill.BackgroundColor = XLColor.Gold;
            worksheet.Column(column).Style.Font.SetBold(true);

            //Tutar sağa yaslama
            var rangeColAmount = GetRowName(column) + "2:" + GetRowName(column) + (currentRow + 2);
            var colAmount = worksheet.Range(rangeColAmount);
            colAmount.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
            return worksheet;
        }

        #endregion

        #region Tender


        public IActionResult TenderInfo(RFilterModelDto model)
        {
            var tenderAllInfo = _tenderService.GetTenderDetail(model.TenderId);
            var tender = tenderAllInfo.Tender;
            var tenderDetailList = tenderAllInfo.TenderDetailList;
            var amountList = tenderAllInfo.TotalAmountTenderDetail;
            var contactList = tenderAllInfo.ContactList;

            string contentRootPath = _env.WebRootPath;//_webHostEnvironment.ContentRootPath;
            XLWorkbook workbook = new XLWorkbook(contentRootPath + "/admin/images/Teklif-Ornegi.xlsx");
            IXLWorksheet worksheet = workbook.Worksheet("Sayfa1");
            if (tenderDetailList.Any())
            {
                var currentRow = 15;
                var counter = 0;
                worksheet.Cell(currentRow, 2).Value = "Sıra No";
                worksheet.Cell(currentRow, 3).Value = "Ürün Adı";
                worksheet.Cell(currentRow, 4).Value = "Miktar";
                worksheet.Cell(currentRow, 5).Value = "Birim";
                worksheet.Cell(currentRow, 6).Value = "Fiyat";
                worksheet.Cell(currentRow, 7).Value = "Toplam";

                //Sıra Kolonu
                var rangeRow = "A15:G15";
                var row1 = worksheet.Range(rangeRow);
                row1.Style.Font.SetBold(true);

                foreach (var item in tenderDetailList)
                {
                    currentRow++;
                    counter++;
                    worksheet.Cell(currentRow, 2).Value = counter;
                    worksheet.Cell(currentRow, 3).Value = item.ProductName;
                    worksheet.Cell(currentRow, 4).Value = item.Piece;
                    worksheet.Cell(currentRow, 5).Value = item.UnitTypeName;
                    worksheet.Cell(currentRow, 6).Value = item.SellingCost;
                    worksheet.Cell(currentRow, 7).Value = item.Piece * item.SellingCost;
                }
                SettingAllColumn(worksheet);
                SetTenderAmountColumnSetting(worksheet, currentRow, 6); //Ürün fiyatı

                //Genel Toplam
                SetTenderAmountColumnSetting(worksheet, currentRow, 7);
                var totalFormul2 = "SUM(G16:G" + currentRow + ")";
                currentRow++;
                worksheet.Cell("G" + currentRow).FormulaA1 = totalFormul2;
                worksheet.Cell("G" + currentRow).Style.Fill.BackgroundColor = XLColor.Gold;

                #region static val
                worksheet.Cell("E8").Value = tender.TenderDate != null ? tender.TenderDate.Value.ToString("d") : "";
                worksheet.Cell("G8").Value = "Satış No: " + tender.SalesNumber;
                worksheet.Cell("C11").Value = string.Join(" ", contactList.Select(s => s.Name));
                worksheet.Cell("C12").Value = tender.Name;
                worksheet.Cell("E11").Value = tender.NameSurname;
                worksheet.Cell("D12").Value = tender.Email;

                worksheet.Columns("B").Style.Font.SetBold(true);
                worksheet.Columns("C").Style.Font.SetBold(true);
                worksheet.Columns("D").Style.Font.SetBold(true);
                worksheet.Columns("E").Style.Font.SetBold(true);
                worksheet.Columns("B").Width = 12;
                worksheet.Columns("C").Width = 40;
                worksheet.Columns("D").Width = 12;
                worksheet.Columns("E").Width = 12;
                worksheet.Columns("F").Width = 12;
                worksheet.Columns("G").Width = 12;
                #endregion
                #region tender FooterInfoText
                var infoText = Regex.Split(tender.FooterInfo, @"<.*?>").Where(w => w.Replace(" ", "") != "\n").ToList();
                currentRow += 2;
                foreach (var item in infoText)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 2).Value = item;
                    worksheet.Range(worksheet.Cell(currentRow, 2), worksheet.Cell(currentRow, 10)).Merge()
                        .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                }
                #endregion
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();
            var folderName = tender.Name + ".xlsx";
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", folderName);
        }
        public IXLWorksheet SetTenderAmountColumnSetting(IXLWorksheet worksheet, int currentRow, int column)
        {
            //ilgili satır ayarları
            worksheet.Cell(currentRow, column).Style.Font.SetBold(true);
            //worksheet.Cell(currentRow, column).Style.Fill.BackgroundColor = XLColor.Gold;
            worksheet.Column(column).Style.Font.SetBold(true);
            worksheet.Column(column).Style.NumberFormat.Format = "#,##0.00₺";

            //Tutar sağa yaslama
            var rangeColAmount = GetRowName(column) + "16:" + GetRowName(column) + currentRow;
            var colAmount = worksheet.Range(rangeColAmount);
            colAmount.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
            return worksheet;
        }
        #endregion
    }
}
