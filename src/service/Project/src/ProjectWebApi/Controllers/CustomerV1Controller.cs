﻿using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

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
    public CustomerV1Controller(IConfigurationStore configStore, ICwsAccountClient cwsAccountClient)
      : base(configStore)
    {
      this.cwsAccountClient = cwsAccountClient;
    }

    /// <summary>
    /// Gets a list of customers for the user in user token. 
    /// </summary>
    [Route("api/v1/customer/all")]
    [HttpGet]
    public async Task<CustomerV1ListResult> GetCustomersForMe()
    {
      var customers = await GetCustomersForUser(userId);
      return customers;
    }

    /// <summary>
    /// Gets a list of customers for the user provided, NOT from the user token. 
    /// </summary>
    [Route("api/v1/customer/all/user")]
    [HttpGet]
    public async Task<CustomerV1ListResult> GetCustomersForUser(string userUid)
    {
      var customers = await cwsAccountClient.GetAccountsForUser(userUid);
      var customerDataResult = new CustomerDataResult();
     
      return new CustomerV1ListResult
      {
        customers = customers.Accounts.Select(c =>
            AutoMapperUtility.Automapper.Map<CustomerData>(c))
            .ToList()
      };
    }

    /// <summary>
    /// Gets a requested customer for the user provided, NOT from the user token. 
    /// </summary>
    [Route("api/v1/customer/user")]
    [HttpGet]
    public async Task<CustomerV1SingleResult> GetCustomerForUser(string userUid, string customerUid)
    {
      var customers = await cwsAccountClient.GetAccountsForUser(userUid);
      var customerDataResult = new CustomerDataResult();
      var foundIt = customers.Accounts.Where(c => c.Id == customerUid).FirstOrDefault();
            
      if (foundIt != null)
        return new CustomerV1SingleResult(AutoMapperUtility.Automapper.Map<CustomerData>(foundIt));
      return null; // todoMaverick
    }

    /// <summary>
    /// Gets the total devices licensed for this customer, NOT from the user token. 
    /// </summary>
    [Route("api/v1/customer/user")]
    [HttpGet]
    public async Task<CustomerV1DeviceLicenseResult> GetCustomerDeviceLicense(string customerUid)
    {
      var deviceLicenses = await cwsAccountClient.GetDeviceLicenses(customerUid);

      // todoMaverick may want to move this, and into executor
      //  Which endpoint does the UI use to actually select the project. 
      //     That is the endpoint which should load any devices for the account.
      //     These need to be loaded into the localDB device table so that shortRaptorAssetIds can be generated.
      //     The user, after adding devices must login to WorksOS to trigger this process,
      //        so that when tag files are loaded, the new deviceTRN+shortRaptorAssetId will be available
      var deviceList = await CwsDeviceClient.GetDevicesForAccount(customerUid);
      foreach (var device in deviceList.Devices)
      {
        // if it exists, does nothing but return a count of 0
        await DeviceRepo.StoreEvent(AutoMapperUtility.Automapper.Map<CreateDeviceEvent>(device));
      }

      return new CustomerV1DeviceLicenseResult(deviceLicenses.Total);    
    }
  }
}

