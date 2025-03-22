using CoreArchV2.Core.Entity.Common;

namespace CoreArchV2.Core.Entity.Logistics
{
    public class VehiclePhysicalImage : Base
    {
        public int VehicleId { get; set; }
        public bool IsAccept { get; set; } = false;
        public int? TypeId { get; set; }
    }
}
