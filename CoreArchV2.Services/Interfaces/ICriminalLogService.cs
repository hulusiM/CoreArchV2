using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Dto.ELogisticsDto;
using CoreArchV2.Services.Services;
using Microsoft.AspNetCore.Http;

namespace CoreArchV2.Services.Interfaces
{
    public interface ICriminalLogService
    {
        PagedList<ECriminalLogDto> GetAllWithPaged(int? page, ECriminalLogDto filterModel);
        EResultDto Insert(IList<IFormFile> files, ECriminalLogDto tempModel);
        ECriminalLogDto GetById(int id);
        EResultDto Update(IList<IFormFile> files, ECriminalLogDto tempModel);
        EResultDto Delete(int id);
    }
}