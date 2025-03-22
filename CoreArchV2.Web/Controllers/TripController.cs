using CoreArchV2.Core.Entity.Logistics;
using CoreArchV2.Core.Entity.TripVehicle;
using CoreArchV2.Core.Enum.TripVehicle;
using CoreArchV2.Data.GenericRepository;
using CoreArchV2.Data.UnitOfWork;
using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Dto.ETripDto;
using CoreArchV2.Services.Interfaces;
using CoreArchV2.Services.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoreArchV2.Web.Controllers
{
    public class TripController : AdminController
    {
        private readonly IUnitOfWork _uow;
        private readonly ITripService _tripService;
        private readonly IGenericRepository<Trip> _tripRepository;
        private readonly IGenericRepository<Vehicle> _vehicleRepository;
        public TripController(ITripService tripService, IUnitOfWork uow)
        {
            _uow = uow;
            _tripService = tripService;
            _vehicleRepository = uow.GetRepository<Vehicle>();
            _tripRepository = uow.GetRepository<Trip>();
        }

        public IActionResult Index(int tripId)
        {
            ViewBag.TripIdForExternalData = tripId;
            return View();
        }

        public IActionResult TripAuthorization(int tripId)
        {
            ViewBag.TripIdForExternalData = tripId;
            return View();
        }

        public IActionResult DriverReport() => View();

        #region TripAuthorization
        public IActionResult TripAuthGetAll(int? page, ETripDto filterModel)
        {
            filterModel.CreatedBy = _loginUserInfo.Id;
            var result = _tripService.GetAllAuthWithPaged(page, filterModel, _loginUserInfo.IsAdmin);
            HttpContext.Session.SetString("PageList", MvcHelper.Pager(result, "/Trip/TripAuthGetAll"));
            return Json(result);
        }
        public IActionResult ChangeAllowedStatus(ETripDto filterModel)
        {
            filterModel.CreatedBy = _loginUserInfo.Id;
            return Json(_tripService.ChangeAllowedStatus(filterModel));
        }
        #endregion


        #region Trip
        public IActionResult TripGetAll(int? page, ETripDto filterModel)
        {
            if (page > 1)
                filterModel.IsFilterMode = true;

            filterModel.CreatedBy = _loginUserInfo.Id;
            var result = _tripService.GetAllWithPaged(page, filterModel, _loginUserInfo.IsAdmin);
            HttpContext.Session.SetString("PageList", MvcHelper.Pager(result, "/Trip/TripGetAll"));
            return Json(result);
        }
        public IActionResult UpdateVehicleKm(ETripDto tempModel)
        {
            tempModel.IsAdmin = _loginUserInfo.IsAdmin;
            tempModel.CreatedBy = _loginUserInfo.Id;
            return Json(_tripService.UpdateVehicleKm(tempModel));
        }
        public IActionResult GetVehicleLastKm(int vehicleId) => Json(_tripService.GetVehicleLastKm(vehicleId));
        public IActionResult TripAddCity(ETripDto tempModel)
        {
            tempModel.CreatedBy = _loginUserInfo.Id;
            tempModel.IsAdmin = _loginUserInfo.IsAdmin;
            return Json(_tripService.TripAddCity(tempModel));
        }
        public IActionResult GetByTripIdHistory(int tripId) => Json(_tripService.GetByTripIdHistory(tripId));
        public IActionResult GetByTripIdHistoryMap(int tripId)
        {
            var trip = _tripRepository.Find(tripId);
            var vehicle = _vehicleRepository.Find(trip.VehicleId);
            if (string.IsNullOrEmpty(vehicle.ArventoNo))
                return Json(1);

            //var diffDate = (trip.EndDate == null ? DateTime.Now : trip.EndDate.Value) - trip.StartDate;
            //if (diffDate.TotalDays > 15)
            //    return Json(2);

            var result = _tripService.GetByTripIdHistoryMap(tripId);
            return Json(result);
        }
        public IActionResult CloseTrip(ETripDto tempModel)
        {
            tempModel.CreatedBy = _loginUserInfo.Id;
            tempModel.IsAdmin = _loginUserInfo.IsAdmin;
            return Json(_tripService.CloseTrip(tempModel));
        }
        public IActionResult ActiveMissionControl() => Json(_tripService.ActiveMissionControl(_loginUserInfo.Id));
        public IActionResult TripInsertUpdate(ETripDto tempModel)
        {
            tempModel.IsAdmin = _loginUserInfo.IsAdmin;
            tempModel.CreatedBy = _loginUserInfo.Id;
            tempModel.InsertType = (int)TripInsertType.Web;
            EResultDto result;
            if (tempModel.Id > 0)
                result = _tripService.TripUpdate(tempModel);
            else
                result = _tripService.TripInsert(tempModel);

            return Json(result);
        }
        public IActionResult GetById(int id) => Json(_tripService.GetById(id));
        public IActionResult Delete(int id) => Json(_tripService.Delete(id, _loginUserInfo.IsAdmin, _loginUserInfo.Id));
        public async Task<IActionResult> GetReport(ETripDto tempModel)
        {
            tempModel.CreatedBy = _loginUserInfo.Id;
            var list = await _tripService.GetReport(tempModel);
            return PartialView("PartialViews/Trip/Table/_TripReportTable", list);
        }
        #endregion

    }
}
