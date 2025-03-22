namespace CoreArchV2.Core.Entity.Common
{
    public class User : Base
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string? UserName { get; set; }
        public string? PassportNo { get; set; }
        public DateTime? BirthDate { get; set; }
        public decimal? Gender { get; set; }
        public string? MobilePhone { get; set; }
        public string? Email { get; set; }
        public int? CityId { get; set; }
        public string? Password { get; set; }
        public bool IsNewPassword { get; set; } = false;
        public string? NewPassword { get; set; }
        public string? Salt { get; set; }
        public Byte[]? Image { get; set; }
        public string? UnitName { get; set; }
        public string? ProjectName { get; set; }
        public int? UnitId { get; set; }
        public int? Flag { get; set; }
        public string? ArventoUserName { get; set; }
        public string? ArventoPassword { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? LoginDate { get; set; }
        public DateTime? LogOutDate { get; set; }
        public bool IsAdmin { get; set; } = false;
        public bool IsSendMail { get; set; } = false;
        public bool? IsSendMailVehicleOpReport { get; set; } = false;
    }
}