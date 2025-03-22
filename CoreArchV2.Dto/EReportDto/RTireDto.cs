using CoreArchV2.Dto.ELogisticsDto;

namespace CoreArchV2.Dto.EReportDto
{
    public class RTireDto
    {
        public int InertCount { get; set; }
        public int DebitCount { get; set; }
        public int EmptyCount { get; set; }
        public int AllCount { get; set; }
        public List<ETireDto> TireList { get; set; }
        public List<ETireDto> TireAllList { get; set; }

    }
}
