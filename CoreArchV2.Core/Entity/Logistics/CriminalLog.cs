using CoreArchV2.Core.Entity.Common;

namespace CoreArchV2.Core.Entity.Logistics
{
    public class CriminalLog : Base
    {
        public int VehicleId { get; set; }
        public DateTime CriminalDate { get; set; } //Ceza tarihi
        public int CriminalTypeId { get; set; } //Ceza tipi
        public decimal Amount { get; set; } //Tutar
        public DateTime NotificationDate { get; set; } //TEbliğ tarihi
        public string CriminalSerialNumber { get; set; } //Ceza seri no
        public decimal? PaidAmount { get; set; } //Ödenen tutar
        public DateTime? PaidDate { get; set; } // ödeme tarihi
        public int CriminalDistrictId { get; set; } //Malatya-Yeşilyurt
        public string? Description { get; set; } //Açıklama
        public int? CriminalOwnerId { get; set; } //Ceza yiyen kişi
    }
}