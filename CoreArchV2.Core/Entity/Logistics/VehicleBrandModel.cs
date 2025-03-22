namespace CoreArchV2.Core.Entity.Logistics
{
    public class VehicleBrandModel
    {
        public int Id { get; set; }
        public bool Status { get; set; }
        public string Name { get; set; }
        public int? ParentId { get; set; }
        public string? Description { get; set; }
    }
}