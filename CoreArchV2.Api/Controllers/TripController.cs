using CoreArchV2.Api.Helper;
using CoreArchV2.Core.Entity.Common;
using CoreArchV2.Core.Entity.Logistics;
using CoreArchV2.Core.Entity.TripVehicle;
using CoreArchV2.Core.Enum;
using CoreArchV2.Core.Enum.TripVehicle;
using CoreArchV2.Data.GenericRepository;
using CoreArchV2.Data.UnitOfWork;
using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Dto.EReportDto;
using CoreArchV2.Dto.ETripDto;
using CoreArchV2.Services.Arvento;
using CoreArchV2.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CoreArchV2.Api.Controllers
{
    [Produces("application/json")]
    [Route("Trip")]
    [ApiController]
    [Authorize]
    public class TripController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly ITripService _tripService;
        private readonly IGenericRepository<City> _cityRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<Vehicle> _vehicleRepository;
        private readonly IGenericRepository<Trip> _tripRepository;
        private readonly IGenericRepository<Unit> _unitRepository;
        private readonly IMobileService _mobileService;
        private readonly IArventoService _arventoService;

        public TripController(ITripService tripService,
            IArventoService arventoService,
            IMobileService mobileService,
            IUnitOfWork uow)
        {
            _uow = uow;
            _arventoService = arventoService;
            _tripService = tripService;
            _vehicleRepository = uow.GetRepository<Vehicle>();
            _unitRepository = uow.GetRepository<Unit>();
            _cityRepository = uow.GetRepository<City>();
            _tripRepository = uow.GetRepository<Trip>();
            _userRepository = uow.GetRepository<User>();
            _mobileService = mobileService;
        }

        [HttpGet("test")]
        public IActionResult test()
        {
            var list = _arventoService.ArventoPlakaHiziSorgula();
            return new ObjectActionResult(
                success: true,
                statusCode: HttpStatusCode.OK,
                data: 1);
        }

        #region Trip
        [HttpGet("GetTripList")]
        public IActionResult GetTripList(int page, int userId)
        {
            var list = _tripService.GetAllWithPaged(page, new ETripDto() { CreatedBy = userId }, false);
            return new ObjectActionResult(
                success: true,
                statusCode: HttpStatusCode.OK,
                data: list);
        }

        [HttpGet("TripGetById")]
        public IActionResult TripGetById(int id)
        {
            try
            {
                var result = _tripService.GetById(id);
                return new ObjectActionResult(
                    success: true,
                    statusCode: HttpStatusCode.OK,
                    data: result);
            }
            catch (Exception ex)
            {
                return new ObjectActionResult(
                      success: false,
                      statusCode: HttpStatusCode.InternalServerError,
                      data: ex.Message);
            }
        }

        [HttpGet("GetVehicleLastKm")]
        public async Task<IActionResult> GetVehicleLastKm(int vehicleId)
        {
            var result = await _tripService.GetVehicleLastKm(vehicleId);
            return new ObjectActionResult(
                 success: true,
                 statusCode: HttpStatusCode.OK,
                 data: result);
        }

        [HttpGet("ActiveMissionControl")]
        public async Task<IActionResult> ActiveMissionControl(int userId)
        {
            var result = await _tripService.ActiveMissionControl(userId);
            if (result.Id > 0)
            {
                return new ObjectActionResult(
                  success: true,
                  statusCode: HttpStatusCode.OK,
                  data: result);
            }
            else
            {
                return Ok(new { Success = false, StatusCode = 500, Message = "", Data = new object[0] });
            }
        }

        [HttpPost("TripInsertUpdate")]
        public async Task<IActionResult> TripInsertUpdate(ETripDto tempModel)
        {
            EResultDto result;
            bool isUpdate = tempModel.Id > 0;
            tempModel.InsertType = (int)TripInsertType.Mobile;
            if (tempModel.Id > 0)
                result = await _tripService.TripUpdate(tempModel);
            else
                result = await _tripService.TripInsert(tempModel);

            if (result.IsSuccess)
            {
                var mobileUser = await _userRepository.SingleOrDefaultAsync(s => s.Id == tempModel.CreatedBy);

                var subject = string.Empty;
                var body = string.Empty;
                var vehicle = _vehicleRepository.Find(tempModel.VehicleId);
                if (isUpdate)
                    subject = "Görev Güncelleme Başarılı";
                else
                    subject = "Görev Açma Başarılı";

                body = "Sayın " + mobileUser.Name + " " + mobileUser.Surname + ", " + vehicle.Plate + " plakalı araçla göreviniz başarılı şekilde açılmıştır. Yasal hız limitlerine dikkat ediniz.";
                await _mobileService.PushNotificationAsync(new EMessageLogDto()
                {
                    Subject = subject,
                    Body = body,
                    Type = (int)MessageLogType.PushNotification,
                    UserId = tempModel.CreatedBy
                });

                return new ObjectActionResult(
                  success: true,
                  statusCode: HttpStatusCode.OK,
                  data: result.Message);
            }
            else
            {
                return new ObjectActionResult(
                    success: false,
                    statusCode: HttpStatusCode.InternalServerError,
                    data: result.Message);
            }
        }

        [HttpPost("TripClose")]
        public async Task<IActionResult> TripClose(ETripDto model)
        {
            model.IsAdmin = false;
            var result = await _tripService.CloseTrip(model);
            if (result.IsSuccess)
            {
                var subject = "Görev Kapatıldı";
                var body = "Görev başarılı şekilde kapatıldı. İyi günler dileriz ";
                await _mobileService.PushNotificationAsync(new EMessageLogDto()
                {
                    Subject = subject,
                    Body = body,
                    Type = (int)MessageLogType.PushNotification,
                    UserId = model.CreatedBy
                });

                return new ObjectActionResult(
                  success: true,
                  statusCode: HttpStatusCode.OK,
                  data: result.Message);
            }
            else
            {
                return new ObjectActionResult(
                    success: false,
                    statusCode: HttpStatusCode.InternalServerError,
                    data: result.Message);
            }
        }

        [HttpGet("TripDelete")]
        public async Task<IActionResult> TripDelete(int id, int userId)
        {
            var result = await _tripService.Delete(id, false, userId);
            if (result.IsSuccess)
            {
                return new ObjectActionResult(
                 success: true,
                 statusCode: HttpStatusCode.OK,
                 data: result.Message);
            }
            else
            {
                return new ObjectActionResult(
                  success: false,
                  statusCode: HttpStatusCode.InternalServerError,
                  data: result.Message);
            }
        }

        [HttpGet("GetByTripIdHistory")]
        public async Task<IActionResult> GetByTripIdHistory(int tripId)
        {
            return new ObjectActionResult(
                 success: true,
                 statusCode: HttpStatusCode.OK,
                 data: await _tripService.GetByTripIdHistory(tripId));
        }
        #endregion

        #region TripAuthorization
        [HttpGet("GetTripAuthList")]
        public async Task<IActionResult> GetTripAuthList(int page, int userId)
        {
            var isAdmin = await _userRepository.FirstOrDefaultNoTrackingAsync(f => f.Id == userId);
            var list = await _tripService.GetAllAuthWithPaged(page, new ETripDto() { CreatedBy = userId }, isAdmin.IsAdmin);
            return new ObjectActionResult(
                success: true,
                statusCode: HttpStatusCode.OK,
                data: list);
        }

        [HttpPost("ChangeAllowedStatus")]
        public IActionResult ChangeAllowedStatus(ETripDto model)
        {
            var result = _tripService.ChangeAllowedStatus(model);
            if (result.IsSuccess)
            {
                return new ObjectActionResult(
                 success: true,
                 statusCode: HttpStatusCode.OK,
                 data: result.Message);
            }
            else
            {
                return new ObjectActionResult(
                  success: false,
                  statusCode: HttpStatusCode.InternalServerError,
                  data: result.Message);
            }
        }

        #endregion

        #region Combobox
        [HttpGet("GetCityAndDistrictCmbx")]
        public async Task<IActionResult> GetCityAndDistrictCmbx(string q)
        {
            if (q.Length > 2)
            {
                var cities = (from u in _cityRepository.GetAll()
                              join u2 in _cityRepository.GetAll() on u.Id equals u2.ParentId
                              select new
                              {
                                  CityId = u2.Id,
                                  Name = u.Name + "-" + u2.Name
                              });

                var result = await Task.FromResult(cities.Where(w => w.Name.Contains(q)).ToList());
                return new ObjectActionResult(
                     success: true,
                     statusCode: HttpStatusCode.OK,
                     data: result);
            }
            else
            {
                return new ObjectActionResult(
               success: true,
               statusCode: HttpStatusCode.OK,
               data: "En az 3 karakter giriniz");
            }
        }


        [HttpGet("GetVehicleCmbx")]
        public async Task<IActionResult> GetVehicleCmbx(string q)
        {
            if (q.Length > 2)
            {
                var result = await Task.FromResult(from v in _vehicleRepository.GetAll()
                                                   join u in _unitRepository.GetAll() on v.LastUnitId equals u.Id into unitL
                                                   from u in unitL.DefaultIfEmpty()
                                                   join u2 in _unitRepository.GetAll() on u.ParentId equals u2.Id into unit2L
                                                   from u2 in unit2L.DefaultIfEmpty()
                                                   where v.Status && v.Plate.Contains(q)
                                                   select new ESelect2Dto()
                                                   {
                                                       id = v.Id,
                                                       Status = v.Status,
                                                       CreatedDate = v.CreatedDate,
                                                       text = v.Plate,
                                                       ParentUnitId = u2.Id,
                                                       VehicleId = v.Id,
                                                       UnitId = u.Id
                                                   });

                var addPlate = SamePlateForNewName(result.ToList()); //Aynı plakaların önüne id ekelniyor

                return new ObjectActionResult(
                success: true,
                statusCode: HttpStatusCode.OK,
                data: addPlate.Select(s => new { VehicleId = s.id, Plate = s.text }).ToList());
            }
            else
            {
                return new ObjectActionResult(
               success: true,
               statusCode: HttpStatusCode.OK,
               data: "En az 3 karakter giriniz");
            }
        }

        [HttpGet("GetTripTypeList")]
        public IActionResult GetTripTypeList()
        {
            var list = new List<EComboboxDto>();
            list.Add(new EComboboxDto() { Id = 1, Name = "Görev" });
            list.Add(new EComboboxDto() { Id = 2, Name = "Görev Dışı" });
            return new ObjectActionResult(
              success: true,
              statusCode: HttpStatusCode.OK,
              data: list.ToList());
        }

        [NonAction]
        public List<ESelect2Dto> SamePlateForNewName(List<ESelect2Dto> resultPlate)
        {
            var isSamePlate = _vehicleRepository.GetAll().Select(s => new Vehicle() { Plate = s.Plate }).ToList().GroupBy(g => g.Plate)
                .Select(s => new RVehicleFuelDto() { Count = s.Count(), Plate = s.First().Plate })
                .Where(w => w.Count > 1).OrderBy(o => o.VehicleId).ToList();

            foreach (var p in isSamePlate)
            {
                var samePlate = resultPlate.Where(w => w.text.Contains(p.Plate)).ToList();
                for (int i = 0; i < samePlate.Count; i++)
                {
                    var temp = resultPlate.FirstOrDefault(f => f.VehicleId == samePlate[i].VehicleId);
                    if (temp != null)
                        resultPlate.FirstOrDefault(f => f.VehicleId == temp.VehicleId).text += "-" + temp.VehicleId;
                }
            }

            return resultPlate.OrderBy(o => o.VehicleId).ToList();
        }
        #endregion

        #region Helper
        [NonAction]
        public string SetTripWithManagerState(int state, bool? isManagerAllowed)
        {
            return isManagerAllowed switch
            {
                null => "Yönetici Onaysız",
                true => "Onaylı",
                false => "Onaysız"
            };
        }
        [NonAction]
        private string SetStateTrip(int state)
        {
            var result = state switch
            {
                (int)TripState.StartTrip => "Görev Başladı",
                (int)TripState.EndTrip => "Görev Bitti",
                (int)TripState.AllowedForManager => "Yönetici Onay Verdi",
                (int)TripState.NotAllowedForManager => "Yönetici İptal Etti",
                (int)TripState.CloseTrip => "İşleme Kapalı",
                _ => "??",
            };
            return result;
        }
        [NonAction]
        public string GetTripType(int type)
        {
            string result = type switch
            {
                (int)TripType.Mission => "Görev",
                (int)TripType.OffDuty => "Görev Dışı",
                _ => ""
            };
            return result;
        }
        #endregion
    }
}
