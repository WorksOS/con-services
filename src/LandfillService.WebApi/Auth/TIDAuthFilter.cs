using System;
using System.Collections.Generic;
using System.Configuration;
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
      // Jwt DecodedJwt = null;

      if (ConfigurationManager.AppSettings["JWT"] != "Disabled")
      {
        Dictionary<long, ProjectDescriptor> projectList = new Dictionary<long, ProjectDescriptor>();
        //Passing the WebAPI Request headers to JWTHelper function to obtain the JWT Token
        var utils = new AuthUtilities(new MySqlCustomerRepository(), new MySqlSubscriptionRepository());
        string message = string.Empty;
        string userUid = string.Empty;
        var customerlist = utils.GetContext(context.Request.Headers, out message, out userUid);
        List<ActiveProjectCustomerSubscriptionModel> projectSubscriptions= new List<ActiveProjectCustomerSubscriptionModel>();
        Log.DebugFormat("Authorization: For userID {0} customer List is {1}", userUid, customerlist.ToString());

        if (customerlist != null)
        {
          foreach (var associatedCustomer in customerlist)
          {
            projectSubscriptions = utils.GetActiveProjectSubscriptionByCustomerId(associatedCustomer.CustomerUID);
            foreach (var projectSubscription in projectSubscriptions)
            {
              projectList.Add(utils.GetProjectBySubscription(projectSubscription.SubscriptionGuid), new ProjectDescriptor(){});
            }
            Log.DebugFormat("Authorization: for Customer: {0} projectList is: {1}", associatedCustomer, projectSubscriptions.ToString());
          }
          
          context.Principal = new LandfillPrincipal(projectList, projectSubscriptions, userUid);
          LoggerSvc.LogMessage("Principal","BuildPrincipal","Claims",JsonConvert.SerializeObject(projectList));
        }
        else
          context.ErrorResult = new AuthenticationFailureResult(message, context.Request);
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
      this.Identity = new GenericIdentity("LandfillUser");
    }

    public LandfillPrincipal(Dictionary<long, ProjectDescriptor> projects, List<ActiveProjectCustomerSubscriptionModel> subscriptions, string userUid)
    {
      this.Identity = new GenericIdentity("LandfillUser");

      Projects = projects;
      Subscriptions = subscriptions;
      UserUid = userUid;
    }

    public string UserUid { get; private set; }

    public IIdentity Identity { get; private set; }

    public Dictionary<long, ProjectDescriptor> Projects { get; private set; }
    public List<ActiveProjectCustomerSubscriptionModel> Subscriptions { get; private set; }

  }

  internal interface ILandfillPrincipal : IPrincipal
  {

    Dictionary<long, ProjectDescriptor> Projects { get; }
    List<ActiveProjectCustomerSubscriptionModel> Subscriptions { get; }
    String UserUid { get; }

  }

}