namespace CoreArchV2.Dto.ECommonDto
{
    public class EUserRoleDto : EIdDto
    {
        public int MemberRoleId { get; set; }
        public int UserId { get; set; }
        public string UserNameSurname { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string FullName { get; set; }
        public string HomePageName { get; set; }
        public int HomePageId { get; set; }
    }
}