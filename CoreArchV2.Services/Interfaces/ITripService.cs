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
        EResultDto TripInsert(ETripDto model);
        EResultDto TripUpdate(ETripDto model);
        ETripDto GetById(int id);
        EResultDto Delete(int id, bool isAdmin, int createdBy);
        EVehicleDto GetVehicleLastKm(int vehicleId);
        EResultDto UpdateVehicleKm(ETripDto model);
        ETripDto ActiveMissionControl(int driveId);
        EResultDto CloseTrip(ETripDto model);
        List<ETripDto> GetByTripIdHistory(int tripId);
        List<EGeneralReport2Dto> GetByTripIdHistoryMap(int tripId);
        EResultDto TripAddCity(ETripDto model);
        Task<List<ETripDto>> GetReport(ETripDto filterModel);
        #endregion

        #region TripAuthorization

        PagedList<ETripDto> GetAllAuthWithPaged(int? page, ETripDto filterModel, bool isAdmin);
        EResultDto ChangeAllowedStatus(ETripDto model);

        #endregion

        string GetTripType(int type);
        string SetStateTrip(int state);
        string GetCityName(int cityId);
    }
}
