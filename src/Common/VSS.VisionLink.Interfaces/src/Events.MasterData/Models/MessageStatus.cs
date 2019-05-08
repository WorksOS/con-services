namespace VSS.VisionLink.Interfaces.Events.MasterData.Models
{
    public enum MessageStatus
    {
        Unknown = -1,
        Pending = 0,
        Sent = 1,
        Acknowledged = 2,
        Suppressed = 3,
    }
}
