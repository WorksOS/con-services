using AutomationCore.API.Framework.Library;
using AutomationCore.Shared.Library;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using VSS.MasterData.Customer.AcceptanceTests.Scenarios.CustomerService;
using VSS.MasterData.Customer.AcceptanceTests.Utils.Config;
using VSS.MasterData.Customer.AcceptanceTests.Utils.Features.Classes.CustomerUserService;

namespace VSS.MasterData.Customer.AcceptanceTests.Scenarios.CustomerUserService
{
  [Binding]
  public class CustomerUserServiceSteps
  {

    #region Variables

    //DB Configuration
    public static string MySqlConnectionString;
    public static string MySqlDBName = CustomerServiceConfig.MySqlDBName;

    public string TestName;
    private static Log4Net Log = new Log4Net(typeof(CustomerUserServiceSteps));
    private static CustomerUserServiceSupport customerUserServiceSupport = new CustomerUserServiceSupport(Log);
    private static CustomerServiceSupport customerServiceSupport = new CustomerServiceSupport(Log);

    #endregion

    #region Step Definition

    [BeforeFeature()]
    public static void InitializeKafka()
    {
      if (FeatureContext.Current.FeatureInfo.Title.Equals("CustomerUserService"))
      {
        KafkaServicesConfig.InitializeKafkaConsumer(customerUserServiceSupport);
      }
    }

    public CustomerUserServiceSteps()
    {
      MySqlConnectionString = CustomerServiceConfig.MySqlConnection + MySqlDBName;
    }

    [Given(@"CustomerUserService Is Ready To Verify '(.*)'")]
    public void GivenCustomerUserServiceIsReadyToVerify(string TestDescription)
    {
      //log the scenario info
      TestName = (ScenarioContext.Current.ScenarioInfo).Title + "_" + TestDescription;
      //TestName = TestDescription;
      LogResult.Report(Log, "log_ForInfo", "Execution started for Test Scenario" + TestName);
    }

    [Given(@"CustomerUserServiceAssociate Request Is Setup With Default Values")]
    public void GivenCustomerUserServiceAssociateRequestIsSetupWithDefaultValues()
    {
      customerServiceSupport.CreateCustomerModel =
         CustomerServiceSteps.GetDefaultValidCustomerServiceCreateRequest();
      customerServiceSupport.PostValidCreateRequestToService();
      customerUserServiceSupport.AssociateCustomerUserModel = GetDefaultValidAssociateCustomerUserServiceRequest();
    }

    [Given(@"CustomerUserServiceDissociate Request Is Setup With Default Values")]
    public void GivenCustomerUserServiceDissociateRequestIsSetupWithDefaultValues()
    {
      customerServiceSupport.CreateCustomerModel =
       CustomerServiceSteps.GetDefaultValidCustomerServiceCreateRequest();
      customerServiceSupport.PostValidCreateRequestToService();
      customerUserServiceSupport.AssociateCustomerUserModel = GetDefaultValidAssociateCustomerUserServiceRequest();
      customerUserServiceSupport.PostValidCustomerUserAssociateRequestToService();
      customerUserServiceSupport.DissociateCustomerUserModel = GetDefaultValidDissociateCustomerUserServiceRequest();
    }

    [Given(@"CustomerUserServiceAssociate Request Is Setup With Invalid Default Values")]
    public void GivenCustomerUserServiceAssociateRequestIsSetupWithInvalidDefaultValues()
    {
      customerUserServiceSupport.InvalidAssociateCustomerUserModel = GetDefaultInValidAssociateCustomerUserServiceRequest();
    }

    [Given(@"CustomerUserServiceDissociate Request Is Setup With Invalid Default Values")]
    public void GivenCustomerUserServiceDissociateRequestIsSetupWithInvalidDefaultValues()
    {
      customerUserServiceSupport.InvalidDissociateCustomerUserModel = GetDefaultInValidDissociateCustomerUserServiceRequest();
    }

    [When(@"I Post Valid CustomerUserServiceAssociate Request")]
    public void WhenIPostValidCustomerUserServiceAssociateRequest()
    {
      customerUserServiceSupport.PostValidCustomerUserAssociateRequestToService();
    }

    [When(@"I Post Valid CustomerUserServiceDissociate Request")]
    public void WhenIPostValidCustomerUserServiceDissociateRequest()
    {
      customerUserServiceSupport.PostValidCustomerUserDissociateRequestToService();
    }

    [When(@"I Set Invalid CustomerUserServiceAssociate CustomerUID To '(.*)'")]
    public void WhenISetInvalidCustomerUserServiceAssociateCustomerUIDTo(string customerUid)
    {
      customerUserServiceSupport.InvalidAssociateCustomerUserModel.CustomerUID = InputGenerator.GetValue(customerUid);
    }

    [When(@"I Post Invalid CustomerUserServiceAssociate Request")]
    public void WhenIPostInvalidCustomerUserServiceAssociateRequest()
    {
      string contentType = "application/json";
      customerUserServiceSupport.PostInValidCustomerUserAssociateRequestToService(contentType, HttpStatusCode.BadRequest);
    }

    [When(@"I Set Invalid CustomerUserServiceAssociate UserUID To '(.*)'")]
    public void WhenISetInvalidCustomerUserServiceAssociateUserUIDTo(string userUid)
    {
      customerUserServiceSupport.InvalidAssociateCustomerUserModel.UserUID = InputGenerator.GetValue(userUid);
    }

    [When(@"I Set Invalid CustomerUserServiceAssociate ActionUTC To '(.*)'")]
    public void WhenISetInvalidCustomerUserServiceAssociateActionUTCTo(string actionUtc)
    {
      customerUserServiceSupport.InvalidAssociateCustomerUserModel.ActionUTC = InputGenerator.GetValue(actionUtc);
    }

    [When(@"I Set Invalid CustomerUserServiceDissociate CustomerUID To '(.*)'")]
    public void WhenISetInvalidCustomerUserServiceDissociateCustomerUIDTo(string customerUid)
    {
      customerUserServiceSupport.InvalidDissociateCustomerUserModel.CustomerUID = InputGenerator.GetValue(customerUid);
    }

    [When(@"I Post Invalid CustomerUserServiceDissociate Request")]
    public void WhenIPostInvalidCustomerUserServiceDissociateRequest()
    {
      string contentType = "application/json";
      customerUserServiceSupport.PostInValidCustomerUserDissociateRequestToService(contentType, HttpStatusCode.BadRequest);
    }

    [When(@"I Set Invalid CustomerUserServiceDissociate UserUID To '(.*)'")]
    public void WhenISetInvalidCustomerUserServiceDissociateUserUIDTo(string userUid)
    {
      customerUserServiceSupport.InvalidDissociateCustomerUserModel.UserUID = InputGenerator.GetValue(userUid);
    }

    [When(@"I Set Invalid CustomerUserServiceDissociate ActionUTC To '(.*)'")]
    public void WhenISetInvalidCustomerUserServiceDissociateActionUTCTo(string actionUtc)
    {
      customerUserServiceSupport.InvalidDissociateCustomerUserModel.ActionUTC = InputGenerator.GetValue(actionUtc);
    }

    [Then(@"The Processed CustomerUserServiceAssociate Message must be available in Kafka topic")]
    public void ThenTheProcessedCustomerUserServiceAssociateMessageMustBeAvailableInKafkaTopic()
    {
      customerUserServiceSupport.VerifyCustomerUserAssociateServiceResponse();
    }

    [Then(@"The Processed CustomerUserServiceDissociate Message must be available in Kafka topic")]
    public void ThenTheProcessedCustomerUserServiceDissociateMessageMustBeAvailableInKafkaTopic()
    {
      customerUserServiceSupport.VerifyCustomerUserDissociateServiceResponse();
    }

    [Then(@"CustomerUserServiceAssociate Response With '(.*)' Should Be Returned")]
    public void ThenCustomerUserServiceAssociateResponseWithShouldBeReturned(string errorMessage)
    {
      customerUserServiceSupport.VerifyErrorResponse(errorMessage);
    }

    [Then(@"CustomerUserServiceDissociate Response With '(.*)' Should Be Returned")]
    public void ThenCustomerUserServiceDissociateResponseWithShouldBeReturned(string errorMessage)
    {
      customerUserServiceSupport.VerifyErrorResponse(errorMessage);
    }

    [Then(@"The AssociateCustomerUserEvent Details Are Stored In VSS DB")]
    public void ThenTheAssociateCustomerUserEventDetailsAreStoredInVSSDB()
    {
      CommonUtil.WaitToProcess("2");

      string customerUID = customerUserServiceSupport.AssociateCustomerUserModel.CustomerUID.ToString();
      string userUID = customerUserServiceSupport.AssociateCustomerUserModel.UserUID.ToString();

      string validateQuery = CustomerServiceMySqlQueries.CustomerUserDetailsByCustomerUID + userUID + "'";

      MySqlUtil.ValidateMySQLQuery(MySqlConnectionString, validateQuery, customerUID);
    }

    [Then(@"The DissociateCustomerUserEvent Details Are Removed In VSS DB")]
    public void ThenTheDissociateCustomerUserEventDetailsAreRemovedInVSSDB()
    {
      CommonUtil.WaitToProcess("2");

      string customerUID = customerUserServiceSupport.DissociateCustomerUserModel.CustomerUID.ToString();
      string userUID = customerUserServiceSupport.DissociateCustomerUserModel.UserUID.ToString();
      string validateQuery = CustomerServiceMySqlQueries.CustomerUserDetails + customerUID + "' AND fk_UserUID='" + userUID + "'";

      MySqlUtil.ValidateMySQLQueryCount(MySqlConnectionString, validateQuery, 0); // There should be no matching rows.
    }

    [When(@"I Set Duplicate AssociateUserCustomer")]
    public void WhenISetDuplicateAssociateUserCustomer()
    {
      List<string> columns = new List<string> { "fk_CustomerUID", "fk_UserUID" };
      List<string> results = new List<string>();
      results = MySqlUtil.ExecuteMySQLQueryResult(MySqlConnectionString, CustomerServiceMySqlQueries.CustomerUserDetailsLimit, columns);

      customerUserServiceSupport.InvalidAssociateCustomerUserModel.CustomerUID = results[0];
      customerUserServiceSupport.InvalidAssociateCustomerUserModel.UserUID = results[1];
    }

    [When(@"I Set DissociateCustomerUser to a non existing user")]
    public void WhenISetDissociateCustomerUserToANonExistingUser()
    {
      List<string> columns = new List<string> { "fk_CustomerUID" };
      List<string> results = new List<string>();
      results = MySqlUtil.ExecuteMySQLQueryResult(MySqlConnectionString, CustomerServiceMySqlQueries.CustomerUserDetailsLimit, columns);

      customerUserServiceSupport.InvalidDissociateCustomerUserModel.CustomerUID = results[0];
    }


    #endregion

    #region Helpers

    public static AssociateCustomerUserEvent GetDefaultValidAssociateCustomerUserServiceRequest()
    {
      AssociateCustomerUserEvent defaultValidAssociateCustomerAssetServiceModel = new AssociateCustomerUserEvent();
      defaultValidAssociateCustomerAssetServiceModel.CustomerUID = customerServiceSupport.CreateCustomerModel.CustomerUID;
      defaultValidAssociateCustomerAssetServiceModel.UserUID = Guid.NewGuid();
      defaultValidAssociateCustomerAssetServiceModel.ActionUTC = DateTime.UtcNow;
      defaultValidAssociateCustomerAssetServiceModel.ReceivedUTC = null;
      return defaultValidAssociateCustomerAssetServiceModel;
    }

    public static DissociateCustomerUserEvent GetDefaultValidDissociateCustomerUserServiceRequest()
    {
      DissociateCustomerUserEvent defaultValidDissociateCustomerAssetServiceModel = new DissociateCustomerUserEvent();
      defaultValidDissociateCustomerAssetServiceModel.CustomerUID = customerUserServiceSupport.AssociateCustomerUserModel.CustomerUID;
      defaultValidDissociateCustomerAssetServiceModel.UserUID = customerUserServiceSupport.AssociateCustomerUserModel.UserUID;
      defaultValidDissociateCustomerAssetServiceModel.ActionUTC = DateTime.UtcNow;
      defaultValidDissociateCustomerAssetServiceModel.ReceivedUTC = null;
      return defaultValidDissociateCustomerAssetServiceModel;
    }

    public static InvalidAssociateCustomerUserEvent GetDefaultInValidAssociateCustomerUserServiceRequest()
    {
      InvalidAssociateCustomerUserEvent defaultInValidAssociateCustomerAssetServiceModel = new InvalidAssociateCustomerUserEvent();
      defaultInValidAssociateCustomerAssetServiceModel.CustomerUID = Guid.NewGuid().ToString();
      defaultInValidAssociateCustomerAssetServiceModel.UserUID = Guid.NewGuid().ToString(); ;
      defaultInValidAssociateCustomerAssetServiceModel.ActionUTC = DateTime.UtcNow.ToString();
      defaultInValidAssociateCustomerAssetServiceModel.ReceivedUTC = null;
      return defaultInValidAssociateCustomerAssetServiceModel;
    }

    public static InvalidDissociateCustomerUserEvent GetDefaultInValidDissociateCustomerUserServiceRequest()
    {
      InvalidDissociateCustomerUserEvent defaultInValidDissociateCustomerAssetServiceModel = new InvalidDissociateCustomerUserEvent();
      defaultInValidDissociateCustomerAssetServiceModel.CustomerUID = Guid.NewGuid().ToString(); ;
      defaultInValidDissociateCustomerAssetServiceModel.UserUID = Guid.NewGuid().ToString(); ;
      defaultInValidDissociateCustomerAssetServiceModel.ActionUTC = DateTime.UtcNow.ToString();
      defaultInValidDissociateCustomerAssetServiceModel.ReceivedUTC = null;
      return defaultInValidDissociateCustomerAssetServiceModel;
    }

    #endregion

  }
}
