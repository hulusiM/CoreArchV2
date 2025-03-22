namespace CoreArchV2.Core.Entity.Common
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? UserId { get; set; }
        public string? Message { get; set; }
    }
}