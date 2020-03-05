using AutomationCore.API.Framework.Library;
using AutomationCore.Shared.Library;
using System;
using System.Collections.Generic;
using TechTalk.SpecFlow;
using VSS.MasterData.Customer.AcceptanceTests.Utils.Config;
using VSS.MasterData.Customer.AcceptanceTests.Utils.Features.Classes.CustomerRelationshipService;

namespace VSS.MasterData.Customer.AcceptanceTests.Scenarios.CustomerRelationship
{
  [Binding]
  public class CustomerRelationshipServiceSteps
  {
    #region Variables

    //DB Configuration
    public static string MySqlConnectionString;
    public static string MySqlDBName;

    public string TestName;
    private static Log4Net Log = new Log4Net(typeof(CustomerRelationshipServiceSteps));
    private static CustomerRelationshipServiceSupport customerRelationshipServiceSupport = new CustomerRelationshipServiceSupport(Log);


    #endregion

    #region Step Definition

    [BeforeFeature()]
    public static void InitializeKafka()
    {
      if (FeatureContext.Current.FeatureInfo.Title.Equals("CustomerRelationshipService"))
      {
        KafkaServicesConfig.InitializeKafkaConsumer(customerRelationshipServiceSupport);
      }
    }

    public CustomerRelationshipServiceSteps()
    {
      MySqlDBName = CustomerServiceConfig.MySqlDBName;
      MySqlConnectionString = CustomerServiceConfig.MySqlConnection + MySqlDBName;
    }

    [Given(@"CustomerRelationshipService Is Ready To Verify '(.*)'")]
    public void GivenCustomerRelationshipServiceIsReadyToVerify(string TestDescription)
    {
      //log the scenario info
      TestName = (ScenarioContext.Current.ScenarioInfo).Title + "_" + TestDescription;
      //TestName = TestDescription;
      LogResult.Report(Log, "log_ForInfo", "Execution started for Test Scenario" + TestName);
    }

    [Given(@"CustomerService CreateCustomerRelationship Request Is Setup With Default Values")]
    public void GivenCustomerServiceCreateCustomerRelationshipRequestIsSetupWithDefaultValues()
    {
      customerRelationshipServiceSupport.CreateCustomerRelationshipModel = GetDefaultValidCustomerRelationshipServiceCreateRequest();
    }

    [When(@"I Post Valid CustomerService CreateCustomerRelationship Request")]
    public void WhenIPostValidCustomerServiceCreateCustomerRelationshipRequest()
    {
      customerRelationshipServiceSupport.PostValidCreateCustomerRelationshipRequestToService();
    }

    [Then(@"The CreateCustomerRelationshipEvent Details Are Stored In VSS DB")]
    public void ThenTheCreateCustomerRelationshipEventDetailsAreStoredInVSSDB()
    {
      CommonUtil.WaitToProcess("2");

      string parentCustomerUid = customerRelationshipServiceSupport.CreateCustomerRelationshipModel.ParentCustomerUID.ToString();
      string childCustomerUid = customerRelationshipServiceSupport.CreateCustomerRelationshipModel.ChildCustomerUID.ToString();

      List<string> columnList = new List<string>() { "fk_ParentCustomerUID", "fk_CustomerUID", "fk_RootCustomerUID", "LeftNodePosition", "RightNodePosition" };
      List<string> parentCustomerDetails = new List<string>();
      parentCustomerDetails.Add(parentCustomerUid);
      parentCustomerDetails.Add(parentCustomerUid);
      parentCustomerDetails.Add(parentCustomerUid);
      parentCustomerDetails.Add("1");
      parentCustomerDetails.Add("4");
      string validateQuery = CustomerServiceMySqlQueries.CustomerRelationshipByParentCustomerUID + parentCustomerUid + "'and fk_CustomerUID='" + parentCustomerUid + "'";
      MySqlUtil.ValidateMySQLQuery(MySqlConnectionString, validateQuery, parentCustomerDetails);

      List<string> childcolumnList = new List<string>() { "fk_ParentCustomerUID", "fk_CustomerUID", "fk_RootCustomerUID", "LeftNodePosition", "RightNodePosition" };
      List<string> childCustomerDetails = new List<string>();
      childCustomerDetails.Add(parentCustomerUid);
      childCustomerDetails.Add(childCustomerUid);
      childCustomerDetails.Add(parentCustomerUid);
      childCustomerDetails.Add("2");
      childCustomerDetails.Add("3");
      string validateChildCustomerQuery = CustomerServiceMySqlQueries.CustomerRelationshipByChildCustomerUID + childCustomerUid + "'";
      MySqlUtil.ValidateMySQLQuery(MySqlConnectionString, validateChildCustomerQuery, childCustomerDetails);
    }

    [Then(@"The CreateCustomerRelationshipEvent Message must be available in Kafka topic")]
    public void ThenTheCreateCustomerRelationshipEventMessageMustBeAvailableInKafkaTopic()
    {
      customerRelationshipServiceSupport.VerifyCustomerRelationshipServiceCreateResponse();
    }

    [Given(@"CustomerService DeleteCustomerRelationship Request Is Setup With Default Values")]
    public void GivenCustomerServiceDeleteCustomerRelationshipRequestIsSetupWithDefaultValues()
    {
      customerRelationshipServiceSupport.CreateCustomerRelationshipModel = GetDefaultValidCustomerRelationshipServiceCreateRequest();
      customerRelationshipServiceSupport.PostValidCreateCustomerRelationshipRequestToService();
      customerRelationshipServiceSupport.DeleteCustomerRelationshipModel = GetDefaultValidCustomerRelationshipServiceDeleteRequest();
    }

    [When(@"I Post Valid CustomerService DeleteCustomerRelationship Request")]
    public void WhenIPostValidCustomerServiceDeleteCustomerRelationshipRequest()
    {
      customerRelationshipServiceSupport.PostValidDeleteCustomerRelationshipRequestToService(customerRelationshipServiceSupport.DeleteCustomerRelationshipModel.ParentCustomerUID, customerRelationshipServiceSupport.DeleteCustomerRelationshipModel.ChildCustomerUID, 
        customerRelationshipServiceSupport.DeleteCustomerRelationshipModel.ActionUTC);
    }

    [Then(@"The DeleteCustomerRelationshipEvent Details Are Stored In VSS DB")]
    public void ThenTheDeleteCustomerRelationshipEventDetailsAreStoredInVSSDB()
    {
      CommonUtil.WaitToProcess("2");

      string parentCustomerUid = customerRelationshipServiceSupport.CreateCustomerRelationshipModel.ParentCustomerUID.ToString();
      string childCustomerUid = customerRelationshipServiceSupport.CreateCustomerRelationshipModel.ChildCustomerUID.ToString();

      List<string> columnList = new List<string>() { "fk_ParentCustomerUID", "fk_CustomerUID", "fk_RootCustomerUID", "LeftNodePosition", "RightNodePosition" };
      List<string> parentCustomerDetails = new List<string>();
      parentCustomerDetails.Add(parentCustomerUid);
      parentCustomerDetails.Add(parentCustomerUid);
      parentCustomerDetails.Add(parentCustomerUid);
      parentCustomerDetails.Add("1");
      parentCustomerDetails.Add("2");
      string validateQuery = CustomerServiceMySqlQueries.CustomerRelationshipByParentCustomerUID + parentCustomerUid + "'";
      MySqlUtil.ValidateMySQLQuery(MySqlConnectionString, validateQuery, parentCustomerDetails);

      List<string> childcolumnList = new List<string>() { "fk_ParentCustomerUID", "fk_CustomerUID", "fk_RootCustomerUID", "LeftNodePosition", "RightNodePosition" };
      List<string> childCustomerDetails = new List<string>();
      childCustomerDetails.Add(childCustomerUid);
      childCustomerDetails.Add(childCustomerUid);
      childCustomerDetails.Add(childCustomerUid);
      childCustomerDetails.Add("1");
      childCustomerDetails.Add("2");
      string validateChildCustomerQuery = CustomerServiceMySqlQueries.CustomerRelationshipByChildCustomerUID + childCustomerUid + "'";
      MySqlUtil.ValidateMySQLQuery(MySqlConnectionString, validateChildCustomerQuery, childCustomerDetails);
    }

    [Then(@"The DeleteCustomerRelationshipEvent Message must be available in Kafka topic")]
    public void ThenTheDeleteCustomerRelationshipEventMessageMustBeAvailableInKafkaTopic()
    {
      customerRelationshipServiceSupport.VerifyCustomerRelationshipServiceDeleteResponse();
    }

    #endregion

    #region Helpers

    public static CreateCustomerRelationshipEvent GetDefaultValidCustomerRelationshipServiceCreateRequest()
    {
      CreateCustomerRelationshipEvent defaultValidCustomerRelationshipServiceCreateModel = new CreateCustomerRelationshipEvent();
      defaultValidCustomerRelationshipServiceCreateModel.ParentCustomerUID = Guid.NewGuid();
      defaultValidCustomerRelationshipServiceCreateModel.ChildCustomerUID = Guid.NewGuid();
      defaultValidCustomerRelationshipServiceCreateModel.ActionUTC = DateTime.UtcNow;
      defaultValidCustomerRelationshipServiceCreateModel.ReceivedUTC = null;
      return defaultValidCustomerRelationshipServiceCreateModel;
    }

    public static DeleteCustomerRelationshipEvent GetDefaultValidCustomerRelationshipServiceDeleteRequest(string parentUid=null,string childUid=null)
    {
      DeleteCustomerRelationshipEvent defaultValidCustomerRelationshipServiceDeleteModel = new DeleteCustomerRelationshipEvent();
      defaultValidCustomerRelationshipServiceDeleteModel.ParentCustomerUID = String.IsNullOrEmpty(parentUid) ? customerRelationshipServiceSupport.CreateCustomerRelationshipModel.ParentCustomerUID : Guid.Parse(parentUid);
      defaultValidCustomerRelationshipServiceDeleteModel.ChildCustomerUID = String.IsNullOrEmpty(childUid) ? customerRelationshipServiceSupport.CreateCustomerRelationshipModel.ChildCustomerUID : Guid.Parse(childUid);
      defaultValidCustomerRelationshipServiceDeleteModel.ActionUTC = DateTime.UtcNow;
      defaultValidCustomerRelationshipServiceDeleteModel.ReceivedUTC = null;
      return defaultValidCustomerRelationshipServiceDeleteModel;
    }

    #endregion
  }
}
