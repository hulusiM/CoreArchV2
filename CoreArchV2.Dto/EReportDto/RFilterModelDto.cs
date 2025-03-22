namespace CoreArchV2.Dto.EReportDto
{
    public class RFilterModelDto : RLoginInfoDto
    {
        public int UserId { get; set; }
        public string DateMonth { get; set; }
        public int? UnitId { get; set; }
        public int DayCount { get; set; }
        public int? ParentUnitId { get; set; }
        public int? CreatedBy { get; set; }
        public string UnitName { get; set; }
        public DateTime StartDate { get; set; } = DateTime.MinValue;
        public DateTime EndDate { get; set; } = DateTime.MinValue;
        public int VehicleId { get; set; }
        public string Plate { get; set; }
        public int RentTypeId { get; set; }
        public int EnginePowerId { get; set; }
        public int FuelStationId { get; set; }
        public int TypeId { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public bool IsTotalAmountForPlate { get; set; }//plaka bazlı toplam tutar mı ?

        public int PageStartCount { get; set; } = 1;
        public bool IsAverageKmAmount { get; set; } = false;
        public bool IsHistoryForDebitList { get; set; }
        public string Search { get; set; }

        public int TenderId { get; set; }
        public int State { get; set; }
    }
}
