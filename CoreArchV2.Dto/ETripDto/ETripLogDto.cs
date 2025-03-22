using CoreArchV2.Dto.ECommonDto;

namespace CoreArchV2.Dto.ETripDto
{
    public class ETripLogDto : EIdDto
    {
        public int TripId { get; set; }
        public DateTime TransactionDate { get; set; }
        public int Km { get; set; }
        public int CityId { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public int State { get; set; }
    }
}
