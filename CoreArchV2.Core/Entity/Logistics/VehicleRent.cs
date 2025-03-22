namespace CoreArchV2.Core.Entity.Logistics
{
    public class VehicleRent
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public int FirmTypeId { get; set; }
        public int RentTypeId { get; set; }
        public int ConfirmUserId { get; set; }
        public string? Description { get; set; }
    }
}