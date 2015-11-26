using System.Threading;
using System.Threading.Tasks;

namespace VSS.VisionLink.Utilization.DataFeed.Interfaces
{
  public interface IKafkaQueuePuller
  {
    Task<bool> PullAndProcess(CancellationToken token);
  }
}