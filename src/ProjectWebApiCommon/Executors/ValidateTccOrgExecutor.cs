using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;
using VSS.TCCFileAccess.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Executors
{
  /// <summary>
  /// The executor which validates the tcc orgShortName received
  ///   Validates against TCC and the ProjectService database.
  /// </summary>
  public class ValidateTccOrgExecutor : RequestExecutorContainer
  {

    /// <summary>
    /// Processes the ValidateTcc orgShortName against customerUid
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a ContractExecutionResult if successful</returns>     
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      ValidateTccAuthorizationRequest validateTccAuthorizationRequest = item as ValidateTccAuthorizationRequest;
      if (validateTccAuthorizationRequest == null)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 86);
      }

      try
      {
        var organizations = await fileRepo.ListOrganizations();
        var tccOrganization
          = organizations.FirstOrDefault(o => o.shortName == validateTccAuthorizationRequest.OrgShortName);
        if (tccOrganization == null)
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 88);
        }
        else if (string.IsNullOrEmpty(tccOrganization.filespaceId) || string.IsNullOrEmpty(tccOrganization.orgId))
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 89,
            JsonConvert.SerializeObject(tccOrganization));
        }
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 87, e.Message);
      }

      return new ContractExecutionResult();
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }

    protected override void ProcessErrorCodes()
    {
    }
  }
}