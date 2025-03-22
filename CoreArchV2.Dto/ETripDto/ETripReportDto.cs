using CoreArchV2.Dto.ECommonDto;

namespace CoreArchV2.Dto.ETripDto
{
    public class ETripReportDto : EIdDto
    {
        public string Plate { get; set; }
        public string Driver { get; set; }
        public string MissionName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalKm { get; set; }
    }
}
