using System.Threading;
using System.Threading.Tasks;

namespace VSS.VisionLink.Landfill.MDM.Interfaces
{
  public interface IKafkaQueuePuller
  {
    Task<bool> PullAndProcess(CancellationToken token);
  }
}