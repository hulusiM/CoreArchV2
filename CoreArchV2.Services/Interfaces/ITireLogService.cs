using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Dto.ELogisticsDto;
using CoreArchV2.Dto.EReportDto;
using CoreArchV2.Services.Services;

namespace CoreArchV2.Services.Interfaces
{
    public interface ITireLogService
    {
        PagedList<ETireDto> GetAllWithPaged(int? page, ETireDto filterModel);
        EResultDto Insert(ETireDto tempModel);
        EResultDto Delete(ETireDto tempModel);
        EResultDto TireDebitInsert(ETireDto model);
        EResultDto GetDebitTireInfo(ETireDto model);
        List<ETireDto> GetTireHistory(ETireDto model);
        EResultDto SetTireInert(ETireDto model);
        RTireDto GetTireReport(ETireDto model);
        EResultDto TireReturn(ETireDto model);
    }
}