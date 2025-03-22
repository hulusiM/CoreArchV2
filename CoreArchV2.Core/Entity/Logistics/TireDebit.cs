using CoreArchV2.Core.Entity.Common;

namespace CoreArchV2.Core.Entity.Logistics
{
    public class TireDebit : Base
    {
        public int VehicleId { get; set; }
        public int WareHouseId { get; set; }//Aldığı depo id
        public int AttachedTireCount { get; set; }//Araca takılan lastik sayısı
        public int State { get; set; }
        public int DimensionTypeId { get; set; }
        public int TireTypeId { get; set; }
        public string? Description { get; set; }
    }
}
