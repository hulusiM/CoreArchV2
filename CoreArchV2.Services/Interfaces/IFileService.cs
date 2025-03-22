using CoreArchV2.Dto.ECommonDto;
using Microsoft.AspNetCore.Http;

namespace CoreArchV2.Services.Interfaces
{
    public interface IFileService
    {
        #region Logistics
        EResultDto FileDeleteLogistics(int fileUploadId);
        EResultDto FileDeletePhysicalImage(int fileUploadId);
        EResultDto FileUploadInsertLogistics(IList<IFormFile> files);
        EResultDto FileUploadInsertVehiclePhysicalImage(IList<IFormFile> files, string fileName);
        #endregion

        #region Tender
        EResultDto FileDeleteTender(int fileUploadId);
        EResultDto FileUploadInsertTender(IList<IFormFile> files);
        #endregion

        #region Notice

        EResultDto FileUploadInsertNoticeUnit(IList<IFormFile> files);
        EResultDto FileDeleteNoticeUnit(int fileUploadId);

        #endregion
    }
}