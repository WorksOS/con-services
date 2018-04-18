using System.Net;
using Newtonsoft.Json;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;

namespace VSS.MasterData.Project.WebAPI.Common.Models
{
  
  /// <summary>
  /// The request representation used to Validate a TCC organization for a customer. 
  /// </summary>
  public class ValidateTccAuthorizationRequest
  {
    protected static ContractExecutionStatesEnum ContractExecutionStatesEnum = new ContractExecutionStatesEnum();
    
    /// <summary>
    /// this relates to tcc's filespace.orgShortName
    /// </summary>
    [JsonProperty(PropertyName = "organization", Required = Required.Always)]
    public string OrgShortName { get; set; }


    /// <summary>
    /// Private constructor
    /// </summary>
    private ValidateTccAuthorizationRequest()
    {
    }

    /// <summary>
    /// Create instance of ValidateTccAuthorizationRequest
    /// </summary>
    public static ValidateTccAuthorizationRequest CreateValidateTccAuthorizationRequest(string orgShortName)
    {
      return new ValidateTccAuthorizationRequest
      {
        OrgShortName = orgShortName
      };
    }

    public void Validate()
    {
      if (string.IsNullOrEmpty(OrgShortName) )
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.GetErrorNumberwithOffset(86),
            string.Format(ContractExecutionStatesEnum.FirstNameWithOffset(86), OrgShortName)));
      }
    }

  }
}
