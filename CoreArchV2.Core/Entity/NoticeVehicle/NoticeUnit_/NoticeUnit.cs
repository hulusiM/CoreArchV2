using CoreArchV2.Core.Entity.Common;

namespace CoreArchV2.Core.Entity.NoticeVehicle.NoticeUnit_
{
    public class NoticeUnit : Base
    {
        public int? DeletedBy { get; set; }
        public string DeleteDescription { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int NoticeType { get; set; }
        public string Description { get; set; }
        public DateTime OpenDate { get; set; }//talep açılma tarihi
        public DateTime? ClosedDate { get; set; }//talep kapanma tarihi
        public int State { get; set; }
    }
}
