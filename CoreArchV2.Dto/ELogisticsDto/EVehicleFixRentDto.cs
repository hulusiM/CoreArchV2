using CoreArchV2.Dto.ECommonDto;

namespace CoreArchV2.Dto.ELogisticsDto
{
    public class EVehicleFixRentDto : EIdDto
    {
        public int? EntityId { get; set; }
        public string Plate { get; set; }
        public string ChassisNo { get; set; }
        public int? ColorId { get; set; }
        public string EngineNo { get; set; }
        public int? VehicleModelId { get; set; }
        public string LicenceSeri { get; set; }
        public string LicenceNo { get; set; }
        public string ModelYear { get; set; }
        public int? UsageTypeId { get; set; }
        public int? FuelTypeId { get; set; }
        public int? FixtureTypeId { get; set; }
        public int? VehicleTypeId { get; set; }
        public int? UnitId { get; set; }
        public int? EnginePowerId { get; set; }
        public int? LastUserId { get; set; }
        public decimal? VehicleFirstCost { get; set; }
        public decimal? VehicleLastCost { get; set; }

        //VehicleRent
        public int VehicleRentId { get; set; }
        public int VehicleId { get; set; }
        public int FirmTypeId { get; set; }
        public int RentTypeId { get; set; }
        public int ConfirmUserId { get; set; }
        public DateTime ContractStartDate { get; set; }
        public DateTime ContractEndDate { get; set; }
        public string Description { get; set; }

        //ortak bilgiler
        public bool? IsTts { get; set; }
        public bool? IsHgs { get; set; }
        public string ArventoNo { get; set; }
        public int? MaxSpeed { get; set; }
        public int? GearTypeId { get; set; }
        public bool? IsPartnerShipRent { get; set; }
        public bool? IsLeasing { get; set; }
        public decimal? FirstKm { get; set; }
        public decimal? LastKm { get; set; }
        public decimal? MaxKmLimit { get; set; }
    }
}