using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using log4net;
using LandfillService.Common;
using Newtonsoft.Json;
using VSS.Customer.Data;
using VSS.Subscription.Data;
using VSS.Subscription.Data.Models;
using VSS.Project.Data;
using VSS.UserCustomer.Data;
using VSS.VisionLink.Utilization.WebApi.Configuration.Principal;
using VSS.VisionLink.Utilization.WebApi.Configuration.Principal.Models;


namespace VSS.VisionLink.Utilization.WebApi.Configuration
{
  public class TIDAuthFilter : IAuthenticationFilter
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public async Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
    {
      string jwtToken;

      if (ConfigurationManager.AppSettings["JWT"] != "Disabled")
      {
        //Passing the WebAPI Request headers to JWTHelper function to obtain the JWT Token
        var utils = new AuthUtilities(new MySqlCustomerRepository(), new MySqlSubscriptionRepository(),
            new MySqlProjectRepository(), new MySqlUserCustomerRepository());
        string message = string.Empty;
        string userUid = string.Empty;
        var customer = utils.GetContext(context.Request.Headers, out message, out userUid);
        Log.DebugFormat("Authorization: For userID {0} customer is {1}", userUid, customer);

        Dictionary<long, ProjectDescriptor> projectList = new Dictionary<long, ProjectDescriptor>();
        IEnumerable<VSS.Project.Data.Models.Project> userProjects;

        if (customer != null)
        {
          userProjects = utils.GetProjectsForUser(userUid);
          projectList = new Dictionary<long, ProjectDescriptor>();
          foreach (var userProject in userProjects)
          {
            projectList.Add(userProject.projectId,
              new ProjectDescriptor
              {
                isLandFill = true,
                isArchived = userProject.isDeleted || userProject.subEndDate < DateTime.UtcNow
              });
          }
          Log.DebugFormat("Authorization: for Customer: {0} projectList is: {1}", customer, projectList.ToString());

          context.Principal = new LandfillPrincipal(projectList, userUid, customer.CustomerUID.ToString());
          LoggerSvc.LogMessage("Principal", "BuildPrincipal", "Claims", JsonConvert.SerializeObject(projectList));
        }
        else
        {
          projectList = new Dictionary<long, ProjectDescriptor>();
          context.Principal = new LandfillPrincipal(projectList, userUid, Guid.Empty.ToString());
          //context.ErrorResult = new AuthenticationFailureResult(message, context.Request);
        }
      }
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
      //Identity = new GenericIdentity("LandfillUser");

      Projects = projects;
      UserUid = userUid;
      CustomerUid = customerUid;
    }

    public string UserUid { get; private set; }
    public string CustomerUid { get; private set; }

    public IIdentity Identity { get; private set; }

    public Dictionary<long, ProjectDescriptor> Projects { get; private set; }

  }

  internal interface ILandfillPrincipal : IPrincipal
  {
    Dictionary<long, ProjectDescriptor> Projects { get; }
    string UserUid { get; }
    string CustomerUid { get; }
  }
}