namespace CoreArchV2.Dto.ECommonDto
{
    public class ESelect2Dto
    {
        public int id { get; set; }
        public bool Status { get; set; }
        public string text { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ParentUnitId { get; set; }
        public int VehicleId { get; set; }
        public int? UnitId { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
    }
}
