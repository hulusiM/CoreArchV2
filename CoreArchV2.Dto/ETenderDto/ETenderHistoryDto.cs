using CoreArchV2.Dto.ECommonDto;

namespace CoreArchV2.Dto.ETenderDto
{
    public class ETenderHistoryDto : EIdDto
    {
        public int TenderId { get; set; }
        public DateTime TransactionDate { get; set; }
        public int? InstitutionId { get; set; }
        public string ObjectTenderDetailModel { get; set; }
        public int State { get; set; }
        public string StateName { get; set; }

        public string InstitutionName { get; set; }
        public string ProjectTypeName { get; set; }
        public string NameSurname { get; set; }

    }
}
