using System.Threading;

namespace VSS.Hosted.VLCommon.ServiceOrchestration.Interfaces
{
  public interface ICancellationTokenControl : ICancellationTokenSource
  {
    void ResetTokenSourceInstance();
    CancellationTokenSource SourceInstance { get; }
  }
}