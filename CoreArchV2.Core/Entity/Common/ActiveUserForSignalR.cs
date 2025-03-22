namespace CoreArchV2.Core.Entity.Common
{
    public class ActiveUserForSignalR
    {
        public int Id { get; set; }
        public bool Status { get; set; } = true;
        public DateTime CreatedDate { get; set; }
        public string Url { get; set; }
        public string Menu { get; set; }
        public string ConnectionId { get; set; }
        public int UserId { get; set; }
    }
}
