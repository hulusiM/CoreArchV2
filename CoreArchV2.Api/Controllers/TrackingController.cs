using CoreArchV2.Api.ApiExtentions;
using CoreArchV2.Api.Helper;
using CoreArchV2.Dto.ATrackingDto;
using CoreArchV2.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CoreArchV2.Api.Controllers
{
    [Produces("application/json")]
    [Route("Tracking")]
    [ApiController]
    [ApiKeyAuthAttribute]
    public class TrackingController : Controller
    {
        private readonly ITrackingService _trackingService;
        public TrackingController(ITrackingService trackingService)
        {
            _trackingService = trackingService;
        }

        [HttpPost("InsertVehicleTracking")]
        public async Task<IActionResult> InsertVehicleTracking([FromBody] AVehicleTrackingRequestDto request)
        {
            var result = await _trackingService.InsertVehicleTracking(request);

            if (result)
                return new ObjectActionResult(
                    success: true,
                    statusCode: HttpStatusCode.OK,
                    data: null);
            else
                return new ObjectActionResult(
                    success: false,
                    statusCode: HttpStatusCode.BadRequest,
                    data: null);
        }


        //[HttpGet("GetCoordinate")]
        //public async Task<IActionResult> GetCoordinate()
        //{
        //    var result = await _trackingService.GetCoordinate();
        //    return new ObjectActionResult(
        //        success: true,
        //        statusCode: HttpStatusCode.OK,
        //        data: result);
        //}
    }
}
