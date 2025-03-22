using CoreArchV2.Core.Entity.Common;

namespace CoreArchV2.Core.Entity.Tender
{
    public class TenderDetail : Base
    {
        public int TenderId { get; set; }
        public string StockCode { get; set; }
        public string ProductName { get; set; }//Ürün adı
        public int Piece { get; set; }//Adet
        public int UnitTypeId { get; set; }//Birim(metre,adet,cm3,cm2,metrekare)
        public int? UnitId { get; set; } //hangi birim analiz yapacak
        public decimal? ProductPrice { get; set; }//ürün fiyatı (birimler araştıracak) Toplam = Piece*ProductPrice
        public string ProductDescription { get; set; }//ürün analiz açıklama
        public decimal? SellingCost { get; set; }//Satış fiyatı(Son fiyatı satış pazarlama belirleyecek)
        public string Description { get; set; }//genel açıklama
        public bool IsPrint { get; set; } //Çıktı alınırken hangileri görünecek
        public bool JobIncrease { get; set; } //İş arttırımı mı ?
        public bool Edited { get; set; } = false; //satır değişikliğe gitti mi 
    }
}