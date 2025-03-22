using AutoMapper;
using CoreArchV2.Data.UnitOfWork;
using CoreArchV2.Dto.EReportDto;
using CoreArchV2.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CoreArchV2.Web.Controllers
{
    public class ReportController : AdminController
    {
        private readonly IMapper _mapper;
        private readonly IReportService _reportService;
        private readonly IUnitOfWork _uow;

        public ReportController(IMapper mapper,
            IReportService reportService,
            IUnitOfWork uow)
        {
            _uow = uow;
            _mapper = mapper;
            _reportService = reportService;
        }

        #region Views
        public IActionResult Vehicle() => View();
        public IActionResult VehicleFuel() => View();
        public IActionResult VehicleMaintenance() => View();
        public IActionResult VehicleCost() => View();
        public IActionResult TripReport() => View();
        #endregion

        #region Vehicle
        public async Task<IActionResult> RentVehicleFirmCount(RFilterModelDto model) => Json(await _reportService.RentVehicleFirmCount(model));
        public async Task<IActionResult> ManagementVehicleCount(RFilterModelDto model) => Json(await _reportService.ManagementVehicleCount(model));
        public async Task<IActionResult> UsageTypeVehicleCount(RFilterModelDto model) => Json(await _reportService.UsageTypeVehicleCount(model));
        public async Task<IActionResult> FixVehicleTotalAmount(RFilterModelDto model) => Json(await _reportService.FixVehicleTotalAmount(model));
        public async Task<IActionResult> ModelYearVehicleCount(RFilterModelDto model) => Json(await _reportService.ModelYearVehicleCount(model));
        public async Task<IActionResult> ModelYearPercentVehicleCount(RFilterModelDto model) => Json(await _reportService.ModelYearPercentVehicleCount(model));
        #endregion

        #region VehicleFuel
        public async Task<IActionResult> HeaderInfoFuel(RFilterModelDto model) => Json(await _reportService.HeaderInfoFuel(ForAutVehicleSetUnitId(model)));
        public async Task<IActionResult> MontlyFuelTotalAmount(RFilterModelDto model) => Json(await _reportService.MontlyFuelTotalAmount(ForAutVehicleSetUnitId(model)));
        public async Task<IActionResult> ProjectFuelTotalAmount(RFilterModelDto model)
        {
            List<RVehicleFuelDto> result;
            if (!_loginUserInfo.IsAdmin) //Proje bazında rapor çeker
                result = await _reportService.SubProjectFuelTotalAmount(ForAutVehicleSetUnitId(model));
            else //Müdürlük bazında rapor çeker
                result = await _reportService.ProjectFuelTotalAmount(ForAutVehicleSetUnitId(model));

            return Json(result);
        }
        public async Task<IActionResult> VehicleFuelTotalAmount(RFilterModelDto model) => Json(await _reportService.VehicleFuelTotalAmount(ForAutVehicleSetUnitId(model)));
        public async Task<IActionResult> VehicleFuelTotalKmSpend(RFilterModelDto model) => Json(await _reportService.VehicleFuelTotalKmSpend(ForAutVehicleSetUnitId(model)));
        public async Task<IActionResult> VehicleFuelTotalSupplierAmount(RFilterModelDto model) => Json(await _reportService.VehicleFuelTotalSupplierAmount(ForAutVehicleSetUnitId(model)));
        public async Task<IActionResult> VehicleDebitAndMonthlyFuelCompare(RFilterModelDto model)
        {
            if (_loginUserInfo.IsAdmin)
                return Json(await _reportService.VehicleDebitAndMonthlyFuelCompare(ForAutVehicleSetUnitId(model)));
            else
                return Json(false);
        }
        #endregion

        #region VehicleMaintenance
        public async Task<IActionResult> HeaderInfoMaintenance(RFilterModelDto model) => Json(await _reportService.HeaderInfoMaintenance(ForAutVehicleSetUnitId(model)));
        public async Task<IActionResult> MontlyMaintenanceTotalAmount(RFilterModelDto model) => Json(await _reportService.MontlyMaintenanceTotalAmount(ForAutVehicleSetUnitId(model)));
        public async Task<IActionResult> ProjectMaintenanceTotalAmount(RFilterModelDto model)
        {
            var result = new List<RVehicleMaintenanceDto>();
            if (!_loginUserInfo.IsAdmin) //Proje bazında rapor çeker
                result = await _reportService.SubProjectMaintenanceTotalAmount(ForAutVehicleSetUnitId(model));
            else //Müdürlük bazında rapor çeker
                result = await _reportService.ProjectMaintenanceTotalAmount(ForAutVehicleSetUnitId(model));

            return Json(result);
        }
        public async Task<IActionResult> VehicleMaintenanceTotalAmount(RFilterModelDto model) => Json(await _reportService.VehicleMaintenanceTotalAmount(ForAutVehicleSetUnitId(model)));
        public async Task<IActionResult> FirmVehicleMaintenanceTotalAmountAndCount(RFilterModelDto model)
        {
            if (_loginUserInfo.IsAdmin)
                return Json(await _reportService.FirmVehicleMaintenanceTotalAmountAndCount(ForAutVehicleSetUnitId(model)));
            else
                return Json(false);
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

        #region VehicleCost
        public async Task<IActionResult> HeaderInfoVehicleCost(RFilterModelDto model) => Json(await _reportService.HeaderInfoVehicleCost(ForAutVehicleSetUnitId(model)));
        public async Task<IActionResult> VehicleCostTotalAmount(RFilterModelDto model)
        {
            model.IsAdmin = _loginUserInfo.IsAdmin;
            return Json(await _reportService.VehicleCostTotalAmount(ForAutVehicleSetUnitId(model)));
        }
        public async Task<IActionResult> MonthlyVehicleCost(RFilterModelDto model)
        {
            model.IsAdmin = _loginUserInfo.IsAdmin;
            return Json(await _reportService.MonthlyVehicleCost(ForAutVehicleSetUnitId(model)));
        }
        public async Task<IActionResult> VehicleDetailCostTotalAmount(RFilterModelDto model) => Json(await _reportService.VehicleDetailCostTotalAmount(ForAutVehicleSetUnitId(model)));
        public async Task<IActionResult> MonthlyHgsTotalAmount(RFilterModelDto model) => Json(await _reportService.MonthlyHgsTotalAmount(ForAutVehicleSetUnitId(model)));
        #endregion

        #region Hgs Bakiye Yükleme Chart
        public async Task<IActionResult> GetHgsReport(RFilterModelDto model) => Json(await _reportService.GetHgsReport(ForAutVehicleSetUnitId(model)));
        #endregion

        #region Trip Chart
        #region User
        public async Task<IActionResult> HeaderInfoTripUser(RFilterModelDto model)
        {
            model.UserId = _loginUserInfo.Id;
            return Json(await _reportService.HeaderInfoTripUser(model));
        }
        public async Task<IActionResult> TripTotalCountUser(RFilterModelDto model)
        {
            model.UserId = _loginUserInfo.Id;
            return Json(await _reportService.TripTotalCountUser(model));
        }
        public async Task<IActionResult> TripTotalKmUser(RFilterModelDto model)
        {
            model.UserId = _loginUserInfo.Id;
            return Json(await _reportService.TripTotalKmUser(model));
        }
        #endregion

        #region Manager
        public async Task<IActionResult> HeaderInfoTrip(RFilterModelDto model) => Json(await _reportService.HeaderInfoTrip(ForAutVehicleSetUnitId(model)));
        public async Task<IActionResult> PlateTrip(RFilterModelDto model) => Json(await _reportService.PlateTrip(ForAutVehicleSetUnitId(model)));
        public async Task<IActionResult> PlateTripKm(RFilterModelDto model) => Json(await _reportService.PlateTripKm(ForAutVehicleSetUnitId(model)));
        public async Task<IActionResult> MonthlyTrip(RFilterModelDto model)
        {
            model.IsAdmin = _loginUserInfo.IsAdmin;
            return Json(await _reportService.MonthlyTrip(ForAutVehicleSetUnitId(model)));
        }
        public async Task<IActionResult> ProjectTrip(RFilterModelDto model)
        {
            var result = new List<RVehicleFuelDto>();
            if (!_loginUserInfo.IsAdmin) //Proje bazında rapor çeker
                result = await _reportService.SubProjectTrip(ForAutVehicleSetUnitId(model));
            else //Müdürlük bazında rapor çeker
                result = await _reportService.ProjectTripTotal(ForAutVehicleSetUnitId(model));

            return Json(result);
        }
        #endregion
        #endregion
    }
}