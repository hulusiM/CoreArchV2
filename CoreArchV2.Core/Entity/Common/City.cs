namespace CoreArchV2.Core.Entity.Common
{
    public class City
    {
        public int Id { get; set; }
        public bool Status { get; set; }
        public string Name { get; set; }
        public int? ParentId { get; set; }
        public int? CountryId { get; set; }
        public string? Coordinates { get; set; }
    }
}