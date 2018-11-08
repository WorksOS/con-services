using Apache.Ignite.Core.Cache.Configuration;

namespace VSS.TRex.GridFabric.Interfaces
{
  public interface ISegmentRetirementQueueKey : IProjectAffinity
  {
    [QuerySqlField(IsIndexed = true)]
    long InsertUTCAsLong { get; set; }

    string ToString();
  }
}
