namespace CoreArchV2.Dto.ECommonDto
{
    public class EUserDto : EIdDto
    {
        public string FullName { get; set; }
        public string RedirectUrl { get; set; }
        public string LoginMessage { get; set; } = "";
        public bool IsAdmin { get; set; }
        public bool IsManager { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string PassportNo { get; set; }
        public DateTime? BirthDate { get; set; }
        public decimal? Gender { get; set; }
        public string MobilePhone { get; set; }
        public string Email { get; set; }
        public int? CityId { get; set; }
        public string Password { get; set; }
        public bool IsNewPassword { get; set; }
        public string NewPassword { get; set; }
        public string Image { get; set; }
        public int? Flag { get; set; }
        public string ArventoUserName { get; set; }
        public string ArventoPassword { get; set; }
        public bool? IsActive { get; set; }
        public bool IsSendMail { get; set; }
        public bool? IsSendMailVehicleOpReport { get; set; }
        public int? UnitId { get; set; }
        public string UnitName { get; set; }
        public int? ParentUnitId { get; set; }
        public DateTime? LoginDate { get; set; }
        public DateTime? LogOutDate { get; set; }
        public bool IsChangePassWarning { get; set; } = false;
        public string ConnectionId { get; set; }
        public string MailSenderInfo { get; set; }
        public string MailSenderVehicleOpInfo { get; set; }
    }
}