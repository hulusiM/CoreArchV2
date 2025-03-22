using CoreArchV2.Dto.ECommonDto;

namespace CoreArchV2.Dto.ENoticeVehicleDto.ENoticeDto_
{
    public class ENoticeDto : EIdDto
    {
        public bool IsSend { get; set; } = false;
        public bool IsAnswer { get; set; } = false;
        public int VehicleId { get; set; }
        public int NoticeType { get; set; }//Hız ihlali,görev dışı kullanım vs
        public string ArventoNo { get; set; } //Arvento sistemindeki arvento numarası
        public string ArventoNo2 { get; set; }
        public string Driver { get; set; } //Sürücü Bilgisi
        public string Driver2 { get; set; } //Sürücü Bilgisi
        public DateTime? TransactionDate { get; set; }
        public DateTime? TransactionDateSpeed { get; set; }
        public DateTime? TransactionDateMission { get; set; }
        public decimal? Speed { get; set; } //Hız
        public string Speed2 { get; set; } //Hız
        public DateTime? FirstRunEngineDate { get; set; } //Motorun ilk açılış tarihi
        public DateTime? LastRunEngineDate { get; set; } //Motoru durdurma tarihi
        public string MissionName { get; set; }
        public string CityName { get; set; }
        public int? CityId { get; set; }
        public decimal? TotalKm { get; set; } //Toplam km
        public string Address { get; set; }
        public string UnitName { get; set; }
        public int ImportType { get; set; } //ekleme tipi (user,excel,arvento)
        public string Description { get; set; }
        public DateTime OpenDate { get; set; }//talep açılma tarihi
        public DateTime? ClosedDate { get; set; }//talep kapanma tarihi
        public int? NoticeUnitId { get; set; }
        public int State { get; set; } //Uyarıldı,Ceza verildi,kapatıldı vs

        public string StateName { get; set; }
        public int UserId { get; set; }
        public string NoticeTypeName { get; set; }
        public string NoticeUnitTypeName { get; set; }
        public string Plate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ToUnitName { get; set; }
        public int? UnitId { get; set; }
        public int? ParentUnitId { get; set; }
        public int? DriverId { get; set; }
        public bool EditMode { get; set; } = false;
        public bool UnitMode { get; set; } = false;
        public bool RedirectMode { get; set; } = false;
        public bool RedirectAnswerMode { get; set; } = false;
        public bool HistoryMode { get; set; } = false;
        public int? NoticeSendUnitState { get; set; }
        public string NoticeSendUnitState2 { get; set; }
        public string NoticeSendUnitDescription { get; set; }
        public int? NoticeSendRedirectUnitState { get; set; }
        public int NoticePunishmentId { get; set; }
        public string NoticeSendRedirectUnitState2 { get; set; }
        public string NoticeSendRedirectUnitDescription { get; set; }
        public decimal? Amount { get; set; }
        public int ToUnitId { get; set; }
        public int MinSpeed { get; set; }
        public int MaxSpeed { get; set; }
        public bool IsReleaseSubNotice { get; set; }

        public List<ENoticeDto> PlateList { get; set; }
        public EResultDto Result { get; set; }
        public int? LastDebitUserId { get; set; }
        public int? LastUnitId { get; set; }
        public int? LastDebitStatus { get; set; }
        public decimal? LastDebitKm { get; set; }
        public string DebitTripUser { get; set; }
        public string TripDescription { get; set; }
        public string TripKm { get; set; }
    }
}
