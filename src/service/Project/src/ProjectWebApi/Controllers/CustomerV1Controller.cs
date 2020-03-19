using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Project.WebAPI.Common.Utilities;

namespace VSS.MasterData.Project.WebAPI.Controllers
{
  /// <summary>
  /// Customer controller v1
  ///     for the UI to get customer list etc as we have no CustomerSvc yet
  /// </summary>
  public class CustomerV1Controller : ProjectBaseController
  {  

    private readonly IAccountClient accountClient;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public CustomerV1Controller(IConfigurationStore configStore, IAccountClient accountClient)
      : base(configStore)
    {
      this.accountClient = accountClient;
    }

    /// <summary>
    /// Gets a list of customers for the user in user token. 
    /// </summary>
    [Route("api/v1/customer/all")]
    [HttpGet]
    public async Task<CustomerDataResult> GetCustomersForMe()
    {
      var customers = await GetCustomersForUser(userId);
      return customers;
    }

    /// <summary>
    /// Gets a list of customers for the user provided, NOT from the user token. 
    /// </summary>
    [Route("api/v1/customer/all/user")]
    [HttpGet]
    public async Task<CustomerDataResult> GetCustomersForUser(string userUid)
    {
      var customers = await accountClient.GetAccountsForUser(userUid);
      var customerDataResult = new CustomerDataResult();
      
      // c == AccountResponseModel      
      return new CustomerDataResult
      {
        customer = customers.Accounts.Select(c =>
            AutoMapperUtility.Automapper.Map<CustomerData>(c))
            .ToList()
      };
    }

    /// <summary>
    /// Gets a requested customer for the user provided, NOT from the user token. 
    /// </summary>
    [Route("api/v1/customer/user")]
    [HttpGet]
    public async Task<CustomerDataResult> GetCustomerForUser(string userUid, string customerUid)
    {
      var customers = await accountClient.GetAccountsForUser(userUid);
      var customerDataResult = new CustomerDataResult();
      var foundIt = customers.Accounts.Where(c => c.Id == customerUid).FirstOrDefault();
      if (foundIt != null)
        customerDataResult.customer.Add(AutoMapperUtility.Automapper.Map<CustomerData>(foundIt));
      return customerDataResult;
    }
  }
}

