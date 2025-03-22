using CoreArchV2.Dto.EApiDto;

namespace CoreArchV2.Services.Interfaces
{
    public interface ILicenceWebService
    {
        Task<AResponseDto> Check();
        Task<AResponseDto> AddUserRole(int createdBy, string model);
        Task<AResponseDto> DeleteUserRole(int createdBy, string model);
        Task<AResponseDto> AddVehicle(int createdBy, string model);
        Task<AResponseDto> DeleteVehicle(int createdBy, string model);
    }
}
