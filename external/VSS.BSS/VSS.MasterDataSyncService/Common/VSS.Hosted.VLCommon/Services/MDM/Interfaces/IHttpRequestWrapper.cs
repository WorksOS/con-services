using VSS.Hosted.VLCommon.Services.MDM.Models;


namespace VSS.Hosted.VLCommon.Services.MDM.Interfaces
{
  public interface IHttpRequestWrapper
  {
    ServiceResponseMessage RequestDispatcher(ServiceRequestMessage svcRequestMessage);
  }
}
