namespace CoreArchV2.Core.Entity.Tender
{
    public class TenderDetailPriceHistory
    {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public int CreatedBy { get; set; }
        public int TenderDetailId { get; set; }
        public decimal Price { get; set; }
        public int CreatedUnitId { get; set; }//değiştiren kullanıcı log kaydı
    }
}
