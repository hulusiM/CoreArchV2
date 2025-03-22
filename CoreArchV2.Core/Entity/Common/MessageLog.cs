namespace CoreArchV2.Core.Entity.Common
{
    public class MessageLog
    {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public int? CreatedBy { get; set; }
        public int UserId { get; set; }
        public int Type { get; set; }
        public string? Subject { get; set; }
        public string Body { get; set; }
        public string? MessageGuid { get; set; }
        public string? MessageTo { get; set; }
        public string? MessageCc { get; set; }
        public string? MessageBcc { get; set; }
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
