namespace CoreArchV2.Core.Entity.Common
{
    public class UserRole : Base
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public int HomePageId { get; set; }
    }
}