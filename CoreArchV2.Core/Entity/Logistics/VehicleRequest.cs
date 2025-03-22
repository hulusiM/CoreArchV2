using CoreArchV2.Core.Entity.Common;

namespace CoreArchV2.Core.Entity.Logistics
{
    public class VehicleRequest : Base
    {
        public int? UnitId { get; set; }
        public int VehicleId { get; set; }
        public int? RequestUserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? RequestNo { get; set; }
        public string? Description { get; set; }
    }
}