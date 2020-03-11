using AutomationCore.API.Framework.Common;
using AutomationCore.API.Framework.Library;
using AutomationCore.Shared.Library;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VSS.KafkaWrapper;
using VSS.KafkaWrapper.Models;
using VSS.MasterData.Subscription.AcceptanceTests.Helpers;
using VSS.MasterData.Subscription.AcceptanceTests.Resources;
using VSS.MasterData.Subscription.AcceptanceTests.Utils.Config;
using VSS.MasterData.Subscription.AcceptanceTests.Utils.Features.Classes.SubscriptionService.AssetSubsciption;
using VSS.MasterData.Subscription.AcceptanceTests.Utils.Features.Classes.SubscriptionService.ProjectSubscription;
using VSS.MasterData.Subscription.AcceptanceTests.Utils.Features.Classes.SubscriptionService.CustomerSubscription;
using VSS.MasterData.Subscription.AcceptanceTests.Utils.Features.Classes.SubscriptionService.AssociateProjectSubscription;
using VSS.MasterData.Subscription.AcceptanceTests.Utils.Features.Classes.SubscriptionService.DissociateProjectSubscription;
using VSS.MasterData.Subscription.AcceptanceTests.Utils.Features.Classes.SubscriptionService;
using VSS.Kafka.Factory.Model;
using VSS.Kafka.Factory.Interface;
using System.IO;
using MySql.Data.MySqlClient;

namespace VSS.MasterData.Subscription.AcceptanceTests.Scenarios.SubscriptionService
{
    public class SubscriptionServiceSupport : IHandler
    {
        public CreateAssetSubscriptionEvent CreateAssetSubscriptionModel = new CreateAssetSubscriptionEvent();
        public UpdateAssetSubscriptionEvent UpdateAssetSubscriptionModel = new UpdateAssetSubscriptionEvent();
        public InvalidCreateAssetSubscriptionEvent InvalidCreateAssetSubscriptionModel = new InvalidCreateAssetSubscriptionEvent();
        public InvalidUpdateAssetSubscriptionEvent InvalidUpdateAssetSubscriptionModel = new InvalidUpdateAssetSubscriptionEvent();
        public CreateCustomerSubscriptionEvent CreateCustomerSubscriptionModel = new CreateCustomerSubscriptionEvent();
        public UpdateCustomerSubscriptionEvent UpdateCustomerSubscriptionModel = new UpdateCustomerSubscriptionEvent();
        public CreateProjectSubscriptionEvent CreateProjectSubscriptionModel = new CreateProjectSubscriptionEvent();
        public UpdateProjectSubscriptionEvent UpdateProjectSubscriptionModel = new UpdateProjectSubscriptionEvent();
        public AssociateProjectSubscriptionEvent AssociateProjectSubscriptionModel = new AssociateProjectSubscriptionEvent();
        public DissociateProjectSubscriptionEvent DissociateProjectSubscriptionModel = new DissociateProjectSubscriptionEvent();
        public InvalidCreateCustomerSubscriptionEvent InvalidCreateCustomerSubscriptionModel = new InvalidCreateCustomerSubscriptionEvent();
        public InvalidUpdateCustomerSubscriptionEvent InvalidUpdateCustomerSubscriptionModel = new InvalidUpdateCustomerSubscriptionEvent();
        public InvalidCreateProjectSubscriptionEvent InvalidCreateProjectSubscriptionModel = new InvalidCreateProjectSubscriptionEvent();
        public InvalidUpdateProjectSubscriptionEvent InvalidUpdateProjectSubscriptionModel = new InvalidUpdateProjectSubscriptionEvent();
        public InvalidAssociateProjectSubscriptionEvent InvalidAssociateProjectSubscriptionModel = new InvalidAssociateProjectSubscriptionEvent();
        public InvalidDissociateProjectSubscriptionEvent InvalidDissociateProjectSubscriptionModel = new InvalidDissociateProjectSubscriptionEvent();
        private static Log4Net Log = new Log4Net(typeof(SubscriptionServiceSupport));

        public string ResponseString = string.Empty;
        public ConsumerWrapper _consumerWrapper;
        public static string UserName;
        public static string PassWord;


        public CreateAssetSubscriptionModel assetSubscriptionCreateResponse = null;
        public UpdateAssetSubscriptionModel assetSubscriptionUpdateResponse = null;
        public CreateProjectSubscriptionModel projectSubscriptionCreateResponse = null;
        public UpdateProjectSubscriptionModel projectSubscriptionUpdateResponse = null;
        public CreateCustomerSubscriptionModel customerSubscriptionCreateResponse = null;
        public UpdateCustomerSubscriptionModel customerSubscriptionUpdateResponse = null;
        public AssociateProjectSubscriptionModel associateProjectSubscriptionResponse = null;
        public DissociateProjectSubscriptionModel dissociateProjectSubscriptionResponse = null;

        public CheckForAssetSubscriptionCreateHandler _checkForAssetSubscriptionCreateHandler;
        public CheckForAssetSubscriptionUpdateHandler _checkForAssetSubscriptionUpdateHandler;
        public CheckForProjectSubscriptionCreateHandler _checkForProjectSubscriptionCreateHandler;
        public CheckForProjectSubscriptionUpdateHandler _checkForProjectSubscriptionUpdateHandler;
        public CheckForCustomerSubscriptionCreateHandler _checkForCustomerSubscriptionCreateHandler;
        public CheckForCustomerSubscriptionUpdateHandler _checkForCustomerSubscriptionUpdateHandler;
        public CheckForAssociateProjectSubscriptionHandler _checkForAssociateProjectSubscriptionHandler;
        public CheckForDissociateProjectSubscriptionHandler _checkForDissociateProjectSubscriptionHandler;

        #region Constructors

        public SubscriptionServiceSupport(Log4Net myLog)
        {
            SubscriptionServiceConfig.SetupEnvironment();
            Log = myLog;
        }

        #endregion

        #region Post Methods

        public void PostValidAssetSubscriptionCreateRequestToService()
        {
            string requestString = JsonConvert.SerializeObject(CreateAssetSubscriptionModel);

            try
            {
                string accessToken = SubscriptionServiceConfig.GetValidUserAccessToken();
                LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
                RestClientUtil.DoHttpRequest(SubscriptionServiceConfig.AssetSubscriptionServiceEndpoint, HeaderSettings.PostMethod, accessToken,
                   HeaderSettings.JsonMediaType, requestString, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);

            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Subscription Service", e);
                throw new Exception(e + " Got Error While Posting Data To Subscription Service");
            }
        }

        public void PostValidAssetSubscriptionUpdateRequestToService()
        {
            string requestString = JsonConvert.SerializeObject(UpdateAssetSubscriptionModel);

            try
            {
                string accessToken = SubscriptionServiceConfig.GetValidUserAccessToken();
                LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
                RestClientUtil.DoHttpRequest(SubscriptionServiceConfig.AssetSubscriptionServiceEndpoint, HeaderSettings.PutMethod, accessToken,
                   HeaderSettings.JsonMediaType, requestString, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);

            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Subscription Service", e);
                throw new Exception(e + " Got Error While Posting Data To Subscription Service");
            }
        }

        public void PostValidCustomerSubscriptionCreateRequestToService()
        {
            string requestString = JsonConvert.SerializeObject(CreateCustomerSubscriptionModel);

            try
            {
                string accessToken = SubscriptionServiceConfig.GetValidUserAccessToken();
                LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
                RestClientUtil.DoHttpRequest(SubscriptionServiceConfig.CustomerSubscriptionServiceEndpoint, HeaderSettings.PostMethod, accessToken,
                   HeaderSettings.JsonMediaType, requestString, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);

            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Subscription Service", e);
                throw new Exception(e + " Got Error While Posting Data To Subscription Service");
            }
        }

        public void PostValidCustomerSubscriptionUpdateRequestToService()
        {
            string requestString = JsonConvert.SerializeObject(UpdateCustomerSubscriptionModel);

            try
            {
                string accessToken = SubscriptionServiceConfig.GetValidUserAccessToken();
                LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
                RestClientUtil.DoHttpRequest(SubscriptionServiceConfig.CustomerSubscriptionServiceEndpoint, HeaderSettings.PutMethod, accessToken,
                   HeaderSettings.JsonMediaType, requestString, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);

            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Subscription Service", e);
                throw new Exception(e + " Got Error While Posting Data To Subscription Service");
            }
        }

        public void PostValidProjectSubscriptionCreateRequestToService()
        {
            string requestString = JsonConvert.SerializeObject(CreateProjectSubscriptionModel);

            try
            {
                string accessToken = SubscriptionServiceConfig.GetValidUserAccessToken();
                LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
                RestClientUtil.DoHttpRequest(SubscriptionServiceConfig.ProjectSubscriptionServiceEndpoint, HeaderSettings.PostMethod, accessToken,
                   HeaderSettings.JsonMediaType, requestString, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);

            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Subscription Service", e);
                throw new Exception(e + " Got Error While Posting Data To Subscription Service");
            }
        }

        public void PostValidProjectSubscriptionUpdateRequestToService()
        {
            string requestString = JsonConvert.SerializeObject(UpdateProjectSubscriptionModel);

            try
            {
                string accessToken = SubscriptionServiceConfig.GetValidUserAccessToken();
                LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
                RestClientUtil.DoHttpRequest(SubscriptionServiceConfig.ProjectSubscriptionServiceEndpoint, HeaderSettings.PutMethod, accessToken,
                   HeaderSettings.JsonMediaType, requestString, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);

            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Subscription Service", e);
                throw new Exception(e + " Got Error While Posting Data To Subscription Service");
            }
        }

        public void PostValidAssociateProjectSubscriptionRequestToService()
        {
            string requestString = JsonConvert.SerializeObject(AssociateProjectSubscriptionModel);

            try
            {
                string accessToken = SubscriptionServiceConfig.GetValidUserAccessToken();
                LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
                RestClientUtil.DoHttpRequest(SubscriptionServiceConfig.ProjectSubscriptionServiceEndpoint + "/AssociateProjectSubscription", HeaderSettings.PostMethod, accessToken,
                   HeaderSettings.JsonMediaType, requestString, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);

            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Subscription Service", e);
                throw new Exception(e + " Got Error While Posting Data To Subscription Service");
            }
        }

        public void PostValidDissociateProjectSubscriptionRequestToService()
        {
            string requestString = JsonConvert.SerializeObject(DissociateProjectSubscriptionModel);

            try
            {
                string accessToken = SubscriptionServiceConfig.GetValidUserAccessToken();
                LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
                RestClientUtil.DoHttpRequest(SubscriptionServiceConfig.ProjectSubscriptionServiceEndpoint + "/DissociateProjectSubscription", HeaderSettings.PostMethod, accessToken,
                   HeaderSettings.JsonMediaType, requestString, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);

            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Subscription Service", e);
                throw new Exception(e + " Got Error While Posting Data To Subscription Service");
            }
        }

        public void PostValidCustomerSubscriptionReadRequestToService(string customerUid)
        {

            try
            {
                string accessToken = SubscriptionServiceConfig.GetValidUserAccessToken();
                LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + customerUid);
                ResponseString = RestClientUtil.DoHttpRequest(SubscriptionServiceConfig.CustomerSubscriptionServiceEndpoint + "/" + customerUid, HeaderSettings.GetMethod, accessToken,
                   HeaderSettings.JsonMediaType, null, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);

            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Subscription Service", e);
                throw new Exception(e + " Got Error While Posting Data To Subscription Service");
            }
        }

        public void PostValidGetActiveProjectSubscriptionForCustomer(string customerUid)
        {

            try
            {
                string accessToken = SubscriptionServiceConfig.GetValidUserAccessToken();
                LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + customerUid);
                ResponseString = RestClientUtil.DoHttpRequest(SubscriptionServiceConfig.CustomerSubscriptionServiceEndpoint + "/" + "project/" + customerUid, HeaderSettings.GetMethod, accessToken,
                   HeaderSettings.JsonMediaType, null, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);

            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Subscription Service", e);
                throw new Exception(e + " Got Error While Posting Data To Subscription Service");
            }
        }

        public void PostInValidAssetSubscriptionCreateRequestToService(string contentType, HttpStatusCode actualResponse)
        {
            string requestString = JsonConvert.SerializeObject(InvalidCreateAssetSubscriptionModel);

            try
            {
                string accessToken = SubscriptionServiceConfig.GetValidUserAccessToken();
                LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
                ResponseString = RestClientUtil.DoInvalidHttpRequest(SubscriptionServiceConfig.AssetSubscriptionServiceEndpoint, HeaderSettings.PostMethod, accessToken,
                   contentType, requestString, actualResponse, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);

            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Subscription Service", e);
                throw new Exception(e + " Got Error While Posting Data To Subscription Service");
            }
        }

        public void PostInValidAssetSubscriptionUpdateRequestToService(string contentType, HttpStatusCode actualResponse)
        {
            string requestString = JsonConvert.SerializeObject(InvalidUpdateAssetSubscriptionModel);

            try
            {
                string accessToken = SubscriptionServiceConfig.GetValidUserAccessToken();
                LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
                ResponseString = RestClientUtil.DoInvalidHttpRequest(SubscriptionServiceConfig.AssetSubscriptionServiceEndpoint, HeaderSettings.PutMethod, accessToken,
                   contentType, requestString, actualResponse, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);

            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Subscription Service", e);
                throw new Exception(e + " Got Error While Posting Data To Subscription Service");
            }
        }

        public void PostInValidCustomerSubscriptionCreateRequestToService(string contentType, HttpStatusCode actualResponse)
        {
            string requestString = JsonConvert.SerializeObject(InvalidCreateCustomerSubscriptionModel);

            try
            {
                string accessToken = SubscriptionServiceConfig.GetValidUserAccessToken();
                LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
                ResponseString = RestClientUtil.DoInvalidHttpRequest(SubscriptionServiceConfig.ProjectSubscriptionServiceEndpoint, HeaderSettings.PostMethod, accessToken,
                   contentType, requestString, actualResponse, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);

            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Subscription Service", e);
                throw new Exception(e + " Got Error While Posting Data To Subscription Service");
            }
        }

        public void PostInValidCustomerSubscriptionUpdateRequestToService(string contentType, HttpStatusCode actualResponse)
        {
            string requestString = JsonConvert.SerializeObject(InvalidUpdateCustomerSubscriptionModel);

            try
            {
                string accessToken = SubscriptionServiceConfig.GetValidUserAccessToken();
                LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
                ResponseString = RestClientUtil.DoInvalidHttpRequest(SubscriptionServiceConfig.ProjectSubscriptionServiceEndpoint, HeaderSettings.PutMethod, accessToken,
                   contentType, requestString, actualResponse, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);

            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Subscription Service", e);
                throw new Exception(e + " Got Error While Posting Data To Subscription Service");
            }
        }

        public void PostInValidProjectSubscriptionCreateRequestToService(string contentType, HttpStatusCode actualResponse)
        {
            string requestString = JsonConvert.SerializeObject(InvalidCreateProjectSubscriptionModel);

            try
            {
                string accessToken = SubscriptionServiceConfig.GetValidUserAccessToken();
                LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
                ResponseString = RestClientUtil.DoInvalidHttpRequest(SubscriptionServiceConfig.ProjectSubscriptionServiceEndpoint, HeaderSettings.PostMethod, accessToken,
                   contentType, requestString, actualResponse, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);
            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Subscription Service", e);
                throw new Exception(e + " Got Error While Posting Data To Subscription Service");
            }
        }

        public void PostInValidProjectSubscriptionUpdateRequestToService(string contentType, HttpStatusCode actualResponse)
        {
            string requestString = JsonConvert.SerializeObject(InvalidUpdateProjectSubscriptionModel);

            try
            {
                string accessToken = SubscriptionServiceConfig.GetValidUserAccessToken();
                LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
                ResponseString = RestClientUtil.DoInvalidHttpRequest(SubscriptionServiceConfig.ProjectSubscriptionServiceEndpoint, HeaderSettings.PutMethod, accessToken,
                   contentType, requestString, actualResponse, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);

            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Subscription Service", e);
                throw new Exception(e + " Got Error While Posting Data To Subscription Service");
            }
        }

        public void PostInValidAssociateProjectSubscriptionRequestToService(string contentType, HttpStatusCode actualResponse)
        {
            string requestString = JsonConvert.SerializeObject(InvalidAssociateProjectSubscriptionModel);

            try
            {
                string accessToken = SubscriptionServiceConfig.GetValidUserAccessToken();
                LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
                ResponseString = RestClientUtil.DoInvalidHttpRequest(SubscriptionServiceConfig.ProjectSubscriptionServiceEndpoint, HeaderSettings.PostMethod, accessToken,
                   contentType, requestString, actualResponse, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);

            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Subscription Service", e);
                throw new Exception(e + " Got Error While Posting Data To Subscription Service");
            }
        }

        public void PostInValidDissociateProjectSubscriptionRequestToService(string contentType, HttpStatusCode actualResponse)
        {
            string requestString = JsonConvert.SerializeObject(InvalidDissociateProjectSubscriptionModel);

            try
            {
                string accessToken = SubscriptionServiceConfig.GetValidUserAccessToken();
                LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
                ResponseString = RestClientUtil.DoInvalidHttpRequest(SubscriptionServiceConfig.ProjectSubscriptionServiceEndpoint, HeaderSettings.PutMethod, accessToken,
                   contentType, requestString, actualResponse, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);

            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Subscription Service", e);
                throw new Exception(e + " Got Error While Posting Data To Subscription Service");
            }
        }

        #endregion

        #region Response Verification

        public void VerifyAssetSubscriptionCreateResponse()
        {
            WaitForKafkaResponseAfterAssetSubscriptionCreate();


            if (CreateAssetSubscriptionModel.SubscriptionUID != null)
                Assert.AreEqual(CreateAssetSubscriptionModel.SubscriptionUID, assetSubscriptionCreateResponse.CreateAssetSubscriptionEvent.SubscriptionUID);
            if (CreateAssetSubscriptionModel.CustomerUID != null)
                Assert.AreEqual(CreateAssetSubscriptionModel.CustomerUID, assetSubscriptionCreateResponse.CreateAssetSubscriptionEvent.CustomerUID);
            if (CreateAssetSubscriptionModel.AssetUID != null)
                Assert.AreEqual(CreateAssetSubscriptionModel.AssetUID, assetSubscriptionCreateResponse.CreateAssetSubscriptionEvent.AssetUID);
            if (CreateAssetSubscriptionModel.SubscriptionType != null)
                Assert.AreEqual(CreateAssetSubscriptionModel.SubscriptionType, assetSubscriptionCreateResponse.CreateAssetSubscriptionEvent.SubscriptionType);
            if (CreateAssetSubscriptionModel.Source == null)
            {
                Assert.AreEqual("Store", assetSubscriptionCreateResponse.CreateAssetSubscriptionEvent.Source);
            }
            else if (CreateAssetSubscriptionModel.Source != null)
            {
                Assert.AreEqual(CreateAssetSubscriptionModel.Source, assetSubscriptionCreateResponse.CreateAssetSubscriptionEvent.Source);
            }
            if (CreateAssetSubscriptionModel.StartDate != null)
                Assert.AreEqual(CreateAssetSubscriptionModel.StartDate.ToString("yyyy-MM-ddTHH:mm:ss"), assetSubscriptionCreateResponse.CreateAssetSubscriptionEvent.StartDate.ToString("yyyy-MM-ddTHH:mm:ss"));
            if (CreateAssetSubscriptionModel.EndDate != null)
                Assert.AreEqual(CreateAssetSubscriptionModel.EndDate.ToString("yyyy-MM-ddTHH:mm:ss"), assetSubscriptionCreateResponse.CreateAssetSubscriptionEvent.EndDate.ToString("yyyy-MM-ddTHH:mm:ss"));

            if (CreateAssetSubscriptionModel.ActionUTC != null)
                Assert.AreEqual(CreateAssetSubscriptionModel.ActionUTC.ToString("yyyyMMddhhmmss"), assetSubscriptionCreateResponse.CreateAssetSubscriptionEvent.ActionUTC.ToString("yyyyMMddhhmmss"));

            assetSubscriptionCreateResponse = null;// Reassigning the response back to null

        }

        public void VerifyAssetSubscriptionUpdateResponse()
        {
            WaitForKafkaResponseAfterAssetSubscriptionUpdate();


            if (UpdateAssetSubscriptionModel.SubscriptionUID != null)
                Assert.AreEqual(UpdateAssetSubscriptionModel.SubscriptionUID, assetSubscriptionUpdateResponse.UpdateAssetSubscriptionEvent.SubscriptionUID);
            if (UpdateAssetSubscriptionModel.CustomerUID != null)
                Assert.AreEqual(UpdateAssetSubscriptionModel.CustomerUID, assetSubscriptionUpdateResponse.UpdateAssetSubscriptionEvent.CustomerUID);
            if (UpdateAssetSubscriptionModel.AssetUID != null)
                Assert.AreEqual(UpdateAssetSubscriptionModel.AssetUID, assetSubscriptionUpdateResponse.UpdateAssetSubscriptionEvent.AssetUID);
            if (UpdateAssetSubscriptionModel.SubscriptionType != null)
                Assert.AreEqual(UpdateAssetSubscriptionModel.SubscriptionType, assetSubscriptionUpdateResponse.UpdateAssetSubscriptionEvent.SubscriptionType);
            if (UpdateAssetSubscriptionModel.Source == null)
            {
                Assert.AreEqual("Store", assetSubscriptionUpdateResponse.UpdateAssetSubscriptionEvent.Source);
            }
            else if (UpdateAssetSubscriptionModel.Source != null)
            {
                Assert.AreEqual(UpdateAssetSubscriptionModel.Source, assetSubscriptionUpdateResponse.UpdateAssetSubscriptionEvent.Source);
            }

            if (UpdateAssetSubscriptionModel.StartDate != null)
                Assert.AreEqual(UpdateAssetSubscriptionModel.StartDate?.ToString("yyyy-MM-ddTHH:mm:ss"), assetSubscriptionUpdateResponse.UpdateAssetSubscriptionEvent.StartDate?.ToString("yyyy-MM-ddTHH:mm:ss"));
            if (UpdateAssetSubscriptionModel.EndDate != null)
                Assert.AreEqual(UpdateAssetSubscriptionModel.EndDate?.ToString("yyyy-MM-ddTHH:mm:ss"), assetSubscriptionUpdateResponse.UpdateAssetSubscriptionEvent.EndDate?.ToString("yyyy-MM-ddTHH:mm:ss"));
            if (UpdateAssetSubscriptionModel.ActionUTC != null)
                Assert.AreEqual(UpdateAssetSubscriptionModel.ActionUTC.ToString("yyyy-MM-ddTHH:mm:ss"), assetSubscriptionUpdateResponse.UpdateAssetSubscriptionEvent.ActionUTC.ToString("yyyy-MM-ddTHH:mm:ss"));

            assetSubscriptionUpdateResponse = null;// Reassigning the response back to null

        }

        public void VerifyCustomerSubscriptionCreateResponse()
        {
            WaitForKafkaResponseAfterCustomerSubscriptionCreate();

            if (customerSubscriptionCreateResponse != null)
            {
                if (CreateCustomerSubscriptionModel.SubscriptionUID != null)
                    Assert.AreEqual(CreateCustomerSubscriptionModel.SubscriptionUID, customerSubscriptionCreateResponse.CreateCustomerSubscriptionEvent.SubscriptionUID);
                if (CreateCustomerSubscriptionModel.CustomerUID != null)
                    Assert.AreEqual(CreateCustomerSubscriptionModel.CustomerUID, customerSubscriptionCreateResponse.CreateCustomerSubscriptionEvent.CustomerUID);
                if (CreateCustomerSubscriptionModel.SubscriptionType != null)
                    Assert.AreEqual(CreateCustomerSubscriptionModel.SubscriptionType, customerSubscriptionCreateResponse.CreateCustomerSubscriptionEvent.SubscriptionType);
                if (CreateCustomerSubscriptionModel.StartDate != null)
                    Assert.AreEqual(CreateCustomerSubscriptionModel.StartDate.ToString("yyyyMMddhhmmss"), customerSubscriptionCreateResponse.CreateCustomerSubscriptionEvent.StartDate.ToString("yyyyMMddhhmmss"));
                if (CreateCustomerSubscriptionModel.EndDate != null)
                    Assert.AreEqual(CreateCustomerSubscriptionModel.EndDate.ToString("yyyyMMddhhmmss"), customerSubscriptionCreateResponse.CreateCustomerSubscriptionEvent.EndDate.ToString("yyyyMMddhhmmss"));
                if (CreateCustomerSubscriptionModel.ActionUTC != null)
                    Assert.AreEqual(CreateCustomerSubscriptionModel.ActionUTC.ToString("yyyyMMddhhmmss"), customerSubscriptionCreateResponse.CreateCustomerSubscriptionEvent.ActionUTC.ToString("yyyyMMddhhmmss"));

                customerSubscriptionCreateResponse = null;// Reassigning the response back to null
            }
        }

        public void VerifyCustomerSubscriptionUpdateResponse()
        {
            WaitForKafkaResponseAfterCustomerSubscriptionUpdate();

            if (UpdateCustomerSubscriptionModel.SubscriptionUID != null)
                Assert.AreEqual(UpdateCustomerSubscriptionModel.SubscriptionUID, customerSubscriptionUpdateResponse.UpdateCustomerSubscriptionEvent.SubscriptionUID);
            if (UpdateCustomerSubscriptionModel.StartDate != null)
                Assert.AreEqual(UpdateCustomerSubscriptionModel.StartDate, customerSubscriptionUpdateResponse.UpdateCustomerSubscriptionEvent.StartDate);
            if (UpdateCustomerSubscriptionModel.EndDate != null)
                Assert.AreEqual(UpdateCustomerSubscriptionModel.EndDate, customerSubscriptionUpdateResponse.UpdateCustomerSubscriptionEvent.EndDate);
            if (UpdateCustomerSubscriptionModel.ActionUTC != null)
                Assert.AreEqual(UpdateCustomerSubscriptionModel.ActionUTC, customerSubscriptionUpdateResponse.UpdateCustomerSubscriptionEvent.ActionUTC);

            customerSubscriptionUpdateResponse = null;// Reassigning the response back to null

        }

        public void VerifyProjectSubscriptionCreateResponse()
        {
            WaitForKafkaResponseAfterProjectSubscriptionCreate();

            if (CreateProjectSubscriptionModel.SubscriptionUID != null)
                Assert.AreEqual(CreateProjectSubscriptionModel.SubscriptionUID, projectSubscriptionCreateResponse.CreateProjectSubscriptionEvent.SubscriptionUID);
            if (CreateProjectSubscriptionModel.CustomerUID != null)
                Assert.AreEqual(CreateProjectSubscriptionModel.CustomerUID, projectSubscriptionCreateResponse.CreateProjectSubscriptionEvent.CustomerUID);
            if (CreateProjectSubscriptionModel.SubscriptionType != null)
                Assert.AreEqual(CreateProjectSubscriptionModel.SubscriptionType, projectSubscriptionCreateResponse.CreateProjectSubscriptionEvent.SubscriptionType);
            if (CreateProjectSubscriptionModel.StartDate != null)
                Assert.AreEqual(CreateProjectSubscriptionModel.StartDate, projectSubscriptionCreateResponse.CreateProjectSubscriptionEvent.StartDate);
            if (CreateProjectSubscriptionModel.EndDate != null)
                Assert.AreEqual(CreateProjectSubscriptionModel.EndDate, projectSubscriptionCreateResponse.CreateProjectSubscriptionEvent.EndDate);
            if (CreateProjectSubscriptionModel.ActionUTC != null)
                Assert.AreEqual(CreateProjectSubscriptionModel.ActionUTC, projectSubscriptionCreateResponse.CreateProjectSubscriptionEvent.ActionUTC);

            projectSubscriptionCreateResponse = null;// Reassigning the response back to null

        }

        public void VerifyProjectSubscriptionUpdateResponse()
        {
            WaitForKafkaResponseAfterProjectSubscriptionUpdate();

            if (UpdateProjectSubscriptionModel.SubscriptionUID != null)
                Assert.AreEqual(UpdateProjectSubscriptionModel.SubscriptionUID, projectSubscriptionUpdateResponse.UpdateProjectSubscriptionEvent.SubscriptionUID);
            if (UpdateProjectSubscriptionModel.CustomerUID != null)
                Assert.AreEqual(UpdateProjectSubscriptionModel.CustomerUID, projectSubscriptionUpdateResponse.UpdateProjectSubscriptionEvent.CustomerUID);
            if (UpdateProjectSubscriptionModel.SubscriptionType != null)
                Assert.AreEqual(UpdateProjectSubscriptionModel.SubscriptionType, projectSubscriptionUpdateResponse.UpdateProjectSubscriptionEvent.SubscriptionType);
            if (UpdateProjectSubscriptionModel.StartDate != null)
                Assert.AreEqual(UpdateProjectSubscriptionModel.StartDate, projectSubscriptionUpdateResponse.UpdateProjectSubscriptionEvent.StartDate);
            if (UpdateProjectSubscriptionModel.EndDate != null)
                Assert.AreEqual(UpdateProjectSubscriptionModel.EndDate, projectSubscriptionUpdateResponse.UpdateProjectSubscriptionEvent.EndDate);
            if (UpdateProjectSubscriptionModel.ActionUTC != null)
                Assert.AreEqual(UpdateProjectSubscriptionModel.ActionUTC, projectSubscriptionUpdateResponse.UpdateProjectSubscriptionEvent.ActionUTC);

            projectSubscriptionUpdateResponse = null;// Reassigning the response back to null
        }

        public void VerifyAssociateProjectSubscriptionResponse()
        {
            WaitForKafkaResponseAfterAssociateProjectSubscription();

            if (AssociateProjectSubscriptionModel.SubscriptionUID != null)
                Assert.AreEqual(AssociateProjectSubscriptionModel.SubscriptionUID, associateProjectSubscriptionResponse.AssociateProjectSubscriptionEvent.SubscriptionUID);
            if (AssociateProjectSubscriptionModel.ProjectUID != null)
                Assert.AreEqual(AssociateProjectSubscriptionModel.ProjectUID, associateProjectSubscriptionResponse.AssociateProjectSubscriptionEvent.ProjectUID);
            if (AssociateProjectSubscriptionModel.EffectiveDate != null)
                Assert.AreEqual(AssociateProjectSubscriptionModel.EffectiveDate, associateProjectSubscriptionResponse.AssociateProjectSubscriptionEvent.EffectiveDate);

            associateProjectSubscriptionResponse = null;// Reassigning the response back to null
        }

        public void VerifyDissociateProjectSubscriptionResponse()
        {
            WaitForKafkaResponseAfterDissociateProjectSubscription();

            if (DissociateProjectSubscriptionModel.SubscriptionUID != null)
                Assert.AreEqual(DissociateProjectSubscriptionModel.SubscriptionUID, dissociateProjectSubscriptionResponse.DissociateProjectSubscriptionEvent.SubscriptionUID);
            if (DissociateProjectSubscriptionModel.ProjectUID != null)
                Assert.AreEqual(DissociateProjectSubscriptionModel.ProjectUID, dissociateProjectSubscriptionResponse.DissociateProjectSubscriptionEvent.ProjectUID);
            if (DissociateProjectSubscriptionModel.EffectiveDate != null)
                Assert.AreEqual(DissociateProjectSubscriptionModel.EffectiveDate, dissociateProjectSubscriptionResponse.DissociateProjectSubscriptionEvent.EffectiveDate);

            dissociateProjectSubscriptionResponse = null;// Reassigning the response back to null
        }

        public void VerifySubscriptionServiceReadResponseAfterCreate()
        {
            SubscriptionServiceReadResponseModel response = JsonConvert.DeserializeObject<SubscriptionServiceReadResponseModel>(ResponseString);
            if (response != null)
            {
                if (response.Subscriptions[0].SubscriptionType != null)
                    Assert.AreEqual(CreateAssetSubscriptionModel.SubscriptionType, response.Subscriptions[0].SubscriptionType);
                if (response.Subscriptions[0].StartDate != null)
                    Assert.AreEqual(CreateAssetSubscriptionModel.StartDate.ToString("dd-MM-yyyyTHH:mm:ss"), DateTimeOffset.Parse(response.Subscriptions[0].StartDate).ToString("dd-MM-yyyyTHH:mm:ss"));
                if (response.Subscriptions[0].EndDate != null)
                    Assert.AreEqual(CreateAssetSubscriptionModel.EndDate.ToString("dd-MM-yyyyTHH:mm:ss"), DateTimeOffset.Parse(response.Subscriptions[0].EndDate).ToString("dd-MM-yyyyTHH:mm:ss"));
            }
        }

        public void VerifySubscriptionServiceReadResponseAfterUpdate()
        {
            SubscriptionServiceReadResponseModel response = JsonConvert.DeserializeObject<SubscriptionServiceReadResponseModel>(ResponseString);
            if (response != null)
            {
                if (response.Subscriptions[0].SubscriptionType != null)
                    Assert.AreEqual(UpdateAssetSubscriptionModel.SubscriptionType, response.Subscriptions[0].SubscriptionType);
                if (response.Subscriptions[0].StartDate != null)
                    Assert.AreEqual(Convert.ToDateTime(UpdateAssetSubscriptionModel.StartDate).ToString("dd-MM-yyyyTHH:mm:ss"), DateTimeOffset.Parse(response.Subscriptions[0].StartDate).ToString("dd-MM-yyyyTHH:mm:ss"));
                if (response.Subscriptions[0].EndDate != null)
                    Assert.AreEqual(Convert.ToDateTime(UpdateAssetSubscriptionModel.EndDate).ToString("dd-MM-yyyyTHH:mm:ss"), DateTimeOffset.Parse(response.Subscriptions[0].EndDate).ToString("dd-MM-yyyyTHH:mm:ss"));
            }
        }

        public void VerifySubscriptionServiceReadResponseAfterMultipleCreate(string subscriptionType1, string subscriptionType2)
        {
            SubscriptionServiceReadResponseModel response = JsonConvert.DeserializeObject<SubscriptionServiceReadResponseModel>(ResponseString);
            if (response != null)
            {
                if (response.Subscriptions[0].SubscriptionType != null)
                    Assert.AreEqual(subscriptionType1, response.Subscriptions[0].SubscriptionType);
                if (response.Subscriptions[0].StartDate != null)
                    Assert.AreEqual(CreateAssetSubscriptionModel.StartDate.ToString("dd-MM-yyyyTHH:mm:ss"), DateTimeOffset.Parse(response.Subscriptions[0].StartDate).ToString("dd-MM-yyyyTHH:mm:ss"));
                if (response.Subscriptions[0].EndDate != null)
                    Assert.AreEqual(CreateAssetSubscriptionModel.EndDate.ToString("dd-MM-yyyyTHH:mm:ss"), DateTimeOffset.Parse(response.Subscriptions[0].EndDate).ToString("dd-MM-yyyyTHH:mm:ss"));
                if (response.Subscriptions[1].SubscriptionType != null)
                    Assert.AreEqual(subscriptionType2, response.Subscriptions[1].SubscriptionType);
                if (response.Subscriptions[1].StartDate != null)
                    Assert.AreEqual(CreateAssetSubscriptionModel.StartDate.ToString("dd-MM-yyyyTHH:mm:ss"), DateTimeOffset.Parse(response.Subscriptions[1].StartDate).ToString("dd-MM-yyyyTHH:mm:ss"));
                if (response.Subscriptions[1].EndDate != null)
                    Assert.AreEqual(CreateAssetSubscriptionModel.EndDate.ToString("dd-MM-yyyyTHH:mm:ss"), DateTimeOffset.Parse(response.Subscriptions[1].EndDate).ToString("dd-MM-yyyyTHH:mm:ss"));
            }
        }

        public void VerifySubscriptionServiceReadResponseForMultipleAssetSubscriptions()
        {
            /*string[] SubscriptionTypes = { "Essentials", "Manual Maintenance Log", "CAT Health", "Standard Health", "CAT Utilization", "Standard Utilization", "CATMAINT", "VLMAINT", "Real Time Digital Switch Alerts", 
                                           "1 minute Update Rate Upgrade", "Connected Site Gateway", "Load & Cycle Monitoring", "3D Project Monitoring", "VisionLink RFID", "Vehicle Connect",
                                           "Unified Fleet", "Advanced Productivity"};*/

            string[] SubscriptionTypes = { "Essentials", "CAT Health" };

            SubscriptionServiceReadResponseModel response = JsonConvert.DeserializeObject<SubscriptionServiceReadResponseModel>(ResponseString);

            if (response != null)
            {
                Assert.AreEqual(2, response.Subscriptions.Count);

                foreach (var subscription in response.Subscriptions)
                {
                    Assert.IsTrue(SubscriptionTypes.Contains(subscription.SubscriptionType));
                }

                foreach (var subscription in response.Subscriptions)
                {
                    Assert.AreEqual(CreateAssetSubscriptionModel.StartDate.ToString("dd-MM-yyyyTHH:mm:ss"), DateTimeOffset.Parse(subscription.StartDate).ToString("dd-MM-yyyyTHH:mm:ss"));
                    Assert.AreEqual(CreateAssetSubscriptionModel.EndDate.ToString("dd-MM-yyyyTHH:mm:ss"), DateTimeOffset.Parse(subscription.EndDate).ToString("dd-MM-yyyyTHH:mm:ss"));
                }
            }

            else
            {
                Assert.Fail("Didn't get the expected response");
            }
        }

        public void VerifySubscriptionServiceReadResponse(DateTime minDate, DateTime maxDate)
        {
            SubscriptionServiceReadResponseModel response = JsonConvert.DeserializeObject<SubscriptionServiceReadResponseModel>(ResponseString);
            if (response != null)
            {
                if (response.Subscriptions[0].SubscriptionType != null)
                    Assert.AreEqual(CreateAssetSubscriptionModel.SubscriptionType, response.Subscriptions[0].SubscriptionType);
                if (response.Subscriptions[0].StartDate != null)
                    Assert.AreEqual(minDate.ToString("dd-MM-yyyyTHH:mm:ss"), DateTimeOffset.Parse(response.Subscriptions[0].StartDate).ToString("dd-MM-yyyyTHH:mm:ss"));
                if (response.Subscriptions[0].EndDate != null)
                    Assert.AreEqual(maxDate.ToString("dd-MM-yyyyTHH:mm:ss"), DateTimeOffset.Parse(response.Subscriptions[0].EndDate).ToString("dd-MM-yyyyTHH:mm:ss"));
            }
            else
            {
                Assert.Fail("Didn't get the expected response");
            }
        }

        public void VerifySubscriptionCount(int count)
        {
            SubscriptionServiceReadResponseModel response = JsonConvert.DeserializeObject<SubscriptionServiceReadResponseModel>(ResponseString);
            if (response != null)
            {
                Assert.AreEqual(count, response.Subscriptions.Count);
            }
            else
            {
                Assert.Fail("Didn't get the expected response");
            }
        }

        #endregion

        #region DB Methods

        public bool ValidateDB(string eventType)
        {
            try
            {
                //WaitForDB();
                bool dbResult = false;
                int expectedResult = 1;
                string query = "";
                if (eventType == "Create")
                {
                    query = string.Format(SubscriptionMySqlQueries.AssetSubscriptionDetails, CreateAssetSubscriptionModel.SubscriptionUID.ToString().Replace("-", ""), CreateAssetSubscriptionModel.AssetUID.ToString().Replace("-", ""),
                        CreateAssetSubscriptionModel.DeviceUID.ToString().Replace("-", ""), CreateAssetSubscriptionModel.StartDate.ToString("yyyy-MM-dd HH:mm:ss.ffffff"), CreateAssetSubscriptionModel.EndDate.ToString("yyyy-MM-dd HH:mm:ss.ffffff"),
                        CreateAssetSubscriptionModel.CustomerUID.ToString().Replace("-", ""), "1", GetSubscriptionSourceId(CreateAssetSubscriptionModel.Source));
                }
                else if(eventType == "Update")
                {
                    string customeruid = UpdateAssetSubscriptionModel.CustomerUID != null ? UpdateAssetSubscriptionModel.CustomerUID.ToString().Replace("-", "") : CreateAssetSubscriptionModel.CustomerUID.ToString().Replace("-", "");
                    string assetuid = UpdateAssetSubscriptionModel.AssetUID != null ? UpdateAssetSubscriptionModel.AssetUID.ToString().Replace("-", "") : CreateAssetSubscriptionModel.AssetUID.ToString().Replace("-", "");
                    string deviceuid = UpdateAssetSubscriptionModel.DeviceUID != null ? UpdateAssetSubscriptionModel.DeviceUID.ToString().Replace("-", "") : CreateAssetSubscriptionModel.DeviceUID.ToString().Replace("-", "");
                    string subscriptionType = UpdateAssetSubscriptionModel.SubscriptionType != null ? UpdateAssetSubscriptionModel.SubscriptionType : CreateAssetSubscriptionModel.SubscriptionType;
                    string source = UpdateAssetSubscriptionModel.Source != null ? UpdateAssetSubscriptionModel.Source : "Store";
                    string startDate = UpdateAssetSubscriptionModel.StartDate != null ? UpdateAssetSubscriptionModel.StartDate?.ToString("yyyy-MM-dd HH:mm:ss.ffffff") : CreateAssetSubscriptionModel.StartDate.ToString("yyyy-MM-dd HH:mm:ss.ffffff");
                    string endDate = UpdateAssetSubscriptionModel.EndDate != null ? UpdateAssetSubscriptionModel.EndDate?.ToString("yyyy-MM-dd HH:mm:ss.ffffff") : CreateAssetSubscriptionModel.EndDate.ToString("yyyy-MM-dd HH:mm:ss.ffffff");

                    query = string.Format(SubscriptionMySqlQueries.AssetSubscriptionDetails, UpdateAssetSubscriptionModel.SubscriptionUID.ToString().Replace("-", ""), assetuid,
                        deviceuid, startDate, endDate, customeruid, "4", GetSubscriptionSourceId(source));
                }

                LogResult.Report(Log, "log_ForInfo", "Query: " + query);
                List<string> queryResults = GetSQLResults(query);
                if (queryResults.Count != 0)
                {
                    if (queryResults[0] != "")
                    {
                        LogResult.Report(Log, "log_ForInfo", "Expected Value: " + expectedResult.ToString() + ", Actual Value: " + queryResults[0]);
                        dbResult = queryResults[0].Equals(expectedResult.ToString());
                    }
                    if (dbResult == false)
                    {
                        LogResult.Report(Log, "log_ForError", "DB Verification Failed");
                        return false;
                    }
                }
                else
                {
                    LogResult.Report(Log, "log_ForError", "No Rows Returned From DB");
                }
                return dbResult;
            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got error while executing db query", e);
                throw new InvalidDataException("Error Occurred while executing db query");
            }
        }

        public List<string> GetSQLResults(string queryString)
        {
            MySqlDataReader dataReader = null;
            List<string> dbResult = new List<string>();
            using (MySqlConnection mySqlConnection = new MySqlConnection(SubscriptionServiceConfig.MySqlConnection))
            {
                try
                {
                    //Open connection 
                    mySqlConnection.Open();
                    //Execute the SQL query
                    MySqlCommand mySqlCommand = new MySqlCommand(queryString, mySqlConnection);
                    //Read the results into a SqlDataReader and store in string variable for later reference
                    dataReader = mySqlCommand.ExecuteReader();
                    while (dataReader != null && dataReader.Read())
                    {
                        if (dataReader.HasRows)
                        {
                            for (int i = 0; i < dataReader.VisibleFieldCount; i++)
                            {
                                dbResult.Add(dataReader[i].ToString());
                            }
                        }
                        //dataReader.ToString();
                    }
                }
                catch (Exception e)
                {
                    LogResult.Report(Log, "log_ForError", "Got error while executing db query", e);
                    throw new InvalidDataException("Error Occurred while executing db query");
                }
            };
            return dbResult;
        }

        #endregion


        public string GetSubscriptionSourceId(string source)
        {
            int sourceId = 0;
            switch (source)
            {
                case "Store":
                    sourceId = 1;
                    break;

                case "SAV":
                    sourceId = 2;
                    break;

                default:
                    sourceId = 1;
                    break;
            }
            return sourceId.ToString();
        }

        #region ErrorResponse Verification
        public void VerifyAssetSubscriptionErrorResponse(string ErrorMessage)
        {
            try
            {
                AssetSubscriptionServiceErrorResponseModel error = JsonConvert.DeserializeObject<AssetSubscriptionServiceErrorResponseModel>(ResponseString);
                string resourceError = SubscriptionMessages.ResourceManager.GetString(ErrorMessage);
                if (error.Modelstate != null)
                {
                    if (error.Modelstate.SubscriptionUID != null)
                        Assert.IsTrue(error.Modelstate.SubscriptionUID[0].ToString().Contains(resourceError));
                    else if (error.Modelstate.CustomerUID != null)
                        Assert.IsTrue(error.Modelstate.CustomerUID[0].ToString().Contains(resourceError));
                    else if (error.Modelstate.AssetUID != null)
                        Assert.IsTrue(error.Modelstate.AssetUID[0].ToString().Contains(resourceError));
                    else if (error.Modelstate.SubscriptionType != null)
                        Assert.AreEqual(resourceError, error.Modelstate.SubscriptionType[0].ToString());
                    else if (error.Modelstate.StartDate != null)
                        Assert.IsTrue(error.Modelstate.StartDate[0].ToString().Contains(resourceError));
                    else if (error.Modelstate.EndDate != null)
                        Assert.IsTrue(error.Modelstate.EndDate[0].ToString().Contains(resourceError));
                    else if (error.Modelstate.ActionUTC != null)
                        Assert.IsTrue(error.Modelstate.ActionUTC[0].ToString().Contains(resourceError));
                    else if (error.Modelstate.DeviceUID != null)
                        Assert.IsTrue(error.Modelstate.DeviceUID[0].ToString().Contains(resourceError));
                    else
                        Assert.AreEqual(SubscriptionMessages.ResourceManager.GetString("ERR_Invalid"), error.Message);
                }
                else
                    Assert.IsTrue(error.Message.Contains(resourceError));
            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While DeSerializing JSON Object", e);
                throw new Exception("Got Error While DeSerializing JSON Object");
            }
        }

        public void VerifyCustomerSubscriptionErrorResponse(string ErrorMessage)
        {
            try
            {
                CustomerSubscriptionServiceErrorResponseModel error = JsonConvert.DeserializeObject<CustomerSubscriptionServiceErrorResponseModel>(ResponseString);
                string resourceError = SubscriptionMessages.ResourceManager.GetString(ErrorMessage);
                if (error.Modelstate != null)
                {
                    if (error.Modelstate.SubscriptionUID != null)
                        Assert.IsTrue(error.Modelstate.SubscriptionUID[0].ToString().Contains(resourceError));
                    else if (error.Modelstate.CustomerUID != null)
                        Assert.IsTrue(error.Modelstate.CustomerUID[0].ToString().Contains(resourceError));
                    else if (error.Modelstate.SubscriptionType != null)
                        Assert.AreEqual(resourceError, error.Modelstate.SubscriptionType[0].ToString());
                    else if (error.Modelstate.StartDate != null)
                        Assert.IsTrue(error.Modelstate.StartDate[0].ToString().Contains(resourceError));
                    else if (error.Modelstate.EndDate != null)
                        Assert.IsTrue(error.Modelstate.EndDate[0].ToString().Contains(resourceError));
                    else if (error.Modelstate.ActionUTC != null)
                        Assert.IsTrue(error.Modelstate.ActionUTC[0].ToString().Contains(resourceError));
                    else
                        Assert.AreEqual(SubscriptionMessages.ResourceManager.GetString("ERR_Invalid"), error.Message);
                }
                else
                    Assert.IsTrue(error.Message.Contains(resourceError));
            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While DeSerializing JSON Object", e);
                throw new Exception("Got Error While DeSerializing JSON Object");
            }
        }

        public void VerifyProjectSubscriptionErrorResponse(string ErrorMessage)
        {
            try
            {
                ProjectSubscriptionServiceErrorResponseModel error = JsonConvert.DeserializeObject<ProjectSubscriptionServiceErrorResponseModel>(ResponseString);
                string resourceError = SubscriptionMessages.ResourceManager.GetString(ErrorMessage);
                if (error.Modelstate != null)
                {
                    if (error.Modelstate.SubscriptionUID != null)
                        Assert.IsTrue(error.Modelstate.SubscriptionUID[0].ToString().Contains(resourceError));
                    else if (error.Modelstate.CustomerUID != null)
                        Assert.IsTrue(error.Modelstate.CustomerUID[0].ToString().Contains(resourceError));
                    else if (error.Modelstate.SubscriptionType != null)
                        Assert.AreEqual(resourceError, error.Modelstate.SubscriptionType[0].ToString());
                    else if (error.Modelstate.StartDate != null)
                        Assert.IsTrue(error.Modelstate.StartDate[0].ToString().Contains(resourceError));
                    else if (error.Modelstate.EndDate != null)
                        Assert.IsTrue(error.Modelstate.EndDate[0].ToString().Contains(resourceError));
                    else if (error.Modelstate.ActionUTC != null)
                        Assert.IsTrue(error.Modelstate.ActionUTC[0].ToString().Contains(resourceError));
                    else
                        Assert.AreEqual(SubscriptionMessages.ResourceManager.GetString("ERR_Invalid"), error.Message);
                }
                else
                    Assert.IsTrue(error.Message.Contains(resourceError));
            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While DeSerializing JSON Object", e);
                throw new Exception("Got Error While DeSerializing JSON Object");
            }
        }

        public void VerifyAssociateProjectSubscriptionErrorResponse(string ErrorMessage)
        {
            try
            {
                AssociateProjectSubscriptionServiceErrorResponseModel error = JsonConvert.DeserializeObject<AssociateProjectSubscriptionServiceErrorResponseModel>(ResponseString);
                string resourceError = SubscriptionMessages.ResourceManager.GetString(ErrorMessage);
                if (error.Modelstate != null)
                {
                    if (error.Modelstate.SubscriptionUID != null)
                        Assert.IsTrue(error.Modelstate.SubscriptionUID[0].ToString().Contains(resourceError));
                    else if (error.Modelstate.ProjectUID != null)
                        Assert.IsTrue(error.Modelstate.ProjectUID[0].ToString().Contains(resourceError));
                    else if (error.Modelstate.EffectiveDate != null)
                        Assert.AreEqual(resourceError, error.Modelstate.EffectiveDate[0].ToString());
                    else if (error.Modelstate.ActionUTC != null)
                        Assert.IsTrue(error.Modelstate.ActionUTC[0].ToString().Contains(resourceError));
                    else
                        Assert.AreEqual(SubscriptionMessages.ResourceManager.GetString("ERR_Invalid"), error.Message);
                }
                else
                    Assert.IsTrue(error.Message.Contains(resourceError));
            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While DeSerializing JSON Object", e);
                throw new Exception("Got Error While DeSerializing JSON Object");
            }
        }

        public void VerifyDissociateProjectSubscriptionErrorResponse(string ErrorMessage)
        {
            try
            {
                DissociateProjectSubscriptionServiceErrorResponseModel error = JsonConvert.DeserializeObject<DissociateProjectSubscriptionServiceErrorResponseModel>(ResponseString);
                string resourceError = SubscriptionMessages.ResourceManager.GetString(ErrorMessage);
                if (error.Modelstate != null)
                {
                    if (error.Modelstate.SubscriptionUID != null)
                        Assert.IsTrue(error.Modelstate.SubscriptionUID[0].ToString().Contains(resourceError));
                    else if (error.Modelstate.ProjectUID != null)
                        Assert.IsTrue(error.Modelstate.ProjectUID[0].ToString().Contains(resourceError));
                    else if (error.Modelstate.EffectiveDate != null)
                        Assert.AreEqual(resourceError, error.Modelstate.EffectiveDate[0].ToString());
                    else if (error.Modelstate.ActionUTC != null)
                        Assert.IsTrue(error.Modelstate.ActionUTC[0].ToString().Contains(resourceError));
                    else
                        Assert.AreEqual(SubscriptionMessages.ResourceManager.GetString("ERR_Invalid"), error.Message);
                }
                else
                    Assert.IsTrue(error.Message.Contains(resourceError));
            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While DeSerializing JSON Object", e);
                throw new Exception("Got Error While DeSerializing JSON Object");
            }
        }
        #endregion

        public void SetupCreateAssetSubscriptionKafkaConsumer(Guid SubscriptionUidToLookFor, DateTime actionUtc)
        {
            _checkForAssetSubscriptionCreateHandler = new CheckForAssetSubscriptionCreateHandler(SubscriptionUidToLookFor, actionUtc);
            SubscribeAndConsumeFromKafka(_checkForAssetSubscriptionCreateHandler);
        }

        public void SetupUpdateAssetSubscriptionKafkaConsumer(Guid SubscriptionUidToLookFor, DateTime actionUtc)
        {
            _checkForAssetSubscriptionUpdateHandler = new CheckForAssetSubscriptionUpdateHandler(SubscriptionUidToLookFor, actionUtc);
            SubscribeAndConsumeFromKafka(_checkForAssetSubscriptionUpdateHandler);
        }

        public void SetupCreateCustomerSubscriptionKafkaConsumer(Guid SubscriptionUidToLookFor, DateTime actionUtc)
        {
            _checkForCustomerSubscriptionCreateHandler = new CheckForCustomerSubscriptionCreateHandler(SubscriptionUidToLookFor, actionUtc);
            SubscribeAndConsumeFromKafka(_checkForCustomerSubscriptionCreateHandler);
        }

        public void SetupUpdateCustomerSubscriptionKafkaConsumer(Guid SubscriptionUidToLookFor, DateTime actionUtc)
        {
            _checkForCustomerSubscriptionUpdateHandler = new CheckForCustomerSubscriptionUpdateHandler(SubscriptionUidToLookFor, actionUtc);
            SubscribeAndConsumeFromKafka(_checkForCustomerSubscriptionUpdateHandler);
        }

        public void SetupCreateProjectSubscriptionKafkaConsumer(Guid SubscriptionUidToLookFor, DateTime actionUtc)
        {
            _checkForProjectSubscriptionCreateHandler = new CheckForProjectSubscriptionCreateHandler(SubscriptionUidToLookFor, actionUtc);
            SubscribeAndConsumeFromKafka(_checkForProjectSubscriptionCreateHandler);
        }

        public void SetupUpdateProjectSubscriptionKafkaConsumer(Guid SubscriptionUidToLookFor, DateTime actionUtc)
        {
            _checkForProjectSubscriptionUpdateHandler = new CheckForProjectSubscriptionUpdateHandler(SubscriptionUidToLookFor, actionUtc);
            SubscribeAndConsumeFromKafka(_checkForProjectSubscriptionUpdateHandler);
        }

        public void SetupAssociateProjectSubscriptionKafkaConsumer(Guid SubscriptionUidToLookFor, DateTime actionUtc)
        {
            _checkForAssociateProjectSubscriptionHandler = new CheckForAssociateProjectSubscriptionHandler(SubscriptionUidToLookFor, actionUtc);
            SubscribeAndConsumeFromKafka(_checkForAssociateProjectSubscriptionHandler);
        }

        public void SetupDissociateProjectSubscriptionKafkaConsumer(Guid SubscriptionUidToLookFor, DateTime actionUtc)
        {
            _checkForDissociateProjectSubscriptionHandler = new CheckForDissociateProjectSubscriptionHandler(SubscriptionUidToLookFor, actionUtc);
            SubscribeAndConsumeFromKafka(_checkForDissociateProjectSubscriptionHandler);
        }

        private void SubscribeAndConsumeFromKafka(CheckForSubscriptionHandler SubscriptionHandler)
        {
            var eventAggregator = new EventAggregator();
            eventAggregator.Subscribe(SubscriptionHandler);
            _consumerWrapper = new ConsumerWrapper(eventAggregator,
                new KafkaConsumerParams("SubscriptionServiceAcceptanceTest", SubscriptionServiceConfig.SubscriptionServiceKafkaUri,
                    SubscriptionServiceConfig.SubscriptionServiceTopic));
            //new Thread(()=>_consumerWrapper.Consume(fetchFromTail: true)){ Priority = ThreadPriority.Highest }.Start();      
            Task.Factory.StartNew(() => _consumerWrapper.ReadOffset(fetchFromTail: true));
            Thread.Sleep(new TimeSpan(0, 0, 10));
        }

        #region Kafka Handler

        public bool BatchRead
        {
            get
            {
                return false;
            }
        }

        public bool ReadAsync
        {
            get
            {
                return false;
            }
        }

        public void Handle(PayloadMessage message)
        {
            try
            {
                if (message.Value == null || message.Value == "null")
                {
                    LogResult.Report(Log, "log_ForInfo", "Kafka Message is Null");
                    return;
                }

                if (CreateAssetSubscriptionModel != null && CreateAssetSubscriptionModel.ActionUTC != null)
                {
                    if (CreateAssetSubscriptionModel.ActionUTC.ToString() != null && message.Value.Contains(CreateAssetSubscriptionModel.ActionUTC.ToString("yyyy-MM-ddThh:mm:ss"))
                        && CreateAssetSubscriptionModel.SubscriptionUID.ToString() != null && message.Value.Contains(CreateAssetSubscriptionModel.SubscriptionUID.ToString()))
                        assetSubscriptionCreateResponse = JsonConvert.DeserializeObject<CreateAssetSubscriptionModel>(message.Value);
                    LogResult.Report(Log, "log_ForInfo", string.Format("Response Received With Offset {0}: {1}", message.OffSet, message.Value));

                    if (UpdateAssetSubscriptionModel != null && UpdateAssetSubscriptionModel.ActionUTC != null && UpdateAssetSubscriptionModel.CustomerUID != Guid.Empty)
                    {
                        if (UpdateAssetSubscriptionModel.ActionUTC.ToString() != null && message.Value.Contains(UpdateAssetSubscriptionModel.ActionUTC.ToString("yyyy-MM-ddThh:mm:ss"))
                            && UpdateAssetSubscriptionModel.SubscriptionUID.ToString() != null && message.Value.Contains(UpdateAssetSubscriptionModel.SubscriptionUID.ToString()))
                            assetSubscriptionUpdateResponse = JsonConvert.DeserializeObject<UpdateAssetSubscriptionModel>(message.Value);
                        LogResult.Report(Log, "log_ForInfo", string.Format("Response Received With Offset {0}: {1}", message.OffSet, message.Value));
                    }

                }

                if (CreateCustomerSubscriptionModel != null && CreateCustomerSubscriptionModel.ActionUTC != null)
                {
                    if (CreateCustomerSubscriptionModel.ActionUTC.ToString() != null && message.Value.Contains(CreateCustomerSubscriptionModel.ActionUTC.ToString("yyyy-MM-ddThh:mm:ss")) && message.Value.Contains(CreateCustomerSubscriptionModel.ReceivedUTC.ToString())
                        && CreateCustomerSubscriptionModel.SubscriptionUID.ToString() != null && message.Value.Contains(CreateCustomerSubscriptionModel.SubscriptionUID.ToString()))
                        customerSubscriptionCreateResponse = JsonConvert.DeserializeObject<CreateCustomerSubscriptionModel>(message.Value);
                    LogResult.Report(Log, "log_ForInfo", string.Format("Response Received With Offset {0}: {1}", message.OffSet, message.Value));

                    if (UpdateCustomerSubscriptionModel != null && UpdateCustomerSubscriptionModel.ActionUTC != null)
                    {
                        if (UpdateCustomerSubscriptionModel.ActionUTC.ToString() != null && message.Value.Contains(UpdateCustomerSubscriptionModel.ActionUTC.ToString("yyyy-MM-ddThh:mm:ss")) && message.Value.Contains(UpdateCustomerSubscriptionModel.ReceivedUTC.ToString())
                            && UpdateCustomerSubscriptionModel.SubscriptionUID.ToString() != null && message.Value.Contains(UpdateCustomerSubscriptionModel.SubscriptionUID.ToString()))
                            customerSubscriptionUpdateResponse = JsonConvert.DeserializeObject<UpdateCustomerSubscriptionModel>(message.Value);
                        LogResult.Report(Log, "log_ForInfo", string.Format("Response Received With Offset {0}: {1}", message.OffSet, message.Value));
                    }

                }

                if (CreateProjectSubscriptionModel != null && CreateProjectSubscriptionModel.ActionUTC != null)
                {
                    if (CreateProjectSubscriptionModel.ActionUTC.ToString() != null && message.Value.Contains(CreateProjectSubscriptionModel.ActionUTC.ToString("yyyy-MM-ddThh:mm:ss")) && message.Value.Contains(CreateProjectSubscriptionModel.ReceivedUTC.ToString())
                        && CreateProjectSubscriptionModel.SubscriptionUID.ToString() != null && message.Value.Contains(CreateProjectSubscriptionModel.SubscriptionUID.ToString()))
                        projectSubscriptionCreateResponse = JsonConvert.DeserializeObject<CreateProjectSubscriptionModel>(message.Value);
                    LogResult.Report(Log, "log_ForInfo", string.Format("Response Received With Offset {0}: {1}", message.OffSet, message.Value));

                    if (UpdateProjectSubscriptionModel != null && UpdateProjectSubscriptionModel.ActionUTC != null)
                    {
                        if (UpdateProjectSubscriptionModel.ActionUTC.ToString() != null && message.Value.Contains(UpdateProjectSubscriptionModel.ActionUTC.ToString("yyyy-MM-ddThh:mm:ss")) && message.Value.Contains(UpdateProjectSubscriptionModel.ReceivedUTC.ToString())
                            && UpdateProjectSubscriptionModel.SubscriptionUID.ToString() != null && message.Value.Contains(UpdateProjectSubscriptionModel.SubscriptionUID.ToString()))
                            projectSubscriptionUpdateResponse = JsonConvert.DeserializeObject<UpdateProjectSubscriptionModel>(message.Value);
                        LogResult.Report(Log, "log_ForInfo", string.Format("Response Received With Offset {0}: {1}", message.OffSet, message.Value));
                    }

                }

                if (AssociateProjectSubscriptionModel != null && AssociateProjectSubscriptionModel.ActionUTC != null)
                {
                    if (AssociateProjectSubscriptionModel.ActionUTC.ToString() != null && message.Value.Contains(AssociateProjectSubscriptionModel.ActionUTC.ToString("yyyy-MM-ddThh:mm:ss")) && message.Value.Contains(AssociateProjectSubscriptionModel.ReceivedUTC.ToString())
                        && AssociateProjectSubscriptionModel.SubscriptionUID.ToString() != null && message.Value.Contains(AssociateProjectSubscriptionModel.SubscriptionUID.ToString()))
                        associateProjectSubscriptionResponse = JsonConvert.DeserializeObject<AssociateProjectSubscriptionModel>(message.Value);
                    LogResult.Report(Log, "log_ForInfo", string.Format("Response Received With Offset {0}: {1}", message.OffSet, message.Value));

                    if (DissociateProjectSubscriptionModel != null && DissociateProjectSubscriptionModel.ActionUTC != null)
                    {
                        if (DissociateProjectSubscriptionModel.ActionUTC.ToString() != null && message.Value.Contains(DissociateProjectSubscriptionModel.ActionUTC.ToString("yyyy-MM-ddThh:mm:ss")) && message.Value.Contains(DissociateProjectSubscriptionModel.ReceivedUTC.ToString())
                            && DissociateProjectSubscriptionModel.SubscriptionUID.ToString() != null && message.Value.Contains(DissociateProjectSubscriptionModel.SubscriptionUID.ToString()))
                            dissociateProjectSubscriptionResponse = JsonConvert.DeserializeObject<DissociateProjectSubscriptionModel>(message.Value);
                        LogResult.Report(Log, "log_ForInfo", string.Format("Response Received With Offset {0}: {1}", message.OffSet, message.Value));
                    }

                }

            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While Handling Response", e);
                throw new Exception(e + "Got Error While Handling Response");
            }
        }


        public void Handle(List<PayloadMessage> messages)
        {

        }

        #endregion

        #region Helpers

        private void WaitForKafkaResponseAfterAssetSubscriptionCreate(bool isPositiveCase = true)
        {
            int i = 0;
            if (!isPositiveCase)
                LogResult.Report(Log, "log_ForInfo", "Expecting No Response From Kafka");
            else
                LogResult.Report(Log, "log_ForInfo", "Waiting " + KafkaServicesConfig.InitialWaitingTimeForReceivingResponseInSeconds + " seconds For Kafka Response");
            for (i = 0; i < KafkaServicesConfig.InitialWaitingTimeForReceivingResponseInSeconds; i++)
            {
                if (CreateAssetSubscriptionModel.SubscriptionUID != Guid.Empty)
                {
                    if (assetSubscriptionCreateResponse != null)
                        break;
                }
                Thread.Sleep(1000);
            }
            if (i >= KafkaServicesConfig.InitialWaitingTimeForReceivingResponseInSeconds && isPositiveCase)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While Waiting For Kafka Response");
                throw new Exception("Got Error While Waiting For Kafka Response");
            }
        }

        private void WaitForKafkaResponseAfterAssetSubscriptionUpdate(bool isPositiveCase = true)
        {
            int i = 0;
            if (!isPositiveCase)
                LogResult.Report(Log, "log_ForInfo", "Expecting No Response From Kafka");
            else
                LogResult.Report(Log, "log_ForInfo", "Waiting " + KafkaServicesConfig.InitialWaitingTimeForReceivingResponseInSeconds + " seconds For Kafka Response");
            for (i = 0; i < KafkaServicesConfig.InitialWaitingTimeForReceivingResponseInSeconds; i++)
            {
                if (UpdateAssetSubscriptionModel.SubscriptionUID != Guid.Empty)
                {
                    if (assetSubscriptionUpdateResponse != null)
                        break;
                }
                Thread.Sleep(1000);
            }
            if (i >= KafkaServicesConfig.InitialWaitingTimeForReceivingResponseInSeconds && isPositiveCase)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While Waiting For Kafka Response");
                throw new Exception("Got Error While Waiting For Kafka Response");
            }
        }

        private void WaitForKafkaResponseAfterCustomerSubscriptionCreate(bool isPositiveCase = true)
        {
            int i = 0;
            if (!isPositiveCase)
                LogResult.Report(Log, "log_ForInfo", "Expecting No Response From Kafka");
            else
                LogResult.Report(Log, "log_ForInfo", "Waiting " + KafkaServicesConfig.InitialWaitingTimeForReceivingResponseInSeconds + " seconds For Kafka Response");
            for (i = 0; i < KafkaServicesConfig.InitialWaitingTimeForReceivingResponseInSeconds; i++)
            {
                if (CreateCustomerSubscriptionModel.SubscriptionUID != Guid.Empty)
                {
                    if (customerSubscriptionCreateResponse != null)
                        break;
                }
                Thread.Sleep(1000);
            }
            if (i >= KafkaServicesConfig.InitialWaitingTimeForReceivingResponseInSeconds && isPositiveCase)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While Waiting For Kafka Response");
                throw new Exception("Got Error While Waiting For Kafka Response");
            }
        }

        private void WaitForKafkaResponseAfterCustomerSubscriptionUpdate(bool isPositiveCase = true)
        {
            int i = 0;
            if (!isPositiveCase)
                LogResult.Report(Log, "log_ForInfo", "Expecting No Response From Kafka");
            else
                LogResult.Report(Log, "log_ForInfo", "Waiting " + KafkaServicesConfig.InitialWaitingTimeForReceivingResponseInSeconds + " seconds For Kafka Response");
            for (i = 0; i < KafkaServicesConfig.InitialWaitingTimeForReceivingResponseInSeconds; i++)
            {
                if (UpdateCustomerSubscriptionModel.SubscriptionUID != Guid.Empty)
                {
                    if (customerSubscriptionUpdateResponse != null)
                        break;
                }
                Thread.Sleep(1000);
            }
            if (i >= KafkaServicesConfig.InitialWaitingTimeForReceivingResponseInSeconds && isPositiveCase)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While Waiting For Kafka Response");
                throw new Exception("Got Error While Waiting For Kafka Response");
            }
        }

        private void WaitForKafkaResponseAfterProjectSubscriptionCreate(bool isPositiveCase = true)
        {
            int i = 0;
            if (!isPositiveCase)
                LogResult.Report(Log, "log_ForInfo", "Expecting No Response From Kafka");
            else
                LogResult.Report(Log, "log_ForInfo", "Waiting " + KafkaServicesConfig.InitialWaitingTimeForReceivingResponseInSeconds + " seconds For Kafka Response");
            for (i = 0; i < KafkaServicesConfig.InitialWaitingTimeForReceivingResponseInSeconds; i++)
            {
                if (CreateProjectSubscriptionModel.SubscriptionUID != Guid.Empty)
                {
                    if (projectSubscriptionCreateResponse != null)
                        break;
                }
                Thread.Sleep(1000);
            }
            if (i >= KafkaServicesConfig.InitialWaitingTimeForReceivingResponseInSeconds && isPositiveCase)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While Waiting For Kafka Response");
                throw new Exception("Got Error While Waiting For Kafka Response");
            }
        }

        private void WaitForKafkaResponseAfterProjectSubscriptionUpdate(bool isPositiveCase = true)
        {
            int i = 0;
            if (!isPositiveCase)
                LogResult.Report(Log, "log_ForInfo", "Expecting No Response From Kafka");
            else
                LogResult.Report(Log, "log_ForInfo", "Waiting " + KafkaServicesConfig.InitialWaitingTimeForReceivingResponseInSeconds + " seconds For Kafka Response");
            for (i = 0; i < KafkaServicesConfig.InitialWaitingTimeForReceivingResponseInSeconds; i++)
            {
                if (UpdateProjectSubscriptionModel.SubscriptionUID != Guid.Empty)
                {
                    if (projectSubscriptionUpdateResponse != null)
                        break;
                }
                Thread.Sleep(1000);
            }
            if (i >= KafkaServicesConfig.InitialWaitingTimeForReceivingResponseInSeconds && isPositiveCase)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While Waiting For Kafka Response");
                throw new Exception("Got Error While Waiting For Kafka Response");
            }
        }

        private void WaitForKafkaResponseAfterAssociateProjectSubscription(bool isPositiveCase = true)
        {
            int i = 0;
            if (!isPositiveCase)
                LogResult.Report(Log, "log_ForInfo", "Expecting No Response From Kafka");
            else
                LogResult.Report(Log, "log_ForInfo", "Waiting " + KafkaServicesConfig.InitialWaitingTimeForReceivingResponseInSeconds + " seconds For Kafka Response");
            for (i = 0; i < KafkaServicesConfig.InitialWaitingTimeForReceivingResponseInSeconds; i++)
            {
                if (AssociateProjectSubscriptionModel.SubscriptionUID != Guid.Empty)
                {
                    if (associateProjectSubscriptionResponse != null)
                        break;
                }
                Thread.Sleep(1000);
            }
            if (i >= KafkaServicesConfig.InitialWaitingTimeForReceivingResponseInSeconds && isPositiveCase)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While Waiting For Kafka Response");
                throw new Exception("Got Error While Waiting For Kafka Response");
            }
        }

        private void WaitForKafkaResponseAfterDissociateProjectSubscription(bool isPositiveCase = true)
        {
            int i = 0;
            if (!isPositiveCase)
                LogResult.Report(Log, "log_ForInfo", "Expecting No Response From Kafka");
            else
                LogResult.Report(Log, "log_ForInfo", "Waiting " + KafkaServicesConfig.InitialWaitingTimeForReceivingResponseInSeconds + " seconds For Kafka Response");
            for (i = 0; i < KafkaServicesConfig.InitialWaitingTimeForReceivingResponseInSeconds; i++)
            {
                if (DissociateProjectSubscriptionModel.SubscriptionUID != Guid.Empty)
                {
                    if (dissociateProjectSubscriptionResponse != null)
                        break;
                }
                Thread.Sleep(1000);
            }
            if (i >= KafkaServicesConfig.InitialWaitingTimeForReceivingResponseInSeconds && isPositiveCase)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While Waiting For Kafka Response");
                throw new Exception("Got Error While Waiting For Kafka Response");
            }
        }

        #endregion

    }

}

