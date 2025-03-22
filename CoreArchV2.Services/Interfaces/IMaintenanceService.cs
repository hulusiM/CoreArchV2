using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Dto.ELogisticsDto;
using CoreArchV2.Services.Services;
using Microsoft.AspNetCore.Http;

namespace CoreArchV2.Services.Interfaces
{
    public interface IMaintenanceService
    {
        PagedList<EMaintenanceDto> GetAllWithPaged(int? page, bool isHgs, EMaintenanceDto filterModel);
        Task<EResultDto> Insert(IList<IFormFile> files, EMaintenanceDto tempModel);
        EMaintenanceDto GetById(int id);
        Task<EResultDto> Update(IList<IFormFile> files, EMaintenanceDto tempModel);
        EResultDto Delete(int id);
        List<EMaintenanceDto> GetByVehicleIdMaintenanceHistory(int vehicleId, bool isHgsFilter);
        List<EFileUploadDto> GetFilesList(int maintenanceId);
    }
}