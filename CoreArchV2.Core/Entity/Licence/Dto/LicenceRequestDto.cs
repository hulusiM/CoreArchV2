namespace CoreArchV2.Core.Entity.Licence.Dto
{
    public class LicenceRequestDto
    {
        public string FirmKey { get; set; }
        public string FirmName { get; set; }
        public string Ip { get; set; }

        //Kullanıcı rol ekleyen ve eklenen kişi
        public int? CreatedBy { get; set; }
        public string Model { get; set; }
    }
}
