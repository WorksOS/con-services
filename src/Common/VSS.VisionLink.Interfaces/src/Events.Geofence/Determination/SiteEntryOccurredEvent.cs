using VSS.VisionLink.Interfaces.Events.Geofence.Context;

namespace VSS.VisionLink.Interfaces.Events.Geofence.Determination
{
    public class SiteEntryOccurredEvent
    {
        public AssetDetail Asset { get; set; }
        public TimestampDetail Timestamp { get; set; }

        public string SiteUid { get; set; }
        public string SiteName { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
