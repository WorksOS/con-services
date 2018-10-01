namespace VSS.TRex.GridFabric.Interfaces
{
  public interface ISegmentRetirementQueueKey : IProjectAffinity
  {
    long InsetUTCasLong { get; set; }

    string ToString();
  }
}
