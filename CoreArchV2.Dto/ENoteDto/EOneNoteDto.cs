using CoreArchV2.Dto.ECommonDto;

namespace CoreArchV2.Dto.ENoteDto
{
    public class EOneNoteDto : EIdDto
    {
        public int VehicleId { get; set; }
        public string Plate { get; set; }
        public string Description { get; set; }
        public string ImportanceLevel { get; set; }
        public int Type { get; set; }
        public string NameSurname { get; set; }
    }
}
