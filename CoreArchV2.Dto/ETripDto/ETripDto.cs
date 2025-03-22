using CoreArchV2.Dto.ECommonDto;

namespace CoreArchV2.Dto.ETripDto
{
    public class ETripDto : EIdDto
    {
        public string MissionName { get; set; }
        public int VehicleId { get; set; }
        public int DriverId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? StartKm { get; set; }
        public decimal? EndKm { get; set; }
        public int StartCityId { get; set; }
        public int? EndCityId { get; set; }
        public string Description { get; set; }


        public string StartCityName { get; set; }
        public string EndCityName { get; set; }
        public int State { get; set; }
        public string StateName { get; set; }
        public decimal TotalKm { get; set; }
        public string UnitName { get; set; }
        public string ProjectName { get; set; }
        public int? UnitId { get; set; }
        public int? ParentUnitId { get; set; }
        public int? SubUnitId { get; set; }
        public int UserUnitId { get; set; }
        public string Plate { get; set; }
        public string Plate2 { get; set; }
        public bool IsFilterMode { get; set; } = false;
        public int DebitPlateCount { get; set; } = 0;
        public int NotOpenMissionCount { get; set; } = 0;
        public List<ETripDto> PlateList { get; set; }
        public List<string> NotOpenMissionList { get; set; }
        public string NotOpenPlateSplit { get; set; }
        public string OpenPlateSplit { get; set; }
        public List<string> OpenMissionList { get; set; }
        public bool? IsManagerAllowed { get; set; }
        public string ManagerAllowed { get; set; }
        public int ManagerAllowedType { get; set; }
        public int TripLogId { get; set; }
        public string CreatedNameSurname { get; set; }

        public int Type { get; set; }
        public int? LogType { get; set; }
        public string TypeName { get; set; }
        public string LogTypeName { get; set; }
        public bool IsPastRecord { get; set; }
        public decimal? VehicleLastKm { get; set; }
        public string TransactionDate { get; set; }
        public DateTime TransactionDate2 { get; set; }
        public int? InsertType { get; set; }
        public string InsertDeviceType { get; set; }
    }
}
