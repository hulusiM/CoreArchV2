using CoreArchV2.Api.Helper;
using CoreArchV2.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CoreArchV2.Api.Controllers
{
    [Produces("application/json")]
    [Route("Tracking")]
    [ApiController]
    public class TrackingController : Controller
    {
        private readonly ITrackingService _trackingService;
        public TrackingController(ITrackingService trackingService)
        {
            _trackingService = trackingService;
        }

        [HttpGet("InsertCoordinate")]
        public async Task<IActionResult> InsertCoordinate(string param)
        {
            await _trackingService.InsertCoordinate(param);
            return new ObjectActionResult(
                success: true,
                statusCode: HttpStatusCode.OK,
                data: null);
        }


        [HttpGet("GetCoordinate")]
        public async Task<IActionResult> GetCoordinate()
        {
            var result = await _trackingService.GetCoordinate();
            return new ObjectActionResult(
                success: true,
                statusCode: HttpStatusCode.OK,
                data: result);
        }
    }
}
