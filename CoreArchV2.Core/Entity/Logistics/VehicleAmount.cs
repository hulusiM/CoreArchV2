using CoreArchV2.Core.Entity.Common;

namespace CoreArchV2.Core.Entity.Logistics
{
    public class VehicleAmount : Base
    {
        public int VehicleId { get; set; }
        public int VehicleContractId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal Amount { get; set; }
        public decimal? AmountIncome { get; set; }
        public decimal? ExtraAmount { get; set; }
        public int TypeId { get; set; }
        public string? Description { get; set; }
    }
}