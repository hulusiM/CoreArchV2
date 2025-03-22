namespace CoreArchV2.Services.Arvento.Dto
{
    public class ESpeedDto
    {
        public string Address { get; set; }
        public string AlarmType { get; set; }
        public string BuildingRegion { get; set; }
        public string DeviceNo { get; set; }
        public int? Direction { get; set; }
        public DateTime? GmtDateTime { get; set; }
        public string Height { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? Speed { get; set; }
    }

    public class ECoordinateDto
    {
        public int VehicleId { get; set; }
        public int? DebitUserId { get; set; }
        public int? UnitId { get; set; }
        public int? ParentUnitId { get; set; }
        public string ArventoNo { get; set; }
        public string Node { get; set; }
        public string Address { get; set; }
        public int? Altitude { get; set; }
        public double? Course { get; set; }
        public string DailyTrip { get; set; }
        public string LocalDateTime { get; set; }
        public DateTime? LocalDateTime2 { get; set; }
        public double? LongitudeX { get; set; }
        public double? LatitudeY { get; set; }
        public string PktType { get; set; }
        public string Region { get; set; }
        public string SCDriver { get; set; }
        public double? Speed { get; set; }
        public int? MaxSpeed { get; set; }
        public string licensePlate { get; set; }
        public string DebitNameSurname { get; set; }
        public int SpeedVehicle { get; set; }//Hız sınırını aşan araç sayısı
        public int RolantiVehicle { get; set; }//Rölanti sayısı
        public int ActiveVehicle { get; set; } //Aktii hareket halinde araç sayısı
    }
}
