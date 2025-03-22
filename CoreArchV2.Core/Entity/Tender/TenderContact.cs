using CoreArchV2.Core.Entity.Common;

namespace CoreArchV2.Core.Entity.Tender
{
    public class TenderContact : Base
    {
        public int TenderId { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
    }
}
