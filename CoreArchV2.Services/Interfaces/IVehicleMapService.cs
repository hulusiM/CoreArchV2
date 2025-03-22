using CoreArchV2.Dto.ELogisticsDto;
using CoreArchV2.Services.Arvento.Dto;

namespace CoreArchV2.Services.Interfaces
{
    public interface IVehicleMapService
    {
        Task<List<ECoordinateDto>> GetAllForMapVehicle(EVehicleDto model);
    }
}
