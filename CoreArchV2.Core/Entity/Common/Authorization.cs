namespace CoreArchV2.Core.Entity.Common
{
    public class Authorization : Base
    {
        public int? ParentId { get; set; }
        public string Name { get; set; }
        public string? Controller { get; set; }
        public string? Action { get; set; }
        public string? Icon { get; set; }
        public bool IsMenu { get; set; }
        public bool IsUncontrolledAuthority { get; set; }
        public int? DisplayOrder { get; set; }
        public string? Attribute { get; set; }
    }
}