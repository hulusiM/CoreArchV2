using CoreArchV2.Core.Entity.Logistics;

namespace CoreArchV2.Dto.ECommonDto
{
    public class EMailDto
    {
        public string Subject { get; set; }
        public string Body { get; set; }
        public string To { get; set; }
        public string Cc { get; set; }
        public string Bcc { get; set; }
        public string RootUrl { get; set; }
        public List<FileUpload> Attachments { get; set; }
    }
}
