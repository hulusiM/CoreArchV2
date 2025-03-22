namespace CoreArchV2.Core.Entity.Logistics
{
    public class VehicleOperatingReport
    {
        public int Id { get; set; }
        public bool Status { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? UniqueLine { get; set; }
        public int VehicleId { get; set; }
        public int? Type { get; set; }
        public string? TypeName { get; set; }
        public DateTime? Tarih { get; set; }
        public string? ArventoNo { get; set; }
        public string? ArventoPlaka { get; set; }
        public string? ZimmetliPlaka { get; set; }

        public int? LastDebitUserId { get; set; }
        public int? LastUnitId { get; set; }
        public int? LastDebitCityId { get; set; }
        public int? LastDebitStatus { get; set; }
        public decimal? LastDebitKm { get; set; }

        public string? MesafeKm { get; set; }
        public int? DuraklamaSuresiSaat { get; set; }
        public int? DuraklamaSuresiDakika { get; set; }
        public int? DuraklamaSuresiSaniye { get; set; }
        public int? RolantiSuresiSaat { get; set; }
        public int? RolantiSuresiDakika { get; set; }
        public int? RolantiSuresiSaniye { get; set; }
        public int? HareketSuresiSaat { get; set; }
        public int? HareketSuresiDakika { get; set; }
        public int? HareketSuresiSaniye { get; set; }
        public int? KontakAcikKalmaSuresiSaat { get; set; }
        public int? KontakAcikKalmaSuresiDakika { get; set; }
        public int? KontakAcikKalmaSuresiSaniye { get; set; }
        public string? MaxHiz { get; set; }
        public string? AracSonDurumBilgileri { get; set; }
        public string? HizAlarm { get; set; }
        public string? SehirIciHizAlarm { get; set; }
        public string? SehirDisiHizAlarm { get; set; }
        public string? OtoyolHizAlarm { get; set; }
        public string? RolantiAlarm { get; set; }
        public string? DuraklamaAlarm { get; set; }
        public string? HareketAlarm { get; set; }
        public string? KontakAcildiAlarm { get; set; }
        public string? KontakKapandiAlarm { get; set; }
        public string? AniHizlanmaAlarm { get; set; }
        public string? AniYavaslamaAlarm { get; set; }
        public string? MotorDevirAsimiAlarm { get; set; }
        public string? IlkKontakAcildi { get; set; }
        public string? SonKontakKapandi { get; set; }
        public string? SurucuTanimaBirimi { get; set; }
        public string? SonHizAlarm { get; set; }
        public string? SonDuraklamaAlarm { get; set; }
        public bool IsSendMail { get; set; } = false;
    }
}
