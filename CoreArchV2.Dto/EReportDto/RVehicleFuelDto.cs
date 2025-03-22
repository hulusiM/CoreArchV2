namespace CoreArchV2.Dto.EReportDto
{
    public class RVehicleFuelDto : RVehicleDto
    {
        public DateTime FuelDate { get; set; }
        public string FuelDate2 { get; set; }
        public decimal? Km { get; set; }
        public decimal? Liter { get; set; }
        public decimal? KmSpend { get; set; }
        public decimal DiscountAmount { get; set; } //İndirimli tutar
        public decimal DiscountPercent { get; set; } //İndirim oranı
        public string StationName { get; set; }
        public string FixtureName { get; set; }
        public int? FixtureTypeId { get; set; }
        public int? State { get; set; }
        public bool IsPublisher { get; set; }
        public decimal? AverageKmAmount { get; set; }
        public decimal? AverageLiter { get; set; }
        public int FuelStationId { get; set; }
        public decimal TotalKm { get; set; }

        public string EnginePowerName { get; set; }
        public int? EnginePowerId { get; set; }
        public List<REnginePowerDto> EnginePowlist { get; set; }
    }

    public class REnginePowerDto
    {
        public string EnginePowerName { get; set; }
        public int? EnginePowerId { get; set; }
    }
}