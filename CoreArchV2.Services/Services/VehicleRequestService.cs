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
using Microsoft.Extensions.Logging;

namespace CoreArchV2.Services.Services
{
    public class VehicleRequestService : IVehicleRequestService
    {
        private readonly IGenericRepository<City> _cityRepository;
        private readonly ILogger<VehicleRequestService> _logger;
        private readonly IGenericRepository<LookUpList> _lookUpListRepository;
        private readonly IMapper _mapper;
        private readonly IGenericRepository<Unit> _unitRepository;
        private readonly IUnitOfWork _uow;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<VehicleDebit> _vehicleDebitRepository;
        private readonly IGenericRepository<VehicleExaminationDate> _vehicleExaminationRepository;
        private readonly IGenericRepository<Vehicle> _vehicleRepository;
        private readonly IGenericRepository<VehicleRequest> _vehicleRequestRepository;

        public VehicleRequestService(IUnitOfWork uow,
            IMapper mapper,
            ILogger<VehicleRequestService> logger)
        {
            _uow = uow;
            _mapper = mapper;
            _logger = logger;
            _vehicleRepository = uow.GetRepository<Vehicle>();
            _lookUpListRepository = uow.GetRepository<LookUpList>();
            _userRepository = uow.GetRepository<User>();
            _cityRepository = uow.GetRepository<City>();
            _unitRepository = uow.GetRepository<Unit>();
            _vehicleRequestRepository = uow.GetRepository<VehicleRequest>();
        }

        public PagedList<EVehicleRequestDto> GetAllWithPaged(int? page, EVehicleRequestDto filterModel)
        {
            //Sıra no için
            var pageStartCount = 0;
            if (!page.HasValue)
                pageStartCount = 1;

            var list = from vr in _vehicleRequestRepository.GetAll()
                       join v in _vehicleRepository.GetAll() on vr.VehicleId equals v.Id
                       join u in _userRepository.GetAll() on vr.RequestUserId equals u.Id into uL
                       from u in uL.DefaultIfEmpty()
                       join b in _unitRepository.GetAll() on vr.UnitId equals b.Id into bL
                       from b in bL.DefaultIfEmpty()
                       select new EVehicleRequestDto
                       {
                           Id = vr.Id,
                           UnitId = vr.UnitId,
                           UnitName = vr.UnitId != null
                               ? "<span class='label bg-orange-400 full-width'>" + b.Name + "</span>"
                               : "",
                           Plate = "<span class='label bg-orange-400 full-width'>" + v.Plate + "</span>",
                           VehicleId = vr.VehicleId,
                           RequestUserId = vr.RequestUserId,
                           RequestNameSurname = vr.RequestUserId != null
                               ? "<span class='label bg-success-300 full-width'>" + u.Name + " " + u.Surname + "/" +
                                 u.MobilePhone + "</span>"
                               : "",
                           StartDate = vr.StartDate,
                           EndDate = vr.EndDate,
                           RequestNo = vr.RequestNo,
                           PageStartCount = pageStartCount == 0 ? page.Value - 1 : pageStartCount - 1,
                           DeleteButtonActive = true,
                           CustomButton =
                               "<li class='text-primary-400'><a data-toggle='modal' onclick='funcEditVehicleRequest(" + vr.Id +
                               ");'><i class='icon-pencil5'></i></a></li>"
                       };

            if (filterModel.VehicleId > 0)
                list = list.Where(w => w.VehicleId == filterModel.VehicleId);

            if (filterModel.RequestUserId > 0)
                list = list.Where(w => w.RequestUserId == filterModel.RequestUserId);

            var lastResult =
                new PagedList<EVehicleRequestDto>(list.OrderByDescending(o => o.Id), page, PagedCount.GridKayitSayisi);
            return lastResult;
        }

        public EResultDto Insert(EVehicleRequestDto tempModel)
        {
            var result = new EResultDto();
            try
            {
                var model = _mapper.Map<VehicleRequest>(tempModel);
                model.CreatedBy = tempModel.CreatedBy;
                var resultEntity = _vehicleRequestRepository.Insert(model);
                _uow.SaveChanges();
                result.Id = resultEntity.Id;
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = "Kayıt sırasında hata oluştu!";
                //_logger.LogError(result.Message + "----" + ex.Message);
            }

            return result;
        }

        public EVehicleRequestDto GetById(int id)
        {
            var result = _vehicleRequestRepository.Find(id);
            var entityDto = _mapper.Map<EVehicleRequestDto>(result);
            return entityDto;
        }

        public EResultDto Update(EVehicleRequestDto tempModel)
        {
            var result = new EResultDto();
            try
            {
                var entity = _vehicleRequestRepository.Find(tempModel.Id);
                var newEntity = _mapper.Map(tempModel, entity);
                _vehicleRequestRepository.Update(newEntity);
                _uow.SaveChanges();
                result.Id = entity.Id;
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
                var entity = _vehicleRequestRepository.Find(id);
                entity.Status = Convert.ToBoolean(Status.Passive);
                _vehicleRequestRepository.Update(entity);
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
    }
}