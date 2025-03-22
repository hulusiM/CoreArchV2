namespace CoreArchV2.Core.Entity.Licence.Dto
{
    public class LicenceKeyDto
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string FirmKey { get; set; }
        public string StatusName { get; set; }
        public string LockName { get; set; }
        public string FirmName { get; set; }
        public string FirmLongName { get; set; }
        public string FirmMailList { get; set; }
        public int MaxUserCount { get; set; }
        public int? ActiveUserCount { get; set; }
        public int MaxVehicleCount { get; set; }
        public int? ActiveVehicleCount { get; set; }
        public string Ip { get; set; }
        public bool IsLock { get; set; }
        public DateTime? LockDate { get; set; }
        public string ErrorMessage { get; set; }
        public string CustomButton { get; set; }
        public int PageStartCount { get; set; }
    }
}
