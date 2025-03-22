using System.ComponentModel.DataAnnotations;

namespace CoreArchV2.Core.Entity.Licence.Entity
{
    public class RequestLog
    {
        [Key]
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? LicenceKeyId { get; set; }
        public string TypeName { get; set; }
        public DateTime RequestDate { get; set; }
        public string RequestJson { get; set; }
        public string ResponseJson { get; set; }
    }
}
