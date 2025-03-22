using CoreArchV2.Core.Entity.Logistics;
using CoreArchV2.Core.Enum;
using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Dto.ELogisticsDto;
using CoreArchV2.Services.Services;

namespace CoreArchV2.Services.Interfaces
{
    public interface IFuelLogService
    {
        PagedList<EFuelLogDto> GetAllWithPaged(int? page, EFuelLogDto filterModel);
        Task<EResultDto> InsertAsync(EFuelLogDto tempModel);
        Task<EResultDto> InsertBulkAsync(EFuelReadExcelDto model);
        Task<EResultDto> UpdateAsync(EFuelLogDto tempModel);
        EResultDto Delete(int id);
        FuelLog GetById(int id);
        List<EFuelLogDto> GetPublishFuel(FuelPublisher publisher);
        bool SetPublishFuel(EFuelLogDto model);
        Task<bool> IsDebitRangeAsync(EFuelLogDto tempModel);
    }
}