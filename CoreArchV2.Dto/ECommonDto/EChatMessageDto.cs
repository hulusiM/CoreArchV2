namespace CoreArchV2.Dto.ECommonDto
{
    public class EChatMessageDto
    {
        public int Id { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? UserId { get; set; }
        public string UserNameSurname { get; set; }
        public string Message { get; set; }
    }
}