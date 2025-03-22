namespace CoreArchV2.Dto.ECommonDto
{
    public class EMessageLogDto
    {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? CreatedBy { get; set; }
        public int UserId { get; set; }
        public int PageStartCount { get; set; }
        public int Type { get; set; }
        public string MessageType { get; set; }
        public string PushToken { get; set; }
        public string MessageTo { get; set; }
        public string CreatedNameSurname { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string MessageGuid { get; set; }
        public string Email { get; set; }
        public string MailCc { get; set; }
        public string MailBcc { get; set; }
        public bool IsAllUser { get; set; } = false;
        public string Status { get; set; }
    }
}
