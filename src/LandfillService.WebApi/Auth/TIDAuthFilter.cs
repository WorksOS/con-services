using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using Common.Models;
using log4net;
using LandfillService.Common;
using Newtonsoft.Json;
using VSS.VisionLink.Utilization.WebApi.Configuration.Principal;
using VSS.VisionLink.Utilization.WebApi.Configuration.Principal.Models;

//using VSS.VisionLink.Utilization.WebApi.Configuration.Principal;
//using VSS.VisionLink.Utilization.WebApi.Configuration.Principal.Models;

namespace LandfillService.WebApi.Auth
{
  public class TIDAuthFilter : IAuthenticationFilter
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
    {
      if (ConfigurationManager.AppSettings["JWT"] != "Disabled")
      {
        //Passing the WebAPI Request headers to JWTHelper function to obtain the JWT Token
        var utils = new AuthUtilities();
        string message;
        string userUid;
        string jwt = string.Empty;
        var customer = utils.GetContext(context.Request.Headers, out message, out userUid, out jwt);
        Log.DebugFormat("Authorization: For userID {0} customer is {1}", userUid, customer?.CustomerUID);

        Dictionary<long, ProjectDescriptor> projectList;

        if (customer != null)
        {
          IEnumerable<Project> userProjects = utils.GetLandfillProjectsForUser(userUid);
          projectList = new Dictionary<long, ProjectDescriptor>();
          
          foreach (var userProject in userProjects)
          {
            projectList.Add(userProject.LegacyProjectID,
              new ProjectDescriptor
              {
                isLandFill = true,
                isArchived = userProject.IsDeleted || userProject.SubscriptionEndDate < DateTime.UtcNow
              });
          }
          Log.DebugFormat("Authorization: for Customer: {0} projectList is: {1}", customer, projectList.ToString());

          context.Principal = new LandfillPrincipal(projectList, userUid, customer.CustomerUID.ToString(), jwt);
          LoggerSvc.LogMessage("Principal", "BuildPrincipal", "Claims", JsonConvert.SerializeObject(projectList));
        }
        else
        {
          projectList = new Dictionary<long, ProjectDescriptor>();
          context.Principal = new LandfillPrincipal(projectList, userUid, Guid.Empty.ToString());
        }
      }
      return Task.FromResult(0);
    }

    public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
    {
      var challenge = new AuthenticationHeaderValue("JWT");
      context.Result = new AddChallengeOnUnauthorizedResult(challenge, context.Result);
      return Task.FromResult(0);
    }


    public bool AllowMultiple { get; private set; }
  }

  public class LandfillPrincipal : ILandfillPrincipal
  {
    public bool IsInRole(string role)
    {
      return false;
    }

    public LandfillPrincipal()
    {
      Identity = new GenericIdentity("LandfillUser");
    }

    public LandfillPrincipal(Dictionary<long, ProjectDescriptor> projects, string userUid, string customerUid) : this()
    {
      Projects = projects;
      UserUid = userUid;
      CustomerUid = customerUid;
    }

    public LandfillPrincipal(Dictionary<long, ProjectDescriptor> projects, string userUid, string customerUid,
      string jwt) : this()
    {
      Projects = projects;
      UserUid = userUid;
      CustomerUid = customerUid;
      JWT = jwt;
    }

    public string UserUid { get; private set; }
    public string CustomerUid { get; private set; }
    public string JWT { get; private set; }

    public IIdentity Identity { get; private set; }

    public Dictionary<long, ProjectDescriptor> Projects { get; private set; }

  }

  internal interface ILandfillPrincipal : IPrincipal
  {
    Dictionary<long, ProjectDescriptor> Projects { get; }
    string UserUid { get; }
    string CustomerUid { get; }
    string JWT { get; }
  }
}