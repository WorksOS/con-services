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
using VSS.KafkaRESTSupport;
using VSS.MasterData.Subscription.AcceptanceTests.Utils.Config;
using VSS.MasterData.Subscription.AcceptanceTests.Scenarios.SubscriptionService;
using VSS.MasterData.Subscription.AcceptanceTests.Utils.Features.Classes.SubscriptionService;


namespace VSS.SubscriptionService.AcceptanceTests.Scenarios.SubscriptionService
{
    [Binding]
    public class SubscriptionServiceSteps
    {
        #region Variables

        public string TestName;
        public string MySqlConnectionString;
        public string MySqlDBName = "VSS-MasterData-Subscription-Dev";

        private static Log4Net Log = new Log4Net(typeof(SubscriptionServiceSteps));
        private static SubscriptionServiceSupport subscriptionServiceSupport = new SubscriptionServiceSupport(Log);
        public static Guid assetUid1 = Guid.NewGuid();
        public static Guid deviceUid1 = Guid.NewGuid();
        public static Guid assetUid2 = Guid.NewGuid();
        public static Guid deviceUid2 = Guid.NewGuid();
        public static Guid customerUid = Guid.NewGuid();
        public static Guid subscriptionUid1 = Guid.NewGuid();
        public static Guid subscriptionUid2 = Guid.NewGuid();

        public DateTime currentDate = DateTime.UtcNow.AddMinutes(-1);
        public DateTime minDate = DateTime.UtcNow.AddYears(-15);
        public DateTime maxDate = DateTime.UtcNow.AddYears(15);

        #endregion

        #region Step Definition

        [BeforeFeature()]
        public static void InitializeKafka()
        {
            if (FeatureContext.Current.FeatureInfo.Title.Equals("SubscriptionService"))
            {
                KafkaServicesConfig.InitializeKafkaConsumer(subscriptionServiceSupport);
            }
        }

        public SubscriptionServiceSteps()
        {
            MySqlConnectionString = SubscriptionServiceConfig.MySqlConnection + MySqlDBName;
        }


        [Given(@"SubscriptionService Is Ready To Verify '(.*)'")]
        public void GivenSubscriptionServiceIsReadyToVerify(string TestDescription)
        {
            //log the scenario info
            TestName = (ScenarioContext.Current.ScenarioInfo).Title + "_" + TestDescription;
            //TestName = TestDescription;
            LogResult.Report(Log, "log_ForInfo", "Execution started for Test Scenario" + TestName);
        }

        [Given(@"SubscriptionService AssetSubscriptionCreate Request Is Setup With Default Values")]
        public void GivenSubscriptionServiceAssetSubscriptionCreateRequestIsSetupWithDefaultValues()
        {
            subscriptionServiceSupport.CreateAssetSubscriptionModel = GetDefaultValidAssetSubscriptionServiceCreateRequest();

        }

        [Given(@"SubscriptionService AssetSubscriptionUpdate Request Is Setup With Default Values")]
        public void GivenSubscriptionServiceAssetSubscriptionUpdateRequestIsSetupWithDefaultValues()
        {
            subscriptionServiceSupport.CreateAssetSubscriptionModel = GetDefaultValidAssetSubscriptionServiceCreateRequest();
            subscriptionServiceSupport.PostValidAssetSubscriptionCreateRequestToService();
            subscriptionServiceSupport.UpdateAssetSubscriptionModel = GetDefaultValidAssetSubscriptionServiceUpdateRequest();
        }

        [Given(@"SubscriptionService AssetSubscriptionCreate Request Is Setup With Invalid Default Values")]
        public void GivenSubscriptionServiceAssetSubscriptionCreateRequestIsSetupWithInvalidDefaultValues()
        {
            subscriptionServiceSupport.InvalidCreateAssetSubscriptionModel = GetDefaultInValidAssetSubscriptionServiceCreateRequest();
        }

        [Given(@"SubscriptionService AssetSubscriptionUpdate Request Is Setup With Invalid Default Values")]
        public void GivenSubscriptionServiceAssetSubscriptionUpdateRequestIsSetupWithInvalidDefaultValues()
        {
            subscriptionServiceSupport.InvalidUpdateAssetSubscriptionModel = GetDefaultInValidAssetSubscriptionServiceUpdateRequest();
        }

        [When(@"I Post Valid SubscriptionService AssetSubscriptionCreate Request")]
        public void WhenIPostValidSubscriptionServiceCreateRequest()
        {
            //subscriptionServiceSupport.SetupCreateAssetSubscriptionKafkaConsumer(subscriptionServiceSupport.CreateAssetSubscriptionModel.SubscriptionUID, subscriptionServiceSupport.CreateAssetSubscriptionModel.ActionUTC);

            subscriptionServiceSupport.PostValidAssetSubscriptionCreateRequestToService();
        }

        [When(@"I Post Valid SubscriptionService AssetSubscriptionUpdate Request")]
        public void WhenIPostValidSubscriptionServiceUpdateRequest()
        {
            //subscriptionServiceSupport.SetupUpdateAssetSubscriptionKafkaConsumer(subscriptionServiceSupport.UpdateAssetSubscriptionModel.SubscriptionUID, subscriptionServiceSupport.UpdateAssetSubscriptionModel.ActionUTC);

            subscriptionServiceSupport.PostValidAssetSubscriptionUpdateRequestToService();
        }

        [When(@"I Set SubscriptionService AssetSubscriptionCreate SubscriptionType To '(.*)'")]
        public void WhenISetSubscriptionServiceCreateSubscriptionTypeTo(string subscriptionType)
        {
            //  subscriptionServiceSupport.CreateAssetSubscriptionModel.SubscriptionType = (SubscriptionType)Enum.Parse(typeof(SubscriptionType), InputGenerator.GetValue(subscriptionType)); 
            subscriptionServiceSupport.CreateAssetSubscriptionModel.SubscriptionType = InputGenerator.GetValue(subscriptionType);
        }

        [When(@"I Set SubscriptionService AssetSubscriptionCreate DeviceUID To '(.*)'")]
        public void WhenISetSubscriptionServiceAssetSubscriptionCreateDeviceUIDTo(string deviceUid)
        {
            subscriptionServiceSupport.CreateAssetSubscriptionModel.DeviceUID = String.IsNullOrEmpty(InputGenerator.GetValue(deviceUid)) ? (Guid?)null : Guid.Parse(InputGenerator.GetValue(deviceUid));
        }

        [Then(@"I Set SubscriptionService AssetSubscriptionCreate Source To '(.*)'")]
        public void ThenISetSubscriptionServiceAssetSubscriptionCreateSourceTo(string source)
        {
            if (source == "NULL_NULL")
            {
                subscriptionServiceSupport.CreateAssetSubscriptionModel.Source = null;
            }
            else
            {
                subscriptionServiceSupport.CreateAssetSubscriptionModel.Source = source;
            }
        }

        [Then(@"I Set SubscriptionService AssetSubscriptionUpdate Source To '(.*)'")]
        public void ThenISetSubscriptionServiceAssetSubscriptionUpdateSourceTo(string source)
        {
            if (source == "NULL_NULL")
            {
                subscriptionServiceSupport.UpdateAssetSubscriptionModel.Source = null;
            }
            else
            {
                subscriptionServiceSupport.UpdateAssetSubscriptionModel.Source = source;
            }
        }


        [When(@"I Set SubscriptionService AssetSubscriptionUpdate DeviceUID To '(.*)'")]
        public void WhenISetSubscriptionServiceAssetSubscriptionUpdateDeviceUIDTo(string deviceUid)
        {
            subscriptionServiceSupport.UpdateAssetSubscriptionModel.DeviceUID = String.IsNullOrEmpty(InputGenerator.GetValue(deviceUid)) ? (Guid?)null : Guid.Parse(InputGenerator.GetValue(deviceUid));
        }

        [When(@"I Set SubscriptionService AssetSubscriptionUpdate AssetUID To '(.*)'")]
        public void WhenISetSubscriptionServiceUpdateAssetUIDTo(string assetUid)
        {
            subscriptionServiceSupport.UpdateAssetSubscriptionModel.AssetUID = String.IsNullOrEmpty(InputGenerator.GetValue(assetUid)) ? (Guid?)null : Guid.Parse(InputGenerator.GetValue(assetUid));
        }

        [When(@"I Set SubscriptionService AssetSubscriptionUpdate SubscriptionType To '(.*)'")]
        public void WhenISetSubscriptionServiceUpdateSubscriptionTypeTo(string subscriptionType)
        {
            //subscriptionServiceSupport.UpdateAssetSubscriptionModel.SubscriptionType = String.IsNullOrEmpty(InputGenerator.GetValue(subscriptionType)) ? (SubscriptionType?)null : (SubscriptionType?)Enum.Parse(typeof(SubscriptionType?), InputGenerator.GetValue(subscriptionType)); 
            subscriptionServiceSupport.UpdateAssetSubscriptionModel.SubscriptionType = InputGenerator.GetValue(subscriptionType);
        }

        [When(@"I Set SubscriptionService AssetSubscriptionUpdate StartDate  To '(.*)'")]
        public void WhenISetSubscriptionServiceUpdateStartDateTo(string startDate)
        {
            subscriptionServiceSupport.UpdateAssetSubscriptionModel.StartDate = String.IsNullOrEmpty(InputGenerator.GetValue(startDate)) ? (DateTime?)null : Convert.ToDateTime(InputGenerator.GetValue(startDate.ToString()));
        }

        [When(@"I Set SubscriptionService AssetSubscriptionUpdate CustomerUID To '(.*)'")]
        public void WhenISetSubscriptionServiceUpdateCustomerUIDTo(string customerUid)
        {
            subscriptionServiceSupport.UpdateAssetSubscriptionModel.CustomerUID = String.IsNullOrEmpty(InputGenerator.GetValue(customerUid)) ? (Guid?)null : Guid.Parse(InputGenerator.GetValue(customerUid));
        }

        [When(@"I Set SubscriptionService AssetSubscriptionUpdate EndDate To '(.*)'")]
        public void WhenISetSubscriptionServiceUpdateEndDateTo(string endDate)
        {
            subscriptionServiceSupport.UpdateAssetSubscriptionModel.EndDate = String.IsNullOrEmpty(InputGenerator.GetValue(endDate)) ? (DateTime?)null : Convert.ToDateTime(InputGenerator.GetValue(endDate.ToString()));
        }

        [When(@"I Set Invalid SubscriptionService AssetSubscriptionCreate SubscriptionUID  To '(.*)'")]
        public void WhenISetInvalidSubscriptionServiceCreateSubscriptionUIDTo(string subscriptionUid)
        {
            subscriptionServiceSupport.InvalidCreateAssetSubscriptionModel.SubscriptionUID = InputGenerator.GetValue(subscriptionUid);
        }

        [When(@"I Post Invalid SubscriptionService AssetSubscriptionCreate Request")]
        public void WhenIPostInvalidSubscriptionServiceCreateRequest()
        {
            string contentType = "application/json";
            subscriptionServiceSupport.PostInValidAssetSubscriptionCreateRequestToService(contentType, HttpStatusCode.BadRequest);
        }

        [When(@"I Set Invalid SubscriptionService AssetSubscriptionCreate CustomerUID To '(.*)'")]
        public void WhenISetInvalidSubscriptionServiceCreateCustomerUIDTo(string customerUid)
        {
            subscriptionServiceSupport.InvalidCreateAssetSubscriptionModel.CustomerUID = InputGenerator.GetValue(customerUid);
        }

        [When(@"I Set Invalid SubscriptionService AssetSubscriptionCreate DeviceUID  To '(.*)'")]
        public void WhenISetInvalidSubscriptionServiceAssetSubscriptionCreateDeviceUIDTo(string deviceUid)
        {
            subscriptionServiceSupport.InvalidCreateAssetSubscriptionModel.DeviceUID = InputGenerator.GetValue(deviceUid);
        }

        [When(@"I Set Invalid SubscriptionService AssetSubscriptionCreate AssetUID To '(.*)'")]
        public void WhenISetInvalidSubscriptionServiceCreateAssetUIDTo(string assetUid)
        {
            subscriptionServiceSupport.InvalidCreateAssetSubscriptionModel.AssetUID = InputGenerator.GetValue(assetUid);
        }

        [When(@"I Set Invalid SubscriptionService AssetSubscriptionCreate SubscriptionType To '(.*)'")]
        public void WhenISetInvalidSubscriptionServiceCreateSubscriptionTypeTo(string subscriptionType)
        {
            subscriptionServiceSupport.InvalidCreateAssetSubscriptionModel.SubscriptionType = InputGenerator.GetValue(subscriptionType);
        }

        [When(@"I Set Invalid SubscriptionService AssetSubscriptionCreate StartDate To '(.*)'")]
        public void WhenISetInvalidSubscriptionServiceCreateStartDateTo(string startDate)
        {
            subscriptionServiceSupport.InvalidCreateAssetSubscriptionModel.StartDate = InputGenerator.GetValue(startDate);
        }

        [When(@"I Set Invalid SubscriptionService AssetSubscriptionCreate EndDate To '(.*)'")]
        public void WhenISetInvalidSubscriptionServiceCreateEndDateTo(string endDate)
        {
            subscriptionServiceSupport.InvalidCreateAssetSubscriptionModel.EndDate = InputGenerator.GetValue(endDate);
        }

        [When(@"I Set Invalid SubscriptionService AssetSubscriptionCreate ActionUTC To '(.*)'")]
        public void WhenISetInvalidSubscriptionServiceCreateActionUTCTo(string actionUtc)
        {
            subscriptionServiceSupport.InvalidCreateAssetSubscriptionModel.ActionUTC = InputGenerator.GetValue(actionUtc);
        }

        [When(@"I Set Invalid SubscriptionService AssetSubscriptionUpdate SubscriptionUID  To '(.*)'")]
        public void WhenISetInvalidSubscriptionServiceUpdateSubscriptionUIDTo(string subscriptionUid)
        {
            subscriptionServiceSupport.InvalidUpdateAssetSubscriptionModel.SubscriptionUID = InputGenerator.GetValue(subscriptionUid);
        }

        [When(@"I Post Invalid SubscriptionService AssetSubscriptionUpdate Request")]
        public void WhenIPostInvalidSubscriptionServiceUpdateRequest()
        {
            string contentType = "application/json";
            subscriptionServiceSupport.PostInValidAssetSubscriptionUpdateRequestToService(contentType, HttpStatusCode.BadRequest);
        }

        [When(@"I Set Invalid SubscriptionService AssetSubscriptionUpdate CustomerUID To '(.*)'")]
        public void WhenISetInvalidSubscriptionServiceUpdateCustomerUIDTo(string customerUid)
        {
            subscriptionServiceSupport.InvalidUpdateAssetSubscriptionModel.CustomerUID = InputGenerator.GetValue(customerUid);
        }

        [When(@"I Set Invalid SubscriptionService AssetSubscriptionUpdate AssetUID To '(.*)'")]
        public void WhenISetInvalidSubscriptionServiceUpdateAssetUIDTo(string assetUid)
        {
            subscriptionServiceSupport.InvalidUpdateAssetSubscriptionModel.AssetUID = InputGenerator.GetValue(assetUid);
        }

        [When(@"I Set Invalid SubscriptionService AssetSubscriptionUpdate SubscriptionType To '(.*)'")]
        public void WhenISetInvalidSubscriptionServiceUpdateSubscriptionTypeTo(string subscriptionType)
        {
            subscriptionServiceSupport.InvalidUpdateAssetSubscriptionModel.SubscriptionType = InputGenerator.GetValue(subscriptionType);
        }

        [When(@"I Set Invalid SubscriptionService AssetSubscriptionUpdate StartDate To '(.*)'")]
        public void WhenISetInvalidSubscriptionServiceUpdateStartDateTo(string startDate)
        {
            subscriptionServiceSupport.InvalidUpdateAssetSubscriptionModel.StartDate = InputGenerator.GetValue(startDate);
        }

        [When(@"I Set Invalid SubscriptionService AssetSubscriptionUpdate EndDate To '(.*)'")]
        public void WhenISetInvalidSubscriptionServiceUpdateEndDateTo(string endDate)
        {
            subscriptionServiceSupport.InvalidUpdateAssetSubscriptionModel.EndDate = InputGenerator.GetValue(endDate);
        }

        [When(@"I Set Invalid SubscriptionService AssetSubscriptionUpdate ActionUTC To '(.*)'")]
        public void WhenISetInvalidSubscriptionServiceUpdateActionUTCTo(string actionUtc)
        {
            subscriptionServiceSupport.InvalidUpdateAssetSubscriptionModel.ActionUTC = InputGenerator.GetValue(actionUtc);
        }

        [Then(@"The Processed SubscriptionService AssetSubscriptionCreate Message must be available in Kafka topic")]
        public void ThenTheProcessedSubscriptionServiceCreateMessageMustBeAvailableInKafkaTopic()
        {
            /*Task.Factory.StartNew(() => subscriptionServiceSupport._consumerWrapper.Consume());
            Thread.Sleep(new TimeSpan(0, 0, 20));
            CreateAssetSubscriptionModel kafkaresponse = subscriptionServiceSupport._checkForAssetSubscriptionCreateHandler.subscriptionEvent;
            Assert.IsTrue(subscriptionServiceSupport._checkForAssetSubscriptionCreateHandler.HasFound()); //Asserts that the CreateAssetEvent has published into the AssetKafkaTopic by validating the presence of the particular AssetUid and actionutc
            subscriptionServiceSupport.VerifyAssetSubscriptionCreateResponse(kafkaresponse);*/

            /*#region RPL
            string groupName = SubscriptionServiceConfig.KafkaGroupName;
            string topicName = SubscriptionServiceConfig.SubscriptionServiceTopic;
            string keyResponseUid = subscriptionServiceSupport.CreateAssetSubscriptionModel.SubscriptionUID.ToString();

            CommonUtil.WaitToProcess(SubscriptionServiceConfig.KafkaWaitTime); //Wait till message pushed into Kafka
            try
            {
              string baseUri = KafkaRESTConsumer.GetKafkaInstanceResponse(groupName);
              var consumeMessage = KafkaRESTConsumer.SearchKafkaConsumedMessage(baseUri, topicName, keyResponseUid);
              CreateAssetSubscriptionModel KafkaResponseJSON = JsonConvert.DeserializeObject<CreateAssetSubscriptionModel>(consumeMessage);
              if (KafkaResponseJSON != null)
                subscriptionServiceSupport.VerifyAssetSubscriptionCreateResponse(KafkaResponseJSON);
              else
              {
                Assert.Fail("Event not available in the kafka topic");
              }
            }

            catch (Exception e)
            {
              LogResult.Report(Log, "log_ForError", "Got Error While Verifying Kafka Message", e);
              throw new Exception(e + " Got Error While Verifying Kafka Message");
            }
            #endregion*/

            subscriptionServiceSupport.VerifyAssetSubscriptionCreateResponse();
        }

        [Then(@"The CreateSubscription Details must be stored in MySql DB")]
        public void ThenTheCreateSubscriptionDetailsMustBeStoredInMySqlDB()
        {
            try
            {
                Assert.IsTrue(subscriptionServiceSupport.ValidateDB("Create"), "DB Verification failed");
                LogResult.Report(Log, "log_ForInfo", "DB Validation Successful\n");
            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While Validating DB", e);
                throw new Exception(e + "Got Error While Validating DB\n");
            }
        }

        [Then(@"The UpdateSubscription Details must be stored in MySql DB")]
        public void ThenTheUpdateSubscriptionDetailsMustBeStoredInMySqlDB()
        {
            try
            {
                Assert.IsTrue(subscriptionServiceSupport.ValidateDB("Update"), "DB Verification failed");
                LogResult.Report(Log, "log_ForInfo", "DB Validation Successful\n");
            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While Validating DB", e);
                throw new Exception(e + "Got Error While Validating DB\n");
            }
        }

        [Then(@"The Processed SubscriptionService AssetSubscriptionUpdate Message must be available in Kafka topic")]
        public void ThenTheProcessedSubscriptionServiceUpdateMessageMustBeAvailableInKafkaTopic()
        {
            /*Task.Factory.StartNew(() => subscriptionServiceSupport._consumerWrapper.Consume());
            Thread.Sleep(new TimeSpan(0, 0, 20));
            UpdateAssetSubscriptionModel kafkaresponse = subscriptionServiceSupport._checkForAssetSubscriptionUpdateHandler.subscriptionEvent;
            Assert.IsTrue(subscriptionServiceSupport._checkForAssetSubscriptionUpdateHandler.HasFound()); //Asserts that the CreateAssetEvent has published into the AssetKafkaTopic by validating the presence of the particular AssetUid and actionutc
            subscriptionServiceSupport.VerifyAssetSubscriptionUpdateResponse(kafkaresponse);*/

            /*#region RPL
            string groupName = SubscriptionServiceConfig.KafkaGroupName;
            string topicName = SubscriptionServiceConfig.SubscriptionServiceTopic;
            string keyResponseUid = subscriptionServiceSupport.UpdateAssetSubscriptionModel.SubscriptionUID.ToString();

            CommonUtil.WaitToProcess(SubscriptionServiceConfig.KafkaWaitTime); //Wait till message pushed into Kafka
            try
            {
              string baseUri = KafkaRESTConsumer.GetKafkaInstanceResponse(groupName);
              var consumeMessage = KafkaRESTConsumer.SearchKafkaConsumedMessage(baseUri, topicName, keyResponseUid);
              UpdateAssetSubscriptionModel KafkaResponseJSON = JsonConvert.DeserializeObject<UpdateAssetSubscriptionModel>(consumeMessage);
              if (KafkaResponseJSON != null)
                subscriptionServiceSupport.VerifyAssetSubscriptionUpdateResponse(KafkaResponseJSON);
              else
              {
                Assert.Fail("Event not available in the kafka topic");
              }
            }

            catch (Exception e)
            {
              LogResult.Report(Log, "log_ForError", "Got Error While Verifying Kafka Message", e);
              throw new Exception(e + " Got Error While Verifying Kafka Message");
            }
            #endregion*/

            subscriptionServiceSupport.VerifyAssetSubscriptionUpdateResponse();
        }

        [Then(@"SubscriptionService AssetSubscriptionCreate Response With '(.*)' Should Be Returned")]
        public void ThenSubscriptionServiceCreateResponseWithShouldBeReturned(string errorMessage)
        {
            subscriptionServiceSupport.VerifyAssetSubscriptionErrorResponse(errorMessage);
        }

        [Then(@"SubscriptionService AssetSubscriptionUpdate Response With '(.*)' Should Be Returned")]
        public void ThenSubscriptionServiceUpdateResponseWithShouldBeReturned(string errorMessage)
        {
            subscriptionServiceSupport.VerifyAssetSubscriptionErrorResponse(errorMessage);
        }

        public static CreateAssetSubscriptionEvent defaultValidAssetSubscriptionServiceCreateModel = new CreateAssetSubscriptionEvent();

        [Then(@"SubscriptionService AssociateProjectSubscription Response With '(.*)' Should Be Returned")]
        public void ThenSubscriptionServiceAssociateProjectSubscriptionResponseWithShouldBeReturned(string errorMessage)
        {
            subscriptionServiceSupport.VerifyAssociateProjectSubscriptionErrorResponse(errorMessage);
        }

        [Then(@"SubscriptionService DissociateProjectSubscription Response With '(.*)' Should Be Returned")]
        public void ThenSubscriptionServiceDissociateProjectSubscriptionResponseWithShouldBeReturned(string errorMessage)
        {
            subscriptionServiceSupport.VerifyDissociateProjectSubscriptionErrorResponse(errorMessage);
        }

        [Given(@"Multiple Subscriptions exist for an asset")]
        public void GivenMultipleSubscriptionsExistForAnAsset()
        {
            subscriptionServiceSupport.CreateAssetSubscriptionModel = GetDefaultValidAssetSubscriptionServiceCreateRequest("Essentials");
            subscriptionServiceSupport.PostValidAssetSubscriptionCreateRequestToService();
            subscriptionServiceSupport.CreateAssetSubscriptionModel = GetDefaultValidAssetSubscriptionServiceCreateRequest("CAT Health");
            subscriptionServiceSupport.PostValidAssetSubscriptionCreateRequestToService();
        }


        [Given(@"SubscriptionType '(.*)' has been setup for multiple assets under a customer")]
        public void GivenSubscriptionTypeHasBeenSetupForMultipleAssetsUnderACustomer(string subscriptionType)
        {
            GetDefaultValidMultipleAssetSubscriptionServiceCreateRequest(subscriptionType);
        }


        public static CreateAssetSubscriptionEvent GetDefaultValidAssetSubscriptionServiceCreateRequest()
        {
            defaultValidAssetSubscriptionServiceCreateModel.SubscriptionUID = subscriptionUid1;
            defaultValidAssetSubscriptionServiceCreateModel.AssetUID = assetUid1;
            defaultValidAssetSubscriptionServiceCreateModel.DeviceUID = deviceUid1;
            defaultValidAssetSubscriptionServiceCreateModel.CustomerUID = customerUid;
            defaultValidAssetSubscriptionServiceCreateModel.SubscriptionType = "Essentials";
            defaultValidAssetSubscriptionServiceCreateModel.Source = "SAV";
            defaultValidAssetSubscriptionServiceCreateModel.StartDate = DateTime.UtcNow;
            defaultValidAssetSubscriptionServiceCreateModel.EndDate = DateTime.UtcNow.AddYears(10);
            defaultValidAssetSubscriptionServiceCreateModel.ActionUTC = DateTime.UtcNow;
            return defaultValidAssetSubscriptionServiceCreateModel;
        }

        public static CreateAssetSubscriptionEvent GetDefaultValidAssetSubscriptionServiceCreateRequest(string subscriptionType)
        {
            defaultValidAssetSubscriptionServiceCreateModel.SubscriptionUID = Guid.NewGuid();
            defaultValidAssetSubscriptionServiceCreateModel.AssetUID = assetUid1;
            defaultValidAssetSubscriptionServiceCreateModel.DeviceUID = deviceUid1;
            defaultValidAssetSubscriptionServiceCreateModel.CustomerUID = customerUid;
            defaultValidAssetSubscriptionServiceCreateModel.SubscriptionType = subscriptionType;
            defaultValidAssetSubscriptionServiceCreateModel.StartDate = DateTime.UtcNow.AddMinutes(-1);
            defaultValidAssetSubscriptionServiceCreateModel.EndDate = DateTime.UtcNow.AddYears(10);
            defaultValidAssetSubscriptionServiceCreateModel.ActionUTC = DateTime.UtcNow.AddMinutes(-1);
            defaultValidAssetSubscriptionServiceCreateModel.ReceivedUTC = null;
            return defaultValidAssetSubscriptionServiceCreateModel;
        }

        public static CreateAssetSubscriptionEvent defaultValidAssetSubscriptionServiceCreateModel1 = new CreateAssetSubscriptionEvent();
        public static CreateAssetSubscriptionEvent defaultValidAssetSubscriptionServiceCreateModel2 = new CreateAssetSubscriptionEvent();
        public void GetDefaultValidMultipleAssetSubscriptionServiceCreateRequest(string subscriptionType)
        {
            defaultValidAssetSubscriptionServiceCreateModel1.SubscriptionUID = subscriptionUid1;
            defaultValidAssetSubscriptionServiceCreateModel1.AssetUID = assetUid1;
            defaultValidAssetSubscriptionServiceCreateModel1.DeviceUID = deviceUid1;
            defaultValidAssetSubscriptionServiceCreateModel1.CustomerUID = customerUid;
            defaultValidAssetSubscriptionServiceCreateModel1.SubscriptionType = subscriptionType;
            defaultValidAssetSubscriptionServiceCreateModel1.StartDate = currentDate;
            defaultValidAssetSubscriptionServiceCreateModel1.EndDate = DateTime.UtcNow.AddYears(10);
            defaultValidAssetSubscriptionServiceCreateModel1.ActionUTC = DateTime.UtcNow.AddMinutes(-1);
            defaultValidAssetSubscriptionServiceCreateModel1.ReceivedUTC = null;

            defaultValidAssetSubscriptionServiceCreateModel2.SubscriptionUID = subscriptionUid2;
            defaultValidAssetSubscriptionServiceCreateModel2.AssetUID = assetUid2;
            defaultValidAssetSubscriptionServiceCreateModel2.DeviceUID = deviceUid2;
            defaultValidAssetSubscriptionServiceCreateModel2.CustomerUID = customerUid;
            defaultValidAssetSubscriptionServiceCreateModel2.SubscriptionType = subscriptionType;
            defaultValidAssetSubscriptionServiceCreateModel2.StartDate = currentDate;
            defaultValidAssetSubscriptionServiceCreateModel2.EndDate = DateTime.UtcNow.AddYears(10);
            defaultValidAssetSubscriptionServiceCreateModel2.ActionUTC = DateTime.UtcNow.AddMinutes(-1);
            defaultValidAssetSubscriptionServiceCreateModel2.ReceivedUTC = null;
        }


        [When(@"I Set Valid SubscriptionService AssetSubscriptionCreate Request StartDate To '(.*)' For First Asset")]
        public void WhenISetValidSubscriptionServiceAssetSubscriptionCreateRequestStartDateToFor(string date)
        {
            if (date == "Min Date")
                defaultValidAssetSubscriptionServiceCreateModel1.StartDate = minDate;

            else if (date == "Current Date")
                defaultValidAssetSubscriptionServiceCreateModel1.StartDate = currentDate;

            else if (date == "Max Date")
                defaultValidAssetSubscriptionServiceCreateModel1.StartDate = maxDate;

        }

        [When(@"I Set Valid SubscriptionService AssetSubscriptionCreate Request EndDate To '(.*)' For First Asset")]
        public void WhenISetValidSubscriptionServiceAssetSubscriptionCreateRequestEndDateToFor(string date)
        {
            if (date == "Min Date")
                defaultValidAssetSubscriptionServiceCreateModel1.EndDate = minDate;

            else if (date == "Current Date")
                defaultValidAssetSubscriptionServiceCreateModel1.EndDate = currentDate;

            else if (date == "Max Date")
                defaultValidAssetSubscriptionServiceCreateModel1.EndDate = maxDate;
        }

        [When(@"I Set Valid SubscriptionService AssetSubscriptionCreate Request StartDate To '(.*)' For Second Asset")]
        public void WhenISetValidSubscriptionServiceAssetSubscriptionCreateRequestStartDateToForSecondAsset(string date)
        {
            if (date == "Min Date")
                defaultValidAssetSubscriptionServiceCreateModel2.EndDate = minDate;

            else if (date == "Current Date")
                defaultValidAssetSubscriptionServiceCreateModel2.EndDate = currentDate;

            else if (date == "Max Date")
                defaultValidAssetSubscriptionServiceCreateModel2.EndDate = maxDate;
        }

        [When(@"I Set Valid SubscriptionService AssetSubscriptionCreate Request EndDate To '(.*)' For Second Asset")]
        public void WhenISetValidSubscriptionServiceAssetSubscriptionCreateRequestEndDateToForSecondAsset(string date)
        {
            if (date == "Min Date")
                defaultValidAssetSubscriptionServiceCreateModel2.EndDate = minDate;

            else if (date == "Current Date")
                defaultValidAssetSubscriptionServiceCreateModel2.EndDate = currentDate;

            else if (date == "Max Date")
                defaultValidAssetSubscriptionServiceCreateModel2.EndDate = maxDate;
        }

        [When(@"I Set Valid SubscriptionService AssetSubscriptionUpdate Request EndDate To '(.*)' For First Asset")]
        public void WhenISetValidSubscriptionServiceAssetSubscriptionUpdateRequestEndDateToForFirstAsset(string date)
        {
            if (date == "Min Date")
            {
                defaultValidAssetSubscriptionServiceUpdateModel.EndDate = minDate;
                defaultValidAssetSubscriptionServiceUpdateModel.ActionUTC = DateTime.UtcNow.AddMinutes(-1);
                defaultValidAssetSubscriptionServiceUpdateModel.SubscriptionUID = defaultValidAssetSubscriptionServiceCreateModel1.SubscriptionUID;
                subscriptionServiceSupport.UpdateAssetSubscriptionModel = defaultValidAssetSubscriptionServiceUpdateModel;
            }

            else if (date == "Current Date")
            {
                defaultValidAssetSubscriptionServiceUpdateModel.EndDate = currentDate;
                defaultValidAssetSubscriptionServiceUpdateModel.ActionUTC = DateTime.UtcNow.AddMinutes(-1);
                defaultValidAssetSubscriptionServiceUpdateModel.SubscriptionUID = defaultValidAssetSubscriptionServiceCreateModel1.SubscriptionUID;
                subscriptionServiceSupport.UpdateAssetSubscriptionModel = defaultValidAssetSubscriptionServiceUpdateModel;
            }

            else if (date == "Max Date")
            {
                defaultValidAssetSubscriptionServiceUpdateModel.EndDate = maxDate;
                defaultValidAssetSubscriptionServiceUpdateModel.ActionUTC = DateTime.UtcNow.AddMinutes(-1);
                defaultValidAssetSubscriptionServiceUpdateModel.SubscriptionUID = defaultValidAssetSubscriptionServiceCreateModel1.SubscriptionUID;
                subscriptionServiceSupport.UpdateAssetSubscriptionModel = defaultValidAssetSubscriptionServiceUpdateModel;
            }
        }

        [When(@"I Set Valid SubscriptionService AssetSubscriptionUpdate Request StartDate To '(.*)' For First Asset")]
        public void WhenISetValidSubscriptionServiceAssetSubscriptionUpdateRequestStartDateToForFirstAsset(string date)
        {
            if (date == "Min Date")
            {
                defaultValidAssetSubscriptionServiceUpdateModel.StartDate = minDate;
                defaultValidAssetSubscriptionServiceUpdateModel.ActionUTC = DateTime.UtcNow.AddMinutes(-1);
                defaultValidAssetSubscriptionServiceUpdateModel.SubscriptionUID = defaultValidAssetSubscriptionServiceCreateModel1.SubscriptionUID;
                subscriptionServiceSupport.UpdateAssetSubscriptionModel = defaultValidAssetSubscriptionServiceUpdateModel;
            }

            else if (date == "Current Date")
            {
                defaultValidAssetSubscriptionServiceUpdateModel.StartDate = currentDate;
                defaultValidAssetSubscriptionServiceUpdateModel.ActionUTC = DateTime.UtcNow.AddMinutes(-1);
                defaultValidAssetSubscriptionServiceUpdateModel.SubscriptionUID = defaultValidAssetSubscriptionServiceCreateModel1.SubscriptionUID;
                subscriptionServiceSupport.UpdateAssetSubscriptionModel = defaultValidAssetSubscriptionServiceUpdateModel;
            }

            else if (date == "Max Date")
            {
                defaultValidAssetSubscriptionServiceUpdateModel.StartDate = maxDate;
                defaultValidAssetSubscriptionServiceUpdateModel.ActionUTC = DateTime.UtcNow.AddMinutes(-1);
                defaultValidAssetSubscriptionServiceUpdateModel.SubscriptionUID = defaultValidAssetSubscriptionServiceCreateModel1.SubscriptionUID;
                subscriptionServiceSupport.UpdateAssetSubscriptionModel = defaultValidAssetSubscriptionServiceUpdateModel;
            }
        }


        [When(@"I Post Valid SubscriptionService Create Request For Multiple Asset Subscriptions")]
        public void WhenIPostValidSubscriptionServiceCreateRequestForMultipleAssetSubscriptions()
        {
            subscriptionServiceSupport.CreateAssetSubscriptionModel = defaultValidAssetSubscriptionServiceCreateModel1;
            subscriptionServiceSupport.PostValidAssetSubscriptionCreateRequestToService();
            subscriptionServiceSupport.CreateAssetSubscriptionModel = defaultValidAssetSubscriptionServiceCreateModel2;
            subscriptionServiceSupport.PostValidAssetSubscriptionCreateRequestToService();
        }

        public static UpdateAssetSubscriptionEvent defaultValidAssetSubscriptionServiceUpdateModel = new UpdateAssetSubscriptionEvent();
        public static UpdateAssetSubscriptionEvent GetDefaultValidAssetSubscriptionServiceUpdateRequest()
        {

            defaultValidAssetSubscriptionServiceUpdateModel.SubscriptionUID = defaultValidAssetSubscriptionServiceCreateModel.SubscriptionUID;
            defaultValidAssetSubscriptionServiceUpdateModel.AssetUID = assetUid1;
            defaultValidAssetSubscriptionServiceUpdateModel.DeviceUID = deviceUid1;
            defaultValidAssetSubscriptionServiceUpdateModel.SubscriptionType = "Standard Health";
            defaultValidAssetSubscriptionServiceUpdateModel.Source = "Store";
            defaultValidAssetSubscriptionServiceUpdateModel.StartDate = DateTime.UtcNow;
            defaultValidAssetSubscriptionServiceUpdateModel.EndDate = DateTime.UtcNow.AddYears(10);
            defaultValidAssetSubscriptionServiceUpdateModel.ActionUTC = DateTime.UtcNow;
            return defaultValidAssetSubscriptionServiceUpdateModel;
        }

        public static InvalidCreateAssetSubscriptionEvent GetDefaultInValidAssetSubscriptionServiceCreateRequest()
        {
            InvalidCreateAssetSubscriptionEvent defaultInValidSubscriptionServiceCreateModel = new InvalidCreateAssetSubscriptionEvent();
            defaultInValidSubscriptionServiceCreateModel.SubscriptionUID = Guid.NewGuid().ToString();
            defaultInValidSubscriptionServiceCreateModel.AssetUID = Guid.NewGuid().ToString();
            defaultInValidSubscriptionServiceCreateModel.DeviceUID = Guid.NewGuid().ToString();
            defaultInValidSubscriptionServiceCreateModel.CustomerUID = Guid.NewGuid().ToString();
            defaultInValidSubscriptionServiceCreateModel.SubscriptionType = "StandardHealth";
            defaultInValidSubscriptionServiceCreateModel.StartDate = DateTime.UtcNow.ToString();
            defaultInValidSubscriptionServiceCreateModel.EndDate = DateTime.UtcNow.AddYears(10).ToString();
            defaultInValidSubscriptionServiceCreateModel.ActionUTC = DateTime.UtcNow.ToString();
            return defaultInValidSubscriptionServiceCreateModel;
        }

        public static InvalidUpdateAssetSubscriptionEvent GetDefaultInValidAssetSubscriptionServiceUpdateRequest()
        {
            InvalidUpdateAssetSubscriptionEvent defaultInValidSubscriptionServiceUpdateModel = new InvalidUpdateAssetSubscriptionEvent();
            defaultInValidSubscriptionServiceUpdateModel.SubscriptionUID = Guid.NewGuid().ToString();
            defaultInValidSubscriptionServiceUpdateModel.AssetUID = Guid.NewGuid().ToString();
            defaultInValidSubscriptionServiceUpdateModel.DeviceUID = Guid.NewGuid().ToString();
            defaultInValidSubscriptionServiceUpdateModel.CustomerUID = Guid.NewGuid().ToString();
            defaultInValidSubscriptionServiceUpdateModel.SubscriptionType = "ManualMaintenanceLog";
            defaultInValidSubscriptionServiceUpdateModel.StartDate = DateTime.UtcNow.ToString();
            defaultInValidSubscriptionServiceUpdateModel.EndDate = DateTime.UtcNow.AddYears(10).ToString();
            defaultInValidSubscriptionServiceUpdateModel.ActionUTC = DateTime.UtcNow.ToString();
            return defaultInValidSubscriptionServiceUpdateModel;
        }

        public static CreateProjectSubscriptionEvent defaultValidProjectSubscriptionServiceCreateModel = new CreateProjectSubscriptionEvent();

        public static CreateProjectSubscriptionEvent GetDefaultValidProjectSubscriptionServiceCreateRequest()
        {
            defaultValidProjectSubscriptionServiceCreateModel.SubscriptionUID = Guid.NewGuid();
            defaultValidProjectSubscriptionServiceCreateModel.CustomerUID = customerUid;
            defaultValidProjectSubscriptionServiceCreateModel.SubscriptionType = "Landfill";
            defaultValidProjectSubscriptionServiceCreateModel.StartDate = DateTime.UtcNow;
            defaultValidProjectSubscriptionServiceCreateModel.EndDate = DateTime.UtcNow.AddYears(10);
            defaultValidProjectSubscriptionServiceCreateModel.ActionUTC = DateTime.UtcNow;
            return defaultValidProjectSubscriptionServiceCreateModel;
        }

        public static UpdateProjectSubscriptionEvent GetDefaultValidProjectSubscriptionServiceUpdateRequest()
        {
            UpdateProjectSubscriptionEvent defaultValidProjectSubscriptionServiceUpdateModel = new UpdateProjectSubscriptionEvent();
            defaultValidProjectSubscriptionServiceUpdateModel.SubscriptionUID = defaultValidProjectSubscriptionServiceCreateModel.SubscriptionUID;
            defaultValidProjectSubscriptionServiceUpdateModel.CustomerUID = customerUid;
            defaultValidProjectSubscriptionServiceUpdateModel.SubscriptionType = "Project Monitoring";
            defaultValidProjectSubscriptionServiceUpdateModel.StartDate = DateTime.UtcNow;
            defaultValidProjectSubscriptionServiceUpdateModel.EndDate = DateTime.UtcNow.AddYears(10);
            defaultValidProjectSubscriptionServiceUpdateModel.ActionUTC = DateTime.UtcNow;
            return defaultValidProjectSubscriptionServiceUpdateModel;
        }

        public static CreateCustomerSubscriptionEvent defaultValidCustomerSubscriptionServiceCreateModel = new CreateCustomerSubscriptionEvent();

        public static CreateCustomerSubscriptionEvent GetDefaultValidCustomerSubscriptionServiceCreateRequest()
        {
            defaultValidCustomerSubscriptionServiceCreateModel.SubscriptionUID = Guid.NewGuid();
            defaultValidCustomerSubscriptionServiceCreateModel.CustomerUID = customerUid;
            defaultValidCustomerSubscriptionServiceCreateModel.SubscriptionType = "Operator Id/ Manage Operators";
            defaultValidCustomerSubscriptionServiceCreateModel.StartDate = DateTime.UtcNow;
            defaultValidCustomerSubscriptionServiceCreateModel.EndDate = DateTime.UtcNow.AddYears(10);
            defaultValidCustomerSubscriptionServiceCreateModel.ActionUTC = DateTime.UtcNow;
            return defaultValidCustomerSubscriptionServiceCreateModel;
        }



        public static UpdateCustomerSubscriptionEvent GetDefaultValidCustomerSubscriptionServiceUpdateRequest()
        {
            UpdateCustomerSubscriptionEvent defaultValidCustomerSubscriptionServiceUpdateModel = new UpdateCustomerSubscriptionEvent();
            defaultValidCustomerSubscriptionServiceUpdateModel.SubscriptionUID = defaultValidCustomerSubscriptionServiceCreateModel.SubscriptionUID;
            defaultValidCustomerSubscriptionServiceUpdateModel.StartDate = DateTime.UtcNow.AddDays(10);
            defaultValidCustomerSubscriptionServiceUpdateModel.EndDate = DateTime.UtcNow.AddDays(10).AddYears(10);
            defaultValidCustomerSubscriptionServiceUpdateModel.ActionUTC = DateTime.UtcNow;
            return defaultValidCustomerSubscriptionServiceUpdateModel;
        }

        public static AssociateProjectSubscriptionEvent defaultValidAssociateProjectSubscriptionServiceModel = new AssociateProjectSubscriptionEvent();

        public static AssociateProjectSubscriptionEvent GetDefaultValidAssociateProjectSubscriptionServiceRequest()
        {
            defaultValidAssociateProjectSubscriptionServiceModel.SubscriptionUID = Guid.NewGuid();
            defaultValidAssociateProjectSubscriptionServiceModel.ProjectUID = Guid.NewGuid();
            defaultValidAssociateProjectSubscriptionServiceModel.EffectiveDate = DateTime.UtcNow.AddSeconds(-15);
            defaultValidAssociateProjectSubscriptionServiceModel.ActionUTC = DateTime.UtcNow;
            return defaultValidAssociateProjectSubscriptionServiceModel;
        }

        public static DissociateProjectSubscriptionEvent defaultValidDissociateProjectSubscriptionServiceModel = new DissociateProjectSubscriptionEvent();

        public static DissociateProjectSubscriptionEvent GetDefaultValidDissociateProjectSubscriptionServiceRequest()
        {
            DissociateProjectSubscriptionEvent defaultValidDissociateProjectSubscriptionServiceModel = new DissociateProjectSubscriptionEvent();
            defaultValidDissociateProjectSubscriptionServiceModel.SubscriptionUID = Guid.NewGuid();
            defaultValidDissociateProjectSubscriptionServiceModel.ProjectUID = Guid.NewGuid();
            defaultValidDissociateProjectSubscriptionServiceModel.EffectiveDate = DateTime.UtcNow;
            defaultValidDissociateProjectSubscriptionServiceModel.ActionUTC = DateTime.UtcNow;
            return defaultValidDissociateProjectSubscriptionServiceModel;
        }

        public static InvalidCreateCustomerSubscriptionEvent GetDefaultInvalidCustomerSubscriptionServiceCreateRequest()
        {
            InvalidCreateCustomerSubscriptionEvent defaultInvalidCustomerSubscriptionServiceCreateModel = new InvalidCreateCustomerSubscriptionEvent();
            defaultInvalidCustomerSubscriptionServiceCreateModel.SubscriptionUID = Guid.NewGuid().ToString();
            defaultInvalidCustomerSubscriptionServiceCreateModel.CustomerUID = Guid.NewGuid().ToString();
            defaultInvalidCustomerSubscriptionServiceCreateModel.SubscriptionType = "Operator Id/ Manage Operators";
            defaultInvalidCustomerSubscriptionServiceCreateModel.StartDate = DateTime.UtcNow.ToString();
            defaultInvalidCustomerSubscriptionServiceCreateModel.EndDate = DateTime.UtcNow.AddYears(10).ToString();
            defaultInvalidCustomerSubscriptionServiceCreateModel.ActionUTC = DateTime.UtcNow.ToString();
            return defaultInvalidCustomerSubscriptionServiceCreateModel;
        }

        public static InvalidUpdateCustomerSubscriptionEvent GetDefaultInvalidCustomerSubscriptionServiceUpdateRequest()
        {
            InvalidUpdateCustomerSubscriptionEvent defaultInvalidCustomerSubscriptionServiceUpdateModel = new InvalidUpdateCustomerSubscriptionEvent();
            defaultInvalidCustomerSubscriptionServiceUpdateModel.SubscriptionUID = defaultInvalidCustomerSubscriptionServiceUpdateModel.SubscriptionUID;
            defaultInvalidCustomerSubscriptionServiceUpdateModel.StartDate = DateTime.UtcNow.ToString();
            defaultInvalidCustomerSubscriptionServiceUpdateModel.EndDate = DateTime.UtcNow.AddYears(10).ToString();
            defaultInvalidCustomerSubscriptionServiceUpdateModel.ActionUTC = DateTime.UtcNow.ToString();
            return defaultInvalidCustomerSubscriptionServiceUpdateModel;
        }


        public static InvalidCreateProjectSubscriptionEvent GetDefaultInvalidProjectSubscriptionServiceCreateRequest()
        {
            InvalidCreateProjectSubscriptionEvent defaultInvalidProjectSubscriptionServiceCreateModel = new InvalidCreateProjectSubscriptionEvent();
            defaultInvalidProjectSubscriptionServiceCreateModel.SubscriptionUID = Guid.NewGuid().ToString();
            defaultInvalidProjectSubscriptionServiceCreateModel.CustomerUID = Guid.NewGuid().ToString();
            defaultInvalidProjectSubscriptionServiceCreateModel.SubscriptionType = "Landfill";
            defaultInvalidProjectSubscriptionServiceCreateModel.StartDate = DateTime.UtcNow.ToString();
            defaultInvalidProjectSubscriptionServiceCreateModel.EndDate = DateTime.UtcNow.AddYears(10).ToString(); ;
            defaultInvalidProjectSubscriptionServiceCreateModel.ActionUTC = DateTime.UtcNow.ToString();
            return defaultInvalidProjectSubscriptionServiceCreateModel;
        }

        public static InvalidUpdateProjectSubscriptionEvent GetDefaultInvalidProjectSubscriptionServiceUpdateRequest()
        {
            InvalidUpdateProjectSubscriptionEvent defaultInvalidProjectSubscriptionServiceUpdateModel = new InvalidUpdateProjectSubscriptionEvent();
            defaultInvalidProjectSubscriptionServiceUpdateModel.SubscriptionUID = Guid.NewGuid().ToString();
            defaultInvalidProjectSubscriptionServiceUpdateModel.CustomerUID = Guid.NewGuid().ToString();
            defaultInvalidProjectSubscriptionServiceUpdateModel.SubscriptionType = "Project Monitoring";
            defaultInvalidProjectSubscriptionServiceUpdateModel.StartDate = DateTime.UtcNow.ToString();
            defaultInvalidProjectSubscriptionServiceUpdateModel.EndDate = DateTime.UtcNow.AddYears(10).ToString(); ;
            defaultInvalidProjectSubscriptionServiceUpdateModel.ActionUTC = DateTime.UtcNow.ToString();
            return defaultInvalidProjectSubscriptionServiceUpdateModel;
        }


        public static InvalidAssociateProjectSubscriptionEvent GetDefaultInvalidAssociateProjectSubscriptionServiceRequest()
        {
            InvalidAssociateProjectSubscriptionEvent defaultInvalidAssociateProjectSubscriptionServiceModel = new InvalidAssociateProjectSubscriptionEvent();
            defaultInvalidAssociateProjectSubscriptionServiceModel.SubscriptionUID = Guid.NewGuid().ToString();
            defaultInvalidAssociateProjectSubscriptionServiceModel.ProjectUID = Guid.NewGuid().ToString();
            defaultInvalidAssociateProjectSubscriptionServiceModel.EffectiveDate = DateTime.UtcNow.ToString();
            defaultInvalidAssociateProjectSubscriptionServiceModel.ActionUTC = DateTime.UtcNow.ToString();
            return defaultInvalidAssociateProjectSubscriptionServiceModel;
        }

        public static InvalidDissociateProjectSubscriptionEvent GetDefaultInvalidDissociateProjectSubscriptionServiceRequest()
        {
            InvalidDissociateProjectSubscriptionEvent defaultInvalidDissociateProjectSubscriptionServiceModel = new InvalidDissociateProjectSubscriptionEvent();
            defaultInvalidDissociateProjectSubscriptionServiceModel.SubscriptionUID = Guid.NewGuid().ToString();
            defaultInvalidDissociateProjectSubscriptionServiceModel.ProjectUID = Guid.NewGuid().ToString();
            defaultInvalidDissociateProjectSubscriptionServiceModel.EffectiveDate = DateTime.UtcNow.ToString();
            defaultInvalidDissociateProjectSubscriptionServiceModel.ActionUTC = DateTime.UtcNow.ToString();
            return defaultInvalidDissociateProjectSubscriptionServiceModel;
        }

        [Given(@"SubscriptionMasterDataConsumerService Is Ready To Verify '(.*)'")]
        public void GivenSubscriptionMasterDataConsumerServiceIsReadyToVerify(string TestDescription)
        {
            //log the scenario info
            TestName = (ScenarioContext.Current.ScenarioInfo).Title + "_" + TestDescription;
            //TestName = TestDescription;
            LogResult.Report(Log, "log_ForInfo", "Execution started for Test Scenario" + TestName);
        }

        [Then(@"The SubscriptionService CustomerSubscriptionCreated Details Are Stored in CustomerSubscription table")]
        public void ThenTheSubscriptionServiceCustomerSubscriptionCreatedDetailsAreStoredInCustomerSubscriptionTable()
        {
            //Thread.Sleep(new TimeSpan(0, 0, 20));
            CommonUtil.WaitToProcess(SubscriptionServiceConfig.KafkaWaitTime); //Wait till message pushed into Kafka and to consume by the service

            string customerSubscriptionQuery = SubscriptionMySqlQueries.CustomerSubscriptionDetailsByCustomerUID + subscriptionServiceSupport.CreateCustomerSubscriptionModel.CustomerUID.ToString() + "'";
            List<string> customerSubscriptionColumnlist = new List<string>() { "fk_CustomerUID", "Name", "StartDate", "EndDate" };
            List<string> customersubscriptionDetails = new List<string>();
            customersubscriptionDetails.Add(subscriptionServiceSupport.CreateCustomerSubscriptionModel.CustomerUID.ToString());
            customersubscriptionDetails.Add(subscriptionServiceSupport.CreateCustomerSubscriptionModel.SubscriptionType);
            customersubscriptionDetails.Add(subscriptionServiceSupport.CreateCustomerSubscriptionModel.StartDate.ToString());
            customersubscriptionDetails.Add(subscriptionServiceSupport.CreateCustomerSubscriptionModel.EndDate.ToString());
            MySqlUtil.ValidateMySQLQuery(MySqlConnectionString, customerSubscriptionQuery, customersubscriptionDetails);
        }

        [Then(@"The SubscriptionService CustomerSubscriptionUpdated Details Are Stored in CustomerSubscription table")]
        public void ThenTheSubscriptionServiceCustomerSubscriptionUpdatedDetailsAreStoredInCustomerSubscriptionTable()
        {
            //Thread.Sleep(new TimeSpan(0, 0, 20));
            CommonUtil.WaitToProcess(SubscriptionServiceConfig.KafkaWaitTime); //Wait till message pushed into Kafka and to consume by the service

            string customerSubscriptionQuery = SubscriptionMySqlQueries.CustomerSubscriptionDetailsByCustomerUID + subscriptionServiceSupport.CreateCustomerSubscriptionModel.CustomerUID.ToString() + "'";
            List<string> customerSubscriptionColumnlist = new List<string>() { "fk_CustomerUID", "Name", "StartDate", "EndDate" };
            List<string> customersubscriptionDetails = new List<string>();
            customersubscriptionDetails.Add(subscriptionServiceSupport.CreateCustomerSubscriptionModel.CustomerUID.ToString());
            customersubscriptionDetails.Add(subscriptionServiceSupport.CreateCustomerSubscriptionModel.SubscriptionType);
            customersubscriptionDetails.Add(string.IsNullOrEmpty(subscriptionServiceSupport.UpdateCustomerSubscriptionModel.StartDate.ToString()) ? subscriptionServiceSupport.CreateCustomerSubscriptionModel.StartDate.ToString() : subscriptionServiceSupport.UpdateCustomerSubscriptionModel.StartDate.ToString());
            customersubscriptionDetails.Add(string.IsNullOrEmpty(subscriptionServiceSupport.UpdateCustomerSubscriptionModel.EndDate.ToString()) ? subscriptionServiceSupport.CreateCustomerSubscriptionModel.EndDate.ToString() : subscriptionServiceSupport.UpdateCustomerSubscriptionModel.EndDate.ToString());
            MySqlUtil.ValidateMySQLQuery(MySqlConnectionString, customerSubscriptionQuery, customersubscriptionDetails);
        }

        [Then(@"The SubscriptionService AssetSubscriptionCreated Details Are Stored in AssetSubscription and CustomerSubscription tables")]
        public void ThenTheSubscriptionServiceAssetSubscriptionCreatedDetailsAreStoredInAssetSubscriptionAndCustomerSubscriptionTables()
        {
            //Thread.Sleep(new TimeSpan(0, 0, 20));
            CommonUtil.WaitToProcess(SubscriptionServiceConfig.KafkaWaitTime); //Wait till message pushed into Kafka and to consume by the service

            string query = SubscriptionMySqlQueries.AssetSubscriptionDetailsByAssetSubscriptionUID + subscriptionServiceSupport.CreateAssetSubscriptionModel.SubscriptionUID.ToString() + "'";
            List<string> columnList = new List<string>() { "fk_AssetUID", "fk_DeviceUID", "StartDate", "EndDate" };
            List<string> assetsubscriptionDetails = new List<string>();
            assetsubscriptionDetails.Add(subscriptionServiceSupport.CreateAssetSubscriptionModel.AssetUID.ToString());
            assetsubscriptionDetails.Add(string.IsNullOrEmpty(subscriptionServiceSupport.CreateAssetSubscriptionModel.DeviceUID.ToString()) ? Guid.Empty.ToString() : subscriptionServiceSupport.CreateAssetSubscriptionModel.DeviceUID.ToString());
            assetsubscriptionDetails.Add(subscriptionServiceSupport.CreateAssetSubscriptionModel.StartDate.ToString());
            assetsubscriptionDetails.Add(subscriptionServiceSupport.CreateAssetSubscriptionModel.EndDate.ToString());
            MySqlUtil.ValidateMySQLQuery(MySqlConnectionString, query, assetsubscriptionDetails);

            string customerSubscriptionQuery = SubscriptionMySqlQueries.CustomerSubscriptionDetailsByCustomerUID + subscriptionServiceSupport.CreateAssetSubscriptionModel.CustomerUID.ToString() + "'";
            List<string> customerSubscriptionColumnlist = new List<string>() { "fk_CustomerUID", "Name", "StartDate", "EndDate" };
            List<string> customersubscriptionDetails = new List<string>();
            customersubscriptionDetails.Add(subscriptionServiceSupport.CreateAssetSubscriptionModel.CustomerUID.ToString());
            customersubscriptionDetails.Add(subscriptionServiceSupport.CreateAssetSubscriptionModel.SubscriptionType);
            customersubscriptionDetails.Add(subscriptionServiceSupport.CreateAssetSubscriptionModel.StartDate.ToString());
            customersubscriptionDetails.Add(subscriptionServiceSupport.CreateAssetSubscriptionModel.EndDate.ToString());
            MySqlUtil.ValidateMySQLQuery(MySqlConnectionString, customerSubscriptionQuery, customersubscriptionDetails);

        }

        [Then(@"The SubscriptionService AssetSubscriptionUpdated Details Are Stored in AssetSubscription and CustomerSubscription tables")]
        public void ThenTheSubscriptionServiceAssetSubscriptionUpdatedDetailsAreStoredInAssetSubscriptionAndCustomerSubscriptionTables()
        {
            //Thread.Sleep(new TimeSpan(0, 0, 20));
            CommonUtil.WaitToProcess(SubscriptionServiceConfig.KafkaWaitTime); //Wait till message pushed into Kafka and to consume by the service

            string query = SubscriptionMySqlQueries.AssetSubscriptionDetailsByAssetSubscriptionUID + subscriptionServiceSupport.UpdateAssetSubscriptionModel.SubscriptionUID.ToString() + "'";
            List<string> columnList = new List<string>() { "fk_AssetUID", "fk_DeviceUID", "StartDate", "EndDate" };
            List<string> assetsubscriptionDetails = new List<string>();
            assetsubscriptionDetails.Add(string.IsNullOrEmpty(subscriptionServiceSupport.UpdateAssetSubscriptionModel.AssetUID.ToString()) ? subscriptionServiceSupport.CreateAssetSubscriptionModel.AssetUID.ToString() : subscriptionServiceSupport.UpdateAssetSubscriptionModel.AssetUID.ToString());
            assetsubscriptionDetails.Add(string.IsNullOrEmpty(subscriptionServiceSupport.UpdateAssetSubscriptionModel.DeviceUID.ToString()) ? subscriptionServiceSupport.CreateAssetSubscriptionModel.DeviceUID.ToString() : subscriptionServiceSupport.UpdateAssetSubscriptionModel.DeviceUID.ToString());
            assetsubscriptionDetails.Add(string.IsNullOrEmpty(subscriptionServiceSupport.UpdateAssetSubscriptionModel.StartDate.ToString()) ? subscriptionServiceSupport.CreateAssetSubscriptionModel.StartDate.ToString() : subscriptionServiceSupport.UpdateAssetSubscriptionModel.StartDate.ToString());
            assetsubscriptionDetails.Add(string.IsNullOrEmpty(subscriptionServiceSupport.UpdateAssetSubscriptionModel.EndDate.ToString()) ? subscriptionServiceSupport.CreateAssetSubscriptionModel.EndDate.ToString() : subscriptionServiceSupport.UpdateAssetSubscriptionModel.EndDate.ToString());
            MySqlUtil.ValidateMySQLQuery(MySqlConnectionString, query, assetsubscriptionDetails);

            string customerSubscriptionQuery = SubscriptionMySqlQueries.CustomerSubscriptionDetailsByCustomerUID + subscriptionServiceSupport.CreateAssetSubscriptionModel.CustomerUID.ToString() + "'";
            List<string> customerSubscriptionColumnlist = new List<string>() { "fk_CustomerUID", "Name", "StartDate", "EndDate" };
            List<string> customersubscriptionDetails = new List<string>();
            customersubscriptionDetails.Add(string.IsNullOrEmpty(subscriptionServiceSupport.UpdateAssetSubscriptionModel.CustomerUID.ToString()) ? subscriptionServiceSupport.CreateAssetSubscriptionModel.CustomerUID.ToString() : subscriptionServiceSupport.UpdateAssetSubscriptionModel.CustomerUID.ToString());
            customersubscriptionDetails.Add(string.IsNullOrEmpty(subscriptionServiceSupport.UpdateAssetSubscriptionModel.SubscriptionType) ? subscriptionServiceSupport.CreateAssetSubscriptionModel.SubscriptionType : subscriptionServiceSupport.UpdateAssetSubscriptionModel.SubscriptionType);
            customersubscriptionDetails.Add(string.IsNullOrEmpty(subscriptionServiceSupport.UpdateAssetSubscriptionModel.StartDate.ToString()) ? subscriptionServiceSupport.CreateAssetSubscriptionModel.StartDate.ToString() : subscriptionServiceSupport.UpdateAssetSubscriptionModel.StartDate.ToString());
            customersubscriptionDetails.Add(string.IsNullOrEmpty(subscriptionServiceSupport.UpdateAssetSubscriptionModel.EndDate.ToString()) ? subscriptionServiceSupport.CreateAssetSubscriptionModel.EndDate.ToString() : subscriptionServiceSupport.UpdateAssetSubscriptionModel.EndDate.ToString());
            MySqlUtil.ValidateMySQLQuery(MySqlConnectionString, customerSubscriptionQuery, customersubscriptionDetails);
        }

        [Then(@"SubscriptionService ProjectSubscriptionCreated Details Are Stored in ProjectSubscription and CustomerSubscription tables")]
        public void ThenSubscriptionServiceProjectSubscriptionCreatedDetailsAreStoredInProjectSubscriptionAndCustomerSubscriptionTables()
        {
            //Thread.Sleep(new TimeSpan(0, 0, 20));
            CommonUtil.WaitToProcess(SubscriptionServiceConfig.KafkaWaitTime); //Wait till message pushed into Kafka and to consume by the service

            string query = SubscriptionMySqlQueries.ProjectSubscriptionDetailsByProjectSubscriptionUID + subscriptionServiceSupport.CreateProjectSubscriptionModel.SubscriptionUID.ToString() + "'";
            List<string> columnList = new List<string>() { "fk_ProjectUID", "StartDate", "EndDate" };
            List<string> projectsubscriptionDetails = new List<string>();
            projectsubscriptionDetails.Add(string.IsNullOrEmpty(subscriptionServiceSupport.AssociateProjectSubscriptionModel.ProjectUID.ToString()) ? string.Empty : (subscriptionServiceSupport.AssociateProjectSubscriptionModel.ProjectUID.ToString()));
            projectsubscriptionDetails.Add(subscriptionServiceSupport.CreateProjectSubscriptionModel.StartDate.ToString());
            projectsubscriptionDetails.Add(subscriptionServiceSupport.CreateProjectSubscriptionModel.EndDate.ToString());
            MySqlUtil.ValidateMySQLQuery(MySqlConnectionString, query, projectsubscriptionDetails);

            string customerSubscriptionQuery = SubscriptionMySqlQueries.CustomerSubscriptionDetailsByCustomerUID + subscriptionServiceSupport.CreateProjectSubscriptionModel.CustomerUID.ToString() + "'";
            List<string> customerSubscriptionColumnlist = new List<string>() { "fk_CustomerUID", "Name", "StartDate", "EndDate" };
            List<string> customersubscriptionDetails = new List<string>();
            customersubscriptionDetails.Add(subscriptionServiceSupport.CreateProjectSubscriptionModel.CustomerUID.ToString());
            customersubscriptionDetails.Add(subscriptionServiceSupport.CreateProjectSubscriptionModel.SubscriptionType);
            customersubscriptionDetails.Add(subscriptionServiceSupport.CreateProjectSubscriptionModel.StartDate.ToString());
            customersubscriptionDetails.Add(subscriptionServiceSupport.CreateProjectSubscriptionModel.EndDate.ToString());
            MySqlUtil.ValidateMySQLQuery(MySqlConnectionString, customerSubscriptionQuery, customersubscriptionDetails);
        }

        [Then(@"SubscriptionService ProjectSubscriptionUpdated Details Are Stored in ProjectSubscription and CustomerSubscription tables")]
        public void ThenSubscriptionServiceProjectSubscriptionUpdatedDetailsAreStoredInProjectSubscriptionAndCustomerSubscriptionTables()
        {
            //Thread.Sleep(new TimeSpan(0, 0, 20));
            CommonUtil.WaitToProcess(SubscriptionServiceConfig.KafkaWaitTime); //Wait till message pushed into Kafka and to consume by the service

            string query = SubscriptionMySqlQueries.ProjectSubscriptionDetailsByProjectSubscriptionUID + subscriptionServiceSupport.CreateProjectSubscriptionModel.SubscriptionUID.ToString() + "'";
            List<string> columnList = new List<string>() { "fk_ProjectUID", "StartDate", "EndDate" };
            List<string> projectsubscriptionDetails = new List<string>();
            projectsubscriptionDetails.Add(string.IsNullOrEmpty(subscriptionServiceSupport.AssociateProjectSubscriptionModel.ProjectUID.ToString()) ? string.Empty : (subscriptionServiceSupport.AssociateProjectSubscriptionModel.ProjectUID.ToString()));
            projectsubscriptionDetails.Add(string.IsNullOrEmpty(subscriptionServiceSupport.UpdateProjectSubscriptionModel.StartDate.ToString()) ? null : (subscriptionServiceSupport.UpdateProjectSubscriptionModel.StartDate.ToString()));
            projectsubscriptionDetails.Add(string.IsNullOrEmpty(subscriptionServiceSupport.UpdateProjectSubscriptionModel.EndDate.ToString()) ? null : (subscriptionServiceSupport.UpdateProjectSubscriptionModel.EndDate.ToString()));
            MySqlUtil.ValidateMySQLQuery(MySqlConnectionString, query, projectsubscriptionDetails);

            string customerSubscriptionQuery = SubscriptionMySqlQueries.CustomerSubscriptionDetailsByCustomerUID + subscriptionServiceSupport.CreateProjectSubscriptionModel.CustomerUID.ToString() + "'";
            List<string> customerSubscriptionColumnlist = new List<string>() { "fk_CustomerUID", "Name", "StartDate", "EndDate" };
            List<string> customersubscriptionDetails = new List<string>();
            customersubscriptionDetails.Add(subscriptionServiceSupport.CreateProjectSubscriptionModel.CustomerUID.ToString());
            customersubscriptionDetails.Add(string.IsNullOrEmpty(subscriptionServiceSupport.UpdateProjectSubscriptionModel.SubscriptionType) ? null : (subscriptionServiceSupport.UpdateProjectSubscriptionModel.SubscriptionType.ToString()));
            customersubscriptionDetails.Add(string.IsNullOrEmpty(subscriptionServiceSupport.UpdateProjectSubscriptionModel.StartDate.ToString()) ? null : (subscriptionServiceSupport.UpdateProjectSubscriptionModel.StartDate.ToString()));
            customersubscriptionDetails.Add(string.IsNullOrEmpty(subscriptionServiceSupport.UpdateProjectSubscriptionModel.EndDate.ToString()) ? null : (subscriptionServiceSupport.UpdateProjectSubscriptionModel.EndDate.ToString()));
            MySqlUtil.ValidateMySQLQuery(MySqlConnectionString, customerSubscriptionQuery, customersubscriptionDetails);
        }

        [Then(@"SubscriptionService ProjectSubscriptionAssociated Details Are Stored in ProjectSubscription and CustomerSubscription tables")]
        public void ThenSubscriptionServiceProjectSubscriptionAssociatedDetailsAreStoredInProjectSubscriptionAndCustomerSubscriptionTables()
        {
            //Thread.Sleep(new TimeSpan(0, 0, 20));
            CommonUtil.WaitToProcess(SubscriptionServiceConfig.KafkaWaitTime); //Wait till message pushed into Kafka and to consume by the service

            string query = SubscriptionMySqlQueries.ProjectSubscriptionDetailsByProjectSubscriptionUID + subscriptionServiceSupport.CreateProjectSubscriptionModel.SubscriptionUID.ToString() + "'";
            List<string> columnList = new List<string>() { "fk_ProjectUID", "StartDate", "EndDate" };
            List<string> projectsubscriptionDetails = new List<string>();
            projectsubscriptionDetails.Add(subscriptionServiceSupport.AssociateProjectSubscriptionModel.ProjectUID.ToString());
            projectsubscriptionDetails.Add(subscriptionServiceSupport.AssociateProjectSubscriptionModel.EffectiveDate.ToString());
            projectsubscriptionDetails.Add(subscriptionServiceSupport.CreateProjectSubscriptionModel.EndDate.ToString());
            MySqlUtil.ValidateMySQLQuery(MySqlConnectionString, query, projectsubscriptionDetails);

            string customerSubscriptionQuery = SubscriptionMySqlQueries.CustomerSubscriptionDetailsByCustomerUID + subscriptionServiceSupport.CreateProjectSubscriptionModel.CustomerUID.ToString() + "'";
            List<string> customerSubscriptionColumnlist = new List<string>() { "fk_CustomerUID", "Name", "StartDate", "EndDate" };
            List<string> customersubscriptionDetails = new List<string>();
            customersubscriptionDetails.Add(subscriptionServiceSupport.CreateProjectSubscriptionModel.CustomerUID.ToString());
            customersubscriptionDetails.Add(subscriptionServiceSupport.CreateProjectSubscriptionModel.SubscriptionType);
            customersubscriptionDetails.Add(subscriptionServiceSupport.AssociateProjectSubscriptionModel.EffectiveDate.ToString());
            customersubscriptionDetails.Add(subscriptionServiceSupport.CreateProjectSubscriptionModel.EndDate.ToString());
            MySqlUtil.ValidateMySQLQuery(MySqlConnectionString, customerSubscriptionQuery, customersubscriptionDetails);
        }

        [Then(@"SubscriptionService ProjectSubscriptionDissociated Details Are Stored in ProjectSubscription and CustomerSubscription tables")]
        public void ThenSubscriptionServiceProjectSubscriptionDissociatedDetailsAreStoredInProjectSubscriptionAndCustomerSubscriptionTables()
        {
            //Thread.Sleep(new TimeSpan(0, 0, 20));
            CommonUtil.WaitToProcess(SubscriptionServiceConfig.KafkaWaitTime); //Wait till message pushed into Kafka and to consume by the service

            string query = SubscriptionMySqlQueries.ProjectSubscriptionDetailsByProjectSubscriptionUID + subscriptionServiceSupport.CreateProjectSubscriptionModel.SubscriptionUID.ToString() + "'";
            List<string> columnList = new List<string>() { "fk_ProjectUID", "StartDate", "EndDate" };
            List<string> projectsubscriptionDetails = new List<string>();
            projectsubscriptionDetails.Add(subscriptionServiceSupport.DissociateProjectSubscriptionModel.ProjectUID.ToString());
            projectsubscriptionDetails.Add(subscriptionServiceSupport.CreateProjectSubscriptionModel.StartDate.ToString());
            projectsubscriptionDetails.Add(subscriptionServiceSupport.DissociateProjectSubscriptionModel.EffectiveDate.ToString());
            MySqlUtil.ValidateMySQLQuery(MySqlConnectionString, query, projectsubscriptionDetails);

            string customerSubscriptionQuery = SubscriptionMySqlQueries.CustomerSubscriptionDetailsByCustomerUID + subscriptionServiceSupport.CreateProjectSubscriptionModel.CustomerUID.ToString() + "'";
            List<string> customerSubscriptionColumnlist = new List<string>() { "fk_CustomerUID", "Name", "StartDate", "EndDate" };
            List<string> customersubscriptionDetails = new List<string>();
            customersubscriptionDetails.Add(subscriptionServiceSupport.CreateProjectSubscriptionModel.CustomerUID.ToString());
            customersubscriptionDetails.Add(subscriptionServiceSupport.CreateProjectSubscriptionModel.SubscriptionType);
            customersubscriptionDetails.Add(subscriptionServiceSupport.CreateProjectSubscriptionModel.StartDate.ToString());
            customersubscriptionDetails.Add(subscriptionServiceSupport.DissociateProjectSubscriptionModel.EffectiveDate.ToString());
            MySqlUtil.ValidateMySQLQuery(MySqlConnectionString, customerSubscriptionQuery, customersubscriptionDetails);
        }

        [When(@"I Set SubscriptionService CustomerSubscriptionUpdate StartDate  To '(.*)'")]
        public void WhenISetSubscriptionServiceCustomerSubscriptionUpdateStartDateTo(string startDate)
        {
            subscriptionServiceSupport.UpdateCustomerSubscriptionModel.StartDate = String.IsNullOrEmpty(InputGenerator.GetValue(startDate)) ? (DateTime?)null : Convert.ToDateTime(InputGenerator.GetValue(startDate.ToString()));
        }

        [When(@"I Set SubscriptionService CustomerSubscriptionUpdate EndDate To '(.*)'")]
        public void WhenISetSubscriptionServiceCustomerSubscriptionUpdateEndDateTo(string endDate)
        {
            subscriptionServiceSupport.UpdateCustomerSubscriptionModel.EndDate = String.IsNullOrEmpty(InputGenerator.GetValue(endDate)) ? (DateTime?)null : Convert.ToDateTime(InputGenerator.GetValue(endDate.ToString()));
        }


        [When(@"I Set Valid SubscriptionService AssetSubscriptionCreate Request AssetUID To Second AssetUID")]
        public void WhenISetValidSubscriptionServiceAssetSubscriptionCreateRequestAssetUIDToSecondAssetUID()
        {
            subscriptionServiceSupport.CreateAssetSubscriptionModel.AssetUID = assetUid2;
        }

        [When(@"I Set Valid SubscriptionService AssetSubscriptionCreate Request StartDate To Min Date")]
        public void WhenISetValidSubscriptionServiceAssetSubscriptionCreateRequestStartDateToMinDate()
        {
            subscriptionServiceSupport.CreateAssetSubscriptionModel.StartDate = minDate;
        }

        [When(@"I Set Valid SubscriptionService AssetSubscriptionCreate Request StartDate To Current Date")]
        public void WhenISetValidSubscriptionServiceAssetSubscriptionCreateRequestStartDateToCurrentDate()
        {
            subscriptionServiceSupport.CreateAssetSubscriptionModel.StartDate = DateTime.UtcNow;
        }

        [When(@"I Set Valid SubscriptionService AssetSubscriptionCreate Request EndDate To Max Date")]
        public void WhenISetValidSubscriptionServiceAssetSubscriptionCreateRequestEndDateToMaxDate()
        {
            subscriptionServiceSupport.CreateAssetSubscriptionModel.EndDate = maxDate;
        }

        [When(@"I Set Valid SubscriptionService AssetSubscriptionCreate Request SubscriptionType To '(.*)'")]
        public void WhenISetValidSubscriptionServiceAssetSubscriptionCreateRequestSubscriptionTypeTo(string subscriptionType)
        {
            subscriptionServiceSupport.CreateAssetSubscriptionModel.SubscriptionType = subscriptionType;
        }

        [When(@"I Set Valid SubscriptionService AssetSubscriptionCreate Request SubscriptionUID To Unique UID")]
        public void WhenISetValidSubscriptionServiceAssetSubscriptionCreateRequestSubscriptionUIDToUniqueUID()
        {
            subscriptionServiceSupport.CreateAssetSubscriptionModel.SubscriptionUID = Guid.NewGuid();
        }

        [Given(@"SubscriptionReadService Is Ready To Verify '(.*)'")]
        public void GivenSubscriptionReadServiceIsReadyToVerify(string TestDescription)
        {
            //log the scenario info
            TestName = (ScenarioContext.Current.ScenarioInfo).Title + "_" + TestDescription;
            //TestName = TestDescription;
            LogResult.Report(Log, "log_ForInfo", "Execution started for Test Scenario" + TestName);
        }

        [Given(@"SubscriptionListWebApi Is Ready To Verify '(.*)'")]
        public void GivenSubscriptionListWebApiIsReadyToVerify(string TestDescription)
        {
            //log the scenario info
            TestName = (ScenarioContext.Current.ScenarioInfo).Title + "_" + TestDescription;
            //TestName = TestDescription;
            LogResult.Report(Log, "log_ForInfo", "Execution started for Test Scenario" + TestName);
        }

        [When(@"I Post Valid SubscriptionService Read Request For The Customer")]
        public void WhenIPostValidSubscriptionServiceReadRequestForTheCustomer()
        {
            //Thread.Sleep(new TimeSpan(0, 0, 20));
            CommonUtil.WaitToProcess(SubscriptionServiceConfig.KafkaWaitTime); //Wait till message pushed into Kafka and to consume by the service
            subscriptionServiceSupport.PostValidCustomerSubscriptionReadRequestToService(subscriptionServiceSupport.CreateAssetSubscriptionModel.CustomerUID.ToString());
        }

        [When(@"I Post Valid GetActiveProjectSubscriptions For The Customer")]
        public void WhenIPostValidGetActiveProjectSubscriptionsForTheCustomer()
        {
            CommonUtil.WaitToProcess(SubscriptionServiceConfig.KafkaWaitTime); //Wait till message pushed into Kafka and to consume by the service
            subscriptionServiceSupport.PostValidGetActiveProjectSubscriptionForCustomer(subscriptionServiceSupport.CreateAssetSubscriptionModel.CustomerUID.ToString());
        }


        [Then(@"The SubscriptionServiceRead Response should return the Subscription Details For The Customer")]
        public void ThenTheSubscriptionServiceReadResponseShouldReturnTheSubscriptionDetailsForTheCustomer()
        {
            subscriptionServiceSupport.VerifySubscriptionServiceReadResponseAfterCreate();
        }

        [Then(@"The SubscriptionServiceRead Response should return the Subscription UpdatedDetails For The Customer")]
        public void ThenTheSubscriptionServiceReadResponseShouldReturnTheSubscriptionUpdatedDetailsForTheCustomer()
        {
            subscriptionServiceSupport.VerifySubscriptionServiceReadResponseAfterUpdate();
        }

        [Then(@"The SubscriptionServiceRead Response should return the MultipleAsset Subscription Details For The Customer With SubscriptionType as '(.*)' and '(.*)'")]
        public void ThenTheSubscriptionServiceReadResponseShouldReturnTheMultipleAssetSubscriptionDetailsForTheCustomerWithSubscriptionTypeAsAnd(string p0, string p1)
        {
            subscriptionServiceSupport.VerifySubscriptionServiceReadResponseAfterMultipleCreate(p0, p1);
        }

        [Then(@"The SubscriptionServiceRead Response should return the MultipleAsset Subscription Details For The Customer")]
        public void ThenTheSubscriptionServiceReadResponseShouldReturnTheMultipleAssetSubscriptionDetailsForTheCustomer()
        {
            subscriptionServiceSupport.VerifySubscriptionServiceReadResponseForMultipleAssetSubscriptions();
        }

        [Then(@"The SubscriptionServiceRead Response should return the Subscription Details For The Customer With Min Start Date and Max End Date")]
        public void ThenTheSubscriptionServiceReadResponseShouldReturnTheSubscriptionDetailsForTheCustomerWithMinStartDateAndMaxEndDate()
        {
            subscriptionServiceSupport.VerifySubscriptionServiceReadResponse(minDate, maxDate);
        }

        [Then(@"The GetSubscriptionDetailsCustomerContext should return the Subscription Details With Start Date as '(.*)' and End Date as '(.*)'")]
        public void ThenTheGetSubscriptionDetailsCustomerContextShouldReturnTheSubscriptionDetailsWithStartDateAsAndEndDateAs(string startDate, string endDate)
        {
            if ((startDate == "Current Date") && (endDate == "Max Date"))
                subscriptionServiceSupport.VerifySubscriptionServiceReadResponse(currentDate, maxDate);
            if ((startDate == "Min Date") && (endDate == "Max Date"))
                subscriptionServiceSupport.VerifySubscriptionServiceReadResponse(minDate, maxDate);
        }

        [When(@"I Set Valid SubscriptionService AssetSubscriptionUpdate Request SubscriptionUID To First Asset SubscriptionUID")]
        public void WhenISetValidSubscriptionServiceAssetSubscriptionUpdateRequestSubscriptionUIDToFirstAssetSubscriptionUID()
        {
            subscriptionServiceSupport.UpdateAssetSubscriptionModel.SubscriptionUID = subscriptionUid1;
        }

        [When(@"I Set Valid SubscriptionService AssetSubscriptionUpdate Request EndDate To Min Date")]
        public void WhenISetValidSubscriptionServiceAssetSubscriptionUpdateRequestEndDateToMinDate()
        {
            subscriptionServiceSupport.UpdateAssetSubscriptionModel.EndDate = minDate;
        }

        [When(@"I Set Valid SubscriptionService AssetSubscriptionUpdate Request ActionUTC To Current Date")]
        public void WhenISetValidSubscriptionServiceAssetSubscriptionUpdateRequestActionUTCToCurrentDate()
        {
            subscriptionServiceSupport.UpdateAssetSubscriptionModel.ActionUTC = DateTime.UtcNow;
        }

        [When(@"I Set Valid SubscriptionService AssetSubscriptionUpdate Request StartDate To Current Date")]
        public void WhenISetValidSubscriptionServiceAssetSubscriptionUpdateRequestStartDateToCurrentDate()
        {
            subscriptionServiceSupport.UpdateAssetSubscriptionModel.StartDate = DateTime.UtcNow;
        }


        [When(@"I Set Valid SubscriptionService AssetSubscriptionUpdate Request EndDate To Max Date")]
        public void WhenISetValidSubscriptionServiceAssetSubscriptionUpdateRequestEndDateToMaxDate()
        {
            subscriptionServiceSupport.UpdateAssetSubscriptionModel.EndDate = maxDate;
        }

        [Given(@"SubscriptionService CustomerSubscriptionCreate Request Is Setup With Default Values")]
        public void GivenSubscriptionServiceCustomerSubscriptionCreateRequestIsSetupWithDefaultValues()
        {
            subscriptionServiceSupport.CreateCustomerSubscriptionModel = GetDefaultValidCustomerSubscriptionServiceCreateRequest();
        }

        [When(@"I Post Valid SubscriptionService CustomerSubscriptionCreate Request")]
        public void WhenIPostValidSubscriptionServiceCustomerSubscriptionCreateRequest()
        {
            //subscriptionServiceSupport.SetupCreateCustomerSubscriptionKafkaConsumer(subscriptionServiceSupport.CreateCustomerSubscriptionModel.SubscriptionUID, subscriptionServiceSupport.CreateCustomerSubscriptionModel.ActionUTC);
            subscriptionServiceSupport.PostValidCustomerSubscriptionCreateRequestToService();
        }

        [Then(@"The Processed SubscriptionService CustomerSubscriptionCreate Message must be available in Kafka topic")]
        public void ThenTheProcessedSubscriptionServiceCustomerSubscriptionCreateMessageMustBeAvailableInKafkaTopic()
        {
            /*Task.Factory.StartNew(() => subscriptionServiceSupport._consumerWrapper.Consume());
            Thread.Sleep(new TimeSpan(0, 0, 20));
            CreateCustomerSubscriptionModel kafkaresponse = subscriptionServiceSupport._checkForCustomerSubscriptionCreateHandler.subscriptionEvent;
            Assert.IsTrue(subscriptionServiceSupport._checkForCustomerSubscriptionCreateHandler.HasFound()); //Asserts that the CreateCustomerSubscriptionEvent has published into the AssetKafkaTopic by validating the presence of the particular AssetUid and actionutc
            subscriptionServiceSupport.VerifyCustomerSubscriptionCreateResponse(kafkaresponse);*/

            /*#region RPL
            string groupName = SubscriptionServiceConfig.KafkaGroupName;
            string topicName = SubscriptionServiceConfig.SubscriptionServiceTopic;
            string keyResponseUid = subscriptionServiceSupport.CreateCustomerSubscriptionModel.SubscriptionUID.ToString();

            CommonUtil.WaitToProcess(SubscriptionServiceConfig.KafkaWaitTime); //Wait till message pushed into Kafka
            try
            {
              string baseUri = KafkaRESTConsumer.GetKafkaInstanceResponse(groupName);
              var consumeMessage = KafkaRESTConsumer.SearchKafkaConsumedMessage(baseUri, topicName, keyResponseUid);
              CreateCustomerSubscriptionModel KafkaResponseJSON = JsonConvert.DeserializeObject<CreateCustomerSubscriptionModel>(consumeMessage);
              if (KafkaResponseJSON != null)
                subscriptionServiceSupport.VerifyCustomerSubscriptionCreateResponse(KafkaResponseJSON);
              else
              {
                Assert.Fail("Event not available in the kafka topic");
              }
            }

            catch (Exception e)
            {
              LogResult.Report(Log, "log_ForError", "Got Error While Verifying Kafka Message", e);
              throw new Exception(e + " Got Error While Verifying Kafka Message");
            }
            #endregion*/

            subscriptionServiceSupport.VerifyCustomerSubscriptionCreateResponse();

        }

        [Given(@"SubscriptionService CustomerSubscriptionUpdate Request Is Setup With Default Values")]
        public void GivenSubscriptionServiceCustomerSubscriptionUpdateRequestIsSetupWithDefaultValues()
        {
            subscriptionServiceSupport.CreateCustomerSubscriptionModel = GetDefaultValidCustomerSubscriptionServiceCreateRequest();
            subscriptionServiceSupport.PostValidCustomerSubscriptionCreateRequestToService();
            subscriptionServiceSupport.UpdateCustomerSubscriptionModel = GetDefaultValidCustomerSubscriptionServiceUpdateRequest();
        }

        [When(@"I Post Valid SubscriptionService CustomerSubscriptionUpdate Request")]
        public void WhenIPostValidSubscriptionServiceCustomerSubscriptionUpdateRequest()
        {
            //subscriptionServiceSupport.SetupUpdateCustomerSubscriptionKafkaConsumer(subscriptionServiceSupport.UpdateCustomerSubscriptionModel.SubscriptionUID, subscriptionServiceSupport.UpdateCustomerSubscriptionModel.ActionUTC);
            subscriptionServiceSupport.PostValidCustomerSubscriptionUpdateRequestToService();
        }

        [Then(@"The Processed SubscriptionService CustomerSubscriptionUpdate Message must be available in Kafka topic")]
        public void ThenTheProcessedSubscriptionServiceCustomerSubscriptionUpdateMessageMustBeAvailableInKafkaTopic()
        {
            /*Task.Factory.StartNew(() => subscriptionServiceSupport._consumerWrapper.Consume());
            Thread.Sleep(new TimeSpan(0, 0, 20));
            UpdateCustomerSubscriptionModel kafkaresponse = subscriptionServiceSupport._checkForCustomerSubscriptionUpdateHandler.subscriptionEvent;
            Assert.IsTrue(subscriptionServiceSupport._checkForCustomerSubscriptionUpdateHandler.HasFound()); //Asserts that the CreateCustomerSubscriptionEvent has published into the AssetKafkaTopic by validating the presence of the particular AssetUid and actionutc
            subscriptionServiceSupport.VerifyCustomerSubscriptionUpdateResponse(kafkaresponse);*/

            /*#region RPL
            string groupName = SubscriptionServiceConfig.KafkaGroupName;
            string topicName = SubscriptionServiceConfig.SubscriptionServiceTopic;
            string keyResponseUid = subscriptionServiceSupport.UpdateCustomerSubscriptionModel.SubscriptionUID.ToString();

            CommonUtil.WaitToProcess(SubscriptionServiceConfig.KafkaWaitTime); //Wait till message pushed into Kafka
            try
            {
              string baseUri = KafkaRESTConsumer.GetKafkaInstanceResponse(groupName);
              var consumeMessage = KafkaRESTConsumer.SearchKafkaConsumedMessage(baseUri, topicName, keyResponseUid);
              UpdateCustomerSubscriptionModel KafkaResponseJSON = JsonConvert.DeserializeObject<UpdateCustomerSubscriptionModel>(consumeMessage);
              if (KafkaResponseJSON != null)
                subscriptionServiceSupport.VerifyCustomerSubscriptionUpdateResponse(KafkaResponseJSON);
              else
              {
                Assert.Fail("Event not available in the kafka topic");
              }
            }

            catch (Exception e)
            {
              LogResult.Report(Log, "log_ForError", "Got Error While Verifying Kafka Message", e);
              throw new Exception(e + " Got Error While Verifying Kafka Message");
            }
            #endregion*/
            subscriptionServiceSupport.VerifyCustomerSubscriptionUpdateResponse();
        }

        [Given(@"SubscriptionService ProjectSubscriptionCreate Request Is Setup With Default Values")]
        public void GivenSubscriptionServiceProjectSubscriptionCreateRequestIsSetupWithDefaultValues()
        {
            subscriptionServiceSupport.CreateProjectSubscriptionModel = GetDefaultValidProjectSubscriptionServiceCreateRequest();
        }

        [When(@"I Post Valid SubscriptionService ProjectSubscriptionCreate Request")]
        public void WhenIPostValidSubscriptionServiceProjectSubscriptionCreateRequest()
        {
            //subscriptionServiceSupport.SetupCreateProjectSubscriptionKafkaConsumer(subscriptionServiceSupport.CreateProjectSubscriptionModel.SubscriptionUID, subscriptionServiceSupport.CreateProjectSubscriptionModel.ActionUTC);
            subscriptionServiceSupport.PostValidProjectSubscriptionCreateRequestToService();
        }

        [Then(@"The Processed SubscriptionService ProjectSubscriptionCreate Message must be available in Kafka topic")]
        public void ThenTheProcessedSubscriptionServiceProjectSubscriptionCreateMessageMustBeAvailableInKafkaTopic()
        {
            /*Task.Factory.StartNew(() => subscriptionServiceSupport._consumerWrapper.Consume());
            Thread.Sleep(new TimeSpan(0, 0, 20));
            CreateProjectSubscriptionModel kafkaresponse = subscriptionServiceSupport._checkForProjectSubscriptionCreateHandler.subscriptionEvent;
            Assert.IsTrue(subscriptionServiceSupport._checkForProjectSubscriptionCreateHandler.HasFound()); //Asserts that the CreateCustomerSubscriptionEvent has published into the AssetKafkaTopic by validating the presence of the particular AssetUid and actionutc
            subscriptionServiceSupport.VerifyProjectSubscriptionCreateResponse(kafkaresponse);*/

            /*#region RPL
            string groupName = SubscriptionServiceConfig.KafkaGroupName;
            string topicName = SubscriptionServiceConfig.SubscriptionServiceTopic;
            string keyResponseUid = subscriptionServiceSupport.CreateProjectSubscriptionModel.SubscriptionUID.ToString();

            CommonUtil.WaitToProcess(SubscriptionServiceConfig.KafkaWaitTime); //Wait till message pushed into Kafka
            try
            {
              string baseUri = KafkaRESTConsumer.GetKafkaInstanceResponse(groupName);
              var consumeMessage = KafkaRESTConsumer.SearchKafkaConsumedMessage(baseUri, topicName, keyResponseUid);
              CreateProjectSubscriptionModel KafkaResponseJSON = JsonConvert.DeserializeObject<CreateProjectSubscriptionModel>(consumeMessage);
              if (KafkaResponseJSON != null)
                subscriptionServiceSupport.VerifyProjectSubscriptionCreateResponse(KafkaResponseJSON);
              else
              {
                Assert.Fail("Event not available in the kafka topic");
              }
            }

            catch (Exception e)
            {
              LogResult.Report(Log, "log_ForError", "Got Error While Verifying Kafka Message", e);
              throw new Exception(e + " Got Error While Verifying Kafka Message");
            }
            #endregion*/
            subscriptionServiceSupport.VerifyProjectSubscriptionCreateResponse();
        }

        [Given(@"SubscriptionService ProjectSubscriptionUpdate Request Is Setup With Default Values")]
        public void GivenSubscriptionServiceProjectSubscriptionUpdateRequestIsSetupWithDefaultValues()
        {
            subscriptionServiceSupport.CreateProjectSubscriptionModel = GetDefaultValidProjectSubscriptionServiceCreateRequest();
            subscriptionServiceSupport.PostValidProjectSubscriptionCreateRequestToService();
            subscriptionServiceSupport.UpdateProjectSubscriptionModel = GetDefaultValidProjectSubscriptionServiceUpdateRequest();
        }

        [When(@"I Post Valid SubscriptionService ProjectSubscriptionUpdate Request")]
        public void WhenIPostValidSubscriptionServiceProjectSubscriptionUpdateRequest()
        {
            //subscriptionServiceSupport.SetupUpdateProjectSubscriptionKafkaConsumer(subscriptionServiceSupport.UpdateProjectSubscriptionModel.SubscriptionUID, subscriptionServiceSupport.UpdateProjectSubscriptionModel.ActionUTC);
            subscriptionServiceSupport.PostValidProjectSubscriptionUpdateRequestToService();
        }

        [Then(@"The Processed SubscriptionService ProjectSubscriptionUpdate Message must be available in Kafka topic")]
        public void ThenTheProcessedSubscriptionServiceProjectSubscriptionUpdateMessageMustBeAvailableInKafkaTopic()
        {
            /*Task.Factory.StartNew(() => subscriptionServiceSupport._consumerWrapper.Consume());
            Thread.Sleep(new TimeSpan(0, 0, 20));
            UpdateProjectSubscriptionModel kafkaresponse = subscriptionServiceSupport._checkForProjectSubscriptionUpdateHandler.subscriptionEvent;
            Assert.IsTrue(subscriptionServiceSupport._checkForProjectSubscriptionUpdateHandler.HasFound()); //Asserts that the CreateCustomerSubscriptionEvent has published into the AssetKafkaTopic by validating the presence of the particular AssetUid and actionutc
            subscriptionServiceSupport.VerifyProjectSubscriptionUpdateResponse(kafkaresponse);*/

            /*#region RPL
            string groupName = SubscriptionServiceConfig.KafkaGroupName;
            string topicName = SubscriptionServiceConfig.SubscriptionServiceTopic;
            string keyResponseUid = subscriptionServiceSupport.UpdateProjectSubscriptionModel.SubscriptionUID.ToString();

            CommonUtil.WaitToProcess(SubscriptionServiceConfig.KafkaWaitTime); //Wait till message pushed into Kafka
            try
            {
              string baseUri = KafkaRESTConsumer.GetKafkaInstanceResponse(groupName);
              var consumeMessage = KafkaRESTConsumer.SearchKafkaConsumedMessage(baseUri, topicName, keyResponseUid);
              UpdateProjectSubscriptionModel KafkaResponseJSON = JsonConvert.DeserializeObject<UpdateProjectSubscriptionModel>(consumeMessage);
              if (KafkaResponseJSON != null)
                subscriptionServiceSupport.VerifyProjectSubscriptionUpdateResponse(KafkaResponseJSON);
              else
              {
                Assert.Fail("Event not available in the kafka topic");
              }
            }

            catch (Exception e)
            {
              LogResult.Report(Log, "log_ForError", "Got Error While Verifying Kafka Message", e);
              throw new Exception(e + " Got Error While Verifying Kafka Message");
            }
            #endregion*/
            subscriptionServiceSupport.VerifyProjectSubscriptionUpdateResponse();

        }

        [Given(@"SubscriptionService AssociateProjectSubscription Request Is Setup With Default Values")]
        public void GivenSubscriptionServiceAssociateProjectSubscriptionRequestIsSetupWithDefaultValues()
        {
            subscriptionServiceSupport.CreateProjectSubscriptionModel = GetDefaultValidProjectSubscriptionServiceCreateRequest();
            subscriptionServiceSupport.PostValidProjectSubscriptionCreateRequestToService();
            subscriptionServiceSupport.AssociateProjectSubscriptionModel = GetDefaultValidAssociateProjectSubscriptionServiceRequest();
            subscriptionServiceSupport.AssociateProjectSubscriptionModel.SubscriptionUID = defaultValidProjectSubscriptionServiceCreateModel.SubscriptionUID;
        }

        [When(@"I Post Valid SubscriptionService AssociateProjectSubscription Request")]
        public void WhenIPostValidSubscriptionServiceAssociateProjectSubscriptionRequest()
        {
            //subscriptionServiceSupport.SetupAssociateProjectSubscriptionKafkaConsumer(subscriptionServiceSupport.AssociateProjectSubscriptionModel.SubscriptionUID, subscriptionServiceSupport.AssociateProjectSubscriptionModel.EffectiveDate);
            subscriptionServiceSupport.PostValidAssociateProjectSubscriptionRequestToService();
        }

        [Then(@"The Processed SubscriptionService AssociateProjectSubscription Message must be available in Kafka topic")]
        public void ThenTheProcessedSubscriptionServiceAssociateProjectSubscriptionMessageMustBeAvailableInKafkaTopic()
        {
            /*Task.Factory.StartNew(() => subscriptionServiceSupport._consumerWrapper.Consume());
            Thread.Sleep(new TimeSpan(0, 0, 20));
            AssociateProjectSubscriptionModel kafkaresponse = subscriptionServiceSupport._checkForAssociateProjectSubscriptionHandler.subscriptionEvent;
            Assert.IsTrue(subscriptionServiceSupport._checkForAssociateProjectSubscriptionHandler.HasFound()); //Asserts that the CreateCustomerSubscriptionEvent has published into the AssetKafkaTopic by validating the presence of the particular AssetUid and actionutc
            subscriptionServiceSupport.VerifyAssociateProjectSubscriptionResponse(kafkaresponse);*/

            /*#region RPL
            string groupName = SubscriptionServiceConfig.KafkaGroupName;
            string topicName = SubscriptionServiceConfig.SubscriptionServiceTopic;
            string keyResponseUid = subscriptionServiceSupport.AssociateProjectSubscriptionModel.SubscriptionUID.ToString();

            CommonUtil.WaitToProcess(SubscriptionServiceConfig.KafkaWaitTime); //Wait till message pushed into Kafka
            try
            {
              string baseUri = KafkaRESTConsumer.GetKafkaInstanceResponse(groupName);
              var consumeMessage = KafkaRESTConsumer.SearchKafkaConsumedMessage(baseUri, topicName, keyResponseUid);
              AssociateProjectSubscriptionModel KafkaResponseJSON = JsonConvert.DeserializeObject<AssociateProjectSubscriptionModel>(consumeMessage);
              if (KafkaResponseJSON != null)
                subscriptionServiceSupport.VerifyAssociateProjectSubscriptionResponse(KafkaResponseJSON);
              else
              {
                Assert.Fail("Event not available in the kafka topic");
              }
            }

            catch (Exception e)
            {
              LogResult.Report(Log, "log_ForError", "Got Error While Verifying Kafka Message", e);
              throw new Exception(e + " Got Error While Verifying Kafka Message");
            }
            #endregion*/
            subscriptionServiceSupport.VerifyAssociateProjectSubscriptionResponse();

        }

        [When(@"I Set Invalid SubscriptionService AssociateProjectSubscription ProjectUID To '(.*)'")]
        public void WhenISetInvalidSubscriptionServiceAssociateProjectSubscriptionProjectUIDTo(string projectUid)
        {
            subscriptionServiceSupport.InvalidAssociateProjectSubscriptionModel.ProjectUID = projectUid;
        }

        [When(@"I Set Invalid SubscriptionService AssociateProjectSubscription EffectiveDate To '(.*)'")]
        public void WhenISetInvalidSubscriptionServiceAssociateProjectSubscriptionEffectiveDateTo(string effectiveDate)
        {
            subscriptionServiceSupport.InvalidAssociateProjectSubscriptionModel.EffectiveDate = effectiveDate;
        }

        [When(@"I Set Invalid SubscriptionService AssociateProjectSubscription SubscriptionUID To '(.*)'")]
        public void WhenISetInvalidSubscriptionServiceAssociateProjectSubscriptionSubscriptionUIDTo(string subscriptionUid)
        {
            subscriptionServiceSupport.InvalidAssociateProjectSubscriptionModel.SubscriptionUID = subscriptionUid;
        }

        [Given(@"SubscriptionService DissociateProjectSubscription Request Is Setup With Default Values")]
        public void GivenSubscriptionServiceDissociateProjectSubscriptionRequestIsSetupWithDefaultValues()
        {
            subscriptionServiceSupport.CreateProjectSubscriptionModel = GetDefaultValidProjectSubscriptionServiceCreateRequest();
            subscriptionServiceSupport.PostValidProjectSubscriptionCreateRequestToService();
            subscriptionServiceSupport.AssociateProjectSubscriptionModel = GetDefaultValidAssociateProjectSubscriptionServiceRequest();
            subscriptionServiceSupport.AssociateProjectSubscriptionModel.SubscriptionUID = subscriptionServiceSupport.CreateProjectSubscriptionModel.SubscriptionUID;
            subscriptionServiceSupport.PostValidAssociateProjectSubscriptionRequestToService();
            subscriptionServiceSupport.DissociateProjectSubscriptionModel.SubscriptionUID = subscriptionServiceSupport.AssociateProjectSubscriptionModel.SubscriptionUID;
            subscriptionServiceSupport.DissociateProjectSubscriptionModel = GetDefaultValidDissociateProjectSubscriptionServiceRequest();
        }

        [When(@"I Post Valid SubscriptionService DissociateProjectSubscription Request")]
        public void WhenIPostValidSubscriptionServiceDissociateProjectSubscriptionRequest()
        {
            //subscriptionServiceSupport.SetupDissociateProjectSubscriptionKafkaConsumer(subscriptionServiceSupport.DissociateProjectSubscriptionModel.SubscriptionUID, subscriptionServiceSupport.DissociateProjectSubscriptionModel.EffectiveDate);
            subscriptionServiceSupport.PostValidDissociateProjectSubscriptionRequestToService();
        }

        [Then(@"The Processed SubscriptionService DissociateProjectSubscription Message must be available in Kafka topic")]
        public void ThenTheProcessedSubscriptionServiceDissociateProjectSubscriptionMessageMustBeAvailableInKafkaTopic()
        {
            /*Task.Factory.StartNew(() => subscriptionServiceSupport._consumerWrapper.Consume());
            Thread.Sleep(new TimeSpan(0, 0, 20));
            DissociateProjectSubscriptionModel kafkaresponse = subscriptionServiceSupport._checkForDissociateProjectSubscriptionHandler.subscriptionEvent;
            Assert.IsTrue(subscriptionServiceSupport._checkForDissociateProjectSubscriptionHandler.HasFound()); //Asserts that the CreateCustomerSubscriptionEvent has published into the AssetKafkaTopic by validating the presence of the particular AssetUid and actionutc
            subscriptionServiceSupport.VerifyDissociateProjectSubscriptionResponse(kafkaresponse);*/

            /*#region RPL
            string groupName = SubscriptionServiceConfig.KafkaGroupName;
            string topicName = SubscriptionServiceConfig.SubscriptionServiceTopic;
            string keyResponseUid = subscriptionServiceSupport.DissociateProjectSubscriptionModel.SubscriptionUID.ToString();

            CommonUtil.WaitToProcess(SubscriptionServiceConfig.KafkaWaitTime); //Wait till message pushed into Kafka
            try
            {
              string baseUri = KafkaRESTConsumer.GetKafkaInstanceResponse(groupName);
              var consumeMessage = KafkaRESTConsumer.SearchKafkaConsumedMessage(baseUri, topicName, keyResponseUid);
              DissociateProjectSubscriptionModel KafkaResponseJSON = JsonConvert.DeserializeObject<DissociateProjectSubscriptionModel>(consumeMessage);
              if (KafkaResponseJSON != null)
                subscriptionServiceSupport.VerifyDissociateProjectSubscriptionResponse(KafkaResponseJSON);
              else
              {
                Assert.Fail("Event not available in the kafka topic");
              }
            }

            catch (Exception e)
            {
              LogResult.Report(Log, "log_ForError", "Got Error While Verifying Kafka Message", e);
              throw new Exception(e + " Got Error While Verifying Kafka Message");
            }
            #endregion*/
            subscriptionServiceSupport.VerifyDissociateProjectSubscriptionResponse();
        }

        [Given(@"SubscriptionService DissociateProjectSubscription Request Is Setup With Invalid Default Values")]
        public void GivenSubscriptionServiceDissociateProjectSubscriptionRequestIsSetupWithInvalidDefaultValues()
        {
            subscriptionServiceSupport.InvalidDissociateProjectSubscriptionModel = GetDefaultInvalidDissociateProjectSubscriptionServiceRequest();
        }

        [Given(@"SubscriptionService AssociateProjectSubscription Request Is Setup With Invalid Default Values")]
        public void GivenSubscriptionServiceAssociateProjectSubscriptionRequestIsSetupWithInvalidDefaultValues()
        {
            subscriptionServiceSupport.InvalidAssociateProjectSubscriptionModel = GetDefaultInvalidAssociateProjectSubscriptionServiceRequest();
        }

        [When(@"I Post Invalid SubscriptionService AssociateProjectSubscription Request")]
        public void WhenIPostInvalidSubscriptionServiceAssociateProjectSubscriptionRequest()
        {
            string contentType = "application/json";
            subscriptionServiceSupport.PostInValidAssociateProjectSubscriptionRequestToService(contentType, HttpStatusCode.BadRequest);
        }

        [When(@"I Post Invalid SubscriptionService DissociateProjectSubscription Request")]
        public void WhenIPostInvalidSubscriptionServiceDissociateProjectSubscriptionRequest()
        {
            string contentType = "application/json";
            subscriptionServiceSupport.PostInValidDissociateProjectSubscriptionRequestToService(contentType, HttpStatusCode.BadRequest);
        }

        [When(@"I Set Invalid SubscriptionService AssociateProjectSubscription ActionUTC To '(.*)'")]
        public void WhenISetInvalidSubscriptionServiceAssociateProjectSubscriptionActionUTCTo(string actionUtc)
        {
            subscriptionServiceSupport.InvalidAssociateProjectSubscriptionModel.ActionUTC = actionUtc;
        }


        [When(@"I Set Invalid SubscriptionService DissociateProjectSubscription ActionUTC To '(.*)'")]
        public void WhenISetInvalidSubscriptionServiceDissociateProjectSubscriptionActionUTCTo(string actionUtc)
        {
            subscriptionServiceSupport.InvalidDissociateProjectSubscriptionModel.ActionUTC = actionUtc;
        }

        [When(@"I Set Invalid SubscriptionService DissociateProjectSubscription ProjectUID To '(.*)'")]
        public void WhenISetInvalidSubscriptionServiceDissociateProjectSubscriptionProjectUIDTo(string projectUid)
        {
            subscriptionServiceSupport.InvalidDissociateProjectSubscriptionModel.ProjectUID = projectUid;
        }

        [When(@"I Set Invalid SubscriptionService DissociateProjectSubscription EffectiveDate To '(.*)'")]
        public void WhenISetInvalidSubscriptionServiceDissociateProjectSubscriptionEffectiveDateTo(string effectiveDate)
        {
            subscriptionServiceSupport.InvalidDissociateProjectSubscriptionModel.EffectiveDate = effectiveDate;
        }

        [Given(@"SubscriptionService CustomerSubscriptionCreate Request Is Setup With Invalid Default Values")]
        public void GivenSubscriptionServiceCustomerSubscriptionCreateRequestIsSetupWithInvalidDefaultValues()
        {
            subscriptionServiceSupport.InvalidCreateCustomerSubscriptionModel = GetDefaultInvalidCustomerSubscriptionServiceCreateRequest();
        }

        [When(@"I Set Invalid SubscriptionService DissociateProjectSubscription SubscriptionUID To '(.*)'")]
        public void WhenISetInvalidSubscriptionServiceDissociateProjectSubscriptionSubscriptionUIDTo(string subscriptionUid)
        {
            subscriptionServiceSupport.InvalidDissociateProjectSubscriptionModel.SubscriptionUID = subscriptionUid;
        }

        [When(@"I Post Invalid SubscriptionService CustomerSubscriptionCreate Request")]
        public void WhenIPostInvalidSubscriptionServiceCustomerSubscriptionCreateRequest()
        {
            string contentType = "application/json";
            subscriptionServiceSupport.PostInValidProjectSubscriptionCreateRequestToService(contentType, HttpStatusCode.BadRequest);
        }

        [When(@"I Set Invalid SubscriptionService CustomerSubscriptionCreate SubscriptionUID  To '(.*)'")]
        public void WhenISetInvalidSubscriptionServiceCustomerSubscriptionCreateSubscriptionUIDTo(string subscriptionUid)
        {
            subscriptionServiceSupport.InvalidCreateCustomerSubscriptionModel.SubscriptionUID = subscriptionUid;
        }

        [When(@"I Set Invalid SubscriptionService CustomerSubscriptionCreate CustomerUID To '(.*)'")]
        public void WhenISetInvalidSubscriptionServiceCustomerSubscriptionCreateCustomerUIDTo(string customerUid)
        {
            subscriptionServiceSupport.InvalidCreateCustomerSubscriptionModel.CustomerUID = customerUid;
        }

        [When(@"I Set Invalid SubscriptionService CustomerSubscriptionCreate SubscriptionType To '(.*)'")]
        public void WhenISetInvalidSubscriptionServiceCustomerSubscriptionCreateSubscriptionTypeTo(string subscriptionType)
        {
            subscriptionServiceSupport.InvalidCreateCustomerSubscriptionModel.SubscriptionType = subscriptionType;
        }

        [When(@"I Set Invalid SubscriptionService CustomerSubscriptionCreate StartDate To '(.*)'")]
        public void WhenISetInvalidSubscriptionServiceCustomerSubscriptionCreateStartDateTo(string startDate)
        {
            subscriptionServiceSupport.InvalidCreateCustomerSubscriptionModel.StartDate = startDate;
        }

        [When(@"I Set Invalid SubscriptionService CustomerSubscriptionCreate EndDate To '(.*)'")]
        public void WhenISetInvalidSubscriptionServiceCustomerSubscriptionCreateEndDateTo(string endDate)
        {
            subscriptionServiceSupport.InvalidCreateCustomerSubscriptionModel.EndDate = endDate;
        }

        [When(@"I Set Invalid SubscriptionService CustomerSubscriptionCreate ActionUTC To '(.*)'")]
        public void WhenISetInvalidSubscriptionServiceCustomerSubscriptionCreateActionUTCTo(string actionUtc)
        {
            subscriptionServiceSupport.InvalidCreateCustomerSubscriptionModel.ActionUTC = actionUtc;
        }

        [Then(@"SubscriptionService CustomerSubscriptionCreate Response With '(.*)' Should Be Returned")]
        public void ThenSubscriptionServiceCustomerSubscriptionCreateResponseWithShouldBeReturned(string errorMessage)
        {
            subscriptionServiceSupport.VerifyCustomerSubscriptionErrorResponse(errorMessage);
        }

        [Given(@"SubscriptionService CustomerSubscriptionUpdate Request Is Setup With Invalid Default Values")]
        public void GivenSubscriptionServiceCustomerSubscriptionUpdateRequestIsSetupWithInvalidDefaultValues()
        {
            subscriptionServiceSupport.InvalidUpdateCustomerSubscriptionModel = GetDefaultInvalidCustomerSubscriptionServiceUpdateRequest();
        }

        [When(@"I Post Invalid SubscriptionService CustomerSubscriptionUpdate Request")]
        public void WhenIPostInvalidSubscriptionServiceCustomerSubscriptionUpdateRequest()
        {
            string contentType = "application/json";
            subscriptionServiceSupport.PostInValidCustomerSubscriptionUpdateRequestToService(contentType, HttpStatusCode.BadRequest);
        }

        [When(@"I Set Invalid SubscriptionService CustomerSubscriptionUpdate SubscriptionUID  To '(.*)'")]
        public void WhenISetInvalidSubscriptionServiceCustomerSubscriptionUpdateSubscriptionUIDTo(string subscriptionUid)
        {
            subscriptionServiceSupport.InvalidUpdateCustomerSubscriptionModel.SubscriptionUID = subscriptionUid;
        }

        [When(@"I Set Invalid SubscriptionService CustomerSubscriptionUpdate StartDate To '(.*)'")]
        public void WhenISetInvalidSubscriptionServiceCustomerSubscriptionUpdateStartDateTo(string startDate)
        {
            subscriptionServiceSupport.InvalidUpdateCustomerSubscriptionModel.StartDate = startDate;
        }

        [When(@"I Set Invalid SubscriptionService CustomerSubscriptionUpdate EndDate To '(.*)'")]
        public void WhenISetInvalidSubscriptionServiceCustomerSubscriptionUpdateEndDateTo(string endDate)
        {
            subscriptionServiceSupport.InvalidUpdateCustomerSubscriptionModel.EndDate = endDate;
        }

        [When(@"I Set Invalid SubscriptionService CustomerSubscriptionUpdate ActionUTC To '(.*)'")]
        public void WhenISetInvalidSubscriptionServiceCustomerSubscriptionUpdateActionUTCTo(string actionUtc)
        {
            subscriptionServiceSupport.InvalidUpdateCustomerSubscriptionModel.ActionUTC = actionUtc;
        }

        [Given(@"SubscriptionService ProjectSubscriptionCreate Request Is Setup With Invalid Default Values")]
        public void GivenSubscriptionServiceProjectSubscriptionCreateRequestIsSetupWithInvalidDefaultValues()
        {
            subscriptionServiceSupport.InvalidCreateProjectSubscriptionModel = GetDefaultInvalidProjectSubscriptionServiceCreateRequest();
        }

        [When(@"I Post Invalid SubscriptionService ProjectSubscriptionCreate Request")]
        public void WhenIPostInvalidSubscriptionServiceProjectSubscriptionCreateRequest()
        {
            string contentType = "application/json";
            subscriptionServiceSupport.PostInValidProjectSubscriptionCreateRequestToService(contentType, HttpStatusCode.BadRequest);
        }


        [Given(@"SubscriptionService ProjectSubscriptionUpdate Request Is Setup With Invalid Default Values")]
        public void GivenSubscriptionServiceProjectSubscriptionUpdateRequestIsSetupWithInvalidDefaultValues()
        {
            subscriptionServiceSupport.InvalidUpdateProjectSubscriptionModel = GetDefaultInvalidProjectSubscriptionServiceUpdateRequest();
        }

        [When(@"I Post Invalid SubscriptionService ProjectSubscriptionUpdate Request")]
        public void WhenIPostInvalidSubscriptionServiceProjectSubscriptionUpdateRequest()
        {
            string contentType = "application/json";
            subscriptionServiceSupport.PostInValidProjectSubscriptionUpdateRequestToService(contentType, HttpStatusCode.BadRequest);
        }

        [When(@"I Set Invalid SubscriptionService ProjectSubscriptionCreate SubscriptionUID  To '(.*)'")]
        public void WhenISetInvalidSubscriptionServiceProjectSubscriptionCreateSubscriptionUIDTo(string subscriptionUid)
        {
            subscriptionServiceSupport.InvalidCreateProjectSubscriptionModel.SubscriptionUID = subscriptionUid;
        }

        [When(@"I Set Invalid SubscriptionService ProjectSubscriptionCreate CustomerUID To '(.*)'")]
        public void WhenISetInvalidSubscriptionServiceProjectSubscriptionCreateCustomerUIDTo(string customerUid)
        {
            subscriptionServiceSupport.InvalidCreateProjectSubscriptionModel.CustomerUID = customerUid;
        }

        [When(@"I Set Invalid SubscriptionService ProjectSubscriptionCreate SubscriptionType To '(.*)'")]
        public void WhenISetInvalidSubscriptionServiceProjectSubscriptionCreateSubscriptionTypeTo(string subscriptionType)
        {
            subscriptionServiceSupport.InvalidCreateProjectSubscriptionModel.SubscriptionType = subscriptionType;
        }

        [When(@"I Set Invalid SubscriptionService ProjectSubscriptionCreate StartDate To '(.*)'")]
        public void WhenISetInvalidSubscriptionServiceProjectSubscriptionCreateStartDateTo(string startDate)
        {
            subscriptionServiceSupport.InvalidCreateProjectSubscriptionModel.StartDate = startDate;
        }

        [When(@"I Set Invalid SubscriptionService ProjectSubscriptionCreate EndDate To '(.*)'")]
        public void WhenISetInvalidSubscriptionServiceProjectSubscriptionCreateEndDateTo(string endDate)
        {
            subscriptionServiceSupport.InvalidCreateProjectSubscriptionModel.EndDate = endDate;
        }

        [When(@"I Set Invalid SubscriptionService ProjectSubscriptionCreate ActionUTC To '(.*)'")]
        public void WhenISetInvalidSubscriptionServiceProjectSubscriptionCreateActionUTCTo(string actionUtc)
        {
            subscriptionServiceSupport.InvalidCreateProjectSubscriptionModel.ActionUTC = actionUtc;
        }

        [When(@"I Set Invalid SubscriptionService ProjectSubscriptionUpdate SubscriptionUID  To '(.*)'")]
        public void WhenISetInvalidSubscriptionServiceProjectSubscriptionUpdateSubscriptionUIDTo(string subscriptionUid)
        {
            subscriptionServiceSupport.InvalidUpdateProjectSubscriptionModel.SubscriptionUID = subscriptionUid;
        }

        [When(@"I Set Invalid SubscriptionService ProjectSubscriptionUpdate CustomerUID To '(.*)'")]
        public void WhenISetInvalidSubscriptionServiceProjectSubscriptionUpdateCustomerUIDTo(string customerUid)
        {
            subscriptionServiceSupport.InvalidUpdateProjectSubscriptionModel.CustomerUID = customerUid;
        }

        [When(@"I Set Invalid SubscriptionService ProjectSubscriptionUpdate SubscriptionType To '(.*)'")]
        public void WhenISetInvalidSubscriptionServiceProjectSubscriptionUpdateSubscriptionTypeTo(string subscriptionType)
        {
            subscriptionServiceSupport.InvalidUpdateProjectSubscriptionModel.SubscriptionType = subscriptionType;
        }

        [When(@"I Set Invalid SubscriptionService ProjectSubscriptionUpdate StartDate To '(.*)'")]
        public void WhenISetInvalidSubscriptionServiceProjectSubscriptionUpdateStartDateTo(string startDate)
        {
            subscriptionServiceSupport.InvalidUpdateProjectSubscriptionModel.StartDate = startDate;
        }

        [When(@"I Set Invalid SubscriptionService ProjectSubscriptionUpdate EndDate To '(.*)'")]
        public void WhenISetInvalidSubscriptionServiceProjectSubscriptionUpdateEndDateTo(string endDate)
        {
            subscriptionServiceSupport.InvalidUpdateProjectSubscriptionModel.EndDate = endDate;
        }

        [When(@"I Set Invalid SubscriptionService ProjectSubscriptionUpdate ActionUTC To '(.*)'")]
        public void WhenISetInvalidSubscriptionServiceProjectSubscriptionUpdateActionUTCTo(string actionUtc)
        {
            subscriptionServiceSupport.InvalidUpdateProjectSubscriptionModel.ActionUTC = actionUtc;
        }

        [Then(@"SubscriptionService CustomerSubscriptionUpdate Response With '(.*)' Should Be Returned")]
        public void ThenSubscriptionServiceCustomerSubscriptionUpdateResponseWithShouldBeReturned(string errorMessage)
        {
            subscriptionServiceSupport.VerifyCustomerSubscriptionErrorResponse(errorMessage);
        }

        [Then(@"SubscriptionService ProjectSubscriptionCreate Response With '(.*)' Should Be Returned")]
        public void ThenSubscriptionServiceProjectSubscriptionCreateResponseWithShouldBeReturned(string errorMessage)
        {
            subscriptionServiceSupport.VerifyProjectSubscriptionErrorResponse(errorMessage);
        }

        [Then(@"SubscriptionService ProjectSubscriptionUpdate Response With '(.*)' Should Be Returned")]
        public void ThenSubscriptionServiceProjectSubscriptionUpdateResponseWithShouldBeReturned(string errorMessage)
        {
            subscriptionServiceSupport.VerifyProjectSubscriptionErrorResponse(errorMessage);
        }

        [When(@"I set SubscriptionService ProjectSubscriptionCreate SubscriptionType To '(.*)'")]
        public void WhenISetSubscriptionServiceProjectSubscriptionCreateSubscriptionTypeTo(string subscriptionType)
        {
            subscriptionServiceSupport.CreateProjectSubscriptionModel.SubscriptionType = InputGenerator.GetValue(subscriptionType);
        }

        [When(@"I set SubscriptionService ProjectSubscriptionUpdate SubscriptionType To '(.*)'")]
        public void WhenISetSubscriptionServiceProjectSubscriptionUpdateSubscriptionTypeTo(string subscriptionType)
        {
            subscriptionServiceSupport.UpdateProjectSubscriptionModel.SubscriptionType = InputGenerator.GetValue(subscriptionType);
        }

        [When(@"I Associate A ProjectSubscription To A Project")]
        public void WhenIAssociateAProjectSubscriptionToAProject()
        {
            subscriptionServiceSupport.AssociateProjectSubscriptionModel.SubscriptionUID = defaultValidProjectSubscriptionServiceCreateModel.SubscriptionUID;
        }

        [When(@"I set SubscriptionService CustomerSubscriptionCreate StartDate To FutureDate")]
        public void WhenISetSubscriptionServiceCustomerSubscriptionCreateStartDateToFutureDate()
        {
            subscriptionServiceSupport.CreateCustomerSubscriptionModel.StartDate = DateTime.UtcNow.AddYears(10);
        }

        [When(@"I set SubscriptionService ProjectSubscriptionCreate StartDate To FutureDate")]
        public void WhenISetSubscriptionServiceProjectSubscriptionCreateStartDateToFutureDate()
        {
            subscriptionServiceSupport.CreateProjectSubscriptionModel.StartDate = DateTime.UtcNow.AddYears(10);
        }


        [Then(@"The SubscriptionServiceRead Response should return the Subscription Details SubscriptionCount as '(.*)'")]
        public void ThenTheSubscriptionServiceReadResponseShouldReturnTheSubscriptionDetailsSubscriptionCountAs(int count)
        {
            subscriptionServiceSupport.VerifySubscriptionCount(count);
        }


        #endregion

    }
}
