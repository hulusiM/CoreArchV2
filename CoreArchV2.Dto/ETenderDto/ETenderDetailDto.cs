using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Dto.ELogisticsDto;

namespace CoreArchV2.Dto.ETenderDto
{
    public class ETenderDetailDto : EIdDto
    {
        public int TenderId { get; set; }
        public string StockCode { get; set; }
        public string ProductName { get; set; }//Ürün adı
        public int Piece { get; set; } //Adet
        public int UnitTypeId { get; set; } //Birim (metre,adet,cm3,cm2,metrekare)
        public int? UnitId { get; set; } //hangi birim analiz yapacak
        public decimal? ProductPrice { get; set; } //ürün fiyatı (birimler araştıracak) Toplam = Piece*ProductPrice
        public string ProductDescription { get; set; } //ürün analiz açıklama
        public decimal? SellingCost { get; set; } //Satış fiyatı(Son fiyatı satış pazarlama belirleyecek)
        public string Description { get; set; } //genel açıklama
        public bool IsPrint { get; set; } //Çıktı alınırken hangileri görünecek
        public bool JobIncrease { get; set; } //İş arttırımı mı ?


        public string UnitTypeName { get; set; }
        public string ProductPriceInfo { get; set; }
        public decimal TotalProductPrice { get; set; }
        public List<EFileUploadDto> files { get; set; }
        public string NameSurname { get; set; }
        public string UnitName { get; set; }
        public bool IsAdmin { get; set; }
        //public bool IsVisible { get; set; }

    }
}