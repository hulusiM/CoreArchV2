using CoreArchV2.Dto.ECommonDto;

namespace CoreArchV2.Dto.ELogisticsDto
{
    public class EMaintenanceDto : EIdDto
    {
        public int VehicleId { get; set; }
        public int RequestUserId { get; set; }
        public int CityId { get; set; }
        public int SupplierId { get; set; }
        public string InvoiceNo { get; set; }
        public int? LastKm { get; set; }
        public string Description { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime? ServiceStartDate { get; set; }
        public DateTime? ServiceEndDate { get; set; }
        public string MaintenanceTypeName { get; set; }
        public string CityName { get; set; }
        public string RequestFullName { get; set; }
        public string Plate { get; set; }
        public string UnitName { get; set; }
        public List<EFileUploadDto> files { get; set; }

        public List<EFileUploadDto> FileUploads { get; set; }
        public string SupplierName { get; set; }
        public int? FixtureTypeId { get; set; }
        public string[] MaintenancePieces { get; set; }
        public List<string> MaintenanceTypeIds { get; set; }
        public bool IsHgsPage { get; set; }
        public decimal? UserFaultAmount { get; set; }
        public string UserFaultDescription { get; set; }
        public string TireInfo { get; set; }
    }
}