namespace CoreArchV2.Dto.ECommonDto
{
    public class EResultDto
    {
        public int? Id { get; set; } = 0;
        public int[] Ids { get; set; }
        public string[] IdNames { get; set; }
        public bool IsSuccess { get; set; } = true; //Değiştirme
        public string Message { get; set; }
    }
}