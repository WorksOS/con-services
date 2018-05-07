using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.Common.Filters.Authentication.Models
{
  /// <summary>
  /// Custom principal for Raptor with list of projects.
  /// </summary>
  public class RaptorPrincipal : TIDCustomPrincipal
  {
    private readonly IProjectListProxy projectProxy;
    private readonly IDictionary<string, string> authNContext;

    //We need to delegate Project retrieval downstream as project may not accessible to a user once it has been created
    public RaptorPrincipal(ClaimsIdentity identity, string customerUid, string customerName, string userEmail, bool isApplication,
      IProjectListProxy projectProxy, IDictionary<string, string> contextHeaders) 
      : base(identity, customerUid, customerName, userEmail, isApplication)
    {
      this.projectProxy = projectProxy;
      authNContext = contextHeaders;
    }

    /// <summary>
    /// Get the project descriptor for the specified project id.
    /// </summary>
    public async Task<ProjectData> GetProject(long projectId)
    {
      var projectDescr = await projectProxy.GetProjectForCustomer(CustomerUid, projectId, authNContext);
      if (projectDescr != null) return projectDescr;

      throw new ServiceException(HttpStatusCode.Unauthorized,
          new ContractExecutionResult(ContractExecutionStatesEnum.AuthError,
            $"Missing Project or project does not belong to specified customer or don't have access to the project {projectId}"));
    }

    /// <summary>
    /// Get the project descriptor for the specified project uid.
    /// </summary>
    public async Task<ProjectData> GetProject(Guid? projectUid)
    {
      if (!projectUid.HasValue)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Missing project UID"));
      }

      return await GetProject(projectUid.ToString());
    }

    /// <summary>
    /// Get the project descriptor for the specified project uid.
    /// </summary>
    public async Task<ProjectData> GetProject(string projectUid)
    {
      if (string.IsNullOrEmpty(projectUid))
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Missing project UID"));

      var projectDescr = await projectProxy.GetProjectForCustomer(CustomerUid, projectUid, authNContext);
      if (projectDescr != null) return projectDescr;

      throw new ServiceException(HttpStatusCode.Unauthorized,
        new ContractExecutionResult(ContractExecutionStatesEnum.AuthError,
          $"Missing Project or project does not belong to specified customer or don't have access to the project {projectUid}"));
    }

    /// <summary>
    /// Gets the legacy Project Id (long) from a ProjectUid (Guid).
    /// </summary>
    public async Task<long> GetLegacyProjectId(Guid? projectUid)
    {
      if (!(this is RaptorPrincipal _))
      {
        throw new ArgumentException("Incorrect request context principal.");
      }

      var project = await GetProject(projectUid);
      var projectId = project.LegacyProjectId;
      if (projectId > 0)
      {
        return projectId;
      }

      throw new ServiceException(
        HttpStatusCode.BadRequest,
        new ContractExecutionResult(ContractExecutionStatesEnum.AuthError, "Missing project ID"));
    }
  }
}
