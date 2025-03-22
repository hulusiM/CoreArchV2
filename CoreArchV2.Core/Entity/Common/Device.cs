namespace CoreArchV2.Core.Entity.Common
{
    public class Device : Base
    {
        public int? UserId { get; set; }
        public string? SystemName { get; set; }
        public string? SystemVersion { get; set; }
        public string? AppVersion { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public string? PushToken { get; set; }

    }
}
