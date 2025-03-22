using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Dto.ELogisticsDto;
using CoreArchV2.Services.Services;

namespace CoreArchV2.Services.Interfaces
{
    public interface IMobileService
    {
        PagedList<EMessageLogDto> GetAllWithPaged(int? page, EMessageLogDto filterModel);
        Task<EResultDto> SendMailAsync(EMessageLogDto entity);
        Task<EResultDto> PushNotificationAsync(EMessageLogDto model);
        Task<string> GetMobileLastVersion();

        //EResultDto InsertVehiclePhysicalImageBase64(EVehiclePhysicalImageDto model);
        EResultDto InsertVehiclePhysicalImageBase64(EVehiclePhysicalImageDto model);
        Task<Core.Entity.Common.Parameter> GetParameterByKey(string key);
    }
}
