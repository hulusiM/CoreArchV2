namespace CoreArchV2.Core.Entity.Track
{
    public class Coordinate
    {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Imei { get; set; }
        public DateTime SignalDate { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
    }
}
