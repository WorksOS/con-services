using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Common.Filters.Authentication.Models
{
  /// <summary>
  /// Custom principal for Raptor with list of projects.
  /// </summary>
  public class RaptorPrincipal : ClaimsPrincipal
  {
    private readonly IProjectListProxy projectProxy;
    private readonly IDictionary<string, string> authNContext;

    //We need to delegate Project retrieval downstream as project may not accessible to a user once it has been created
    public RaptorPrincipal(ClaimsIdentity identity, string customerUid,
      string username, string customername, IProjectListProxy projectProxy, IDictionary<string, string> contextHeaders, bool isApplication = false) : base(identity)
    {
      CustomerUid = customerUid;
      IsApplication = isApplication;
      UserEmail = username;
      CustomerName = customername;
      this.projectProxy = projectProxy;
      authNContext = contextHeaders;
    }

    public string CustomerUid { get; }

    public string UserEmail { get; }

    public string CustomerName { get; }

    public bool IsApplication { get; }

    /// <summary>
    /// Get the project descriptor for the specified project id.
    /// </summary>
    public ProjectData GetProject(long projectId)
    {
      var projectDescr = projectProxy.GetProjectForCustomer(CustomerUid, projectId, authNContext).Result;
      if (projectDescr != null) return projectDescr;

      throw new ServiceException(HttpStatusCode.Unauthorized,
          new ContractExecutionResult(ContractExecutionStatesEnum.AuthError,
            $"Missing Project or project does not belong to specified customer or don't have access to the project {projectId}"));
    }

    /// <summary>
    /// Get the project descriptor for the specified project uid.
    /// </summary>
    public ProjectData GetProject(Guid? projectUid)
    {
      if (!projectUid.HasValue)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Missing project UID"));
      }

      return GetProject(projectUid.ToString());
    }

    /// <summary>
    /// Get the project descriptor for the specified project uid.
    /// </summary>
    public ProjectData GetProject(string projectUid)
    {
      if (string.IsNullOrEmpty(projectUid))
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Missing project UID"));

      var projectDescr = projectProxy.GetProjectForCustomer(CustomerUid, projectUid, authNContext).Result;
      if (projectDescr != null) return projectDescr;

      throw new ServiceException(HttpStatusCode.Unauthorized,
        new ContractExecutionResult(ContractExecutionStatesEnum.AuthError,
          $"Missing Project or project does not belong to specified customer or don't have access to the project {projectUid}"));
    }

    /// <summary>
    /// Gets the legacy Project Id (long) from a ProjectUid (Guid).
    /// </summary>
    public long GetLegacyProjectId(Guid? projectUid)
    {
      if (!(this is RaptorPrincipal _))
      {
        throw new ArgumentException("Incorrect request context principal.");
      }

      var projectId = GetProject(projectUid).LegacyProjectId;
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