using System.Net;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace CCSS.TagFileSplitter.WebAPI.Common.Models
{
  public class TargetService
  {
    // this is the name to find url in configStore
    // for our (CCSS) 3dpm service, this will be == ServiceNameConstants.PRODUCTIVITY3D_SERVICE
    //     this name is significant for determining if target is to get TMC direct events
    // for the VSS 3dpm service, this MUST be == ServiceNameConstants.PRODUCTIVITY3D_VSS_SERVICE
    //     this name is significant to tagFileHarvester
    public string ServiceName { get; }

    // apiVersion is an enum inside service discovery to format url (will always be public)
    private string StringApiVersion { get; }

    public ApiVersion TargetApiVersion { get; set; }

    public string AutoRoute { get; }
    public string DirectRoute { get; }

    public TargetService(string serviceName, string apiVersion, string autoRoute, string directRoute)
    {
      ServiceName = serviceName;
      StringApiVersion = apiVersion;
      AutoRoute = autoRoute;
      DirectRoute = directRoute;
    }

    public void Validate()
    {
      if (string.IsNullOrEmpty(ServiceName))
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Service name missing"));

      if (string.IsNullOrEmpty(StringApiVersion))
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Api version missing"));

      if (System.Enum.IsDefined(typeof(ApiVersion), StringApiVersion.ToUpper()))
      {
        System.Enum.TryParse<ApiVersion>(StringApiVersion.ToUpper(), out var value);
        TargetApiVersion = value;
      }
      else
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Api version invalid"));

      if (string.IsNullOrEmpty(AutoRoute))
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Auto route missing"));

      if (string.IsNullOrEmpty(DirectRoute))
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Direct route missing"));
    }
  }
}
