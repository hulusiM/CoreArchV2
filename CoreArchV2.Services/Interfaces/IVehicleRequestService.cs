using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Dto.ELogisticsDto;
using CoreArchV2.Services.Services;

namespace CoreArchV2.Services.Interfaces
{
    public interface IVehicleRequestService
    {
        PagedList<EVehicleRequestDto> GetAllWithPaged(int? page, EVehicleRequestDto filterModel);
        EResultDto Insert(EVehicleRequestDto tempModel);
        EVehicleRequestDto GetById(int id);
        EResultDto Update(EVehicleRequestDto tempModel);
        EResultDto Delete(int id);
    }
}