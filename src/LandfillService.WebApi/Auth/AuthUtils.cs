using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using VSP.MasterData.Customer.WebAPI.Models;
using VSS.Customer.Data.Interfaces;
using VSS.Subscription.Data.Models;
using VSS.Subscription.Model.Interfaces;
using VSS.VisionLink.Utilization.WebApi.Helpers;

public class AuthUtilities
{
  private readonly ICustomerService _dataService;
  private readonly ISubscriptionService _subscriptionService;

  public AuthUtilities(ICustomerService dataService, ISubscriptionService subscriptionService)
  {
    _subscriptionService = subscriptionService;
    _dataService = dataService;
  }

  public List<AssociatedCustomer> GetContext(HttpRequestHeaders headers, out string errorMessage, out string userId)
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
          var custList = _dataService.GetAssociatedCustomerbyUserUid(Guid.Parse(JwtHelper.DecodeJwtToken(token).Uuid));
          userId = JwtHelper.DecodeJwtToken(token).Uuid;
          List<AssociatedCustomer> associatedCustomers =
            custList.Select(
              x =>
                new AssociatedCustomer()
                {
                  CustomerUID = Guid.Parse(x.CustomerUID),
                  CustomerName = x.CustomerName,
                  CustomerType = (CustomerType) x.fk_CustomerTypeID
                }).ToList();
          errorMessage = "";
          return associatedCustomers;
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


  public List<CustomerSubscriptionModel> GetSubscriptionByCustomerId(Guid customerGuid)
  {
    try
    {
      var subscriptions = _subscriptionService.GetSubscriptionForCustomer(customerGuid);
      return subscriptions;
    }
    catch (Exception ex)
    {
      return null;
    }
  }

  public List<ActiveProjectCustomerSubscriptionModel> GetActiveProjectSubscriptionByCustomerId(Guid customerGuid)
  {
    try
    {
      var subscriptions = _subscriptionService.GetActiveProjectSubscriptionForCustomer(customerGuid);
      return subscriptions;
    }
    catch (Exception ex)
    {
      return null;
    }
  }

  public int GetProjectBySubscription(string projectSubscriptionUid)
  {
    try
    {
      // When updating Subscription project, Merino needs to add GetProjectBySubscription() to MySqlSubscriptionService
      var project = _subscriptionService.GetProjectBySubscription(projectSubscriptionUid);
      return project;
    }
    catch (Exception ex)
    {
      return -1;
    }
  }

}