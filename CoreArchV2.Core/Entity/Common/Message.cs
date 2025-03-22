namespace CoreArchV2.Core.Entity.Common
{
    public class Message
    {
        public int Id { get; set; }
        public bool? Status { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public int? Type { get; set; }
        public string? Description { get; set; }
        public string? Head { get; set; }
        public int? UserId { get; set; }
        public bool UnRead { get; set; }
    }
}