using CoreArchV2.Dto.ECommonDto;

namespace CoreArchV2.Dto.ELogisticsDto
{
    public class ECriminalLogDto : EIdDto
    {
        public int VehicleId { get; set; }
        public int CityId { get; set; }
        public string Plate { get; set; } //Ceza seri no
        public DateTime CriminalDate { get; set; } //Ceza tarihi
        public int CriminalTypeId { get; set; } //Ceza tipi
        public string CriminalTypeName { get; set; } //Ceza tipi
        public string PaidNameSurname { get; set; } //Ceza sahibi
        public decimal Amount { get; set; } //Tutar
        public DateTime NotificationDate { get; set; } //Tebliğ tarihi
        public string CriminalSerialNumber { get; set; } //Ceza seri no
        public decimal? PaidAmount { get; set; } //Ödenen tutar
        public int? PaidUserId { get; set; } //Ödeyen kişi
        public int CriminalPercent { get; set; } //indirim oranı
        public DateTime? PaidDate { get; set; } //Ödeme tarihi
        public string CityAndDistrict { get; set; }
        public int CriminalDistrictId { get; set; } //Malatya-Yeşilyurt
        public string Description { get; set; } //Açıklama
        public int? CriminalOwnerId { get; set; } //Ceza yiyen kişi


        public List<EFileUploadDto> files { get; set; }

        public List<EFileUploadDto> FileUploads { get; set; }
    }
}