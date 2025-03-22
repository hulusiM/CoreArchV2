using CoreArchV2.Core.Entity.Logistics;
using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Dto.ELogisticsDto;
using CoreArchV2.Services.Arvento.Dto;
using CoreArchV2.Services.Services;

namespace CoreArchV2.Services.Interfaces
{
    public interface IOutOfHourService
    {
        PagedList<EOutOfHourDto> GetAllWithPaged(int? page, EOutOfHourDto filterModel);
        Task<List<EOutOfHourDto>> GetAllList(EOutOfHourDto filterModel);
        Task<List<EGeneralReport2Dto>> GetOutOfHourMap(int vecOperationId);

        PagedList<EOutOfHourDto> GetAllParamOutOfHourWithPaged(int? page, EOutOfHourDto filterModel);
        EResultDto ParamOutOfHourDelete(int id);
        EResultDto ParamOutOfHourSave(VehicleOperatingReportParam entity);
        List<EUnitDto> ComboParamOutOfUnitList();
    }
}
