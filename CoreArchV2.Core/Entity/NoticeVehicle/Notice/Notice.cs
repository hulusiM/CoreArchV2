using CoreArchV2.Core.Entity.Common;

namespace CoreArchV2.Core.Entity.NoticeVehicle.Notice
{
    public class Notice : Base
    {
        public int VehicleId { get; set; }
        public int NoticeType { get; set; }//Hız ihlali,görev dışı kullanım vs
        public string? ArventoNo { get; set; } //Arvento sistemindeki arvento numarası
        public string? Driver { get; set; } //Sürücü Bilgisi
        public DateTime? TransactionDate { get; set; }
        public decimal? Speed { get; set; } //Hız
        public DateTime? FirstRunEngineDate { get; set; } //Motorun ilk açılış tarihi
        public DateTime? LastRunEngineDate { get; set; } //Motoru durdurma tarihi
        public string? MissionName { get; set; }
        public int? CityId { get; set; }

        public int? LastDebitUserId { get; set; }
        public int? LastUnitId { get; set; }
        public int? LastDebitStatus { get; set; }
        public decimal? LastDebitKm { get; set; }


        public decimal? TotalKm { get; set; } //Toplam km
        public string? Address { get; set; }
        public int ImportType { get; set; } //ekleme tipi (user,excel,arvento)
        public string? Description { get; set; }
        public int? NoticeUnitId { get; set; }
        public int State { get; set; } //Uyarıldı,Ceza verildi,kapatıldı vs
    }
}
