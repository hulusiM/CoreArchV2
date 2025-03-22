using CoreArchV2.Core.Entity.Track;

namespace CoreArchV2.Services.Interfaces
{
    public interface ITrackingService
    {
        Task InsertCoordinate(string param);
        Task<List<Coordinate>> GetCoordinate();
    }
}
