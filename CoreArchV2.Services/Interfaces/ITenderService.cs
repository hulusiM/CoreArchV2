using CoreArchV2.Core.Entity.Tender;
using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Dto.ETenderDto;
using CoreArchV2.Services.Services;
using Microsoft.AspNetCore.Http;

namespace CoreArchV2.Services.Interfaces
{
    public interface ITenderService
    {
        PagedList<ETender_Dto> GetAllWithPaged(int? page, ETender_Dto filterModel, bool isAdmin);
        PagedList<ETender_Dto> GetAllForUnitWithPaged(int? page, ETender_Dto filterModel, bool isAdmin);

        #region Tender
        EResultDto InsertTender(IList<IFormFile> files, ETender_Dto entity);
        EResultDto UpdateTender(IList<IFormFile> files, ETender_Dto entity);
        ETender_Dto GetByIdTender(int id);
        EResultDto DeleteTender(int id);
        EResultDto CreateNewSalesNumber(int userId);
        List<ETenderHistoryDto> GetByIdTenderHistory(int tenderId);
        #endregion

        #region Tender Contact
        EResultDto InsertUpdateTenderContract(ETenderAllDto model);
        List<TenderContact> GetByTenderIdContactList(int tenderId);
        #endregion

        #region TenderDetail
        ETenderAllDto GetTenderDetail(int tenderId);
        EResultDto InsertUpdateTenderDetail(ETenderAllDto entity, int loginUnitId);
        List<ETenderDetailDto> GetTenderDetailPriceHistory(int tenderDetailId);
        #endregion

        #region Tender Unit
        List<ETenderDetailDto> GetTenderDetailForUnit(int tenderId, int loginUnitId, bool isAdmin);
        ETenderDetailDto GetTenderDetailByTenderDetailId(int tenderDetailId, int loginUnitId, bool isAdmin);
        EResultDto InsertUpdateTenderDetailForUnit(IList<IFormFile> files, ETenderDetailDto model, int loginUnitId);

        #endregion
    }
}