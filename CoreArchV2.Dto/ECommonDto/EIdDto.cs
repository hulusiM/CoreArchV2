namespace CoreArchV2.Dto.ECommonDto
{
    public class EIdDto : EMenuBtnDto
    {
        public int Id { get; set; }
        public bool Status { get; set; } = true;
        public string StatusName { get; set; }
        public string NameSurname { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public int CreatedBy { get; set; }
        public int LoginUserId { get; set; }
        public int? LoginUnitId { get; set; }
        public int? LoginParentUnitId { get; set; }
        public string CreatedIp { get; set; } = "1";
        public int PageStartCount { get; set; }
        public int ListCount { get; set; } = 50;
        public bool IsAdmin { get; set; }
    }
}