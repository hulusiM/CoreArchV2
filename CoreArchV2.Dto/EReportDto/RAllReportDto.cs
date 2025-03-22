namespace CoreArchV2.Dto.EReportDto
{
    public class RAllReportDto
    {
        public RDashboardDto HeaderInfoVehicleCost { get; set; }
        public RVehicleCostDto VehicleCostTotalAmount { get; set; }
        public List<RVehicleDto> VehicleCostTotalAmountDetail { get; set; }
    }
}
