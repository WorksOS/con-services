using System;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VSS.MasterData.Repositories;
using VSS.Common.Exceptions;

namespace VSS.Productivity3D.Filter.Common.Executors
{
  public class FilterValidation
  {
    /// <summary>
    /// Validates a project identifier.
    /// </summary>
    /// <param name="projectUid">The project uid.</param>
    /// <returns></returns>
    public static async Task ValidateProjectUid(IProjectRepository projectRepo,
      ILogger log,
      IServiceExceptionHandler serviceExceptionHandler, string customerUid, string projectUid)
    {
      var project =
        (await projectRepo.GetProjectsForCustomer(customerUid).ConfigureAwait(false)).FirstOrDefault(
          p => string.Equals(p.ProjectUID, projectUid, StringComparison.OrdinalIgnoreCase));
      if (project == null)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 1);
      }

      log.LogInformation($"projectUid {projectUid} validated");
    }
  }
}