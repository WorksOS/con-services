using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Entitlements.Common.Models.Response
{
  public class EntitlementResponseModel : ContractExecutionResult
  {
    private EntitlementResponseModel()
    {
      
    }

    public static EntitlementResponseModel Ok(string message = null)
    {
      return new EntitlementResponseModel()
      {
        Message = message ?? DefaultMessage
      };
    }

    public static EntitlementResponseModel Failed(int code, string message)
    {
      return new EntitlementResponseModel() {Code = code, Message = message};
    }
  }
}
