using CoreArchV2.Core.Entity.Common;

namespace CoreArchV2.Core.Entity.NoticeVehicle.NoticeUnit_
{
    public class NoticeUnitHistory : Base
    {
        public int NoticeUnitId { get; set; }
        public string Description { get; set; }
        public int? ToUnitId { get; set; }
        public int State { get; set; }
    }
}
