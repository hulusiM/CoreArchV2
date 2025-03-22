using CoreArchV2.Core.Entity.Common;

namespace CoreArchV2.Core.Entity.Logistics
{
    public class VehicleExaminationDate : Base
    {
        public int VehicleId { get; set; }
        public DateTime? KDocumentEndDate { get; set; }
        public DateTime? KaskoEndDate { get; set; }
        public DateTime? TrafficEndDate { get; set; }
        public DateTime? ExaminationEndDate { get; set; }
    }
}