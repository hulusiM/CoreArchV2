using CoreArchV2.Core.Entity.Common;
using CoreArchV2.Core.Entity.Logistics;
using CoreArchV2.Data.GenericRepository;
using CoreArchV2.Data.UnitOfWork;
using CoreArchV2.Dto.ELogisticsDto;
using CoreArchV2.Services.Arvento.Dto;
using CoreArchV2.Services.Interfaces;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;

namespace CoreArchV2.Services.Services
{
    public class VehicleMapService : IVehicleMapService
    {
        private readonly IUnitOfWork _uow;
        private readonly IGenericRepository<Vehicle> _vehicleRepository;
        private readonly IGenericRepository<Unit> _unitRepository;
        private readonly IGenericRepository<User> _userRepository;

        public VehicleMapService(IUnitOfWork uow)
        {
            _uow = uow;
            _vehicleRepository = uow.GetRepository<Vehicle>();
            _unitRepository = uow.GetRepository<Unit>();
            _userRepository = uow.GetRepository<User>();
        }

        public async Task<List<ECoordinateDto>> GetAllForMapVehicle(EVehicleDto model)
        {
            var list = from v in _vehicleRepository.GetAll()
                       join unit in _unitRepository.GetAll() on v.LastUnitId equals unit.Id into unitL
                       from unit in unitL.DefaultIfEmpty()
                       join u2 in _userRepository.GetAll() on v.LastUserId equals u2.Id into uL
                       from u2 in uL.DefaultIfEmpty()
                       where v.Status && v.Latitude > 0 && v.LastCoordinateInfo != null
                       select new ECoordinateDto()
                       {
                           UnitId = unit.Id,
                           ParentUnitId = unit.ParentId,
                           VehicleId = v.Id,
                           DebitNameSurname = u2.Name + " " + u2.Surname + "/" + u2.MobilePhone,
                           LatitudeY = v.Latitude,
                           LongitudeX = v.Longitude,
                           licensePlate = v.Plate,
                           Speed = v.LastSpeed,
                           MaxSpeed = v.MaxSpeed,
                           Address = v.LastAddress,
                           LocalDateTime = v.LastCoordinateInfo.Value.ToString("dd-MM-yyyy HH:mm:ss")
                       };

            if (!model.IsAdmin)//admin değilse sadece yetkili olduğu müdürlüğü listele
            {
                if (model.LoginParentUnitId > 0)
                    model.ParentUnitId = model.LoginParentUnitId;

                if (model.LoginUnitId > 0)
                    model.UnitId = model.LoginUnitId;
            }

            if (model.ParentUnitId > 0)
                list = list.Where(w => w.ParentUnitId == model.ParentUnitId);

            if (model.UnitId > 0)
                list = list.Where(w => w.UnitId == model.UnitId);

            if (model.VehicleId > 0)
                list = list.Where(w => w.VehicleId == model.VehicleId);

            if (model.DebitUserId > 0)
                list = list.Where(w => w.DebitUserId == model.DebitUserId);

            if (model.MinSpeed > 0)
                list = list.Where(w => w.Speed >= model.MinSpeed);

            if (model.MaxSpeed > 0)
                list = list.Where(w => w.Speed <= model.MaxSpeed);

            if (model.Status2 > 0)
            {
                if (model.Status2 == 1)//Duran
                    list = list.Where(w => w.Speed == 0);
                else if (model.Status2 == 2)//Hız Sınırını Aşan
                    list = list.Where(w => w.Speed > w.MaxSpeed);
                else if (model.Status2 == 3)//Normal
                    list = list.Where(w => w.Speed > 0);
            }

            if (!string.IsNullOrEmpty(model.CityName))
                list = list.Where(w => w.Address.Contains(model.CityName));

            var result = list.ToList();
            //rapor bilgileri
            var first_ = result.FirstOrDefault();
            if (first_ != null)
            {
                first_.SpeedVehicle = result.Count(c => c.MaxSpeed < c.Speed);
                first_.RolantiVehicle = result.Count(c => c.Speed == 0);
                first_.ActiveVehicle = result.Count(c => c.Speed > 0);
            }

            return await Task.FromResult(result);
        }
    }
}
