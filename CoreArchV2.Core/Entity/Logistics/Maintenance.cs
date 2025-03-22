using CoreArchV2.Core.Entity.Common;

namespace CoreArchV2.Core.Entity.Logistics
{
    public class Maintenance : Base
    {
        public int VehicleId { get; set; }
        public int RequestUserId { get; set; }
        public string? InvoiceNo { get; set; }
        public int SupplierId { get; set; }
        public int? LastKm { get; set; }
        public string? Description { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime? ServiceStartDate { get; set; }
        public DateTime? ServiceEndDate { get; set; }
        public decimal? UserFaultAmount { get; set; }
        public string? UserFaultDescription { get; set; }
    }
}