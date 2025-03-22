using CoreArchV2.Core.Entity.Common;

namespace CoreArchV2.Core.Entity.Logistics
{
    public class Tire : Base
    {
        public int? WareHouseId { get; set; }
        public int DimensionTypeId { get; set; }
        public int TireTypeId { get; set; }
        public int State { get; set; }
        public int? VehicleId { get; set; }
    }
}