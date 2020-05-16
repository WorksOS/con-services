using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.Filter.Common.Filters.Authentication
{
  /// <summary>
  /// Custom principal for 3dpm filter service
  /// </summary>
  public class FilterPrincipal : TIDCustomPrincipal
  {
    private readonly IProjectProxy _projectProxy;
    private readonly IHeaderDictionary _contextHeaders;

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
      IProjectProxy projectProxy, IHeaderDictionary contextHeaders)
      : base(identity, customerUid, customerName, userEmail, isApplication)
    {
      _projectProxy = projectProxy;
      _contextHeaders = contextHeaders;
    }

    /// <summary>
    /// Get the project descriptor for the specified project id.
    /// </summary>
    /// <param name="projectUid">The project ID</param>
    /// <returns>Project descriptor</returns>
    public async Task<ProjectData> GetProject(string projectUid)
    {
      var project = await _projectProxy.GetProjectForCustomer(CustomerUid, projectUid, _contextHeaders);

      if (project != null) { return project; }

      throw new ServiceException(HttpStatusCode.Unauthorized,
          new ContractExecutionResult(ContractExecutionStatesEnum.AuthError,
            $"Missing Project or project does not belong to customer {CustomerUid}:{UserEmail} or don't have access to the project {projectUid}"));
    }
  }
}
