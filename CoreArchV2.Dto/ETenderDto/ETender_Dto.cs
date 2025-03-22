using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Dto.ELogisticsDto;

namespace CoreArchV2.Dto.ETenderDto
{
    public class ETender_Dto : EIdDto
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

        public string InstitutionName { get; set; }


        public List<EFileUploadDto> files { get; set; }
        public string TenderStateName { get; set; }
        public string Description { get; set; }
        public int? UnitId { get; set; }
        public string UnitName { get; set; }
        public string Email { get; set; }
        public int WaitRequest { get; set; }
    }
}