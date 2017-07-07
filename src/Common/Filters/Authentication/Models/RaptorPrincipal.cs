using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using VSS.Productivity3D.Common.Contracts;
using VSS.Productivity3D.Common.ResultHandling;

namespace VSS.Productivity3D.Common.Filters.Authentication.Models
{
  /// <summary>
  /// Custom principal for Raptor with list of projects.
  /// </summary>
  public class RaptorPrincipal : ClaimsPrincipal
  {
    public RaptorPrincipal(ClaimsIdentity identity, string customerUid, List<ProjectDescriptor> projects, bool isApplication = false) : base(identity)
    {
      CustomerUid = customerUid;
      Projects = projects;
      this.isApplication = isApplication;
    }

    public string CustomerUid { get; }

    public List<ProjectDescriptor> Projects { get; }

    public bool isApplication { get; private set; } = false;

    /// <summary>
    /// Get the project descriptor for the specified project id.
    /// </summary>
    /// <param name="projectId">The project ID</param>
    /// <returns>Project descriptor</returns>
    public ProjectDescriptor GetProject(long projectId)
    {
      var projectDescr = Projects.Where(p => p.projectId == projectId).FirstOrDefault();
      if (projectDescr == null)
      {
        throw new ServiceException(HttpStatusCode.Unauthorized,
          new ContractExecutionResult(ContractExecutionStatesEnum.AuthError, 
          string.Format("Missing Project or project does not belong to specified customer or don't have access to the project {0}", projectId)));
      }
      return projectDescr;
    }

    /// <summary>
    ///  Get the project descriptor for the specified project uid.
    /// </summary>
    /// <param name="projectUid">THe project UID</param>
    /// <returns>Project descriptor</returns>
    public ProjectDescriptor GetProject(Guid? projectUid)
    {
      if (!projectUid.HasValue)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Missing project UID"));
      }
      return GetProject(projectUid.ToString());
    }

    /// <summary>
    ///  Get the project descriptor for the specified project uid.
    /// </summary>
    /// <param name="projectUid">THe project UID</param>
    /// <returns>Project descriptor</returns>
    public ProjectDescriptor GetProject(string projectUid)
    {
      if (string.IsNullOrEmpty(projectUid))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Missing project UID"));
      }
      var projectDescr = Projects.Where(p => string.Equals(p.projectUid, projectUid, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
      if (projectDescr == null)
      {
        throw new ServiceException(HttpStatusCode.Unauthorized,
          new ContractExecutionResult(ContractExecutionStatesEnum.AuthError, 
          string.Format("Missing Project or project does not belong to specified customer or don't have access to the project {0}", projectUid)));
      }
      return projectDescr;
    }

    /// <summary>
    /// Gets the legacy project id for the specified project uid
    /// </summary>
    /// <param name="projectUid">THe project UID</param>
    /// <returns>Legacy project ID</returns>
    public long GetProjectId(Guid? projectUid)
    {
      var projectDescr = GetProject(projectUid);
      long projectId = projectDescr.projectId;
      if (projectId <= 0)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.AuthError, "Missing project ID"));
      }
      return projectId;
    }

  }
}
