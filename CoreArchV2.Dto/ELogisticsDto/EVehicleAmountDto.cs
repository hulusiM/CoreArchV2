using CoreArchV2.Dto.ECommonDto;

namespace CoreArchV2.Dto.ELogisticsDto
{
    public class EVehicleAmountDto : EIdDto
    {
        public int VehicleId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal AmountExpense { get; set; }
        public decimal AmountIncome { get; set; }
        public decimal? ExtraAmount { get; set; }
        public int TypeId { get; set; }
        public string Description { get; set; }
        public string VehicleAmountTypeName { get; set; }

        public DateTime ContractStartDate { get; set; }
        public DateTime ContractEndDate { get; set; }

        public decimal TotalTodayExpense { get; set; }
        public decimal TotalTodayIncome { get; set; }

        public decimal AllTotalExpense { get; set; }
        public decimal AllTotalIncome { get; set; }

        public int VehicleContractId { get; set; }
        public string DeleteVehicleInfo { get; set; }
    }
}