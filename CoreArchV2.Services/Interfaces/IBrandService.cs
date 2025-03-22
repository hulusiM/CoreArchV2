using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Services.Services;

namespace CoreArchV2.Services.Interfaces
{
    public interface IBrandService
    {
        Task<PagedList<EUnitDto>> GetAllWithPagedBrand(int? page, EUnitDto filterModel);
        EResultDto UpdateBrand(EUnitDto tempModel);
        EResultDto InsertBrand(EUnitDto tempModel);
        EResultDto DeleteBrand(int id);
        EUnitDto GetByIdBrand(int id);
    }
}
