using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Cache;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using LandfillService.Common;
using LandfillService.WebApi.Models;
using Newtonsoft.Json;
using VSP.MasterData.Customer.Data;
using VSS.Subscription.Data.Models;
using VSS.Subscription.Data.MySql;
using VSS.VisionLink.Utilization.WebApi.Configuration.Principal;
using VSS.VisionLink.Utilization.WebApi.Configuration.Principal.Models;
using VSS.VisionLink.Utilization.WebApi.Helpers;


namespace VSS.VisionLink.Utilization.WebApi.Configuration
{
  public class TIDAuthFilter : IAuthenticationFilter
  {
    public async Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
    {
      string jwtToken;
      Jwt DecodedJwt = null;

      if (ConfigurationManager.AppSettings["JWT"] != "Disabled")
      {
        Dictionary<long, ProjectDescriptor> projectList = new Dictionary<long, ProjectDescriptor>();
        //Passing the WebAPI Request headers to JWTHelper function to obtain the JWT Token
        var utils = new AuthUtilities(new CustomerDataService(), new MySqlSubscriptionService());
        string message = string.Empty;
        string userUid = string.Empty;
        var customerlist = utils.GetContext(context.Request.Headers, out message, out userUid);
        List<CustomerSubscriptionModel> projectSubscriptions= new List<CustomerSubscriptionModel>();
        if (customerlist != null)
        {
          foreach (var associatedCustomer in customerlist)
          {
            projectSubscriptions = utils.GetActiveProjectSubscriptionByCustomerId(associatedCustomer.CustomerUID);
            foreach (var projectSubscription in projectSubscriptions)
            {
              projectList.Add(utils.GetProjectBySubscripion(projectSubscription.ProjectSubscriptionUID), new ProjectDescriptor(){});
            }
          }
          //Build principal here
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

    public LandfillPrincipal(Dictionary<long, ProjectDescriptor> projects, List<CustomerSubscriptionModel> subscribtions, string userUid)
    {
      this.Identity = new GenericIdentity("LandfillUser");

      Projects = projects;
      Subscribtions = subscribtions;
      UserUid = userUid;
    }

    public string UserUid { get; private set; }

    public IIdentity Identity { get; private set; }

    public Dictionary<long, ProjectDescriptor> Projects { get; private set; }
    public List<CustomerSubscriptionModel> Subscribtions { get; private set; }

  }

  internal interface ILandfillPrincipal : IPrincipal
  {

    Dictionary<long, ProjectDescriptor> Projects { get; }
    List<CustomerSubscriptionModel> Subscribtions { get; }
    String UserUid { get; }

  }

}