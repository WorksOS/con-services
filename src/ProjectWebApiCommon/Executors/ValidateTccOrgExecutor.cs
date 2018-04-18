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
      else
      {
        Organization tccOrganization = null;

        try
        {
          var organizations = await fileRepo.ListOrganizations();
          tccOrganization
            = (from o in organizations
              where o.shortName == validateTccAuthorizationRequest.OrgShortName
              select o)
            .First();
          if (tccOrganization == null)
          {
            serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 88);
          }
          else if(string.IsNullOrEmpty(tccOrganization.filespaceId) || string.IsNullOrEmpty(tccOrganization.orgId))
          {
            serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 89, JsonConvert.SerializeObject(tccOrganization));
          }
        }
        catch (Exception e)
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 87, e.Message);
        }

        // Nice, but probably overkill and not done in CGen. Potentially needed for TagFileprocessing. 
        //try
        //{
        //  var customerTccOrg = await customerRepo.GetCustomerWithTccOrg(Guid.Parse(customerUid));
        //  if (customerTccOrg == null || !string.Equals(customerTccOrg.TCCOrgID, tccOrganization.orgId, StringComparison.OrdinalIgnoreCase))
        //  {
        //    serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 91);
        //  }
        //}
        //catch (Exception e)
        //{
        //  serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 90, e.Message);
        //}
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