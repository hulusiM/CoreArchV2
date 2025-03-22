namespace CoreArchV2.Core.Entity.NoticeVehicle.NoticeUnit_
{
    public class NoticeSendUnit
    {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public int CreatedBy { get; set; }
        public int NoticeId { get; set; }
        public int NoticeUnitId { get; set; }
        public int ToUnitId { get; set; }
        public int? State { get; set; }
        public string Description { get; set; }
    }
}
