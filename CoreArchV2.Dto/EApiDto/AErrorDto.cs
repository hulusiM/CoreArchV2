namespace CoreArchV2.Dto.EApiDto
{
    public class AErrorDto
    {
        public AErrorDto()
        {
            Errors = new List<string>();
        }

        public int Status { get; set; }
        public List<string> Errors { get; set; }
    }
}