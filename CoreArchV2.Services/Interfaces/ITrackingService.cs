using CoreArchV2.Core.Entity.Track;
using CoreArchV2.Dto.ATrackingDto;

namespace CoreArchV2.Services.Interfaces
{
    public interface ITrackingService
    {
        Task<bool> InsertVehicleTracking(AVehicleTrackingRequestDto request);
        Task<List<VehicleTracking>> GetCoordinate();
    }
}
