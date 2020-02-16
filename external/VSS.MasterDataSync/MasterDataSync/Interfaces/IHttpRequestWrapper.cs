using VSS.Nighthawk.MasterDataSync.Models;

namespace VSS.Nighthawk.MasterDataSync.Interfaces
{
  public interface IHttpRequestWrapper
  {
    ServiceResponseMessage RequestDispatcher(ServiceRequestMessage svcRequestMessage);
  }
}
