namespace CoreArchV2.Dto.ECommonDto
{
    public class ENotificationDto
    {
        public int TimeUpRentACar { get; set; } //Sözleşmesi biten araçlar
        public int TimeDownRentACar { get; set; } //Sözleşmesi yaklaşan araçlar
        public int TimeUpDocumentACar { get; set; }
    }
}
