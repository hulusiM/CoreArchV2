using Microsoft.AspNetCore.Mvc;

namespace CoreArchV2.Web.Controllers
{
    public class MapController : Controller
    {
        //private readonly IVehicleMapService _vehicleMapService;
        //public MapController(IVehicleMapService vehicleMapService)
        //{
        //    _vehicleMapService = vehicleMapService;
        //}

        public IActionResult AllVehicleMap()
        {
            return View();
        }

        //[HttpPost]
        //public async Task<IActionResult> GetAllForMapVehicle(EVehicleDto model)
        //{
        //    model.IsAdmin=_loginUserInfo.IsAdmin;
        //    model.LoginUserId= _loginUserInfo.Id;
        //    model.LoginUnitId = _loginUserInfo.UnitId;
        //    model.LoginParentUnitId = _loginUserInfo.ParentUnitId;

        //    var allVehicle = await _vehicleMapService.GetAllForMapVehicle(model);
        //    return Json(allVehicle);
        //}
    }
}
