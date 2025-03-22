using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Dto.ELogisticsDto;
using CoreArchV2.Dto.ENoticeVehicleDto.ENoticeDto_;

namespace CoreArchV2.Dto.ENoticeVehicleDto.ENoticeUnitDto_
{
    public class ENoticeUnitDto : EIdDto
    {
        public int NoticeType { get; set; }
        public int FromUnitId { get; set; }
        public int ToUnitId { get; set; }
        public int? CreatedUnitId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Description { get; set; }
        public int State { get; set; }
        public int RedirectType { get; set; }
        public string StateName { get; set; }
        public decimal? Amount { get; set; }
        public int DriverId { get; set; }

        public List<ENoticeDto> NoticeList { get; set; }
        public List<EFileUploadDto> files { get; set; }

        public string FromUnitName { get; set; }
        public string ToUnitName { get; set; }
        public string Image { get; set; }
    }
}
