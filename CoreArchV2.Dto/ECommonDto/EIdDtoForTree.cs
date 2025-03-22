namespace CoreArchV2.Dto.ECommonDto
{
    public class EIdDtoForTree : EMenuBtnDto
    {
        public int id { get; set; } //rol menüde böyle görünmesi lazım,değişemez!!!
        public bool Status { get; set; }
        public string StatusName { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedIp { get; set; }
        public int PageStartCount { get; set; }
    }
}