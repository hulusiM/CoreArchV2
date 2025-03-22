using CoreArchV2.Dto.ECommonDto;

namespace CoreArchV2.Dto.ELogisticsDto
{
    public class EFuelReadExcelDto : EIdDto
    {
        public int FuelStationId { get; set; }
        public decimal DiscountPercent { get; set; } = 0;
        public string[] HtmlString { get; set; }
        public string UserName { get; set; }
        public List<EFuelLogDto> FuelList { get; set; }
    }
}
