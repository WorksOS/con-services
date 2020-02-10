using System;
using TechTalk.SpecFlow;
using AutomationCore.API.Framework.Library;
using AutomationCore.Shared.Library;
using VSS.MasterData.Customer.AcceptanceTests.Scenarios.CustomerUserService;
using VSS.MasterData.Customer.AcceptanceTests.Utils.Features.Classes.CustomerUserService;
using VSS.MasterData.Customer.AcceptanceTests.Scenarios.CustomerService;
using AutomationCore.API.Framework.Common.Config.TPaaSServicesConfig;
using AutomationCore.API.Framework.Common.Features.TPaaS;
using VSS.MasterData.Customer.AcceptanceTests.Scenarios.CustomerRelationship;
using VSS.MasterData.Customer.AcceptanceTests.Utils.Config;
using System.Configuration;
using System.Collections.Generic;
using VSS.MasterData.Customer.AcceptanceTests.Utils.Features.Classes.AccountHierarchyWebAPI;
using VSS.MasterData.Customer.AcceptanceTests.Scenarios.CustomerAssetService;
using VSS.MasterData.Customer.AcceptanceTests.Utils.Features.Classes.CustomerAssetService;
using AutomationCore.API.Framework.Common;
using System.Net;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using System.IO;
using System.Threading;

namespace VSS.MasterData.Customer.AcceptanceTests.Scenarios.AccountHierarchyWebAPI
{
  [Binding]
  public class AccountHierarchyWebAPISteps
  {
    public static Log4Net Log = new Log4Net(typeof(AccountHierarchyWebAPISteps));
    public string TestName;

    public static AssociateCustomerUserEvent DefaultCustomerUserRelationship = new AssociateCustomerUserEvent();
    public static List<CustomerDetailsModel> CustomerDetailsList = new List<CustomerDetailsModel>();

    private static CustomerAssetServiceSupport customerAssetServiceSupport = new CustomerAssetServiceSupport(Log);
    private AssociateCustomerAssetEvent AssociateCustomerAssetServiceModel = new AssociateCustomerAssetEvent();

    public static CustomerUserAssociationModel CustomerUserAssocication = new CustomerUserAssociationModel();
    public static List<CustomerUserAssociationModel> CustomerUserAssocicationList = new List<CustomerUserAssociationModel>();

    public static Dictionary<Guid, bool> IsValidCustomerAsset = new Dictionary<Guid, bool>();

    private static CustomerDetailsModel CustomerDetails = new CustomerDetailsModel();
    private static CustomerServiceSupport customerServiceSupport = new CustomerServiceSupport(Log);
    private static CustomerUserServiceSupport customerUserServiceSupport = new CustomerUserServiceSupport(Log);
    private static CustomerRelationshipServiceSupport customerRelationshipServiceSupport = new CustomerRelationshipServiceSupport(Log);

    public static Guid AccountHierarchyUserUID = Guid.Parse(ConfigurationManager.AppSettings["AccountHierarchyUserUID"]);

    public AccountHierarchyResponse CustomerHierarchyResponseByUserUID = new AccountHierarchyResponse();

    public AccountHierarchyResponse CustomersForUserAccountDBResult = new AccountHierarchyResponse();


    public CustomerDetails CustDetails = new CustomerDetails();

    public Dictionary<string, Guid> CustomerList = new Dictionary<string, Guid>();
    public List<string> CustomerName = new List<string>();

    public Guid TargetCustomerUID;



    string accessToken = string.Empty;

    public static string UserName = "66257Dealer.single@gmail.com";
    public static string PassWord = "Password1!";

    public static string AccountHierarchyByUserUIDResponseString;
    public static string AccountHierarchyByCustomerUIDResponseString;
    public static int DBPersistenceWaitTime = int.Parse(System.Configuration.ConfigurationManager.AppSettings["DBPersistenceWaitTime"]);

    public string UCID = "UCID" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
    public bool IsSameUCID;
    public bool CustomerHasAccount = false;


    AccountHierarchyWebAPISteps()
    {
      CustomerServiceConfig.SetupEnvironment();
      TPaaSServicesConfig.TPaaSTokenEndpoint = TokenService.GetTokenAPIEndpointUpdated(TPaaSServicesConfig.TPaaSTokenEndpoint, UserName, PassWord);
      accessToken = TokenService.GetAccessToken(TPaaSServicesConfig.TPaaSTokenEndpoint, "E4wERYf22xcMKVRTRQBtU6gkSqoa", "9NoQS8Tg01pXrtLp963Ap7BUvCga");

      CustomerHierarchyResponseByUserUID.Customers = new List<CustomerDetails>();
      CustomersForUserAccountDBResult.Customers = new List<CustomerDetails>();
    }

    [Given(@"AccountHierarchyWebApi Is Ready To Verify '(.*)'")]
    public void GivenAccountHierarchyWebApiIsReadyToVerify(string TestDescription)
    {
      TestName = (ScenarioContext.Current.ScenarioInfo).Title + "_" + TestDescription;
      //TestName = TestDescription;
      LogResult.Report(Log, "log_ForInfo", "Execution started for Test Scenario" + TestName);
    }

    [Given(@"User Is Created And Associated With '(.*)' '(.*)' With '(.*)'")]
    public void GivenUserIsCreatedAndAssociatedWithWith(int customerCount, string customerType, string HasValidCustomerAsset)
    {
      if (HasValidCustomerAsset == "True")
      {
        CreateCustomerUserAssetRelationship(customerType, customerCount, true);
      }
      else
      {
        CreateCustomerUserAssetRelationship(customerType, customerCount, false);
      }

    }

    [When(@"I Perform GetAccountHierarchyByUserUID")]
    public void WhenIPerformGetAccountHierarchyByUserUID()
    {
      GetAccountHierarchyByUserUID();
    }

    public void GetAccountHierarchyByCustomerUID()
    {
      try
      {
        CustomerServiceConfig.AccountHierarchyByCustomerUIDEndPoint = CustomerServiceConfig.AccountHierarchyByCustomerUIDEndPoint + TargetCustomerUID.ToString();
        LogResult.Report(Log, "log_ForInfo", "Performing Get Account Hierarchy By User UID");
        AccountHierarchyByCustomerUIDResponseString = RestClientUtil.DoHttpRequest(CustomerServiceConfig.AccountHierarchyByCustomerUIDEndPoint, HeaderSettings.GetMethod, accessToken, HeaderSettings.JsonMediaType, null, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);

      }
      catch (Exception e)
      {
        LogResult.Report(Log, "log_ForError", "Got Error While Performing Get Account Hierarchy By User UID", e);
        throw new Exception(e + " Got Error While Performing Get Account Hierarchy By User UID");
      }
    }

    [Then(@"The GetAccountHierarchyByCustomerUID Response Should Return The Customer AccountHierarchy")]
    public void ThenTheGetAccountHierarchyByCustomerUIDResponseShouldReturnTheCustomerAccountHierarchy()
    {
      ScenarioContext.Current.Pending();
    }


    public void GetAccountHierarchyByUserUID()
    {
      // AccountHierarchyByUserUIDResponseString
      try
      {
        LogResult.Report(Log, "log_ForInfo", "Performing Get Account Hierarchy By User UID");
        AccountHierarchyByUserUIDResponseString = RestClientUtil.DoHttpRequest(CustomerServiceConfig.AccountHierarchyByUserUIDEndPoint, HeaderSettings.GetMethod, accessToken, HeaderSettings.JsonMediaType, null, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);

      }
      catch (Exception e)
      {
        LogResult.Report(Log, "log_ForError", "Got Error While Performing Get Account Hierarchy By User UID", e);
        throw new Exception(e + " Got Error While Performing Get Account Hierarchy By User UID");
      }
    }


    [Given(@"'(.*)' '(.*)' Is Associated With '(.*)' '(.*)'")]
    public void GivenIsAssociatedWith(string customerType, string customerName, string associateCustomerType, string associateCustomerName)
    {
      string parentcustomerName, childCustomerName;
      Guid parentcustomerUID = Guid.Empty, childCustomerUID = Guid.Empty;
      parentcustomerName = associateCustomerName;
      childCustomerName = customerName;
      parentcustomerName = (CustomerName.Where(x => x.Contains(parentcustomerName)).SingleOrDefault());
      childCustomerName = (CustomerName.Where(x => x.Contains(childCustomerName)).SingleOrDefault());

      if (parentcustomerName != null)
        parentcustomerUID = CustomerList[parentcustomerName];
      if (childCustomerName != null)
        childCustomerUID = CustomerList[childCustomerName];

      bool isAccount;
      if (childCustomerUID == Guid.Empty)
        CreateCustomer(customerType, customerName);
      if (parentcustomerUID == Guid.Empty)
        CreateCustomer(associateCustomerType, associateCustomerName);

      if (CustomerHasAccount == false && customerType == "Customer")
      {
        CreateCustomer("Account", "Account1");
        isAccount = true;
        CreateRelationship(associateCustomerName, "Account1", isAccount);
      }
      if (customerType == "Customer" && CustomerHasAccount == true)
      {
        isAccount = true;
        CreateRelationship(associateCustomerName, "Account1", isAccount);
      }
      if (customerType == "Dealer")
      {
        isAccount = false;
        CreateRelationship(customerName, associateCustomerName, isAccount);
      }

    }

    [Given(@"'(.*)' Has '(.*)' Accounts With '(.*)'")]
    public void GivenHasAccountsWith(string customerName, int accountCount, string ucid)
    {

      string childCustName;
      string ParentcustomerName = string.Empty;
      bool isAccount;
      CustomerHasAccount = true;

      ParentcustomerName = (CustomerName.Where(x => x.Contains(customerName)).SingleOrDefault());

      if (ParentcustomerName == null)
        CreateCustomer("Customer", customerName);
      for (int i = 0; i < accountCount; i++)
      {
        isAccount = true;
        childCustName = "Account" + i;
        if (ucid == "SameUCID")
          IsSameUCID = true;
        CreateCustomer("Account", childCustName);
        CreateRelationship(customerName, childCustName, isAccount);

      }
    }

    public void CreateRelationship(string parentcustomerName, string childCustomerName, bool isAccount)
    {
      parentcustomerName = (CustomerName.Where(x => x.Contains(parentcustomerName)).SingleOrDefault());
      childCustomerName = (CustomerName.Where(x => x.Contains(childCustomerName)).SingleOrDefault());

      Guid parentcustomerUID = CustomerList[parentcustomerName];
      Guid childCustomerUID = CustomerList[childCustomerName];


      //customerRelationshipServiceSupport.CreateCustomerRelationshipModel = CustomerRelationshipServiceSteps.GetDefaultValidCustomerRelationshipServiceCreateRequest();
      customerRelationshipServiceSupport.CreateCustomerRelationshipModel.ParentCustomerUID = parentcustomerUID;
      customerRelationshipServiceSupport.CreateCustomerRelationshipModel.ActionUTC = DateTime.UtcNow;
      customerRelationshipServiceSupport.CreateCustomerRelationshipModel.ReceivedUTC = null;
      if (isAccount == true)
      {
        customerRelationshipServiceSupport.CreateCustomerRelationshipModel.AccountCustomerUID = childCustomerUID;
        customerRelationshipServiceSupport.CreateCustomerRelationshipModel.ChildCustomerUID = parentcustomerUID;

      }
      else
      {
        customerRelationshipServiceSupport.CreateCustomerRelationshipModel.ChildCustomerUID = childCustomerUID;
        customerRelationshipServiceSupport.CreateCustomerRelationshipModel.AccountCustomerUID = null;
      }
      customerRelationshipServiceSupport.PostValidCreateCustomerRelationshipRequestToService();

    }

    [Then(@"The GetAccountHierarchyByUserUID Response Should Return The User AccountHierarchy")]
    public void ThenTheGetAccountHierarchyByUserUIDResponseShouldReturnTheUserAccountHierarchy()
    {
      bool dbResult = false;
      CustomerHierarchyResponseByUserUID = JsonConvert.DeserializeObject<AccountHierarchyResponse>(AccountHierarchyByUserUIDResponseString);
      string getDealersAssociatedWithUserQuery = AccountHierarchyWebAPIDBQueries.GetHierarchyByUserUID_Dealer;
      string getCustomersAssociatedWithUserQuery = AccountHierarchyWebAPIDBQueries.GetHierarchyByUserUID_Customer;
      GetCustomersFromDBByUserUID(getDealersAssociatedWithUserQuery);
      GetCustomersFromDBByUserUID(getCustomersAssociatedWithUserQuery);


      foreach (var item in CustomersForUserAccountDBResult.Customers)
      {
        var CustomerInfo = CustomerHierarchyResponseByUserUID.Customers.FirstOrDefault(x => x.CustomerUID == new Guid(item.CustomerUID).ToString());
        if (CustomerInfo != null)
        {
          if (CustomerInfo.CustomerType == item.CustomerType && CustomerInfo.DealerAccountCode == item.DealerAccountCode && CustomerInfo.DisplayName == item.DisplayName && CustomerInfo.NetworkDealerCode == item.NetworkDealerCode && CustomerInfo.Name == item.Name)
            dbResult = true;
          else
          {
            dbResult = false;
            break;
          }
        }
      }


      Assert.IsTrue(dbResult, "DB Comparison Failure");
    }

    public void CleanUpUserCustomerAssociation()
    {
      string query = string.Format("Delete from `VSS-Customer-ALPHA`.UserCustomer where fk_UserUID=unhex('{0}')", ConfigurationManager.AppSettings["AccountHierarchyUserUID"].Replace("-", ""));
      MySqlCommand command = new MySqlCommand();
      // command.Connection = CustomerServiceConfig.MySqlConnection;

    }

    [Given(@"User Is Created And Associated To '(.*)' '(.*)'")]
    public void GivenUserIsCreatedAndAssociatedTo(string customerType, string customerName)
    {
      CreateCustomerUserAssetRelationship(customerType, customerName);
    }


    public void GetCustomersFromDBByUserUID(string query)
    {

      MySqlDataReader reader;
      reader = MySqlUtil.ExecuteMySQLQuery(CustomerServiceConfig.MySqlConnection, query);
      if (reader.HasRows)
      {
        while (reader.Read())
        {
          CustDetails.CustomerUID = reader["CustomerUID"].ToString().ToLower();
          CustDetails.Name = reader["CustomerName"].ToString();
          if (reader["CustomerTypeID"].ToString() == "1")
          {
            CustDetails.CustomerType = "Dealer";
            CustDetails.NetworkDealerCode = reader["NetworkDealerCode"].ToString();
            CustDetails.DisplayName = "(" + CustDetails.NetworkDealerCode + ") " + CustDetails.Name;
          }
          else if (reader["CustomerTypeID"].ToString() == "2")
          {
            CustDetails.CustomerType = "Customer";
            CustDetails.NetworkCustomerCode = reader["NetworkCustomerCode"].ToString();
            CustDetails.DisplayName = "(" + CustDetails.NetworkCustomerCode + ") " + CustDetails.Name;
          }

          CustomersForUserAccountDBResult.Customers.Add(CustDetails);
        }
      }


    }

    [When(@"I Perform GetAccountHierarchyByCustomerUID")]
    public void WhenIPerformGetAccountHierarchyByCustomerUID()
    {
      GetAccountHierarchyByCustomerUID();
    }




    public void CreateCustomerUserAssetRelationship(string customerType, int customerCount, bool HasValidCustomerAsset)
    {


      for (int i = 0; i < customerCount; i++)
      {

        CreateCustomer(customerType);

        CustomerDetails.CustomerName = customerServiceSupport.CreateCustomerModel.CustomerName;
        CustomerDetails.CustomerType = customerServiceSupport.CreateCustomerModel.CustomerType;
        CustomerDetails.CustomerUID = customerServiceSupport.CreateCustomerModel.CustomerUID;
        CustomerDetails.DealerAccountCode = customerServiceSupport.CreateCustomerModel.DealerAccountCode;
        CustomerDetails.DealerNetwork = customerServiceSupport.CreateCustomerModel.DealerNetwork;
        CustomerDetails.NetworkCustomerCode = customerServiceSupport.CreateCustomerModel.NetworkCustomerCode;
        CustomerDetails.NetworkDealerCode = customerServiceSupport.CreateCustomerModel.NetworkDealerCode;

        CustomerDetailsList.Add(CustomerDetails);

        CreateUserCustomerRelationship(CustomerDetails.CustomerUID);

        IsValidCustomerAsset.Add(CustomerDetails.CustomerUID, HasValidCustomerAsset);

        CreateCustomerAssetRelationship(CustomerDetails.CustomerUID);

      }

    }

    public void CreateCustomerUserAssetRelationship(string customerType, string customerName)
    {


      bool HasValidCustomerAsset = true;

      CreateCustomer(customerType, customerName);

      CustomerDetails.CustomerName = customerServiceSupport.CreateCustomerModel.CustomerName;
      CustomerDetails.CustomerType = customerServiceSupport.CreateCustomerModel.CustomerType;
      CustomerDetails.CustomerUID = customerServiceSupport.CreateCustomerModel.CustomerUID;
      CustomerDetails.DealerAccountCode = customerServiceSupport.CreateCustomerModel.DealerAccountCode;
      CustomerDetails.DealerNetwork = customerServiceSupport.CreateCustomerModel.DealerNetwork;
      CustomerDetails.NetworkCustomerCode = customerServiceSupport.CreateCustomerModel.NetworkCustomerCode;
      CustomerDetails.NetworkDealerCode = customerServiceSupport.CreateCustomerModel.NetworkDealerCode;

      CustomerDetailsList.Add(CustomerDetails);

      CreateUserCustomerRelationship(CustomerDetails.CustomerUID);

      IsValidCustomerAsset.Add(CustomerDetails.CustomerUID, HasValidCustomerAsset);

      CreateCustomerAssetRelationship(CustomerDetails.CustomerUID);
      TargetCustomerUID = customerServiceSupport.CreateCustomerModel.CustomerUID;


    }

    public void CreateCustomer(string customerType)
    {
      customerServiceSupport.CreateCustomerModel =
   CustomerServiceSteps.GetDefaultValidCustomerServiceCreateRequest();
      if (customerType == "Dealer")
        customerServiceSupport.CreateCustomerModel.CustomerType = "Dealer";
      else if (customerType == "Customer")
        customerServiceSupport.CreateCustomerModel.CustomerType = "Customer";


      customerServiceSupport.PostValidCreateRequestToService();
    }

    public void CreateCustomer(string customerType, string customerName)
    {
      customerServiceSupport.CreateCustomerModel =
   CustomerServiceSteps.GetDefaultValidCustomerServiceCreateRequest();

      customerServiceSupport.CreateCustomerModel.CustomerType = customerType;
      customerServiceSupport.CreateCustomerModel.CustomerName = customerName + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      if (customerType == "Account" && IsSameUCID)
        customerServiceSupport.CreateCustomerModel.NetworkCustomerCode = UCID;

      CustomerList.Add(customerServiceSupport.CreateCustomerModel.CustomerName, customerServiceSupport.CreateCustomerModel.CustomerUID);
      CustomerName.Add(customerServiceSupport.CreateCustomerModel.CustomerName);

      customerServiceSupport.PostValidCreateRequestToService();
    }

    public void CreateUserCustomerRelationship(Guid customerUID)
    {
      bool userCreated = false;
      Guid useruid = AccountHierarchyUserUID;

      if (!userCreated)
      {
        customerUserServiceSupport.AssociateCustomerUserModel = CustomerUserServiceSteps.GetDefaultValidAssociateCustomerUserServiceRequest();
        customerUserServiceSupport.AssociateCustomerUserModel.CustomerUID = customerUID;
        customerUserServiceSupport.AssociateCustomerUserModel.UserUID = useruid;
        useruid = customerUserServiceSupport.AssociateCustomerUserModel.UserUID;
        userCreated = true;
        CustomerUserAssocication.UserUID = customerUserServiceSupport.AssociateCustomerUserModel.UserUID;
        CustomerUserAssocication.CustomerUID = customerUID;

        CustomerUserAssocicationList.Add(CustomerUserAssocication);
      }
      else
      {
        customerUserServiceSupport.AssociateCustomerUserModel = CustomerUserServiceSteps.GetDefaultValidAssociateCustomerUserServiceRequest();
        customerUserServiceSupport.AssociateCustomerUserModel.UserUID = useruid;
      }


      customerUserServiceSupport.PostValidCustomerUserAssociateRequestToService();
    }

    public void CreateCustomerAssetRelationship(Guid customerUID)
    {
      customerAssetServiceSupport.AssociateCustomerAssetModel = CustomerAssetServiceSteps.GetDefaultValidAssociateCustomerAssetServiceRequest();
      customerAssetServiceSupport.AssociateCustomerAssetModel.CustomerUID = customerUID;
      customerAssetServiceSupport.PostValidCustomerAssetAssociateRequestToService();
    }




  }
}
