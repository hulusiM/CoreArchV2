using CoreArchV2.Core.Entity.Common;
using CoreArchV2.Core.Entity.Logistics;
using CoreArchV2.Core.Entity.Track;
using CoreArchV2.Data.GenericRepository;
using CoreArchV2.Data.UnitOfWork;
using CoreArchV2.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreArchV2.Services.Services
{
    public class VehicleMapService : IVehicleMapService
    {
        private readonly IUnitOfWork _uow;
        private readonly IGenericRepository<Vehicle> _vehicleRepository;
        private readonly IGenericRepository<Unit> _unitRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<VehicleTracking> _vehicleTrackingRepository;

        public VehicleMapService(IUnitOfWork uow)
        {
            _uow = uow;
            _vehicleRepository = uow.GetRepository<Vehicle>();
            _unitRepository = uow.GetRepository<Unit>();
            _userRepository = uow.GetRepository<User>();
            _vehicleTrackingRepository = uow.GetRepository<VehicleTracking>();
        }

        public async Task<List<VehicleTracking>> GetTrackingCoordinateList()
        {
            var dateNow = DateTime.Now.AddSeconds(5);
            var list = await _vehicleTrackingRepository.Where(w => w.SignalDate >= dateNow).OrderByDescending(o => o.Id).Take(1).ToListAsync();

            return list;
        }
    }
}
