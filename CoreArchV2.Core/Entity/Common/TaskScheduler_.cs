namespace CoreArchV2.Core.Entity.Common
{
    public class TaskScheduler_
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int? TypeId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? LastRunDate { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
