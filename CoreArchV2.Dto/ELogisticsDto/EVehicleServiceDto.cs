using CoreArchV2.Core.Entity.Common;

namespace CoreArchV2.Dto.ELogisticsDto
{
    public class EVehicleServiceDto : Base
    {
        public int VehicleId { get; set; }
        public int? VehicleDebitId { get; set; }
        public DateTime StartDate { get; set; }
        public string Description { get; set; }
        public string TempPlateNo { get; set; }
    }
}
