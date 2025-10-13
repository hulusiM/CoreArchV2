namespace CoreArchV2.Core.Entity.Track
{
    public class VehicleTracking
    {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Imei { get; set; }
        public DateTime SignalDate { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public int? Speed { get; set; }
        public int? Km { get; set; }
        public int? EngineSpeed { get; set; }//Motor Devri
    }
}
