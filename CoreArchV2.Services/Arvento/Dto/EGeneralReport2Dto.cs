namespace CoreArchV2.Services.Arvento.Dto
{
    public class EGeneralReport2Dto
    {
        public int KayitNo { get; set; }
        public string CihazNo { get; set; }
        public string Plaka { get; set; }
        public string Surucu { get; set; }
        public DateTime Tarih { get; set; }
        public string Tur { get; set; }
        public string Hiz { get; set; }
        public string Adres { get; set; }
        public string Enlem { get; set; }
        public string Boylam { get; set; }
        public string IconUrl { get; set; } = "http://leafletjs.com/examples/custom-icons/leaf-red.png";
        public string DuraklamaSuresi { get; set; }
    }
}