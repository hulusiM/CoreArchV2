using CoreArchV2.Core.Entity.Common;

namespace CoreArchV2.Core.Entity.Logistics
{
    public class Vehicle : Base
    {
        public int? EntityId { get; set; }
        public string Plate { get; set; }
        public string? ChassisNo { get; set; }
        public int? ColorId { get; set; }
        public string? EngineNo { get; set; }
        public int? VehicleModelId { get; set; }
        public string? LicenceSeri { get; set; }
        public string? LicenceNo { get; set; }
        public string? ModelYear { get; set; }
        public int? FuelTypeId { get; set; }
        public int? FixtureTypeId { get; set; }
        public int? VehicleTypeId { get; set; }
        public int? EnginePowerId { get; set; }
        public int? LastUnitId { get; set; }
        public string? ArventoNo { get; set; }
        public int? MaxSpeed { get; set; }
        public decimal? VehicleFirstCost { get; set; }
        public decimal? VehicleLastCost { get; set; }
        public bool? IsTts { get; set; }
        public bool? IsHgs { get; set; }
        public int? LastUserId { get; set; }
        public int? LastCityId { get; set; }
        public int? LastStatus { get; set; }
        public decimal? LastKm { get; set; }
        public int? GearTypeId { get; set; }
        public bool? IsPartnerShipRent { get; set; }
        public bool? IsLeasing { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? LastAddress { get; set; }
        public double? LastSpeed { get; set; }
        public DateTime? LastCoordinateInfo { get; set; }
    }
}