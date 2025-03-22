using CoreArchV2.Core.Entity.Common;

namespace CoreArchV2.Core.Entity.Logistics
{
    public class VehicleDebit : Base
    {
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int VehicleId { get; set; }
        public int? DebitUserId { get; set; }
        public int? DeliveryUserId { get; set; }
        public int? UnitId { get; set; }
        public int? CityId { get; set; }
        public int? LastKm { get; set; }
        public int? UsageTypeId { get; set; }
        public int? State { get; set; }
        public string? Description { get; set; }
        public string? TempPlateNo { get; set; }
    }
}