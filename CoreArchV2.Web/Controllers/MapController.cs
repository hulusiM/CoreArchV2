using CoreArchV2.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CoreArchV2.Web.Controllers
{
    public class MapController : Controller
    {
        private readonly IVehicleMapService _vehicleMapService;
        public MapController(IVehicleMapService vehicleMapService)
        {
            _vehicleMapService = vehicleMapService;
        }

        public IActionResult AllVehicleMap()
        {
            return View();
        }

        public IActionResult TrackingVehicleMap()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetTrackingCoordinateList()
        {
            var list = await _vehicleMapService.GetTrackingCoordinateList();
            return Json(list);
        }
    }
}
