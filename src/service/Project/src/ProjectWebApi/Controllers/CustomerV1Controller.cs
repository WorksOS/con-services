using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.Utilities;

namespace VSS.MasterData.Project.WebAPI.Controllers
{
  /// <summary>
  /// Customer controller v1 for the UI to get customer list etc as we have no CustomerSvc yet
  /// </summary>
  public class CustomerV1Controller : ProjectBaseController
  {
    private readonly ICwsAccountClient _cwsAccountClient;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public CustomerV1Controller(ICwsAccountClient cwsAccountClient)
    {
      _cwsAccountClient = cwsAccountClient;
    }

    /// <summary>
    /// Gets a list of customers for the user in user token.
    /// </summary>
    [HttpGet("api/v1/customers/accounthierarchy")]
    public async Task<IActionResult> GetHierarchy()
    {
      Logger.LogInformation(nameof(GetHierarchy));

      var customers = await _cwsAccountClient.GetMyAccounts(new Guid(UserId), customHeaders);

      var result = new AccountHierarchy
      {
        UserUid = UserId,
        Customers = customers.Accounts.Select(c => AutoMapperUtility.Automapper.Map<AccountHierarchyCustomer>(c)).ToList()
      };

      // The previous customer endpoint was PascalCase, and calling code (eg GQL) expects that.
      return Json(result, new JsonSerializerSettings
      {
        ContractResolver = new DefaultContractResolver()
      });
    }

    /// <summary>
    /// Gets the total devices licensed for this customer.
    /// Also triggers a lazy load of devices from cws, so that shortRaptorAssetId is generated.
    /// </summary>
    [HttpGet("api/v1/customer/license/{customerUid}")]
    public async Task<CustomerV1DeviceLicenseResult> GetCustomerDeviceLicense(string customerUid)
    {
      Logger.LogInformation(nameof(GetCustomerDeviceLicense));
      var deviceLicenses = await _cwsAccountClient.GetDeviceLicenses(new Guid(customerUid), customHeaders);

      return new CustomerV1DeviceLicenseResult(deviceLicenses.Total);
    }
  }
}
