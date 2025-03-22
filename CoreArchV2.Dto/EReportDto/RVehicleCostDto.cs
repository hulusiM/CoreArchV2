namespace CoreArchV2.Dto.EReportDto
{
    public class RVehicleCostDto : RVehicleDto
    {
        public int CostType { get; set; }
        public string CostTypeName { get; set; }
        //public int RentType { get; set; }
        //public string RentTypeName { get; set; }
        public DateTime CostDate { get; set; }
        public List<RVehicleSubCostDto> SubContractlist { get; set; }
        public int VehicleContractId { get; set; }
        public int MonthCount { get; set; }
        public decimal DatesRangeCost { get; set; }
        public int DayCount { get; set; }
        //public string RentFirmName { get; set; }

        public List<RVehicleCostDto> ChartPlate { get; set; }
        public List<RVehicleCostDto> ChartProject { get; set; }
        public List<RVehicleCostDto> ChartFirmName { get; set; }
        public List<RVehicleCostDto> ChartRentTypeName { get; set; }
        public string Button { get; set; } = "";
    }
}
