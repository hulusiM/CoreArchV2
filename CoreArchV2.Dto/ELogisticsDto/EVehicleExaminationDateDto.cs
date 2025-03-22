namespace CoreArchV2.Dto.ELogisticsDto
{
    public class EVehicleExaminationDateDto
    {
        public int VehicleId { get; set; }
        public string Plate { get; set; }
        public string Name { get; set; }
        public string KDocumentEndDate { get; set; }
        public string KaskoEndDate { get; set; }
        public string TrafficEndDate { get; set; }
        public string ExaminationEndDate { get; set; }
        public string Button { get; set; } = "";
    }
}
