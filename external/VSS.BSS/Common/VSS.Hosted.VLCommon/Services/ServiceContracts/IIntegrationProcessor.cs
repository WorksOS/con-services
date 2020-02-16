using System.ServiceModel;
using VSS.Hosted.VLCommon.ServiceContracts;

namespace VSS.Hosted.VLCommon
{
  [ServiceContract(Namespace = ContractConstants.IntegrationNS)]
  public interface IIntegrationProcessor
  {
    [OperationContract(IsOneWay = true)]
    void Process(ByteArrayMessageWrapper message);
  }
}
