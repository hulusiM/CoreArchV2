namespace CoreArchV2.Dto.ENoticeVehicleDto.ENoticeDto_
{
    public class ENoticeReadExcelDto
    {
        public string[] HtmlString { get; set; }
        public int CreatedBy { get; set; }
        public List<ENoticeDto> NoticeList { get; set; }
        public int NoticeType { get; set; }
        public int ImportType { get; set; }
    }
}
