using CoreArchV2.Core.Entity.Common;

namespace CoreArchV2.Core.Entity.Logistics
{
    public class VehicleContract : Base
    {
        public int VehicleId { get; set; }
        public decimal? FirstKm { get; set; }
        public decimal? LastKm { get; set; }
        public decimal? MaxKmLimit { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}