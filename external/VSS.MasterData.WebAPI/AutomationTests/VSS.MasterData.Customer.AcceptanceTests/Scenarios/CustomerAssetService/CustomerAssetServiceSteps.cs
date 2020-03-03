using AutomationCore.API.Framework.Library;
using AutomationCore.Shared.Library;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using VSS.MasterData.Customer.AcceptanceTests.Scenarios.CustomerService;
using VSS.MasterData.Customer.AcceptanceTests.Utils.Config;
using VSS.MasterData.Customer.AcceptanceTests.Utils.Features.Classes.CustomerAssetService;


namespace VSS.MasterData.Customer.AcceptanceTests.Scenarios.CustomerAssetService
{
  [Binding]
  public class CustomerAssetServiceSteps
  {

    #region Variables

    //DB Configuration
    public static string MySqlConnectionString;
    public static string MySqlDBName;

    public string TestName;
    private static Log4Net Log = new Log4Net(typeof(CustomerAssetServiceSteps));
    private static CustomerAssetServiceSupport customerAssetServiceSupport = new CustomerAssetServiceSupport(Log);
    private static CustomerServiceSupport customerServiceSupport = new CustomerServiceSupport(Log);

    #endregion

    #region Step Definition

    [BeforeFeature()]
    public static void InitializeKafka()
    {
      if (FeatureContext.Current.FeatureInfo.Title.Equals("CustomerAssetService"))
      {
        KafkaServicesConfig.InitializeKafkaConsumer(customerAssetServiceSupport);
      }
    }

    public CustomerAssetServiceSteps()
    {
      MySqlDBName = CustomerServiceConfig.MySqlDBName;
      MySqlConnectionString = CustomerServiceConfig.MySqlConnection + MySqlDBName;
    }

    [Given(@"CustomerAssetService Is Ready To Verify '(.*)'")]
    public void GivenCustomerAssetServiceIsReadyToVerify(string TestDescription)
    {
      //log the scenario info
      TestName = (ScenarioContext.Current.ScenarioInfo).Title + "_" + TestDescription;
      //TestName = TestDescription;
      LogResult.Report(Log, "log_ForInfo", "Execution started for Test Scenario" + TestName);
    }

    [Given(@"CustomerAssetServiceAssociate Request Is Setup With Default Values")]
    public void GivenCustomerAssetServiceAssociateRequestIsSetupWithDefaultValues()
    {
      customerServiceSupport.CreateCustomerModel = CustomerServiceSteps.GetDefaultValidCustomerServiceCreateRequest();
      customerServiceSupport.PostValidCreateRequestToService();
      customerAssetServiceSupport.AssociateCustomerAssetModel = GetDefaultValidAssociateCustomerAssetServiceRequest();
    }

    [Given(@"CustomerAssetServiceDissociate Request Is Setup With Default Values")]
    public void GivenCustomerAssetServiceDissociateRequestIsSetupWithDefaultValues()
    {
      customerServiceSupport.CreateCustomerModel = CustomerServiceSteps.GetDefaultValidCustomerServiceCreateRequest();
      customerServiceSupport.PostValidCreateRequestToService();
      customerAssetServiceSupport.AssociateCustomerAssetModel = GetDefaultValidAssociateCustomerAssetServiceRequest();
      customerAssetServiceSupport.PostValidCustomerAssetAssociateRequestToService();
      customerAssetServiceSupport.DissociateCustomerAssetModel = GetDefaultValidDissociateCustomerAssetServiceRequest();
    }

    [Given(@"CustomerAssetServiceAssociate Request Is Setup With Invalid Default Values")]
    public void GivenCustomerAssetServiceAssociateRequestIsSetupWithInvalidDefaultValues()
    {
      customerAssetServiceSupport.InvalidAssociateCustomerAssetModel = GetDefaultInValidAssociateCustomerAssetServiceRequest();
    }

    [Given(@"CustomerAssetServiceDissociate Request Is Setup With Invalid Default Values")]
    public void GivenCustomerAssetServiceDissociateRequestIsSetupWithInvalidDefaultValues()
    {
      customerAssetServiceSupport.InvalidDissociateCustomerAssetModel = GetDefaultInValidDissociateCustomerAssetServiceRequest();
    }

    [When(@"I Post Valid CustomerAssetServiceAssociate Request")]
    public void WhenIPostValidCustomerAssetServiceAssociateRequest()
    {
      customerAssetServiceSupport.PostValidCustomerAssetAssociateRequestToService();
    }

    [When(@"I Post Valid CustomerAssetServiceDissociate Request")]
    public void WhenIPostValidCustomerAssetServiceDissociateRequest()
    {
      customerAssetServiceSupport.PostValidCustomerAssetDissociateRequestToService();
    }

    [When(@"I Set CustomerAssetServiceAssociate RelationType To '(.*)'")]
    public void WhenISetCustomerAssetServiceAssociateRelationTypeTo(string relationType)
    {
      customerAssetServiceSupport.AssociateCustomerAssetModel.RelationType = relationType;
    }

    [When(@"I Set Invalid CustomerAssetServiceAssociate RelationType To '(.*)'")]
    public void WhenISetInvalidCustomerAssetServiceAssociateRelationTypeTo(string relationType)
    {
      customerAssetServiceSupport.InvalidAssociateCustomerAssetModel.RelationType = InputGenerator.GetValue(relationType);
    }

    [When(@"I Post Invalid CustomerAssetServiceAssociate Request")]
    public void WhenIPostInvalidCustomerAssetServiceAssociateRequest()
    {
      string contentType = "application/json";
      customerAssetServiceSupport.PostInValidCustomerAssetAssociateRequestToService(contentType, HttpStatusCode.BadRequest);
    }

    [When(@"I Set Invalid CustomerAssetServiceAssociate CustomerUID To '(.*)'")]
    public void WhenISetInvalidCustomerAssetServiceAssociateCustomerUIDTo(string customerUid)
    {
      customerAssetServiceSupport.InvalidAssociateCustomerAssetModel.CustomerUID = InputGenerator.GetValue(customerUid);
    }

    [When(@"I Set Invalid CustomerAssetServiceAssociate AssetUID To '(.*)'")]
    public void WhenISetInvalidCustomerAssetServiceAssociateAssetUIDTo(string assetUid)
    {
      customerAssetServiceSupport.InvalidAssociateCustomerAssetModel.AssetUID = InputGenerator.GetValue(assetUid);
    }

    [When(@"I Set Invalid CustomerAssetServiceAssociate ActionUTC To '(.*)'")]
    public void WhenISetInvalidCustomerAssetServiceAssociateActionUTCTo(string actionUtc)
    {
      customerAssetServiceSupport.InvalidAssociateCustomerAssetModel.ActionUTC = InputGenerator.GetValue(actionUtc);
    }

    [When(@"I Post Invalid CustomerAssetServiceDissociate Request")]
    public void WhenIPostInvalidCustomerAssetServiceDissociateRequest()
    {
      string contentType = "application/json";
      customerAssetServiceSupport.PostInValidCustomerAssetDissociateRequestToService(contentType, HttpStatusCode.BadRequest);
    }

    [When(@"I Set Invalid CustomerServiceDissociate CustomerUID To '(.*)'")]
    public void WhenISetInvalidCustomerServiceDissociateCustomerUIDTo(string customerUid)
    {
      customerAssetServiceSupport.InvalidDissociateCustomerAssetModel.CustomerUID = InputGenerator.GetValue(customerUid);
    }

    [When(@"I Set Invalid CustomerServiceDissociate AssetUID To '(.*)'")]
    public void WhenISetInvalidCustomerServiceDissociateAssetUIDTo(string assetUid)
    {
      customerAssetServiceSupport.InvalidDissociateCustomerAssetModel.AssetUID = InputGenerator.GetValue(assetUid);
    }

    [When(@"I Set Invalid CustomerServiceDissociate ActionUTC To '(.*)'")]
    public void WhenISetInvalidCustomerServiceDissociateActionUTCTo(string actionUtc)
    {
      customerAssetServiceSupport.InvalidDissociateCustomerAssetModel.ActionUTC = InputGenerator.GetValue(actionUtc);
    }

    [Then(@"The Processed CustomerAssetServiceAssociate Message must be available in Kafka topic")]
    public void ThenTheProcessedCustomerAssetServiceAssociateMessageMustBeAvailableInKafkaTopic()
    {
      customerAssetServiceSupport.VerifyCustomerAssetAssociateServiceResponse();
    }

    [Then(@"The Processed CustomerAssetServiceDissociate Message must be available in Kafka topic")]
    public void ThenTheProcessedCustomerAssetServiceDissociateMessageMustBeAvailableInKafkaTopic()
    {
      customerAssetServiceSupport.VerifyCustomerAssetDissociateServiceResponse();
    }

    [Then(@"CustomerAssetServiceAssociate Response With '(.*)' Should Be Returned")]
    public void ThenCustomerAssetServiceAssociateResponseWithShouldBeReturned(string errorMessage)
    {
      customerAssetServiceSupport.VerifyErrorResponse(errorMessage);
    }

    [Then(@"CustomerAssetServiceDissociate Response With '(.*)' Should Be Returned")]
    public void ThenCustomerAssetServiceDissociateResponseWithShouldBeReturned(string errorMessage)
    {
      customerAssetServiceSupport.VerifyErrorResponse(errorMessage);
    }

    [Then(@"The AssociateCustomerAssetEvent Details Are Stored In VSS DB")]
    public void ThenTheAssociateCustomerUserEventDetailsAreStoredInVSSDB()
    {
      CommonUtil.WaitToProcess("2");

      string customerUID = customerAssetServiceSupport.AssociateCustomerAssetModel.CustomerUID.ToString();
      string assetUID = customerAssetServiceSupport.AssociateCustomerAssetModel.AssetUID.ToString();
      string assetRelationType = customerAssetServiceSupport.AssociateCustomerAssetModel.RelationType.ToString();


      List<string> columnList = new List<string>() { "fk_CustomerUID", "fk_AssetUID", "fk_AssetRelationTypeID" };
      List<string> createCustomerDetails = new List<string>();
      createCustomerDetails.Add(customerUID);
      createCustomerDetails.Add(assetUID);
      createCustomerDetails.Add(GetAssetRelationTypeId(assetRelationType));

      string validateQuery = CustomerServiceMySqlQueries.CustomerAssetDetailsByCustomerUID + customerUID + "'";

      MySqlUtil.ValidateMySQLQuery(MySqlConnectionString, validateQuery, customerUID);
    }

    [Then(@"The DissociateCustomerAssetEvent Details Are Removed In VSS DB")]
    public void ThenTheDissociateCustomerUserEventDetailsAreRemovedInVSSDB()
    {
      CommonUtil.WaitToProcess("2");

      string customerUID = customerAssetServiceSupport.DissociateCustomerAssetModel.CustomerUID.ToString();
      string userUID = customerAssetServiceSupport.DissociateCustomerAssetModel.AssetUID.ToString();
      string validateQuery = CustomerServiceMySqlQueries.CustomerUserDetails + customerUID + "' AND fk_UserUID='" + userUID + "'";

      MySqlUtil.ValidateMySQLQueryCount(MySqlConnectionString, validateQuery, 0); // There should be no matching rows.
    }

    [When(@"I Set Duplicate AssociateAssetCustomer")]
    public void WhenISetDuplicateAssociateAssetCustomer()
    {
      List<string> columns = new List<string> {"fk_CustomerUID","fk_AssetUID"};
      List<string> results = new List<string>();
      results = MySqlUtil.ExecuteMySQLQueryResult(MySqlConnectionString, CustomerServiceMySqlQueries.CustomerAssetDetails, columns);

      customerAssetServiceSupport.InvalidAssociateCustomerAssetModel.CustomerUID = results[0];
      customerAssetServiceSupport.InvalidAssociateCustomerAssetModel.AssetUID = results[1];
    }

    [When(@"I Set DissociateCustomerAsset to a non existing asset")]
    public void WhenISetDissociateCustomerAssetToANonExistingAsset()
    {
      List<string> columns = new List<string> { "fk_CustomerUID" };
      List<string> results = new List<string>();
      results = MySqlUtil.ExecuteMySQLQueryResult(MySqlConnectionString, CustomerServiceMySqlQueries.CustomerAssetDetails, columns);

      customerAssetServiceSupport.InvalidDissociateCustomerAssetModel.CustomerUID = results[0];

    }

    [When(@"I Set AssociateCustomerAsset to an existing customer")]
    public void WhenISetAssociateCustomerAssetToAnExistingCustomer()
    {
      List<string> columns = new List<string> { "fk_CustomerUID" };
      List<string> results = new List<string>();
      results = MySqlUtil.ExecuteMySQLQueryResult(MySqlConnectionString, CustomerServiceMySqlQueries.CustomerAssetDetails, columns);

      customerAssetServiceSupport.InvalidAssociateCustomerAssetModel.CustomerUID = results[0];
    }


    #endregion

    #region Helpers

    public static AssociateCustomerAssetEvent GetDefaultValidAssociateCustomerAssetServiceRequest()
    {
      AssociateCustomerAssetEvent defaultValidAssociateCustomerAssetServiceModel = new AssociateCustomerAssetEvent();
      defaultValidAssociateCustomerAssetServiceModel.CustomerUID = customerServiceSupport.CreateCustomerModel.CustomerUID;
      defaultValidAssociateCustomerAssetServiceModel.AssetUID = Guid.NewGuid();
      defaultValidAssociateCustomerAssetServiceModel.RelationType = "Dealer";
      defaultValidAssociateCustomerAssetServiceModel.ActionUTC = DateTime.UtcNow;
      defaultValidAssociateCustomerAssetServiceModel.ReceivedUTC = null;
      return defaultValidAssociateCustomerAssetServiceModel;
    }

    public static DissociateCustomerAssetEvent GetDefaultValidDissociateCustomerAssetServiceRequest()
    {
      DissociateCustomerAssetEvent defaultValidDissociateCustomerAssetServiceModel = new DissociateCustomerAssetEvent();
      defaultValidDissociateCustomerAssetServiceModel.CustomerUID = customerServiceSupport.CreateCustomerModel.CustomerUID;
      defaultValidDissociateCustomerAssetServiceModel.AssetUID = customerAssetServiceSupport.AssociateCustomerAssetModel.AssetUID;
      defaultValidDissociateCustomerAssetServiceModel.ActionUTC = DateTime.UtcNow;
      defaultValidDissociateCustomerAssetServiceModel.ReceivedUTC = null;
      return defaultValidDissociateCustomerAssetServiceModel;
    }

    public static InvalidAssociateCustomerAssetEvent GetDefaultInValidAssociateCustomerAssetServiceRequest()
    {
      InvalidAssociateCustomerAssetEvent defaultInValidAssociateCustomerAssetServiceModel = new InvalidAssociateCustomerAssetEvent();
      defaultInValidAssociateCustomerAssetServiceModel.CustomerUID = Guid.NewGuid().ToString();
      defaultInValidAssociateCustomerAssetServiceModel.AssetUID = Guid.NewGuid().ToString(); ;
      defaultInValidAssociateCustomerAssetServiceModel.RelationType = "Customer";
      defaultInValidAssociateCustomerAssetServiceModel.ActionUTC = DateTime.UtcNow.ToString();
      defaultInValidAssociateCustomerAssetServiceModel.ReceivedUTC = null;
      return defaultInValidAssociateCustomerAssetServiceModel;
    }

    public static InvalidDissociateCustomerAssetEvent GetDefaultInValidDissociateCustomerAssetServiceRequest()
    {
      InvalidDissociateCustomerAssetEvent defaultInValidDissociateCustomerAssetServiceModel = new InvalidDissociateCustomerAssetEvent();
      defaultInValidDissociateCustomerAssetServiceModel.CustomerUID = Guid.NewGuid().ToString(); ;
      defaultInValidDissociateCustomerAssetServiceModel.AssetUID = Guid.NewGuid().ToString(); ;
      defaultInValidDissociateCustomerAssetServiceModel.ActionUTC = DateTime.UtcNow.ToString();
      defaultInValidDissociateCustomerAssetServiceModel.ReceivedUTC = null;
      return defaultInValidDissociateCustomerAssetServiceModel;
    }

    public string GetAssetRelationTypeId(string customerTypeName)
    {
      RelationType assetRelationTypeId;
      RelationType.TryParse(customerTypeName, out assetRelationTypeId);

      return (((int)assetRelationTypeId).ToString());
    }

    #endregion

  }
}
