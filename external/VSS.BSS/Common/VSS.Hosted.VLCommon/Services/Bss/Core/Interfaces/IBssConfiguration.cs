namespace VSS.Hosted.VLCommon.Bss
{
  public interface IBssConfiguration
  {
    string ToEmailAddress { get; }
    string FromEmailAddress { get; }
    int MessageQueueFailedCountMaximum { get; }
  }
}