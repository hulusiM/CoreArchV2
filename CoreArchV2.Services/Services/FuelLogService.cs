using AutoMapper;
using CoreArchV2.Core.Entity.Common;
using CoreArchV2.Core.Entity.Logistics;
using CoreArchV2.Core.Enum;
using CoreArchV2.Data.GenericRepository;
using CoreArchV2.Data.UnitOfWork;
using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Dto.ELogisticsDto;
using CoreArchV2.Dto.EReportDto;
using CoreArchV2.Services.Interfaces;
using CoreArchV2.Services.SignalR;
using CoreArchV2.Utilies;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Transactions;

namespace CoreArchV2.Services.Services
{
    public class FuelLogService : IFuelLogService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IGenericRepository<City> _cityRepository;
        private readonly IGenericRepository<FuelLog> _fuelLogRepository;
        private readonly ILogger<FuelLogService> _logger;
        private readonly IGenericRepository<LookUpList> _lookUpListRepository;
        private readonly IGenericRepository<Vehicle> _vehicleRepository;
        private readonly IHubContext<SignalRHub> _hubContext;
        private readonly IMessageService _messageService;
        private readonly IReportService _reportService;

        public FuelLogService(IUnitOfWork uow,
            IMapper mapper,
            ILogger<FuelLogService> logger,
            IMessageService messageService,
            IReportService reportService,
            IHubContext<SignalRHub> hubContext
            )
        {
            _uow = uow;
            _mapper = mapper;
            _logger = logger;
            _reportService = reportService;
            _lookUpListRepository = uow.GetRepository<LookUpList>();
            _fuelLogRepository = uow.GetRepository<FuelLog>();
            _vehicleRepository = uow.GetRepository<Vehicle>();
            _cityRepository = uow.GetRepository<City>();
            _messageService = messageService;
            _hubContext = hubContext;
        }


        public PagedList<EFuelLogDto> GetAllWithPaged(int? page, EFuelLogDto filterModel)
        {
            //Sıra no için
            var pageStartCount = 0;
            if (!page.HasValue)
                pageStartCount = 1;

            var list = from fl in _fuelLogRepository.GetAll()
                       join v in _vehicleRepository.GetAll() on fl.VehicleId equals v.Id
                       join l in _lookUpListRepository.GetAll() on fl.FuelStationId equals l.Id
                       where fl.Status == Convert.ToBoolean(Status.Active)
                       select new EFuelLogDto
                       {
                           Id = fl.Id,
                           VehicleId = fl.VehicleId,
                           Km = fl.Km,
                           TransactionDate = fl.TransactionDate,
                           TotalAmount = fl.TotalAmount,
                           DiscountPercent = "%" + fl.DiscountPercent,
                           FuelStationId = fl.FuelStationId,
                           FuelStationName = "<span class='label bg-orange-400 full-width'>" + l.Name + "</span>",
                           TotalAmountDiscount = fl.TotalAmount - fl.TotalAmount * fl.DiscountPercent / 100,
                           Description = fl.Description,
                           Plate = "<span class='label bg-success-400 full-width'>" + v.Plate + " (" + v.Id + ")</span>",
                           PageStartCount = pageStartCount == 0 ? page.Value - 1 : pageStartCount - 1,
                           CustomButton = "<li class='text-primary-400'><a data-toggle='modal' onclick='funcEditFuel(" +
                                          fl.Id + ");'><i class='icon-pencil5'></i></a></li>",
                           DeleteButtonActive = true
                       };

            if (filterModel.VehicleId > 0)
                list = list.Where(w => w.VehicleId == filterModel.VehicleId);

            if (filterModel.FuelStationId > 0)
                list = list.Where(w => w.FuelStationId == filterModel.FuelStationId);

            if (filterModel.TransactionDate != DateTime.MinValue)
            {
                var startDate = filterModel.TransactionDate;
                var endDate = filterModel.TransactionDate.AddMonths(1).AddDays(-1);
                list = list.Where(w => w.TransactionDate >= startDate && w.TransactionDate <= endDate);
            }

            var lastResult =
                new PagedList<EFuelLogDto>(list.OrderByDescending(o => o.TransactionDate), page, PagedCount.GridKayitSayisi);
            return lastResult;
        }

        public async Task<EResultDto> InsertAsync(EFuelLogDto tempModel)
        {
            var result = new EResultDto();
            try
            {
                if (tempModel.TransactionDate <= DateTime.Now.Date)
                {
                    if (await IsDebitRangeAsync(tempModel))
                    {
                        var model = _mapper.Map<FuelLog>(tempModel);
                        model.CreatedBy = tempModel.CreatedBy;
                        model.IsPublisher = false;
                        model.InsertType = (int)FuelInsertType.User;
                        var resultEntity = _fuelLogRepository.InsertAsync(model);
                        _uow.SaveChanges();
                        result.Id = resultEntity.Id;
                        result.IsSuccess = true;
                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.Message = "Girilen kayıt zimmet aralığında değildir, ilgili plakanın zimmettini kontrol ediniz.";
                    }
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = "Yakıt tarihi, gelecek tarih olamaz!";
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = "Kayıt sırasında hata oluştu!";
                //_logger.LogError(result.Message + "----" + ex.Message);
            }

            return result;
        }

        public async Task<EResultDto> InsertBulkAsync(EFuelReadExcelDto model)
        {
            var result = new EResultDto();
            try
            {
                if (model != null & model.FuelList.Count > 0 && model.FuelStationId > 0)
                {
                    //plakalar db'de var mı kontrol ediliyor
                    var entities = new List<FuelLog>();
                    foreach (var item in model.FuelList)
                    {
                        var entity = _vehicleRepository.Find(item.VehicleId);
                        if (entity != null)
                        {
                            entities.Add(new FuelLog()
                            {
                                CreatedBy = model.CreatedBy,
                                VehicleId = entity.Id,
                                TransactionDate = item.TransactionDate,
                                TotalAmount = item.TotalAmount,
                                DiscountPercent = model.DiscountPercent,
                                Km = item.Km ?? 0,
                                Description = model.UserName + " kullanıcısı tarafından excel'den toplu olarak eklenmiştir.",
                                FuelStationId = model.FuelStationId,
                                IsPublisher = false
                            });
                        }
                    }

                    using (var scope = new TransactionScope())
                    {
                        await _fuelLogRepository.InsertRangeAsync(entities);
                        _uow.SaveChanges();
                        scope.Complete();
                        result.Message = "Tüm kayıtlar başarıyla eklendi";
                    }
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = "Excel okunamadı";
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = "Hata oluştu";
            }

            return result;
        }

        //Girilen yakıt verisi zimmet aralığında mı kontrol eder
        public async Task<bool> IsDebitRangeAsync(EFuelLogDto tempModel)
        {
            var debitList = await _reportService.GetDebitListRange(new RFilterModelDto() { VehicleId = tempModel.VehicleId, EndDate = DateTime.Now });
            var result = false;
            var isAny = debitList.Any(w => w.StartDate <= tempModel.TransactionDate && tempModel.TransactionDate < w.EndDate);
            if (isAny)
                result = true;
            else
                result = false;

            return result;
        }

        public async Task<EResultDto> UpdateAsync(EFuelLogDto tempModel)
        {
            var result = new EResultDto();
            try
            {
                if (tempModel.TransactionDate <= DateTime.Now.Date)
                {
                    if (await IsDebitRangeAsync(tempModel))
                    {
                        var entity = _fuelLogRepository.Find(tempModel.Id);
                        var newEntity = _mapper.Map(tempModel, entity);
                        newEntity.InsertType = (int)FuelInsertType.User;
                        _fuelLogRepository.Update(newEntity);
                        _uow.SaveChanges();
                        result.Id = entity.Id;
                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.Message = "Girilen kayıt zimmet aralığında değildir, ilgili plakanın zimmettini kontrol ediniz.";
                    }
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = "Yakıt tarihi, gelecek tarih olamaz!";
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = "Güncelleme sırasında hata oluştu!";
                //_logger.LogError(result.Message + "----" + ex.Message);
            }

            return result;
        }

        public EResultDto Delete(int id)
        {
            var result = new EResultDto();
            try
            {
                var entity = _fuelLogRepository.Find(id);
                entity.Status = Convert.ToBoolean(Status.Passive);
                _fuelLogRepository.Update(entity);
                _uow.SaveChanges();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = "Silme sırasında hata oluştu!";
                //_logger.LogError(result.Message + "----" + ex.Message);
            }

            return result;
        }

        public FuelLog GetById(int id)
        {
            return _fuelLogRepository.Find(id);
        }

        //Yayınlanan/yayınlanmayan yakıtları listeler
        public List<EFuelLogDto> GetPublishFuel(FuelPublisher publisher)
        {
            var list = new List<FuelLog>();
            if (publisher == FuelPublisher.DisabledPublish)
                list = _fuelLogRepository.GetAll().Where(w => w.Status == true && w.IsPublisher == false).ToList();
            else if (publisher == FuelPublisher.Publish)
                list = _fuelLogRepository.GetAll().Where(w => w.Status == true && w.IsPublisher == true).ToList();
            else if (publisher == FuelPublisher.Delete)
                list = _fuelLogRepository.GetAll().Where(w => w.Status == true).ToList();

            var monthGroup = list.GroupBy(g => g.TransactionDate.ToString("yyyy MMMM", new CultureInfo("tr-TR"))).Select(s => new EFuelLogDto()
            {
                DateName = s.First().TransactionDate.ToString("yyyy MMMM", new CultureInfo("tr-TR")),
                ListCount = s.GroupBy(g => g.VehicleId).Count(),
                TransactionDate = s.First().TransactionDate
            }).OrderBy(o => o.TransactionDate).ToList();
            return monthGroup;
        }

        //Yakıtları yayına alır/yayından kaldırır
        public bool SetPublishFuel(EFuelLogDto model)
        {
            try
            {
                var list = new List<FuelLog>();
                foreach (var item in model.Dates)
                    list.AddRange(_fuelLogRepository.Where(w => w.Status == true && w.TransactionDate.Year == item.Year && w.TransactionDate.Month == item.Month).ToList());

                if (model.Publisher == FuelPublisher.DisabledPublish)//yayından kaldır
                    list.ForEach(f => f.IsPublisher = false);
                else if (model.Publisher == FuelPublisher.Publish)//yayınla
                    list.ForEach(f => f.IsPublisher = true);
                else if (model.Publisher == FuelPublisher.Delete)//sil
                    list.ForEach(f => f.Status = false);

                _fuelLogRepository.UpdateRange(list);
                _uow.SaveChanges();

                //yayınlanınca mesaj gitsin,pasife alınınca yayınlanma mesajı silinsin
                SendMessageAndInsertMessage(model);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public void SendMessageAndInsertMessage(EFuelLogDto model)
        {
            try
            {
                foreach (var item in model.Dates.GroupBy(g => g.Date.ToString("yyyy MMMM")))
                {
                    var message = "<b>" + item.Key.ToString() + "</b> tarihli yakıt raporu yayınlandı.";
                    if (model.Publisher == FuelPublisher.Publish && !_messageService.IsAny(message))
                    {
                        var entity = new Message()
                        {
                            Description = message,
                            Head = "Yakıt Rapor Hk.",
                            Type = (int)MessageType.Website,
                            UnRead = false
                        };
                        _messageService.FuelMessageInsert(entity, true);
                    }
                    else
                    {
                        model.Description = message;
                        _messageService.DeleteRange(model);
                    }
                }
            }
            catch (Exception e)
            {
            }
        }


    }
}