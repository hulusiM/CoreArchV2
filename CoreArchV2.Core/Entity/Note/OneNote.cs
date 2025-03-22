using CoreArchV2.Core.Entity.Common;

namespace CoreArchV2.Core.Entity.Note
{
    public class OneNote : Base
    {
        public int VehicleId { get; set; }
        //public string Header { get; set; }
        public string? Description { get; set; }
        public int ImportanceLevel { get; set; }
        public int Type { get; set; }
    }
}
