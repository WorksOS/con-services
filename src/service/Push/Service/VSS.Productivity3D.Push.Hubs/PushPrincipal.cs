using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.Push.Hubs
  {
    /// <summary>
    /// Custom principal for Raptor with list of projects.
    /// </summary>
    public class PushPrincipal : TIDCustomPrincipal
    {
      private readonly IProjectProxy projectProxy;
      private readonly IDictionary<string, string> authNContext;

      public PushPrincipal(ClaimsIdentity identity, string customerUid, string customerName, string userEmail, bool isApplication, string tpaasApplicationName,
        IProjectProxy projectProxy, IDictionary<string, string> contextHeaders)
        : base(identity, customerUid, customerName, userEmail, isApplication, tpaasApplicationName)
      {
        this.projectProxy = projectProxy;
        authNContext = contextHeaders;
      }

      /// <summary>
      /// Get the project descriptor for the specified project uid.
      /// Also authenticates project availability and customer/project relationship
      /// </summary>
      public async Task<ProjectData> GetProject(string projectUid)
      {
        if (string.IsNullOrEmpty(projectUid))
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Missing project UID"));
        }

        var projectDescr = await projectProxy.GetProjectForCustomer(CustomerUid, projectUid, authNContext);
        if (projectDescr != null)
        {
          return projectDescr;
        }

        throw new ServiceException(HttpStatusCode.Unauthorized,
          new ContractExecutionResult(ContractExecutionStatesEnum.AuthError,
            $"Missing Project or project does not belong to specified customer or don't have access to the project {projectUid}"));
      }

    }
  }
