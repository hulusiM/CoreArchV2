namespace CoreArchV2.Dto.EReportDto
{
    public class RVehicleDayCostDto
    {
        //public DateTime Date { get; set; }
        public int DayCount { get; set; }
        public decimal Amount { get; set; }
        public decimal ExtraAmount { get; set; }
        public decimal ArventoSim { get; set; }
        public DateTime? Date { get; set; }
    }
}
