namespace CoreArchV2.Dto.ECommonDto
{
    public class EUnitDto : EIdDto
    {
        public string ParentName { get; set; }
        public string Name { get; set; }
        public int? ParentId { get; set; }
        public string Code { get; set; }
        public bool? IsTenderVisible { get; set; }
    }
}