using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
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
    public static async Task ValidateCustomerProject(IProjectListProxy projectListProxy,
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
          $"ValidateCustomerProject: projectListProxy.GetProjectsV4 failed with exception. customerUid:{customerUid} projectUid:{projectUid}. Exception Thrown: {e.Message}. ");
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 7, e.Message);
      }

      if (project == null)
      {
        log.LogInformation(
          $"ValidateCustomerProject: projectListProxy: customerUid:{customerUid} projectUid:{projectUid}. returned no project match");
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 8);
      }

      log.LogInformation(
        $"ValidateCustomerProject: succeeded: customerUid:{customerUid} projectUid:{projectUid}.");
    }

  }
}