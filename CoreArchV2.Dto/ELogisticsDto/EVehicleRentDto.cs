using CoreArchV2.Dto.ECommonDto;

namespace CoreArchV2.Dto.ELogisticsDto
{
    public class EVehicleRentDto : EIdDto
    {
        public int VehicleId { get; set; }
        public int FirmTypeId { get; set; }
        public int RentTypeId { get; set; }
        public int UserId { get; set; }
        public int ConfirmUserId { get; set; }
        public DateTime ContractStartDate { get; set; }
        public DateTime ContractEndDate { get; set; }
        public string Description { get; set; }
        public int? FixtureTypeId { get; set; }
        public string Plate { get; set; }
    }
}