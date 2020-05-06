using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Controllers
{
  /// <summary>
  /// Customer controller v1
  ///     for the UI to get customer list etc as we have no CustomerSvc yet
  /// </summary>
  public class CustomerV1Controller : ProjectBaseController
  {
    private readonly ICwsAccountClient cwsAccountClient;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public CustomerV1Controller(ICwsAccountClient cwsAccountClient)
    {
      this.cwsAccountClient = cwsAccountClient;
    }

    /// <summary>
    /// Gets a list of customers for the user in user token. 
    /// </summary>
    [Route("api/v1/customers/me")]
    [HttpGet]
    public async Task<CustomerV1ListResult> GetCustomersForMe()
    {
      Logger.LogInformation(nameof(GetCustomersForMe));
      var customers = await cwsAccountClient.GetMyAccounts(new Guid(userId), customHeaders);
      return new CustomerV1ListResult
      {
        Customers = customers.Accounts.Select(c =>
            AutoMapperUtility.Automapper.Map<CustomerData>(c))
            .ToList()
      };
    }

    /// <summary>
    /// Gets a list of customers for the user in user token. 
    /// </summary>
    [Route("api/v1/customers/accounthierarchy")]
    [HttpGet]
    public async Task<IActionResult> GetHierarchy()
    {
      Logger.LogInformation(nameof(GetCustomersForMe));
      var customers = await cwsAccountClient.GetMyAccounts(new Guid(userId), customHeaders);

      var result = new AccountHierarchy
      {
        UserUid = userId,
        Customers = customers.Accounts.Select(c =>
            AutoMapperUtility.Automapper.Map<AccountHierarchyCustomer>(c))
          .ToList()
      };

      // The previous customer endpoint was Pascal  Cased, and calling code (eg GQL) expects that.
      var response = Json(result, new JsonSerializerSettings
      {
        ContractResolver = new DefaultContractResolver()
      } );
      return response;
    }

    /// <summary>
    /// Gets the total devices licensed for this customer. 
    ///   Also triggers a lazy load of devices from cws, so that shortRaptorAssetId is generated.
    /// </summary>
    [Route("api/v1/customer/license/{customerUid}")]
    [HttpGet]
    public async Task<CustomerV1DeviceLicenseResult> GetCustomerDeviceLicense(string customerUid)
    {
      Logger.LogInformation($"{nameof(GetCustomerDeviceLicense)}");
      var deviceLicenses = await cwsAccountClient.GetDeviceLicenses(new Guid(customerUid), customHeaders);
      return new CustomerV1DeviceLicenseResult(deviceLicenses.Total);
    }
  }
}

