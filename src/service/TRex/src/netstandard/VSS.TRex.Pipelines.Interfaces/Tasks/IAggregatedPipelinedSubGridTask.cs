using VSS.TRex.Interfaces;

namespace VSS.TRex.Pipelines.Interfaces.Tasks
{
  public interface IAggregatedPipelinedSubGridTask : ITask
  {
    ISubGridRequestsAggregator Aggregator { get; set; }
  }
}
