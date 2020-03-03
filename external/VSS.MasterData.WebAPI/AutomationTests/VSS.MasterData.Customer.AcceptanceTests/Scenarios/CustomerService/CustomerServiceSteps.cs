using AutomationCore.API.Framework.Library;
using AutomationCore.Shared.Library;
using System;
using System.Collections.Generic;
using System.Net;
using TechTalk.SpecFlow;
using VSS.MasterData.Customer.AcceptanceTests.Utils.Config;
using VSS.MasterData.Customer.AcceptanceTests.Utils.Features.Classes.CustomerService;


namespace VSS.MasterData.Customer.AcceptanceTests.Scenarios.CustomerService
{
    [Binding]
    public class CustomerServiceSteps
    {

        #region Variables

        //DB Configuration
        public static string MySqlConnectionString;
        public static string MySqlDBName = CustomerServiceConfig.MySqlDBName;

        public string TestName;
        private static Log4Net Log = new Log4Net(typeof(CustomerServiceSteps));
        private static CustomerServiceSupport customerServiceSupport = new CustomerServiceSupport(Log);

        #endregion

        #region StepDefinition

        [BeforeFeature()]
        public static void InitializeKafka()
        {
            if (FeatureContext.Current.FeatureInfo.Title.Equals("CustomerService"))
            {
                KafkaServicesConfig.InitializeKafkaConsumer(customerServiceSupport);
            }
        }

        public CustomerServiceSteps()
        {
            MySqlConnectionString = CustomerServiceConfig.MySqlConnection ;
        }

        [Given(@"CustomerService Is Ready To Verify '(.*)'")]
        public void GivenCustomerServiceIsReadyToVerify(string TestDescription)
        {
            //log the scenario info
            TestName = (ScenarioContext.Current.ScenarioInfo).Title + "_" + TestDescription;
            //TestName = TestDescription;
            LogResult.Report(Log, "log_ForInfo", "Execution started for Test Scenario" + TestName);
        }

        [Given(@"CustomerServiceCreate Request Is Setup With Default Values")]
        public void GivenCustomerServiceCreateRequestIsSetupWithDefaultValues()
        {
            customerServiceSupport.CreateCustomerModel = GetDefaultValidCustomerServiceCreateRequest();
        }

        [When(@"I Post Valid CustomerServiceCreate Request")]
        public void WhenIPostValidCustomerServiceCreateRequest()
        {
            customerServiceSupport.PostValidCreateRequestToService();
        }

        [When(@"The Processed CustomerServiceCreate Message must be available in Kafka topic")]
        public void WhenTheProcessedCustomerServiceCreateMessageMustBeAvailableInKafkaTopic()
        {
            customerServiceSupport.VerifyCustomerServiceCreateResponse();
        }


        [Then(@"The Processed CustomerServiceCreate Message must be available in Kafka topic")]
        public void ThenTheProcessedCustomerServiceCreateMessageMustBeAvailableInKafkaTopic()
        {
            customerServiceSupport.VerifyCustomerServiceCreateResponse();
        }

        [Given(@"CustomerServiceUpdate Request Is Setup With Default Values")]
        public void GivenCustomerServiceUpdateRequestIsSetupWithDefaultValues()
        {
            customerServiceSupport.CreateCustomerModel = GetDefaultValidCustomerServiceCreateRequest();
            customerServiceSupport.PostValidCreateRequestToService();
            customerServiceSupport.UpdateCustomerModel = GetDefaultValidCustomerServiceUpdateRequest();
        }

        [When(@"I Set CustomerServiceUpdate DealerNetwork To '(.*)'")]
        public void WhenISetCustomerServiceUpdateDealerNetworkTo(string dealerNetwork)
        {
            customerServiceSupport.UpdateCustomerModel.DealerNetwork = InputGenerator.GetValue(dealerNetwork);
        }

        [When(@"I Set Invalid CustomerServiceUpdate CustomerName To '(.*)'")]
        public void WhenISetInvalidCustomerServiceUpdateCustomerNameTo(string customerName)
        {
            customerServiceSupport.InvalidUpdateCustomerModel.CustomerName = InputGenerator.GetValue(customerName);
        }

        [When(@"I Set Invalid CustomerServiceUpdate DealerNetwork To '(.*)'")]
        public void WhenISetInvalidCustomerServiceUpdateDealerNetworkTo(string dealerNetwork)
        {
            customerServiceSupport.InvalidUpdateCustomerModel.DealerNetwork = InputGenerator.GetValue(dealerNetwork);
        }

        [When(@"I Set Invalid CustomerServiceUpdate NetworkDealerCode To '(.*)'")]
        public void WhenISetInvalidCustomerServiceUpdateNetworkDealerCodeTo(string networkDealercode)
        {
            customerServiceSupport.InvalidUpdateCustomerModel.NetworkDealerCode = InputGenerator.GetValue(networkDealercode);
        }

        [When(@"I Set Invalid CustomerServiceUpdate NetworkCustomerCode To '(.*)'")]
        public void WhenISetInvalidCustomerServiceUpdateNetworkCustomerCodeTo(string networkCustomercode)
        {
            customerServiceSupport.InvalidUpdateCustomerModel.NetworkCustomerCode = InputGenerator.GetValue(networkCustomercode);
        }

        [When(@"I Set Invalid CustomerServiceUpdate DealerAccountCode To '(.*)'")]
        public void WhenISetInvalidCustomerServiceUpdateDealerAccountCodeTo(string dealerAccountcode)
        {
            customerServiceSupport.InvalidUpdateCustomerModel.DealerAccountCode = InputGenerator.GetValue(dealerAccountcode);
        }

        [When(@"I Set Invalid CustomerServiceUpdate PrimaryContactEmail To '(.*)'")]
        public void WhenISetInvalidCustomerServiceUpdatePrimaryContactEmailTo(string primaryContactEmail)
        {
            customerServiceSupport.InvalidUpdateCustomerModel.PrimaryContactEmail = InputGenerator.GetValue(primaryContactEmail);
        }

        [When(@"I Set Invalid CustomerServiceUpdate FirstName To '(.*)'")]
        public void WhenISetInvalidCustomerServiceUpdateFirstNameTo(string firstName)
        {
            customerServiceSupport.InvalidUpdateCustomerModel.FirstName = InputGenerator.GetValue(firstName);
        }

        [When(@"I Set Invalid CustomerServiceUpdate LastName To '(.*)'")]
        public void WhenISetInvalidCustomerServiceUpdateLastNameTo(string lastName)
        {
            customerServiceSupport.InvalidUpdateCustomerModel.LastName = InputGenerator.GetValue(lastName);
        }

        [When(@"I Post Valid CustomerServiceUpdate Request")]
        public void WhenIPostValidCustomerServiceUpdateRequest()
        {
            customerServiceSupport.PostValidUpdateRequestToService();
        }

        [Then(@"The Processed CustomerServiceUpdate Message must be available in Kafka topic")]
        public void ThenTheProcessedCustomerServiceUpdateMessageMustBeAvailableInKafkaTopic()
        {
            customerServiceSupport.VerifyCustomerServiceUpdateResponse();
        }

        [Given(@"CustomerServiceDelete Request Is Setup With Default Values")]
        public void GivenCustomerServiceDeleteRequestIsSetupWithDefaultValues()
        {
            customerServiceSupport.CreateCustomerModel = GetDefaultValidCustomerServiceCreateRequest();
            customerServiceSupport.PostValidCreateRequestToService();
            customerServiceSupport.DeleteCustomerModel = GetDefaultValidCustomerServiceDeleteRequest();
        }

        [When(@"I Post Valid CustomerServiceDelete Request")]
        public void WhenIPostValidCustomerServiceDeleteRequest()
        {
            //customerServiceSupport.SetupDeleteCustomerKafkaConsumer(customerServiceSupport.DeleteCustomerModel.CustomerUID, customerServiceSupport.DeleteCustomerModel.ActionUTC);

            customerServiceSupport.PostValidDeleteRequestToService(customerServiceSupport.DeleteCustomerModel.CustomerUID, customerServiceSupport.DeleteCustomerModel.ActionUTC);
        }

        [Then(@"The Processed CustomerServiceDelete Message must be available in Kafka topic")]
        public void ThenTheProcessedCustomerServiceDeleteMessageMustBeAvailableInKafkaTopic()
        {
            customerServiceSupport.VerifyCustomerServiceDeleteResponse();
        }

        [When(@"I Set CustomerServiceCreate NetworkDealerCode To '(.*)'")]
        public void WhenISetCustomerServiceCreateNetworkDealerCodeTo(string networkDealerCode)
        {
            customerServiceSupport.CreateCustomerModel.NetworkDealerCode = InputGenerator.GetValue(networkDealerCode);
        }

        [When(@"I Set CustomerServiceCreate NetworkCustomerCode To '(.*)'")]
        public void WhenISetCustomerServiceCreateNetworkCustomerCodeTo(string networkCustomerCode)
        {
            customerServiceSupport.CreateCustomerModel.NetworkCustomerCode = InputGenerator.GetValue(networkCustomerCode);
        }

        [When(@"I Set CustomerServiceCreate CustomerType To '(.*)'")]
        public void WhenISetCustomerServiceCreateCustomerTypeTo(string customerType)
        {
            customerServiceSupport.CreateCustomerModel.CustomerType = InputGenerator.GetValue(customerType); ;
        }

        [When(@"I Set CustomerServiceCreate DealerAccountCode To '(.*)'")]
        public void WhenISetCustomerServiceCreateDealerAccountCodeTo(string dealerAccountCode)
        {
            customerServiceSupport.CreateCustomerModel.DealerAccountCode = InputGenerator.GetValue(dealerAccountCode);
        }

        [When(@"I Set CustomerServiceCreate PrimaryContactEmail To '(.*)'")]
        public void WhenISetCustomerServiceCreatePrimaryContactEmailTo(string primaryContactEmail)
        {
            customerServiceSupport.CreateCustomerModel.PrimaryContactEmail = InputGenerator.GetValue(primaryContactEmail);
        }

        [When(@"I Set CustomerServiceCreate FirstName To '(.*)'")]
        public void WhenISetCustomerServiceCreateFirstNameTo(string firstName)
        {
            customerServiceSupport.CreateCustomerModel.FirstName = InputGenerator.GetValue(firstName);
        }

        [When(@"I Set CustomerServiceCreate LastName To '(.*)'")]
        public void WhenISetCustomerServiceCreateLastNameTo(string lastName)
        {
            customerServiceSupport.CreateCustomerModel.LastName = InputGenerator.GetValue(lastName);
        }

        [When(@"I Set CustomerServiceUpdate CustomerName To '(.*)'")]
        public void WhenISetCustomerServiceUpdateCustomerNameTo(string customerName)
        {
            customerServiceSupport.UpdateCustomerModel.CustomerName = InputGenerator.GetValue(customerName);
        }

        [When(@"I Set CustomerServiceUpdate NetworkDealerCode To '(.*)'")]
        public void WhenISetCustomerServiceUpdateNetworkDealerCodeTo(string networkDealerCode)
        {
            customerServiceSupport.UpdateCustomerModel.NetworkDealerCode = InputGenerator.GetValue(networkDealerCode);
        }

        [When(@"I Set CustomerServiceUpdate NetworkCustomerCode To '(.*)'")]
        public void WhenISetCustomerServiceUpdateNetworkCustomerCodeTo(string networkCustomerCode)
        {
            customerServiceSupport.UpdateCustomerModel.NetworkCustomerCode = InputGenerator.GetValue(networkCustomerCode);
        }

        [When(@"I Set CustomerServiceUpdate BSSID To '(.*)'")]
        public void WhenISetCustomerServiceUpdateBSSIDTo(string bssId)
        {
            customerServiceSupport.UpdateCustomerModel.BSSID = InputGenerator.GetValue(bssId);
        }

        [When(@"I Set CustomerServiceUpdate DealerAccountCode To '(.*)'")]
        public void WhenISetCustomerServiceUpdateDealerAccountCodeTo(string dealerAccountCode)
        {
            customerServiceSupport.UpdateCustomerModel.DealerAccountCode = InputGenerator.GetValue(dealerAccountCode);
        }

        [When(@"I Set CustomerServiceUpdate PrimaryContactEmail To '(.*)'")]
        public void WhenISetCustomerServiceUpdatePrimaryContactEmailTo(string primaryContactEmail)
        {
            customerServiceSupport.UpdateCustomerModel.PrimaryContactEmail = InputGenerator.GetValue(primaryContactEmail);
        }

        [When(@"I Set CustomerServiceUpdate FirstName To '(.*)'")]
        public void WhenISetCustomerServiceUpdateFirstNameTo(string firstName)
        {
            customerServiceSupport.UpdateCustomerModel.FirstName = InputGenerator.GetValue(firstName);
        }

        [When(@"I Set CustomerServiceUpdate LastName To '(.*)'")]
        public void WhenISetCustomerServiceUpdateLastNameTo(string lastName)
        {
            customerServiceSupport.UpdateCustomerModel.LastName = InputGenerator.GetValue(lastName);
        }

        [Given(@"CustomerServiceCreate Request Is Setup With Invalid Default Values")]
        public void GivenCustomerServiceCreateRequestIsSetupWithInvalidDefaultValues()
        {
            customerServiceSupport.InvalidCreateCustomerModel = GetDefaultInValidCustomerServiceCreateRequest();
        }

        [When(@"I Set Invalid CustomerServiceCreate CustomerName To '(.*)'")]
        public void WhenISetInvalidCustomerServiceCreateCustomerNameTo(string customerName)
        {
            customerServiceSupport.InvalidCreateCustomerModel.CustomerName = InputGenerator.GetValue(customerName);
        }

        [When(@"I Post Invalid CustomerServiceCreate Request")]
        public void WhenIPostInvalidCustomerServiceCreateRequest()
        {
            string contentType = "application/json";
            customerServiceSupport.PostInValidCreateRequestToService(contentType, HttpStatusCode.BadRequest);
        }

        [Then(@"CustomerServiceCreate Response With '(.*)' Should Be Returned")]
        public void ThenCustomerServiceCreateResponseWithShouldBeReturned(string errorMessage)
        {
            customerServiceSupport.VerifyErrorResponse(errorMessage);
        }

        [When(@"I Set Invalid CustomerServiceCreate CustomerType To '(.*)'")]
        public void WhenISetInvalidCustomerServiceCreateCustomerTypeTo(string customerType)
        {
            customerServiceSupport.InvalidCreateCustomerModel.CustomerType = InputGenerator.GetValue(customerType);
        }

        [When(@"I Set CustomerServiceCreate BSSID To '(.*)'")]
        public void WhenISetCustomerServiceCreateBSSIDTo(string bssId)
        {
            customerServiceSupport.CreateCustomerModel.BSSID = InputGenerator.GetValue(bssId);
        }

        [When(@"I Set Invalid CustomerServiceCreate DealerNetwork To '(.*)'")]
        public void WhenISetInvalidCustomerServiceCreateDealerNetworkTo(string dealerNetwork)
        {
            customerServiceSupport.InvalidCreateCustomerModel.DealerNetwork = InputGenerator.GetValue(dealerNetwork);
        }

        [When(@"I Set Invalid CustomerServiceCreate CustomerUID To '(.*)'")]
        public void WhenISetInvalidCustomerServiceCreateCustomerUIDTo(string customerUid)
        {
            customerServiceSupport.InvalidCreateCustomerModel.CustomerUID = InputGenerator.GetValue(customerUid);
        }

        [When(@"I Set Invalid CustomerServiceCreate ActionUTC To '(.*)'")]
        public void WhenISetInvalidCustomerServiceCreateActionUTCTo(string actionUtc)
        {
            customerServiceSupport.InvalidCreateCustomerModel.ActionUTC = InputGenerator.GetValue(actionUtc);
        }

        [When(@"I Set Invalid CustomerServiceCreate PrimaryContactEmail To '(.*)'")]
        public void WhenISetInvalidCustomerServiceCreatePrimaryContactEmailTo(string primaryContactEmail)
        {
            customerServiceSupport.InvalidCreateCustomerModel.PrimaryContactEmail = InputGenerator.GetValue(primaryContactEmail);
        }

        [Given(@"CustomerServiceUpdate Request Is Setup With Invalid Default Values")]
        public void GivenCustomerServiceUpdateRequestIsSetupWithInvalidDefaultValues()
        {
            customerServiceSupport.InvalidUpdateCustomerModel = GetDefaultInValidCustomerServiceUpdateRequest();
        }

        [When(@"I Set Invalid CustomerServiceUpdate CustomerUID To '(.*)'")]
        public void WhenISetInvalidCustomerServiceUpdateCustomerUIDTo(string customerUid)
        {
            customerServiceSupport.InvalidUpdateCustomerModel.CustomerUID = InputGenerator.GetValue(customerUid);
        }

        [When(@"I Post Invalid CustomerServiceUpdate Request")]
        public void WhenIPostInvalidCustomerServiceUpdateRequest()
        {
            string contentType = "application/json";
            customerServiceSupport.PostInValidUpdateRequestToService(contentType, HttpStatusCode.BadRequest);
        }

        [Then(@"CustomerServiceUpdate Response With '(.*)' Should Be Returned")]
        public void ThenCustomerServiceUpdateResponseWithShouldBeReturned(string errorMessage)
        {
            customerServiceSupport.VerifyErrorResponse(errorMessage);
        }

        [When(@"I Set Invalid CustomerServiceUpdate BSSID To '(.*)'")]
        public void WhenISetInvalidCustomerServiceUpdateBSSIDTo(string bssId)
        {
            customerServiceSupport.InvalidUpdateCustomerModel.BSSID = InputGenerator.GetValue(bssId);
        }

        [When(@"I Set Invalid CustomerServiceUpdate ActionUTC To '(.*)'")]
        public void WhenISetInvalidCustomerServiceUpdateActionUTCTo(string actionUtc)
        {
            customerServiceSupport.InvalidUpdateCustomerModel.ActionUTC = InputGenerator.GetValue(actionUtc);
        }

        [Given(@"CustomerServiceDelete Request Is Setup With Invalid Default Values")]
        public void GivenCustomerServiceDeleteRequestIsSetupWithInvalidDefaultValues()
        {
            customerServiceSupport.InvalidDeleteCustomerModel = GetDefaultInValidCustomerServiceDeleteRequest();
        }

        [When(@"I Set Invalid CustomerServiceDelete CustomerUID To '(.*)'")]
        public void WhenISetInvalidCustomerServiceDeleteCustomerUIDTo(string customerUid)
        {
            customerServiceSupport.InvalidDeleteCustomerModel.CustomerUID = InputGenerator.GetValue(customerUid);
        }

        [When(@"I Post Invalid CustomerServiceDelete Request")]
        public void WhenIPostInvalidCustomerServiceDeleteRequest()
        {
            string contentType = "application/json";
            customerServiceSupport.PostInValidDeleteRequestToService(customerServiceSupport.InvalidDeleteCustomerModel.CustomerUID,
            customerServiceSupport.InvalidDeleteCustomerModel.ActionUTC, contentType, HttpStatusCode.BadRequest);
        }

        [Then(@"CustomerServiceDelete Response With '(.*)' Should Be Returned")]
        public void ThenCustomerServiceDeleteResponseWithShouldBeReturned(string errorMessage)
        {
            customerServiceSupport.VerifyErrorResponse(errorMessage);
        }

        [When(@"I Set Invalid CustomerServiceDelete ActionUTC To '(.*)'")]
        public void WhenISetInvalidCustomerServiceDeleteActionUTCTo(string actionUtc)
        {
            customerServiceSupport.InvalidDeleteCustomerModel.ActionUTC = InputGenerator.GetValue(actionUtc);
        }

        [Then(@"The CreateCustomerEvent Details Are Stored In VSS DB")]
        public void ThenTheCreateCustomerEventDetailsAreStoredInVSSDB()
        {
            string customerUID = customerServiceSupport.CreateCustomerModel.CustomerUID.ToString().Replace("-","");
            string customerName = customerServiceSupport.CreateCustomerModel.CustomerName;
            string customerType = customerServiceSupport.CreateCustomerModel.CustomerType.ToString();
            string primaryContactEmail = customerServiceSupport.CreateCustomerModel.PrimaryContactEmail == null ? "" : customerServiceSupport.CreateCustomerModel.PrimaryContactEmail;
            string firstName = customerServiceSupport.CreateCustomerModel.FirstName == null ? "" : customerServiceSupport.CreateCustomerModel.FirstName;
            string lastName = customerServiceSupport.CreateCustomerModel.LastName == null ? "" : customerServiceSupport.CreateCustomerModel.LastName;

            CommonUtil.WaitToProcess("2"); //Wait for the data to get persisted in DB

            List<string> columnList = new List<string>() { "CustomerUID", "CustomerName", "CustomerType", "PrimaryContactEmail", "FirstName", "LastName" };
            List<string> createCustomerDetails = new List<string>();
            createCustomerDetails.Add(customerUID.ToUpper());
            createCustomerDetails.Add(customerName);
            createCustomerDetails.Add(GetCustomerTypeId(customerType));
            createCustomerDetails.Add(primaryContactEmail);
            createCustomerDetails.Add(firstName);
            createCustomerDetails.Add(lastName);

            string validateQuery = CustomerServiceMySqlQueries.CustomerDetailsByCustomerUID + customerUID + "')";
            string validateDateQuery = CustomerServiceMySqlQueries.CustomerUpdateUTCByCustomerUID + customerUID + "')";

            MySqlUtil.ValidateMySQLQuery(MySqlConnectionString, validateQuery, createCustomerDetails);
            //MySqlUtil.ValidateMySQLDateValueQuery(MySqlConnectionString, validateDateQuery, DateTime.UtcNow.AddMinutes(-2), "LESS_THAN_DB");
        }


        [Then(@"The UpdateCustomerEvent Details Are Stored In VSS DB")]
        public void ThenTheUpdateCustomerEventDetailsAreStoredInVSSDB()
        {
            string customerUID = customerServiceSupport.UpdateCustomerModel.CustomerUID.ToString().Replace("-","");
            string customerName = String.IsNullOrEmpty(customerServiceSupport.UpdateCustomerModel.CustomerName) ? customerServiceSupport.CreateCustomerModel.CustomerName : customerServiceSupport.UpdateCustomerModel.CustomerName;
            string customerType = customerServiceSupport.CreateCustomerModel.CustomerType.ToString();
            string primaryContactEmail = customerServiceSupport.UpdateCustomerModel.PrimaryContactEmail == null ? customerServiceSupport.CreateCustomerModel.PrimaryContactEmail : customerServiceSupport.UpdateCustomerModel.PrimaryContactEmail;
            string firstName = customerServiceSupport.UpdateCustomerModel.FirstName == null ? customerServiceSupport.CreateCustomerModel.FirstName : customerServiceSupport.UpdateCustomerModel.FirstName;
            string lastName = customerServiceSupport.UpdateCustomerModel.LastName == null ? customerServiceSupport.CreateCustomerModel.LastName : customerServiceSupport.UpdateCustomerModel.LastName;

            CommonUtil.WaitToProcess("2"); //Wait for the data to get persisted in DB

            List<string> columnList = new List<string>() { "CustomerUID", "CustomerName", "CustomerType", "PrimaryContactEmail", "FirstName", "LastName" };
            List<string> updateCustomerDetails = new List<string>();
            updateCustomerDetails.Add(customerUID.ToUpper());
            updateCustomerDetails.Add(customerName);
            updateCustomerDetails.Add(GetCustomerTypeId(customerType));
            updateCustomerDetails.Add(primaryContactEmail);
            updateCustomerDetails.Add(firstName);
            updateCustomerDetails.Add(lastName);

            string validateQuery = CustomerServiceMySqlQueries.CustomerDetailsByCustomerUID + customerUID + "')";
            string validateDateQuery = CustomerServiceMySqlQueries.CustomerUpdateUTCByCustomerUID + customerUID + "')";

            MySqlUtil.ValidateMySQLQuery(MySqlConnectionString, validateQuery, updateCustomerDetails);
            MySqlUtil.ValidateMySQLDateValueQuery(MySqlConnectionString, validateDateQuery, DateTime.UtcNow.AddMinutes(-2), "LESS_THAN_DB");
        }

        [Then(@"The DeleteCustomerEvent Details Are Removed In VSS DB")]
        public void ThenTheDeleteCustomerEventDetailsAreRemovedInVSSDB()
        {
            string customerUID = customerServiceSupport.DeleteCustomerModel.CustomerUID.ToString();

            CommonUtil.WaitToProcess("2"); //Wait for the data to get persisted in DB

            customerUID = customerServiceSupport.DeleteCustomerModel.CustomerUID.ToString();
            string validateQuery = CustomerServiceMySqlQueries.CustomerDetailsUpdateByCustomerUID + customerUID + "'";

            MySqlUtil.ValidateMySQLQueryCount(MySqlConnectionString, validateQuery, 0); // There should be no matching rows.
        }

        [When(@"I Set Duplicate CustomerUID Value To '(.*)'")]
        public void WhenISetDuplicateCustomerUIDValueTo(string customerUID)
        {
            customerUID = MySqlUtil.ExecuteMySQLQueryResult(MySqlConnectionString, CustomerServiceMySqlQueries.CustomerDetails);
            customerServiceSupport.InvalidCreateCustomerModel.CustomerUID = Guid.Parse(customerUID).ToString();
        }


        #endregion

        #region Helpers

        public static CreateCustomerEvent GetDefaultValidCustomerServiceCreateRequest()
        {
            CreateCustomerEvent defaultValidCustomerServiceCreateModel = new CreateCustomerEvent();
            defaultValidCustomerServiceCreateModel.CustomerName = "AutoTestAPICreateCustomerName" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
            defaultValidCustomerServiceCreateModel.CustomerType = "Dealer";
            defaultValidCustomerServiceCreateModel.BSSID = "BSS" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
            defaultValidCustomerServiceCreateModel.DealerNetwork = "AutoTestAPIDealerNetwork" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
            defaultValidCustomerServiceCreateModel.NetworkDealerCode = "AutoTestAPINetworkDealerCode" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
            defaultValidCustomerServiceCreateModel.NetworkCustomerCode = "AutoTestAPINetworkCustomerCode" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
            defaultValidCustomerServiceCreateModel.DealerAccountCode = "AutoTestAPIDealerAccountCode" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
            defaultValidCustomerServiceCreateModel.CustomerUID = Guid.NewGuid();
            defaultValidCustomerServiceCreateModel.ActionUTC = DateTime.UtcNow;
            defaultValidCustomerServiceCreateModel.ReceivedUTC = null;
            defaultValidCustomerServiceCreateModel.PrimaryContactEmail = "AutoTestAPIEmail" + DateTime.UtcNow.ToString("yyyyMMddhhmmss") + "@trimble.com";
            defaultValidCustomerServiceCreateModel.FirstName = "AutoTestAPIFirstName" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
            defaultValidCustomerServiceCreateModel.LastName = "AutoTestAPILastName" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
            return defaultValidCustomerServiceCreateModel;
        }

        public static UpdateCustomerEvent GetDefaultValidCustomerServiceUpdateRequest()
        {
            UpdateCustomerEvent defaultValidCustomerServiceUpdateModel = new UpdateCustomerEvent();
            defaultValidCustomerServiceUpdateModel.CustomerName = "AutoTestAPICreateCustomerName" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
            defaultValidCustomerServiceUpdateModel.BSSID = "BSS" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
            defaultValidCustomerServiceUpdateModel.DealerNetwork = "AutoTestAPIDealerNetwork" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
            defaultValidCustomerServiceUpdateModel.NetworkDealerCode = "AutoTestAPINetworkDealerCode" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
            defaultValidCustomerServiceUpdateModel.NetworkCustomerCode = "AutoTestAPINetworkCustomerCode" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
            defaultValidCustomerServiceUpdateModel.DealerAccountCode = "AutoTestAPIDealerAccountCode" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
            defaultValidCustomerServiceUpdateModel.CustomerUID = customerServiceSupport.CreateCustomerModel.CustomerUID;
            defaultValidCustomerServiceUpdateModel.ActionUTC = DateTime.UtcNow;
            defaultValidCustomerServiceUpdateModel.ReceivedUTC = null;
            defaultValidCustomerServiceUpdateModel.PrimaryContactEmail = "AutoTestAPIEmail" + DateTime.UtcNow.ToString("yyyyMMddhhmmss") + "@trimble.com";
            defaultValidCustomerServiceUpdateModel.FirstName = "AutoTestAPIFirstName" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
            defaultValidCustomerServiceUpdateModel.LastName = "AutoTestAPILastName" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
            return defaultValidCustomerServiceUpdateModel;
        }

        public static DeleteCustomerEvent GetDefaultValidCustomerServiceDeleteRequest()
        {
            DeleteCustomerEvent defaultValidCustomerServiceDeleteModel = new DeleteCustomerEvent();
            defaultValidCustomerServiceDeleteModel.CustomerUID = customerServiceSupport.CreateCustomerModel.CustomerUID;
            defaultValidCustomerServiceDeleteModel.ActionUTC = DateTime.UtcNow;
            defaultValidCustomerServiceDeleteModel.ReceivedUTC = null;
            return defaultValidCustomerServiceDeleteModel;
        }

        public static InvalidCreateCustomerEvent GetDefaultInValidCustomerServiceCreateRequest()
        {
            InvalidCreateCustomerEvent defaultInValidCustomerServiceCreateModel = new InvalidCreateCustomerEvent();
            defaultInValidCustomerServiceCreateModel.CustomerName = "AutoTestAPICreateCustomerName" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
            defaultInValidCustomerServiceCreateModel.CustomerType = "Dealer";
            defaultInValidCustomerServiceCreateModel.BSSID = "BSS" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
            defaultInValidCustomerServiceCreateModel.DealerNetwork = "AutoTestAPIDealerNetwork" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
            defaultInValidCustomerServiceCreateModel.NetworkDealerCode = "AutoTestAPINetworkDealerCode" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
            defaultInValidCustomerServiceCreateModel.NetworkCustomerCode = "AutoTestAPINetworkCustomerCode" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
            defaultInValidCustomerServiceCreateModel.DealerAccountCode = "AutoTestAPIDealerAccountCode" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
            defaultInValidCustomerServiceCreateModel.CustomerUID = Guid.NewGuid().ToString();
            defaultInValidCustomerServiceCreateModel.ActionUTC = DateTime.UtcNow.ToString();
            defaultInValidCustomerServiceCreateModel.ReceivedUTC = null;
            defaultInValidCustomerServiceCreateModel.PrimaryContactEmail = "AutoTestAPIEmail" + DateTime.UtcNow.ToString("yyyyMMddhhmmss") + "@trimble.com";
            defaultInValidCustomerServiceCreateModel.FirstName = "AutoTestAPIFirstName" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
            defaultInValidCustomerServiceCreateModel.LastName = "AutoTestAPILastName" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
            return defaultInValidCustomerServiceCreateModel;
        }

        public static InvalidUpdateCustomerEvent GetDefaultInValidCustomerServiceUpdateRequest()
        {
            InvalidUpdateCustomerEvent defaultInValidCustomerServiceUpdateModel = new InvalidUpdateCustomerEvent();
            defaultInValidCustomerServiceUpdateModel.CustomerName = "AutoTestAPICreateCustomerName" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
            defaultInValidCustomerServiceUpdateModel.BSSID = "BSS" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
            defaultInValidCustomerServiceUpdateModel.DealerNetwork = "AutoTestAPIDealerNetwork" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
            defaultInValidCustomerServiceUpdateModel.NetworkDealerCode = "AutoTestAPINetworkDealerCode" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
            defaultInValidCustomerServiceUpdateModel.NetworkCustomerCode = "AutoTestAPINetworkCustomerCode" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
            defaultInValidCustomerServiceUpdateModel.DealerAccountCode = "AutoTestAPIDealerAccountCode" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
            defaultInValidCustomerServiceUpdateModel.CustomerUID = Guid.NewGuid().ToString();
            defaultInValidCustomerServiceUpdateModel.ActionUTC = DateTime.UtcNow.ToString();
            defaultInValidCustomerServiceUpdateModel.ReceivedUTC = null;
            defaultInValidCustomerServiceUpdateModel.PrimaryContactEmail = "AutoTestAPIEmail" + DateTime.UtcNow.ToString("yyyyMMddhhmmss") + "@trimble.com";
            defaultInValidCustomerServiceUpdateModel.FirstName = "AutoTestAPIFirstName" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
            defaultInValidCustomerServiceUpdateModel.LastName = "AutoTestAPILastName" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
            return defaultInValidCustomerServiceUpdateModel;
        }

        public static InvalidDeleteCustomerEvent GetDefaultInValidCustomerServiceDeleteRequest()
        {
            InvalidDeleteCustomerEvent defaultInValidCustomerServiceDeleteModel = new InvalidDeleteCustomerEvent();
            defaultInValidCustomerServiceDeleteModel.CustomerUID = Guid.NewGuid().ToString();
            defaultInValidCustomerServiceDeleteModel.ActionUTC = DateTime.UtcNow.ToString();
            defaultInValidCustomerServiceDeleteModel.ReceivedUTC = null;
            return defaultInValidCustomerServiceDeleteModel;
        }

        public string GetCustomerTypeId(string customerTypeName)
        {
            CustomerType customerTypeId;
            CustomerType.TryParse(customerTypeName, out customerTypeId);

            return (((int)customerTypeId).ToString());
        }

        #endregion

    }
}
