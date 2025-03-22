namespace CoreArchV2.Dto.ECommonDto
{
    public class ERoleAuthorizationDto : EIdDto
    {
        public int RoleId { get; set; }
        public int AuthorizationId { get; set; }
        public EStateDto State { get; set; }
        public string RoleName { get; set; }
        public List<int> AuthorizationIdList { get; set; }
        public bool? HomePageId { get; set; }
    }
}