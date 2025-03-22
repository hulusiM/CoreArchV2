using CoreArchV2.Core.Entity.Common;

namespace CoreArchV2.Core.Entity.TripVehicle
{
    public class Trip : Base
    {
        public string? MissionName { get; set; }
        public int VehicleId { get; set; }
        public int DriverId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal StartKm { get; set; }
        public decimal? EndKm { get; set; }
        public int StartCityId { get; set; }
        public int? EndCityId { get; set; }
        public string? Description { get; set; }
        public bool? IsManagerAllowed { get; set; }
        public int Type { get; set; }
        public int State { get; set; }
        public int? InsertType { get; set; }
        public int? UnitId { get; set; }
    }
}
