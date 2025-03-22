namespace CoreArchV2.Core.Entity.Common
{
    public abstract class Base
    {
        public int Id { get; set; }
        public bool Status { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public int CreatedBy { get; set; }
        public string CreatedIp { get; set; } = "1";
    }
}