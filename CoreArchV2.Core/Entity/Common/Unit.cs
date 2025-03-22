namespace CoreArchV2.Core.Entity.Common
{
    public class Unit
    {
        public int Id { get; set; }
        public bool Status { get; set; }
        public string Name { get; set; }
        public int? ParentId { get; set; }
        public string? Code { get; set; }
        public bool? IsTenderVisible { get; set; }
    }
}