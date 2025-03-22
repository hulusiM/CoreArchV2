using CoreArchV2.Dto.ECommonDto;

namespace CoreArchV2.Dto.ELogisticsDto
{
    public class EVehicleDto : EIdDto
    {
        public int VehicleId { get; set; }
        public int? EntityId { get; set; }
        public string EntityName { get; set; }
        public int? UnitId { get; set; }
        public int? ParentUnitId { get; set; }
        public string UnitName { get; set; }
        public string Code { get; set; }
        public int? UnitParentId { get; set; }
        public string UnitParentName { get; set; }
        public string Plate { get; set; }
        public string Plate2 { get; set; }
        public string ChassisNo { get; set; }
        public int? ColorId { get; set; }
        public string ColorName { get; set; }
        public int[] CityId { get; set; }
        public string CityName { get; set; }
        public string EngineNo { get; set; }
        public int? VehicleModelId { get; set; }
        public string LicenceSeri { get; set; }
        public string LicenceNo { get; set; }
        public string ModelYear { get; set; }
        public int? UsageTypeId { get; set; }
        public int? FuelTypeId { get; set; }
        public int? DebitUserId { get; set; }
        public int? DeliveryUserId { get; set; }
        public string DeliveryUserName { get; set; }
        public string DebitNameSurname2 { get; set; }
        public string CreatedUserName { get; set; }
        public int? FixtureTypeId { get; set; }
        public int FirmTypeId { get; set; }
        public string RentFirmName { get; set; }
        public decimal? LastKm { get; set; }
        public int? LastCityId { get; set; }
        public string LastCityName { get; set; }
        public int? VehicleTypeId { get; set; }
        public int VehicleDebitId { get; set; }
        public string ArventoNo { get; set; }
        public int? MinSpeed { get; set; }
        public int? MaxSpeed { get; set; }

        public string IsHgsName { get; set; }
        public bool? IsTts { get; set; }
        public string IsTtsName { get; set; }
        public int? LastStatus { get; set; }
        public string Description { get; set; }

        public DateTime? KDocumentEndDate { get; set; } = null;
        public DateTime? KaskoEndDate { get; set; } = null;
        public DateTime? TrafficEndDate { get; set; } = null;
        public DateTime? ExaminationEndDate { get; set; } = null;

        public string DebitNameSurname { get; set; }
        public DateTime? DebitCreatedDate { get; set; }
        public string FixtureName { get; set; }
        public string VehicleTypeName { get; set; }
        public string FuelTypeName { get; set; }
        public string UsageTypeName { get; set; }
        public string VehicleModelName { get; set; }
        public DateTime? DebitStartDate { get; set; }
        public DateTime? DebitEndDate { get; set; }
        public string DebitState { get; set; }
        public int? DebitState2 { get; set; }

        public EVehicleRentDto VehicleRentDto { get; set; }
        public List<EFileUploadDto> files { get; set; }
        public EVehicleTransferFileDto VehicleTransfer { get; set; }
        public string Search { get; set; }
        public string ContractDateRange { get; set; }
        public decimal? ContractPrice { get; set; }
        public decimal? TotalPrice { get; set; }
        public int Status2 { get; set; }
        public DateTime TransactionDate { get; set; }
        public string TempPlateNo { get; set; }
        public string TempPlateNo2 { get; set; }
        public int VehiclePhysicalImageId { get; set; }
        public string GearTypeName { get; set; }
        public int? GearTypeId { get; set; }
        public bool? IsPartnerShipRent { get; set; }
        public string PartnerShipName { get; set; }
        public bool? IsLeasing { get; set; }
        public string LeasingName { get; set; }
    }
}