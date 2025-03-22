using CoreArchV2.Dto.ECommonDto;

namespace CoreArchV2.Dto.ETenderDto
{
    public class ETenderAllDto : EIdDto
    {
        public int TenderId { get; set; }
        public decimal TotalAmountTenderDetail { get; set; }
        public ETender_Dto Tender { get; set; }
        public List<ETenderDetailDto> TenderDetailList { get; set; }
        public List<ESelect2Dto> ContactList { get; set; }
    }
}
