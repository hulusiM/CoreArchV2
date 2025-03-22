using CoreArchV2.Dto.ECommonDto;

namespace CoreArchV2.Dto.ELogisticsDto
{
    public class EOutOfHourDto : EIdDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string UniqueLine { get; set; }
        public int VehicleId { get; set; }
        public int? Type { get; set; }
        public int? LastDebitId { get; set; }
        public int? LastDebitStatus { get; set; }
        public string TypeName { get; set; }
        public DateTime? Tarih { get; set; }
        public string ArventoStartEndDate { get; set; }
        public string ArventoNo { get; set; }
        public string ArventoDebitPlateNo { get; set; }
        public string DebitTripUser { get; set; }
        public string MesafeKm { get; set; }
        public string TripKm { get; set; }
        public string DuraklamaSuresi { get; set; }
        public string RolantiSuresi { get; set; }
        public string HareketSuresi { get; set; }
        public string KontakAcikKalmaSuresi { get; set; }
        public string IlkSonKontakAcildi { get; set; }
        public string MaxHiz { get; set; }
        public string TripDescription { get; set; }

        public string AracSonDurumBilgileri { get; set; }
        public string HizAlarm { get; set; }
        public string SehirIciHizAlarm { get; set; }
        public string SehirDisiHizAlarm { get; set; }
        public string OtoyolHizAlarm { get; set; }
        public string RolantiAlarm { get; set; }
        public string DuraklamaAlarm { get; set; }
        public string HareketAlarm { get; set; }
        public string KontakAcildiAlarm { get; set; }
        public string KontakKapandiAlarm { get; set; }
        public string AniHizlanmaAlarm { get; set; }
        public string AniYavaslamaAlarm { get; set; }
        public string MotorDevirAsimiAlarm { get; set; }
        public string SurucuTanimaBirimi { get; set; }
        public string SonHizAlarm { get; set; }
        public string SonDuraklamaAlarm { get; set; }

        public int? UnitId { get; set; }
        public int? ParentUnitId { get; set; }
        public string UnitName { get; set; }
    }
}
