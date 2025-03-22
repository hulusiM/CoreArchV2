namespace CoreArchV2.Dto.ECommonDto
{
    public class ESelect2ResultDto
    {
        public List<EUnitDto> ComboList { get; set; }
        public bool IsSuccess { get; set; } = true;
        public string Message { get; set; }
    }
}
