using CoreArchV2.Core.Entity.Common;

namespace CoreArchV2.Core.Entity.Logistics
{
    public class FuelLog : Base
    {
        public int VehicleId { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal? Km { get; set; }
        public decimal? Liter { get; set; }
        public string? Description { get; set; }
        public int FuelStationId { get; set; }
        public bool IsPublisher { get; set; }
        public int? InsertType { get; set; }
    }
}