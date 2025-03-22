namespace CoreArchV2.Core.Entity.Common
{
    public class LoginLog
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public DateTime? LoginDate { get; set; }
        public DateTime? LogOutDate { get; set; }
    }
}