using CoreArchV2.Core.Entity.NoticeVehicle.NoticeUnit_;
using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Dto.ENoticeVehicleDto.ENoticeDto_;
using CoreArchV2.Dto.ENoticeVehicleDto.ENoticeUnitDto_;
using CoreArchV2.Services.Services;
using Microsoft.AspNetCore.Http;

namespace CoreArchV2.Services.Interfaces
{
    public interface INoticeService
    {
        #region Notice
        PagedList<ENoticeDto> GetAllWithPaged(int? page, ENoticeDto filterModel);
        PagedList<ENoticeDto> GetAllSpeedWithPaged(int? page, ENoticeDto filterModel);
        Task<List<ENoticeDto>> GetAllSpeed(ENoticeDto filterModel);
        Task<EResultDto> InsertBulkAsync(ENoticeReadExcelDto model);
        EResultDto InsertNotice(ENoticeDto tempModel);
        EResultDto UpdateNotice(ENoticeDto tempModel);
        Task<ENoticeDto> GetByIdNoticeAsync(int id);
        EResultDto DeleteNotice(int id);
        NoticeUnit Find(int id);
        #endregion

        #region NoticeUnit
        PagedList<ENoticeDto> GetAllUnitWithPaged(int? page, ENoticeDto filterModel);
        List<ENoticeDto> GetUnitIdStartEndDateVehicleList(ENoticeUnitDto filterModel);
        EResultDto InsertNoticeUnit(IList<IFormFile> files, ENoticeUnitDto tempModel);
        EResultDto UpdateNoticeUnit(IList<IFormFile> files, ENoticeUnitDto tempModel);
        EResultDto DeleteNoticeUnit(ENoticeDto model);
        bool PlateWithProcess(int type);
        Task<ENoticeUnitDto> GetByIdNoticeUnitAsync(int id);

        #endregion

        #region NoticeUnit Answer
        PagedList<ENoticeDto> GetAllUnitAnswerWithPaged(int? page, ENoticeDto filterModel);
        EResultDto InsertNoticeUnitAnswer(ENoticeUnitDto model);
        List<ENoticeDto> GetNoticeUnitAnswerList(int noticeUnitId, int userId);
        List<ENoticeDto> GetNoticeUnitAnswerRedirectList(int noticeUnitId, int userId);
        bool IsAutForNoticeUnit(int noticeUnitId, int userId);
        //bool IsAutForNoticeUnitRedirect(int noticeUnitId, int userId);

        #endregion

        #region NoticeUnit Redirect
        EResultDto IsRedirectNoticeUnit(int noticeUnitId);
        List<ENoticeDto> RedirectVehicleList(int noticeUnitId);
        EResultDto InsertRedirectNotice(ENoticeUnitDto model);
        EResultDto InsertNoticeUnitRedirectAnswer(ENoticeUnitDto model);
        #endregion

        #region NoticeUnitHistory
        List<ENoticeUnitDto> GetNoticeUnitHistory(int noticeUnitId, int userId, bool isAdmin);
        List<ENoticeDto> GetNoticeHistoryResultList(int noticeUnitId, int userId);
        #endregion
    }
}
