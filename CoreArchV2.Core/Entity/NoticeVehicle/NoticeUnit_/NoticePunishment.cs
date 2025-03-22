namespace CoreArchV2.Core.Entity.NoticeVehicle.NoticeUnit_
{
    public class NoticePunishment
    {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public int CreatedBy { get; set; }
        public int NoticeId { get; set; }
        public int NoticeUnitId { get; set; }
        public int? DriverId { get; set; } //Uyarı alan kullanıcı
        public int? ConfirmUserId { get; set; } //Kaydı değerlendiren kullanıcı
        public DateTime? ConfirmDate { get; set; } //Kaydı değerlendirme tarihi
        public int ToUnitId { get; set; }
        public decimal? Amount { get; set; }
        public int NoticeSendUnitId { get; set; }
        public int? State { get; set; }
        public string Description { get; set; }
    }
}
