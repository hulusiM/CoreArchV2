using CoreArchV2.Core.Entity.Common;

namespace CoreArchV2.Core.Entity.TripVehicle
{
    public class TripLog : Base
    {
        public int TripId { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal? Km { get; set; }
        public int? CityId { get; set; }
        public string? Description { get; set; }
        public int? Type { get; set; }
        public int State { get; set; }
    }
}
