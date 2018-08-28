using VSS.TRex.Interfaces;
using VSS.TRex.Pipelines.Tasks.Interfaces;

namespace VSS.TRex.Pipelines.Interfaces.Tasks
{
  public interface IAggregatedPipelinedSubGridTask : ITask
  {
    ISubGridRequestsAggregator Aggregator { get; set; }
  }
}
