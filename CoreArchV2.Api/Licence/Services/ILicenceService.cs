using CoreArchV2.Api.Helper;
using CoreArchV2.Core.Entity.Licence.Dto;

namespace CoreArchV2.Api.Licence.Services
{
    public interface ILicenceService
    {
        Task<ObjectActionResult> LicenceControl(LicenceRequestDto model);
        Task<ObjectActionResult> AddUserRole(LicenceRequestDto model);
        Task<ObjectActionResult> DeleteUserRole(LicenceRequestDto model);
        Task<ObjectActionResult> AddVehicle(LicenceRequestDto model);
        Task<ObjectActionResult> DeleteVehicle(LicenceRequestDto model);
    }
}
