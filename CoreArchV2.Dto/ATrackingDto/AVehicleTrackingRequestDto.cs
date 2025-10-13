namespace CoreArchV2.Dto.ATrackingDto
{
    public class AVehicleTrackingRequestDto
    {
        public string Imei { get; set; }
        public DateTime SignalDate { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public int? Speed { get; set; }
        public int? Km { get; set; }
        public int? EngineSpeed { get; set; }//Motor Devri
    }
}
