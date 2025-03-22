namespace CoreArchV2.Dto.EReportDto
{
    public class RVehicleSubCostDto
    {
        public bool Status { get; set; }
        public int VehicleId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal Amount { get; set; }
        public decimal? ExtraAmount { get; set; }
        //public int MonthCount { get; set; }
        //public int DayCount { get; set; }
        //public decimal AmountDay { get; set; }

        public int CostType { get; set; }
        public string CostTypeName { get; set; }
        public string Plate { get; set; }
    }
}
