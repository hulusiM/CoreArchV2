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
using CoreArchV2.Utilies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Transactions;

namespace CoreArchV2.Services.Services
{
    public class MaintenanceService : IMaintenanceService
    {
        private readonly IGenericRepository<City> _cityRepository;
        private readonly IGenericRepository<FileUpload> _fileUploadRepository;
        private readonly ILogger<MaintenanceService> _logger;
        private readonly IGenericRepository<LookUpList> _lookUpListRepository;
        private readonly IGenericRepository<MaintenanceFile> _maintenanceFileRepository;
        private readonly IGenericRepository<Maintenance> _maintenanceRepository;
        private readonly IGenericRepository<MaintenanceType> _maintenanceTypeRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<Vehicle> _vehicleRepository;
        private readonly IReportService _reportService;
        private readonly IGenericRepository<Tire> _tireRepository;

        private readonly IWebHostEnvironment _env;


        public MaintenanceService(IUnitOfWork uow,
            IMapper mapper,
            IReportService reportService,
            ILogger<MaintenanceService> logger,
            IWebHostEnvironment env)
        {
            _uow = uow;
            _env = env;
            _mapper = mapper;
            _logger = logger;
            _lookUpListRepository = uow.GetRepository<LookUpList>();
            _userRepository = uow.GetRepository<User>();
            _maintenanceRepository = uow.GetRepository<Maintenance>();
            _maintenanceTypeRepository = uow.GetRepository<MaintenanceType>();
            _maintenanceFileRepository = uow.GetRepository<MaintenanceFile>();
            _vehicleRepository = uow.GetRepository<Vehicle>();
            _cityRepository = uow.GetRepository<City>();
            _fileUploadRepository = uow.GetRepository<FileUpload>();
            _reportService = reportService;
            _tireRepository = uow.GetRepository<Tire>();
        }

        public PagedList<EMaintenanceDto> GetAllWithPaged(int? page, bool isHgs, EMaintenanceDto filterModel)
        {
            //Sıra no için
            var pageStartCount = 0;
            if (!page.HasValue)
                pageStartCount = 1;

            var list = (from m in _maintenanceRepository.GetAll()
                        join v in _vehicleRepository.GetAll() on m.VehicleId equals v.Id
                        join u in _userRepository.GetAll() on m.RequestUserId equals u.Id
                        join l2 in _lookUpListRepository.GetAll() on m.SupplierId equals l2.Id
                        where m.Status == Convert.ToBoolean(Status.Active)
                        select new EMaintenanceDto
                        {
                            Id = m.Id,
                            Plate = "<span class='label bg-success-400 full-width'>" + v.Plate + " (" + v.Id + ")</span>",
                            TotalAmount = m.TotalAmount,
                            VehicleId = v.Id,
                            RequestFullName = u.Name + " " + u.Surname + "/" + u.MobilePhone,
                            RequestUserId = m.RequestUserId,
                            InvoiceNo = m.InvoiceNo,
                            InvoiceDate = m.InvoiceDate,
                            LastKm = m.LastKm,
                            FixtureTypeId = v.FixtureTypeId,
                            SupplierId = m.SupplierId,
                            UserFaultAmount = m.UserFaultAmount,
                            UserFaultDescription = m.UserFaultDescription,
                            //Description = m.Description.Length > 30 ? (m.Description.Substring(0, 30) + " ...") : m.Description,
                            DeleteButtonActive = true,
                            CustomButton = "<li class='text-primary-400'><a data-toggle='modal' onclick='funcEditMaintenance(" +
                                           m.Id + ");'><i class='icon-pencil5'></i></a></li>",
                            PageStartCount = pageStartCount == 0 ? page.Value - 1 : pageStartCount - 1
                        });

            if (isHgs)
                list = list.Where(w => w.SupplierId == (int)Supplier.Ptt);
            else
                list = list.Where(w => w.SupplierId != (int)Supplier.Ptt);

            if (filterModel.RequestUserId > 0)
                list = list.Where(w => w.RequestUserId == filterModel.RequestUserId);

            if (filterModel.VehicleId > 0)
                list = list.Where(w => w.VehicleId == filterModel.VehicleId);

            if (filterModel.SupplierId > 0)
                list = list.Where(w => w.SupplierId == filterModel.SupplierId);

            if (filterModel.FixtureTypeId > 0)
                list = list.Where(w => w.FixtureTypeId == filterModel.FixtureTypeId);

            if (!string.IsNullOrEmpty(filterModel.InvoiceNo))
                list = list.Where(w => w.InvoiceNo.Contains(filterModel.InvoiceNo));

            return new PagedList<EMaintenanceDto>(list.OrderByDescending(o => o.InvoiceDate), page, PagedCount.GridKayitSayisi);
        }

        public EMaintenanceDto GetById(int id)
        {
            var result = _maintenanceRepository.Find(id);
            var entityDto = _mapper.Map<EMaintenanceDto>(result);

            var files = (from m in _maintenanceRepository.GetAll()
                         join mf in _maintenanceFileRepository.GetAll() on m.Id equals mf.MaintenanceId
                         join fu in _fileUploadRepository.GetAll() on mf.FileUploadId equals fu.Id
                         where m.Id == id
                         select new EFileUploadDto
                         {
                             Id = fu.Id,
                             MaintenanceId = m.Id,
                             Name = fu.Name,
                             Extention = fu.Extention,
                             FileSize = fu.FileSize,
                             MaintenanceFileId = mf.Id,
                             UserId = m.RequestUserId,
                             VehicleId = m.VehicleId,
                         }).ToList();

            entityDto.files = files;
            entityDto.MaintenanceTypeIds = _maintenanceTypeRepository.GetAll().Where(w => w.MaintenanceId == id)
                .Select(s => s.TypeId.ToString()).ToList();

            var tire = (from t in _tireRepository.GetAll()
                        join l in _lookUpListRepository.GetAll() on t.DimensionTypeId equals l.Id
                        join l2 in _lookUpListRepository.GetAll() on t.TireTypeId equals l2.Id
                        where t.Status && t.VehicleId == entityDto.VehicleId
                        select new ETireDto()
                        {
                            DimensionTypeName = l2.Name + "/Ebat: " + l.Name,
                        }).FirstOrDefault();
            entityDto.TireInfo = tire != null ? tire.DimensionTypeName : "";
            return entityDto;
        }


        public async Task<EResultDto> Insert(IList<IFormFile> files, EMaintenanceDto tempModel)
        {
            var result = new EResultDto();
            result.IsSuccess = false;
            try
            {
                if (!string.IsNullOrEmpty(tempModel.InvoiceNo) &&
                    _maintenanceRepository.Where(w => w.InvoiceNo == tempModel.InvoiceNo).FirstOrDefault() != null)
                    result.Message = "Bu fatura numarasıyla daha önce kayıt yapılmıştır.";
                else if (tempModel.UserFaultAmount != null && tempModel.TotalAmount < tempModel.UserFaultAmount)
                    result.Message = "Kullanıcı hata tutarı, genel toplam tutarını geçemez.";
                else if (tempModel.InvoiceDate > DateTime.Now.Date)
                    result.Message = "Fatura tarihi, gelecek tarih olamaz.";
                else if (!await IsDebitRangeAsync(tempModel))
                    result.Message = "Girilen kayıt zimmet aralığında değildir, ilgili plakanın zimmettini kontrol ediniz.";
                else
                {
                    using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        var model = _mapper.Map<Maintenance>(tempModel);
                        model.CreatedBy = tempModel.CreatedBy;
                        var resultEntity = _maintenanceRepository.Insert(model);
                        _uow.SaveChanges();

                        //MaintenanceType table insert
                        foreach (var item in tempModel.MaintenanceTypeIds[0].Split(','))
                            _maintenanceTypeRepository.Insert(new MaintenanceType
                            { MaintenanceId = resultEntity.Id, TypeId = Convert.ToInt32(item) });

                        _uow.SaveChanges();

                        if (files.Count > 0)
                        {
                            result = FileInsert(files, resultEntity.Id);
                            if (!result.IsSuccess)
                                return new EResultDto { IsSuccess = false };
                        }

                        scope.Complete();
                        result.IsSuccess = true;
                        result.Id = resultEntity.Id;
                    }
                }
            }
            catch (Exception ex)
            {
                var fs = new FileService(_uow, _env);
                fs.FileUploadWithTransactionDelete(result.Ids, result.IdNames, "uploads/logistics/");
                result.IsSuccess = false;
                result.Message = "Hata oluştu";
            }

            return result;
        }


        public async Task<EResultDto> Update(IList<IFormFile> files, EMaintenanceDto tempModel)
        {
            var result = new EResultDto();
            result.IsSuccess = false;
            try
            {
                if (tempModel.InvoiceDate > DateTime.Now.Date)
                    result.Message = "Fatura tarihi, gelecek tarih olamaz!";
                else if (!await IsDebitRangeAsync(tempModel))
                    result.Message = "Girilen kayıt zimmet aralığında değildir, ilgili plakanın zimmettini kontrol ediniz.";
                else if (tempModel.UserFaultAmount != null && tempModel.TotalAmount < tempModel.UserFaultAmount)
                    result.Message = "Kullanıcı hata tutarı, genel toplam tutarını geçemez.";
                else
                {
                    using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        //Eski maintenanceType table bilgileri siliniyor
                        var maintenanceTypeIds = _maintenanceTypeRepository.GetAll()
                            .Where(w => w.MaintenanceId == tempModel.Id).ToList();
                        _maintenanceTypeRepository.DeleteRange(maintenanceTypeIds);
                        _uow.SaveChanges();

                        //Yeni maintenanceType bilgileri ekleniyor
                        foreach (var item in tempModel.MaintenanceTypeIds[0].Split(','))
                            _maintenanceTypeRepository.Insert(new MaintenanceType
                            { MaintenanceId = tempModel.Id, TypeId = Convert.ToInt32(item) });

                        var entity = _maintenanceRepository.Find(tempModel.Id);
                        var newEntity = _mapper.Map(tempModel, entity);
                        _maintenanceRepository.Update(newEntity);
                        _uow.SaveChanges();

                        if (files.Count > 0)
                        {
                            result = FileInsert(files, newEntity.Id);
                            if (!result.IsSuccess)
                                return new EResultDto { IsSuccess = false };
                        }

                        scope.Complete();
                        result.IsSuccess = true;
                        result.Id = entity.Id;
                    }
                }
            }
            catch (Exception ex)
            {
                var fs = new FileService(_uow, _env);
                fs.FileUploadWithTransactionDelete(result.Ids, result.IdNames, "uploads/logistics/");
                result.IsSuccess = false;
                result.Message = "Hata oluştu";
            }
            return result;
        }

        //Girilen bakım/onarım verisi zimmet aralığında mı kontrol eder
        public async Task<bool> IsDebitRangeAsync(EMaintenanceDto tempModel)
        {
            var debitList = await _reportService.GetDebitListRange(new RFilterModelDto() { VehicleId = tempModel.VehicleId, EndDate = DateTime.Now });
            var result = false;
            var isAny = debitList.Any(w => w.StartDate <= tempModel.InvoiceDate && tempModel.InvoiceDate < w.EndDate);
            if (isAny)
                result = true;
            else
                result = false;

            return result;
        }

        public EResultDto Delete(int id)
        {
            var result = new EResultDto();
            try
            {
                var entity = _maintenanceRepository.Find(id);
                entity.Status = Convert.ToBoolean(Status.Passive);
                _maintenanceRepository.Update(entity);
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

        public List<EMaintenanceDto> GetByVehicleIdMaintenanceHistory(int vehicleId, bool isHgsFilter)
        {
            var list = (from m in _maintenanceRepository.GetAll()
                        join v in _vehicleRepository.GetAll() on m.VehicleId equals v.Id
                        join u in _userRepository.GetAll() on m.RequestUserId equals u.Id
                        join l in _lookUpListRepository.GetAll() on m.SupplierId equals l.Id
                        where m.Status == true && m.VehicleId == vehicleId
                        select new EMaintenanceDto
                        {
                            Id = m.Id,
                            CreatedDate = m.CreatedDate,
                            Plate = v.Plate,
                            VehicleId = v.Id,
                            RequestFullName = u.Name != null
                                ? "<a data-toggle='modal' href='#PersonModal' onclick='funcGetPerson(" + u.Id +
                                  ")' class='text-danger'>" + u.Name + " " + u.Surname + "</a>"
                                : "",
                            SupplierName = l.Name,
                            SupplierId = m.SupplierId,
                            InvoiceNo = m.InvoiceNo,
                            LastKm = m.LastKm,
                            UserFaultAmount = m.UserFaultAmount,
                            UserFaultDescription = m.UserFaultDescription,
                            Description = m.Description,
                            TotalAmount = m.TotalAmount,
                            InvoiceDate = m.InvoiceDate
                        });

            if (isHgsFilter)
                list = list.Where(w => w.SupplierId == (int)Supplier.Ptt);
            else
                list = list.Where(w => w.SupplierId != (int)Supplier.Ptt);

            var result = list.OrderByDescending(o => o.CreatedDate).ToList();
            for (var i = 0; i < result.Count(); i++)
            {
                result[i].MaintenancePieces = (from mt in _maintenanceTypeRepository.GetAll()
                                               join l in _lookUpListRepository.GetAll() on mt.TypeId equals l.Id
                                               where mt.MaintenanceId == result[i].Id
                                               select new { l.Name })
                    .Select(s => s.Name)
                    .ToArray();

                result[i].CustomButton = GetFilesList(result[i].Id).Any()
                    ? "<a onclick='getVehicleMaintenanceFilesList(" + result[i].Id +
                      ")' class='text-danger'><i class='icon-folder-search'></i></a>"
                    : "Dosya Bulunamadı";
            }
            return result;
        }

        public List<EFileUploadDto> GetFilesList(int maintenanceId)
        {
            var result = (from m in _maintenanceRepository.GetAll()
                          join mf in _maintenanceFileRepository.GetAll() on m.Id equals mf.MaintenanceId
                          join fu in _fileUploadRepository.GetAll() on mf.FileUploadId equals fu.Id
                          where m.Id == maintenanceId
                          select new EFileUploadDto
                          {
                              Id = fu.Id,
                              MaintenanceId = m.Id,
                              Name = fu.Name,
                              Extention = fu.Extention,
                              FileSize = fu.FileSize,
                              MaintenanceFileId = mf.Id,
                              UserId = m.RequestUserId,
                              VehicleId = m.VehicleId
                          }).ToList();
            return result;
        }


        public EResultDto FileInsert(IList<IFormFile> files, int maintenanceId)
        {
            var result = new EResultDto();
            var fs = new FileService(_uow, _env);
            try
            {
                if (maintenanceId > 0)
                {
                    result = fs.FileUploadInsertLogistics(files);
                    if (result.IsSuccess)
                    {
                        foreach (var item in result.Ids)
                        {
                            var entity = _maintenanceFileRepository.Insert(new MaintenanceFile
                            {
                                FileUploadId = item,
                                MaintenanceId = maintenanceId
                            });
                        }
                        _uow.SaveChanges();
                        result.Id = maintenanceId;
                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.Message = "Dosya yüklemede hata oluştu";
                    }
                }
                else
                    result.IsSuccess = false;
            }
            catch (Exception)
            {
                fs.FileUploadWithTransactionDelete(result.Ids, result.IdNames, "uploads/logistics/");
                result.IsSuccess = false;
                result.Message = "Hata oluştu";
            }
            return result;
        }
    }
}