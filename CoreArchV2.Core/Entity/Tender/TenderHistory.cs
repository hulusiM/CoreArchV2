using CoreArchV2.Core.Entity.Common;

namespace CoreArchV2.Core.Entity.Tender
{
    public class TenderHistory : Base
    {
        public int TenderId { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.Now;
        public int? InstitutionId { get; set; }
        public string ObjectTenderDetailModel { get; set; }
        public int State { get; set; }
    }
}
