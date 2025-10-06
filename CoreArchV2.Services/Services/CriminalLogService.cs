using AutoMapper;
using CoreArchV2.Core.Entity.Common;
using CoreArchV2.Core.Entity.Logistics;
using CoreArchV2.Core.Enum;
using CoreArchV2.Data.GenericRepository;
using CoreArchV2.Data.UnitOfWork;
using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Dto.ELogisticsDto;
using CoreArchV2.Services.Interfaces;
using CoreArchV2.Utilies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Transactions;

namespace CoreArchV2.Services.Services
{
    public class CriminalLogService : ICriminalLogService
    {
        private readonly IGenericRepository<City> _cityRepository;
        private readonly IGenericRepository<CriminalFile> _criminalFileRepository;
        private readonly IGenericRepository<CriminalLog> _criminalLogRepository;
        private readonly IGenericRepository<FileUpload> _fileUploadRepository;
        private readonly ILogger<CriminalLogService> _logger;
        private readonly IGenericRepository<LookUpList> _lookUpListRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<Vehicle> _vehicleRepository;

        private readonly IWebHostEnvironment _env;


        public CriminalLogService(IUnitOfWork uow,
            IMapper mapper,
            ILogger<CriminalLogService> logger,
            IWebHostEnvironment env)
        {
            _uow = uow;
            _env = env;
            _mapper = mapper;
            _logger = logger;
            _lookUpListRepository = uow.GetRepository<LookUpList>();
            _userRepository = uow.GetRepository<User>();
            _vehicleRepository = uow.GetRepository<Vehicle>();
            _cityRepository = uow.GetRepository<City>();
            _fileUploadRepository = uow.GetRepository<FileUpload>();
            _criminalLogRepository = uow.GetRepository<CriminalLog>();
            _criminalFileRepository = uow.GetRepository<CriminalFile>();
        }

        public PagedList<ECriminalLogDto> GetAllWithPaged(int? page, ECriminalLogDto filterModel)
        {
            //Sıra no için
            var pageStartCount = 0;
            if (!page.HasValue)
                pageStartCount = 1;

            var list = from cl in _criminalLogRepository.GetAll()
                       join l in _lookUpListRepository.GetAll() on cl.CriminalTypeId equals l.Id
                       join v in _vehicleRepository.GetAll() on cl.VehicleId equals v.Id
                       join c in _cityRepository.GetAll() on cl.CriminalDistrictId equals c.Id
                       join c2 in _cityRepository.GetAll() on c.ParentId equals c2.Id
                       join u in _userRepository.GetAll() on cl.CriminalOwnerId equals u.Id into uL
                       from u in uL.DefaultIfEmpty()
                       where cl.Status == true
                       select new ECriminalLogDto
                       {
                           Id = cl.Id,
                           CriminalTypeName = "<span class='label bg-orange-400 full-width'>" + l.Name + "</span>",
                           Amount = cl.Amount,
                           VehicleId = cl.VehicleId,
                           CityId = c2.Id,
                           CriminalOwnerId = cl.CriminalOwnerId,
                           CriminalDate = cl.CriminalDate,
                           PaidDate = cl.PaidDate,
                           PaidNameSurname = u.Name + " " + u.Surname,
                           Plate = "<span class='label bg-success-400 full-width'>" + v.Plate + "</span>",
                           PaidAmount = cl.PaidAmount,
                           CityAndDistrict = c2.Name + "/" + c.Name,
                           CustomButton = "<li class='text-primary-400'><a data-toggle='modal' onclick='funcEditCriminal(" +
                                          cl.Id + ");'><i class='icon-pencil5'></i></a></li>",
                           PageStartCount = pageStartCount == 0 ? page.Value - 1 : pageStartCount - 1,
                           DeleteButtonActive = true
                       };

            if (filterModel.CriminalTypeId > 0)
                list = list.Where(w => w.CriminalTypeId == filterModel.CriminalTypeId);

            if (filterModel.CriminalOwnerId > 0)
                list = list.Where(w => w.CriminalOwnerId == filterModel.CriminalOwnerId);

            if (filterModel.VehicleId > 0)
                list = list.Where(w => w.VehicleId == filterModel.VehicleId);

            if (filterModel.CityId > 0)
                list = list.Where(w => w.CityId == filterModel.CityId);

            return new PagedList<ECriminalLogDto>(list.OrderByDescending(o => o.Id), page, PagedCount.GridKayitSayisi);
        }

        public ECriminalLogDto GetById(int id)
        {
            var result = _criminalLogRepository.Find(id);
            var entityDto = _mapper.Map<ECriminalLogDto>(result);
            //entityDto.CityId = _cityRepository.Find(entityDto.CriminalDistrictId).Id;

            var files = (from c in _criminalLogRepository.GetAll()
                         join cf in _criminalFileRepository.GetAll() on c.Id equals cf.CriminalLogId
                         join fu in _fileUploadRepository.GetAll() on cf.FileUploadId equals fu.Id
                         where c.Id == id
                         select new EFileUploadDto
                         {
                             Id = fu.Id,
                             Name = fu.Name,
                             Extention = fu.Extention,
                             FileSize = fu.FileSize,
                             CriminalFileId = cf.Id,
                             CriminalTypeId = c.CriminalTypeId,
                             VehicleId = c.VehicleId,
                             CriminalId = c.Id
                         }).ToList();
            entityDto.files = files;
            return entityDto;
        }


        public EResultDto Insert(IList<IFormFile> files, ECriminalLogDto tempModel)
        {
            var result = new EResultDto();
            try
            {
                if (tempModel.CriminalDate.Date <= DateTime.Now.Date)
                {
                    using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        var model = _mapper.Map<CriminalLog>(tempModel);
                        model.CreatedBy = tempModel.CreatedBy;
                        var resultEntity = _criminalLogRepository.Insert(model);
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
                else
                {
                    result.IsSuccess = false;
                    result.Message = "Ceza tarihi, ileri tarihli olamaz";
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


        public EResultDto Update(IList<IFormFile> files, ECriminalLogDto tempModel)
        {
            var result = new EResultDto();
            try
            {
                if (tempModel.CriminalDate.Date <= DateTime.Now.Date)
                {
                    using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        var entity = _criminalLogRepository.Find(tempModel.Id);
                        var resultEntity = _mapper.Map(tempModel, entity);
                        _criminalLogRepository.Update(resultEntity);
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
                else
                {
                    result.IsSuccess = false;
                    result.Message = "Ceza tarihi, ileri tarihli olamaz";
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

        public EResultDto Delete(int id)
        {
            var result = new EResultDto();
            try
            {
                var entity = _criminalLogRepository.Find(id);
                entity.Status = Convert.ToBoolean(Status.Passive);
                _criminalLogRepository.Update(entity);
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


        public EResultDto FileInsert(IList<IFormFile> files, int criminalId)
        {
            var result = new EResultDto();
            var fs = new FileService(_uow, _env);
            try
            {
                if (criminalId > 0)
                {
                    result = fs.FileUploadInsertLogistics(files);
                    if (result.IsSuccess && result.Ids.Length > 0)
                    {
                        foreach (var item in result.Ids)
                        {
                            var entity = _criminalFileRepository.Insert(new CriminalFile
                            {
                                FileUploadId = item,
                                CriminalLogId = criminalId
                            });
                        }
                        _uow.SaveChanges();
                        result.Id = criminalId;
                    }
                    else
                        result.IsSuccess = false;
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