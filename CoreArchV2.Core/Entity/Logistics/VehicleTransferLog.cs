using CoreArchV2.Core.Entity.Common;

namespace CoreArchV2.Core.Entity.Logistics
{
    public class VehicleTransferLog : Base
    {
        public int VehicleId { get; set; }
        public decimal? SalesCost { get; set; }
        public int TransferTypeId { get; set; }
        public DateTime? Date { get; set; }
        public string? Description { get; set; }
    }
}