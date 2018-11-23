using VSS.TRex.Interfaces;

namespace VSS.TRex.Pipelines.Interfaces.Tasks
{
  public interface IAggregatedPipelinedSubGridTask : ITRexTask
  {
    ISubGridRequestsAggregator Aggregator { get; set; }
  }
}
