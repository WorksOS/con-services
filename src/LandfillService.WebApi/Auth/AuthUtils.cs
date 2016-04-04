using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using VSP.MasterData.Customer.WebAPI.Models;
using VSS.Customer.Data.Interfaces;
using VSS.Project.Data.Interfaces;
using VSS.Project.Data.Models;
using VSS.Subscription.Data.Interfaces;
using VSS.Subscription.Data.Models;
using VSS.UserCustomer.Data.Interfaces;
using VSS.VisionLink.Utilization.WebApi.Helpers;

public class AuthUtilities
{
  private readonly ICustomerService _customerService;
  private readonly ISubscriptionService _subscriptionService;
  private readonly IProjectService _projectService;
  private readonly IUserCustomerService _userCustomerService;

  public AuthUtilities(ICustomerService customerService, ISubscriptionService subscriptionService, IProjectService projectService, IUserCustomerService userCustomerService)
  {
    _subscriptionService = subscriptionService;
    _customerService = customerService;
    _projectService = projectService;
    _userCustomerService = userCustomerService;
  }

  public AssociatedCustomer GetContext(HttpRequestHeaders headers, out string errorMessage, out string userId)
  {
    userId = String.Empty;
    try
    {
      string token = String.Empty;
      var jwt = JwtHelper.TryGetJwtToken(headers, out token);
      if (jwt)
      {
        if (JwtHelper.IsValidJwtToken(token))
        {
          userId = JwtHelper.DecodeJwtToken(token).Uuid;
          var customer = this._customerService.GetAssociatedCustomerbyUserUid(Guid.Parse(userId));
          AssociatedCustomer associatedCustomer = new AssociatedCustomer()
          {
              CustomerUID = Guid.Parse(customer.CustomerUid),
              CustomerName = customer.CustomerName,
              CustomerType = customer.CustomerType
          };
           
          errorMessage = "";
          return associatedCustomer;
        }
        errorMessage = "Invalid token";
        return null;
      }
      errorMessage = "No token";
      return null;
    }
    catch (Exception ex)
    {
      errorMessage = "Can not retrieve cusomer context";
      return null;
    }
  }

  public List<VSS.Project.Data.Models.Project> GetProjectsForUser(string userUid)
  {
    try
    {
      return _projectService.GetProjectsForUser(userUid).ToList();  
    }
    catch (Exception ex)
    {
      return null;
    }
  }

  

}