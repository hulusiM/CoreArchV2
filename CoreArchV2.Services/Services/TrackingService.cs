using AutoMapper;
using CoreArchV2.Core.Entity.Track;
using CoreArchV2.Data.GenericRepository;
using CoreArchV2.Data.UnitOfWork;
using CoreArchV2.Dto.ATrackingDto;
using CoreArchV2.Services.Interfaces;

namespace CoreArchV2.Services.Services
{
    public class TrackingService : ITrackingService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IGenericRepository<VehicleTracking> _vehicleTrackingRepository;
        public TrackingService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
            _vehicleTrackingRepository = uow.GetRepository<VehicleTracking>();
        }

        public async Task<bool> InsertVehicleTracking(AVehicleTrackingRequestDto request)
        {
            var result = false;
            try
            {
                var entities = _mapper.Map<VehicleTracking>(request);
                entities.CreatedDate = DateTime.Now;

                await _vehicleTrackingRepository.InsertAsync(entities);
                await _uow.SaveChangesAsync();
                result = true;
            }
            catch (Exception)
            {
                //hata maili
            }

            return result;
        }


        public async Task<List<VehicleTracking>> GetCoordinate()
        {
            var list = new List<VehicleTracking>();
            try
            {
                list = await Task.FromResult(_vehicleTrackingRepository.GetAll().OrderByDescending(o => o.Id).Take(10).ToList());
            }
            catch (Exception)
            {
                list = null;
            }

            return list;
        }
    }
}
