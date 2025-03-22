using System.ComponentModel.DataAnnotations;

namespace CoreArchV2.Core.Entity.Licence.Entity
{
    public class LicenceKey
    {
        [Key]
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string FirmKey { get; set; }
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
    }
}
