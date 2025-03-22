using CoreArchV2.Core.Enum;
using CoreArchV2.Dto.ECommonDto;

namespace CoreArchV2.Dto.ELogisticsDto
{
    public class EFuelLogDto : EIdDto
    {
        public int VehicleId { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalAmountDiscount { get; set; }
        public string DiscountPercent { get; set; }
        public decimal? Km { get; set; }
        public decimal? Liter { get; set; }
        public string Description { get; set; }
        public string Plate { get; set; }
        public int FuelStationId { get; set; }
        public int? LastKm { get; set; }
        public string FuelStationName { get; set; }
        public string UnitName { get; set; }
        public FuelPublisher Publisher { get; set; }
        public string DateName { get; set; }
        public DateTime[] Dates { get; set; }
        public decimal? UserFaultAmount { get; set; }
    }
}