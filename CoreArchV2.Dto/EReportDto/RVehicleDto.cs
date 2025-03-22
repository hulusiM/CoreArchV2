namespace CoreArchV2.Dto.EReportDto
{
    public class RVehicleDto
    {
        public int Id { get; set; }
        public bool Status { get; set; }
        public int Count { get; set; }
        public string Name { get; set; }
        public int VehicleId { get; set; }
        public string Plate { get; set; }
        public decimal Amount { get; set; }
        public decimal ExtraAmount { get; set; }
        public decimal ArventoAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal? UserFaultAmount { get; set; }
        public string UserFaultDescription { get; set; }
        public int? UnitId { get; set; }
        public int? ParentUnitId { get; set; }
        public string UnitName { get; set; }
        public string ProjectName { get; set; }
        public string VehicleModelName { get; set; }
        public string VehicleTypeName { get; set; }
        public string DateMonth { get; set; }
        public DateTime StartDate { get; set; } = DateTime.MinValue;
        public DateTime EndDate { get; set; } = DateTime.MinValue;
        public string SupplierName { get; set; }
        public string UserNameSurname { get; set; }
        public string DebitNameSurname { get; set; }
        public int SupplierId { get; set; }
        public int? UsageTypeId { get; set; }
        public int PlateCount { get; set; }
        public List<string> Plates { get; set; }
        public List<decimal> Amounts { get; set; }
        public List<int> VehicleIds { get; set; }
        public string DebitDateRange { get; set; }
        public string VehicleModelYear { get; set; }
        public string RentFirmName { get; set; }
        public int? RentType { get; set; }
        public string RentTypeName { get; set; }
        public bool IsAllMonth { get; set; }//Tüm ay mı?
    }
}