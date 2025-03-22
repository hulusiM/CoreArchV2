using CoreArchV2.Core.Entity.Common;

namespace CoreArchV2.Core.Entity.Tender
{
    public class Tender : Base
    {
        public int CreatedUnitId { get; set; }//Satış kaydı oluşturan birim id
        public string SalesNumber { get; set; }//Satış numarası
        public int ProjectDetailId { get; set; }//Proje tipi(Hizmet,malzeme,yapım satışı)
        //public int InstitutionId { get; set; }//İlgili kurum
        public string Name { get; set; }      //Proje Adı
        public int ContactTypeId { get; set; }

        public DateTime? TenderDate { get; set; } //İhale tarihi
        public DateTime? DeliveryDate { get; set; }//ihale teslim tarihi (ihale bitiş tarihinden önce bitebilir)
        public int? JobIncreaseDay { get; set; }//iş arttırımı olduysa gün sayısı
        public bool IsWarranty { get; set; } //Garantisi var mı ?
        public int? AdditionalTime { get; set; }//Ek süre gün sayısı
        public DateTime? WarrantyEndDate { get; set; }//Garanti bitiş süresi (Teslim süresine ay eklenerek bulunabilir)
        public int State { get; set; }//Taslak,ihale bilgileri toplandı,Bekleme aşamasında,işe başlandı,iş arttırımına gidildi,Ek süre,teslim edildi,garanti başladı,garanti bitti
        public int? CityId { get; set; }
        public int? MoneyTypeId { get; set; } //Para tipi
        public decimal? ExchangePrice { get; set; } //döviz kuru o anlık ücreti
        public string FooterInfo { get; set; }//tenderdetail footer bilgisi
        public int? ChangedCount { get; set; }//ihale değiştirme sayısı
    }
}