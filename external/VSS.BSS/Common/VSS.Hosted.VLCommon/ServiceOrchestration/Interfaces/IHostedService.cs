namespace VSS.Hosted.VLCommon.ServiceOrchestration.Interfaces
{
  public interface IHostedService
  {
    void Start();
    void Stop();
	  string GetName();
  }
}
