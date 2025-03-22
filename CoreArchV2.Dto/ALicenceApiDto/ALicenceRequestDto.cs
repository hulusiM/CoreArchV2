namespace CoreArchV2.Dto.ALicenceApiDto
{
    public class ALicenceRequestDto
    {
        public string FirmKey { get; set; }
        public string FirmName { get; set; }
        public string Ip { get; set; }

        //Kullanıcı rol ekleyen ve eklenen kişi
        public int? CreatedBy { get; set; }
        public string Model { get; set; }
    }
}
