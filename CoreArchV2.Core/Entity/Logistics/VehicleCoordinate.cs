namespace CoreArchV2.Core.Entity.Logistics
{
    public class VehicleCoordinate
    {
        public Int64 Id { get; set; }
        public int VehicleId { get; set; }
        public DateTime LocalDate { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string? Speed { get; set; }
        public string? Driver { get; set; }
    }
}
