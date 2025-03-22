using CoreArchV2.Dto.ECommonDto;

namespace CoreArchV2.Dto.ELogisticsDto
{
    public class EVehicleRequestDto : EIdDto
    {
        public int? UnitId { get; set; }
        public string UnitName { get; set; }
        public int VehicleId { get; set; }
        public string Plate { get; set; }
        public int? RequestUserId { get; set; }
        public string RequestNameSurname { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string RequestNo { get; set; }
        public string Description { get; set; }
    }
}