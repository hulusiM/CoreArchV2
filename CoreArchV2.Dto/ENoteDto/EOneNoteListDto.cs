namespace CoreArchV2.Dto.ENoteDto
{
    public class EOneNoteListDto
    {
        public List<EOneNoteDto> ToDo { get; set; }
        public List<EOneNoteDto> Process { get; set; }
        public List<EOneNoteDto> Finished { get; set; }
    }
}
