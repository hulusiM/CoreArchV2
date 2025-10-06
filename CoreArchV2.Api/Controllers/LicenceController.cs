using CoreArchV2.Api.Licence.Services;
using CoreArchV2.Core.Entity.Licence.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreArchV2.Api.Controllers
{
    [Produces("application/json")]
    [Route("Licence")]
    [ApiController]
    [Authorize]
    public class LicenceController : Controller
    {
        private readonly ILicenceService _licenceService;
        public LicenceController(ILicenceService licenceService)
        {
            _licenceService = licenceService;
        }

        [HttpPost("Check")]
        public async Task<IActionResult> LicenceControl(LicenceRequestDto model) => await _licenceService.LicenceControl(model);

        [HttpPost("AddUserRole")]
        public async Task<IActionResult> AddUserRole(LicenceRequestDto model) => await _licenceService.AddUserRole(model);

        [HttpPost("DeleteUserRole")]
        public async Task<IActionResult> DeleteUserRole(LicenceRequestDto model) => await _licenceService.DeleteUserRole(model);

        [HttpPost("AddVehicle")]
        public async Task<IActionResult> AddVehicle(LicenceRequestDto model) => await _licenceService.AddVehicle(model);

        [HttpPost("DeleteVehicle")]
        public async Task<IActionResult> DeleteVehicle(LicenceRequestDto model) => await _licenceService.DeleteVehicle(model);
    }
}
