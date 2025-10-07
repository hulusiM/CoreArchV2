using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Dto.ELogisticsDto;
using CoreArchV2.Dto.ETripDto;
using CoreArchV2.Services.Arvento.Dto;
using CoreArchV2.Services.Services;

namespace CoreArchV2.Services.Interfaces
{
    public interface ITripService
    {
        #region Trip
        PagedList<ETripDto> GetAllWithPaged(int? page, ETripDto filterModel, bool isAdmin);
        IQueryable<ETripDto> GetAllTrip(int? page, int pageStartCount, ETripDto filterModel, bool isAdmin = false);
        List<ETripDto> GetAllTrip(ETripDto filterModel);
        Task<EResultDto> TripInsert(ETripDto model);
        Task<EResultDto> TripUpdate(ETripDto model);
        ETripDto GetById(int id);
        Task<EResultDto> Delete(int id, bool isAdmin, int createdBy);
        Task<EVehicleDto> GetVehicleLastKm(int vehicleId);
        EResultDto UpdateVehicleKm(ETripDto model);
        Task<ETripDto> ActiveMissionControl(int driveId);
        Task<EResultDto> CloseTrip(ETripDto model);
        Task<List<ETripDto>> GetByTripIdHistory(int tripId);
        Task<List<EGeneralReport2Dto>> GetByTripIdHistoryMap(int tripId);
        EResultDto TripAddCity(ETripDto model);
        Task<List<ETripDto>> GetReport(ETripDto filterModel);
        #endregion

        #region TripAuthorization

        Task<PagedList<ETripDto>> GetAllAuthWithPaged(int? page, ETripDto filterModel, bool isAdmin);
        EResultDto ChangeAllowedStatus(ETripDto model);

        #endregion

        string GetTripType(int type);
        string SetStateTrip(int state);
        string GetCityName(int cityId);
    }
}
