using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using VSP.MasterData.Customer.Data;
using VSP.MasterData.Customer.WebAPI.Models;
using VSS.Subscription.Data.Models;
using VSS.Subscription.Model.Interfaces;
using VSS.VisionLink.Utilization.WebApi.Helpers;

public class AuthUtilities
{
  private readonly ICustomerDataService _dataService;
  private readonly ISubscriptionService _subscriptionService;

  public AuthUtilities(ICustomerDataService dataService, ISubscriptionService subscriptionService)
  {
    _subscriptionService = subscriptionService;
    _dataService = dataService;
  }

  public List<AssociatedCustomer> GetContext(HttpRequestHeaders headers, out string errorMessage)
  {
    try
    {
      string token = String.Empty;
      var jwt = JwtHelper.TryGetJwtToken(headers, out token);
      if (jwt)
      {
        if (JwtHelper.IsValidJwtToken(token))
        {
          var custList = _dataService.GetAssociatedCustomerbyUserUid(Guid.Parse(JwtHelper.DecodeJwtToken(token).Uuid));
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
  public List<CustomerSubscriptionModel> GetActiveProjectSubscriptionByCustomerId(Guid customerGuid)
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


  public int GetProjectBySubscripion(string projectSubscriptionUid)
  {
    try
    {
      var project = _subscriptionService.GetProjectBySubscripion(projectSubscriptionUid);
      return project;
    }
    catch (Exception ex)
    {
      return -1;
    }
  }
}