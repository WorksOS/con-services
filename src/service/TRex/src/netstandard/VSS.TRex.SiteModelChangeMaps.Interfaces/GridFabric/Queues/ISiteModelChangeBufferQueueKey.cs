using VSS.TRex.GridFabric.Interfaces;

namespace VSS.TRex.SiteModelChangeMaps.Interfaces.GridFabric.Queues
{
  public interface ISiteModelChangeBufferQueueKey : IProjectAffinity
  {
    /// <summary>
    /// The UTC date at which this element was added, expressed in ticks
    /// </summary>
    long InsertUTCTicks { get; set; }
  }
}
