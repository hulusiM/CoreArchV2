namespace CoreArchV2.Core.Entity.Licence.Entity
{
    public class WebLog
    {
        public int Id { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string OldLog { get; set; }
        public string NewLog { get; set; }
    }
}
