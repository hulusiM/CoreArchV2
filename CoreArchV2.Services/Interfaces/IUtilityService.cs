using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Services.Services;

namespace CoreArchV2.Services.Interfaces
{
    public interface IUtilityService
    {
        #region Logistics Unit
        Task<PagedList<EUnitDto>> GetAllWithPagedUnit(int? page, EUnitDto filterModel);
        EResultDto UpdateUnit(EUnitDto tempModel);
        EResultDto InsertUnit(EUnitDto tempModel);
        EResultDto DeleteUnit(int id);
        EUnitDto GetByIdUnit(int id);
        #endregion


        #region Tender Unit
        Task<PagedList<EUnitDto>> GetAllWithPagedInstitution(int? page, EUnitDto filterModel);
        EResultDto UpdateInstitution(EUnitDto tempModel);
        EResultDto InsertInstitution(EUnitDto tempModel);
        EResultDto DeleteInstitution(int id);
        EUnitDto GetByIdInstitution(int id);
        #endregion

    }
}
