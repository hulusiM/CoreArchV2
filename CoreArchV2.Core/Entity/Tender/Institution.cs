namespace CoreArchV2.Core.Entity.Tender
{
    public class Institution
    {
        public int Id { get; set; }
        public bool Status { get; set; }
        public string Name { get; set; }
        public int? ParentId { get; set; }
        public string Code { get; set; }
    }
}
