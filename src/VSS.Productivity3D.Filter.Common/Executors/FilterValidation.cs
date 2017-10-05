using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.Productivity3D.Filter.Common.Executors
{
  public class FilterValidation
  {
    /// <summary>
    /// Validates a customer-project relationship, and projectUid.
    /// </summary>
    /// <param name="customHeaders"></param>
    /// <param name="customerUid"></param>
    /// <param name="projectUid">The project uid.</param>
    /// <param name="projectListProxy"></param>
    /// <param name="log"></param>
    /// <param name="serviceExceptionHandler"></param>
    /// <returns></returns>
    public static async Task ValidateProjectForCustomer(IProjectListProxy projectListProxy,
      ILogger log, IServiceExceptionHandler serviceExceptionHandler, IDictionary<string, string> customHeaders,
      string customerUid, string projectUid)
    {
      ProjectData project = null;
      try
      {
        project = (await projectListProxy.GetProjectsV4(customerUid, customHeaders).ConfigureAwait(false))
          .SingleOrDefault(p => p.ProjectUid == projectUid);
      }
      catch (Exception e)
      {
        log.LogError(
          $"ValidateProjectForCustomer: projectListProxy.GetProjectsV4 failed with exception. customerUid:{customerUid} projectUid:{projectUid}. Exception Thrown: {e.Message}. ");
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 7, e.Message);
      }

      if (project == null)
      {
        log.LogInformation(
          $"ValidateProjectForCustomer: projectListProxy: customerUid:{customerUid} projectUid:{projectUid}. returned no project match");
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 8);
      }

      log.LogInformation(
        $"ValidateProjectForCustomer: succeeded: customerUid:{customerUid} projectUid:{projectUid}.");
    }

    /// <summary>
    /// Notify raptor of an updated/deleted filterUid.
    ///    Note that it does not notify of a created filterUid.
    /// </summary>
    public static async Task NotifyRaptorFilterChange(IRaptorProxy raptorProxy,
      ILogger log, IServiceExceptionHandler serviceExceptionHandler, 
      string filterUid, string projectUid)
    {
      BaseDataResult notificationResult = null;
      try
      {
        notificationResult = await raptorProxy.NotifyFilterChange(new Guid(filterUid), new Guid(projectUid));
      }
      catch (Exception e)
      {
        log.LogError(
          $"NotifyRaptorFilterChange: RaptorServices failed with exception. filterUid:{filterUid} projectUid:{projectUid}. Exception Thrown: {e.Message}. ");
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 30, "raptorProxy.NotifyFilterChange", e.Message);
      }

      log.LogDebug(
        $"NotifyRaptorFilterChange: NotifyFilterChange in RaptorServices returned code: {notificationResult?.Code ?? -1} Message {notificationResult?.Message ?? "notificationResult == null"}.");

      if (notificationResult != null && notificationResult.Code != 0)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 29, notificationResult.Code.ToString(), notificationResult.Message);
    }

  }
}