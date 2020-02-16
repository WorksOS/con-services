using System.Threading;

namespace VSS.Hosted.VLCommon.ServiceOrchestration.Interfaces
{
  public interface ICancellationTokenSource
  {
    CancellationToken Token { get; }
  }
}