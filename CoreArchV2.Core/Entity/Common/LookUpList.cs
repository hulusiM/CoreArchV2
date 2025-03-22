namespace CoreArchV2.Core.Entity.Common
{
    public class LookUpList
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Code { get; set; }
        public int Type { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? EMail { get; set; }
        public string? Description { get; set; }
        public int? CityId { get; set; }
    }
}