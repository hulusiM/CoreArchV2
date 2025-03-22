namespace CoreArchV2.Core.Entity.Logistics
{
    public class VehicleModel
    {
        public int Id { get; set; }
        public bool Status { get; set; }
        public string Name { get; set; }
        public int VehicleBrandId { get; set; }
        public string Description { get; set; }
    }
}