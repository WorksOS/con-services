using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.Productivity3D.Filter.Common.Filters.Authentication
{
  /// <summary>
  /// Custom principal for 3dpm filter service
  /// </summary>
  public class FilterPrincipal : TIDCustomPrincipal
  {
    private readonly IProjectListProxy ProjectProxy;
    private readonly IDictionary<string, string> ContextHeaders;

    /// <inheritdoc />
    /// <summary>
    /// Initializes a new instance of the <see cref="T:VSS.Productivity3D.Filter.WebApi.Filters.FilterPrincipal" /> class.
    /// </summary>
    /// <param name="identity">The identity.</param>
    /// <param name="customerUid">The customer uid.</param>
    /// <param name="customerName"></param>
    /// <param name="userEmail">The user email address or application.</param>
    /// <param name="isApplication">if set to <c>true</c> [is application].</param>
    /// <param name="projectProxy">Project proxy to use</param>
    /// <param name="contextHeaders">HTTP request context headers</param>
    public FilterPrincipal(ClaimsIdentity identity, string customerUid, string customerName, string userEmail, bool isApplication,
      IProjectListProxy projectProxy, IDictionary<string, string> contextHeaders)
      : base(identity, customerUid, customerName, userEmail, isApplication)
    {
      ProjectProxy = projectProxy;
      ContextHeaders = contextHeaders;
    }

    /// <summary>
    /// Get the project descriptor for the specified project id.
    /// </summary>
    /// <param name="projectUid">The project ID</param>
    /// <returns>Project descriptor</returns>
    public async Task<ProjectData> GetProject(string projectUid)
    {
      var project = await ProjectProxy.GetProjectForCustomer(CustomerUid, projectUid, ContextHeaders);

      if (project != null) { return project; }
  
      throw new ServiceException(HttpStatusCode.Unauthorized,
          new ContractExecutionResult(ContractExecutionStatesEnum.AuthError,
            $"Missing Project or project does not belong to customer {CustomerUid}:{UserEmail} or don't have access to the project {projectUid}"));
    }

  }
}