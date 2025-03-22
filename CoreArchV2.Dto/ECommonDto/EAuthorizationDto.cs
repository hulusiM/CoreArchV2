namespace CoreArchV2.Dto.ECommonDto
{
    public class EAuthorizationDto : EIdDtoForTree
    {
        public int? ParentId { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public int RoleId { get; set; }
        public string Controller { get; set; }
        public EStateDto state { get; set; }
        public string RoleName { get; set; }
        public string Action { get; set; }
        public string icon { get; set; } //rol sayfası icon
        public string text { get; set; } //rol sayfası text
        public int? DisplayOrder { get; set; }
        public bool IsMenu { get; set; }
        public bool IsUnControlledAuthority { get; set; }
        public List<EAuthorizationDto> children { get; set; } = new List<EAuthorizationDto>();
        public string Attribute { get; set; }
    }
}