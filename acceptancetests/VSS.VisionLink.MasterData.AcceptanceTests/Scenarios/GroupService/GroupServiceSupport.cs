using AutomationCore.Shared.Library;
using VSS.VisionLink.MasterData.AcceptanceTests.Utils.Features.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.MasterData.AcceptanceTests.Utils.Config;
using VSS.VisionLink.MasterData.AcceptanceTests.Utils.Features.Classes.GroupService;
using VSS.KafkaWrapper;
using VSS.VisionLink.MasterData.AcceptanceTests.Helpers;
using VSS.KafkaWrapper.Models;
using AutomationCore.API.Framework.Library;
using Newtonsoft.Json;
using AutomationCore.API.Framework.Common;
using System.Net;
using VSS.VisionLink.MasterData.AcceptanceTests.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace VSS.VisionLink.MasterData.AcceptanceTests.Scenarios.GroupService
{
    class GroupServiceSupport
    {
        # region Variables

        private static Log4Net Log = new Log4Net(typeof(GroupServiceSupport));
        public ConsumerWrapper _consumerWrapper;
        public CheckForGroupCreateHandler _checkForGroupCreateHandler;
        public CheckForGroupUpdateHandler _checkForGroupUpdateHandler;
        public CheckForGroupDeleteHandler _checkForGroupDeleteHandler;
        public static string UserName = "";
        public static string PassWord = "";
        public string ResponseString = string.Empty;
        public CreateGroupServiceEvent CreateGroupModel = new CreateGroupServiceEvent();

        public UpdateGroupServiceEvent UpdateGroupModel = new UpdateGroupServiceEvent();

        public DeleteGroupServiceEvent DeleteGroupModel = new DeleteGroupServiceEvent();
        public InValidCreateGroupServiceEvent InValidCreateGroupModel = new InValidCreateGroupServiceEvent();
        public InValidUpdateGroupServiceEvent InValidUpdateGroupModel = new InValidUpdateGroupServiceEvent();
        public InValidDeleteGroupServiceEvent InValidDeleteGroupModel = new InValidDeleteGroupServiceEvent();
        #endregion

        public GroupServiceSupport(Log4Net myLog)
        {
            MasterDataConfig.SetupEnvironment();
            Log = myLog;
        }
        public void SetupCreateGroupKafkaConsumer(Guid groupUidToLookFor, Guid userUidToLookFor, DateTime actionUtc)
        {
            _checkForGroupCreateHandler = new CheckForGroupCreateHandler(groupUidToLookFor, userUidToLookFor, actionUtc);
            SubscribeAndConsumeFromKafka(_checkForGroupCreateHandler);
        }

        public void SetupUpdateGroupKafkaConsumer(Guid groupUidToLookFor, Guid userUidToLookFor, DateTime actionUtc)
        {
            _checkForGroupUpdateHandler = new CheckForGroupUpdateHandler(groupUidToLookFor, userUidToLookFor, actionUtc);
            SubscribeAndConsumeFromKafka(_checkForGroupUpdateHandler);
        }

        public void SetupDeleteGroupKafkaConsumer(Guid groupUidToLookFor, Guid userUidToLookFor, DateTime actionUtc)
        {
            _checkForGroupDeleteHandler = new CheckForGroupDeleteHandler(groupUidToLookFor, userUidToLookFor, actionUtc);
            SubscribeAndConsumeFromKafka(_checkForGroupDeleteHandler);
        }

        private void SubscribeAndConsumeFromKafka(CheckForGroupHandler groupHandler)
        {
            var eventAggregator = new EventAggregator();
            eventAggregator.Subscribe(groupHandler);
            _consumerWrapper = new ConsumerWrapper(eventAggregator,
                new KafkaConsumerParams("GroupServiceAcceptanceTest", MasterDataConfig.GroupServiceKafkaUri,
                    MasterDataConfig.GroupServiceTopic));
            //new Thread(()=>_consumerWrapper.Consume(fetchFromTail: true)){ Priority = ThreadPriority.Highest }.Start();      
            Task.Factory.StartNew(() => _consumerWrapper.ReadOffset(fetchFromTail: true));
            Thread.Sleep(new TimeSpan(0, 0, 10));
        }

        #region Post Methods

        public void PostValidCreateRequestToService()
        {
            string requestString = JsonConvert.SerializeObject(CreateGroupModel);

            try
            {
                LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
                RestClientUtil.DoHttpRequest(MasterDataConfig.GroupServiceEndpoint, HeaderSettings.PostMethod, UserName, PassWord, HeaderSettings.JsonMediaType, requestString, HttpStatusCode.OK);

            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Group Service", e);
                throw new Exception(e + " Got Error While Posting Data To Group Service");
            }

        }

        public void PostValidUpdateRequestToService()
        {
            string requestString = JsonConvert.SerializeObject(UpdateGroupModel);

            try
            {
                LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
                RestClientUtil.DoHttpRequest(MasterDataConfig.GroupServiceEndpoint, HeaderSettings.PutMethod, UserName, PassWord, HeaderSettings.JsonMediaType, requestString, HttpStatusCode.OK);

            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Group Service", e);
                throw new Exception(e + " Got Error While Posting Data To Group Service");
            }

        }

        public void PostValidDeleteRequestToService(Guid groupUid, Guid userUid, DateTime actionUtc)
        {
            string actionUtcString = actionUtc.ToString("yyyy-MM-ddThh:mm:ss");
            string groupUID = groupUid.ToString();
            string userUID = userUid.ToString();

             //RestClientUtil.DoHttpRequest(string.Format("{0}/{1}?ActionUTC={2}", MasterDataConfig.AssetServiceEndpoint, assetUID, actionUtcString),
            try
            {
                LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + groupUID + userUID);
                RestClientUtil.DoHttpRequest(MasterDataConfig.GroupServiceEndpoint+"/groupUID/userUID?groupUID="+groupUID+"&userUID="+userUID+"&actionUTC="+actionUtcString, 
                    HeaderSettings.DeleteMethod, 
                    UserName, PassWord, HeaderSettings.JsonMediaType, null, HttpStatusCode.OK);

            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Group Service", e);
                throw new Exception(e + " Got Error While Posting Data To Group Service");
            }

        }
        #endregion

        public void PostInValidCreateRequestToService(string contentType, HttpStatusCode actualResponse)
        {
            string requestString = JsonConvert.SerializeObject(InValidCreateGroupModel);

            try
            {
                LogResult.Report(Log, "log_ForInfo", "Posting the request with InValid Values: " + requestString);
                ResponseString=RestClientUtil.DoInvalidHttpRequest(MasterDataConfig.GroupServiceEndpoint, HeaderSettings.PostMethod, UserName, PassWord, contentType, requestString, actualResponse);

            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Group Service", e);
                throw new Exception(e + " Got Error While Posting Data To Group Service");
            }

        }

        public void PostInValidUpdateRequestToService(string contentType, HttpStatusCode actualResponse)
        {
            string requestString = JsonConvert.SerializeObject(InValidUpdateGroupModel);

            try
            {
                LogResult.Report(Log, "log_ForInfo", "Posting the request with InValid Values: " + requestString);
                ResponseString=RestClientUtil.DoInvalidHttpRequest(MasterDataConfig.GroupServiceEndpoint, HeaderSettings.PutMethod, UserName, PassWord, contentType, requestString, actualResponse);

            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Group Service", e);
                throw new Exception(e + " Got Error While Posting Data To Group Service");
            }

        }


        public void PostInValidDeleteRequestToService(string groupUid, string userUid, string actionUtc, string contentType, HttpStatusCode actualResponse)
        {
            //string actionUtcString = actionUtc.ToString("yyyy-MM-ddThh:mm:ss");
            //string groupUID = groupUid.ToString();
            //string userUID = userUid.ToString();
            try
            {
                LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + groupUid + userUid);
                ResponseString = RestClientUtil.DoInvalidHttpRequest(string.Format("{0}/{1}?ActionUTC={2}", MasterDataConfig.GroupServiceEndpoint, groupUid, userUid, actionUtc),
                  HeaderSettings.DeleteMethod, UserName, PassWord, contentType, null, actualResponse);
            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Group Service", e);
                throw new Exception(e + " Got Error While Posting Data To Asset Service");
            }

        }

        #region Response Verification

        public void VerifyGroupServiceCreateResponse(CreateGroupServiceModel kafkaresponse)
        {
          if (kafkaresponse != null)
          {
            if (kafkaresponse.CreateGroupEvent.GroupName != null)
              Assert.AreEqual(CreateGroupModel.GroupName, kafkaresponse.CreateGroupEvent.GroupName);
            if (kafkaresponse.CreateGroupEvent.CustomerUID != null)
              Assert.AreEqual(CreateGroupModel.CustomerUID, kafkaresponse.CreateGroupEvent.CustomerUID);
            if (kafkaresponse.CreateGroupEvent.UserUID != null)
              Assert.AreEqual(CreateGroupModel.UserUID, kafkaresponse.CreateGroupEvent.UserUID);
            if (kafkaresponse.CreateGroupEvent.AssetUID != null)
              Assert.AreEqual(CreateGroupModel.AssetUID, kafkaresponse.CreateGroupEvent.AssetUID);
            if (kafkaresponse.CreateGroupEvent.GroupUID != null)
              Assert.AreEqual(CreateGroupModel.GroupUID, kafkaresponse.CreateGroupEvent.GroupUID);
            if (kafkaresponse.CreateGroupEvent.ActionUTC != null)
              Assert.AreEqual(CreateGroupModel.ActionUTC, kafkaresponse.CreateGroupEvent.ActionUTC);
          }
        }

        public void VerifyGroupServiceUpdateResponse(UpdateGroupServiceModel kafkaresponse)
        {
          if (kafkaresponse != null)
          {
            if (kafkaresponse.UpdateGroupEvent.GroupName != null)
              Assert.AreEqual(UpdateGroupModel.GroupName, kafkaresponse.UpdateGroupEvent.GroupName);
            if (kafkaresponse.UpdateGroupEvent.UserUID != null)
              Assert.AreEqual(UpdateGroupModel.UserUID, kafkaresponse.UpdateGroupEvent.UserUID);
            if (kafkaresponse.UpdateGroupEvent.AssociatedAssetUID != null)
              Assert.AreEqual(UpdateGroupModel.AssociatedAssetUID, kafkaresponse.UpdateGroupEvent.AssociatedAssetUID);
            if (kafkaresponse.UpdateGroupEvent.DissociatedAssetUID != null)
              Assert.AreEqual(UpdateGroupModel.DissociatedAssetUID, kafkaresponse.UpdateGroupEvent.DissociatedAssetUID);
            if (kafkaresponse.UpdateGroupEvent.GroupUID != null)
              Assert.AreEqual(UpdateGroupModel.GroupUID, kafkaresponse.UpdateGroupEvent.GroupUID);
            if (kafkaresponse.UpdateGroupEvent.ActionUTC != null)
              Assert.AreEqual(UpdateGroupModel.ActionUTC, kafkaresponse.UpdateGroupEvent.ActionUTC);
          }
        }

        public void VerifyGroupServiceDeleteResponse(DeleteGroupServiceModel kafkaresponse)
        {
          if (kafkaresponse != null)
          {
            if (kafkaresponse.DeleteGroupEvent.UserUID != null)
              Assert.AreEqual(DeleteGroupModel.UserUID, kafkaresponse.DeleteGroupEvent.UserUID);
            if (kafkaresponse.DeleteGroupEvent.GroupUID != null)
              Assert.AreEqual(DeleteGroupModel.GroupUID, kafkaresponse.DeleteGroupEvent.GroupUID);
            if (kafkaresponse.DeleteGroupEvent.ActionUTC != null)
              Assert.AreEqual(DeleteGroupModel.ActionUTC, kafkaresponse.DeleteGroupEvent.ActionUTC);
          }
        }
        #endregion

        #region ErrorResponse Verification
        public void VerifyErrorResponse(string ErrorMessage)
        {
            try
            {
                ErrorResponseModel error = JsonConvert.DeserializeObject<ErrorResponseModel>(ResponseString);
                string resourceError = MasterDataMessages.ResourceManager.GetString(ErrorMessage);
                if (error.ModelState != null)
                {
                    if (error.ModelState.GroupName != null)
                       Assert.AreEqual(resourceError, error.ModelState.GroupName[0].ToString());                    
                    else if (error.ModelState.GroupUID != null)
                        Assert.AreEqual(resourceError, error.ModelState.GroupUID[0].ToString());
                    else if (error.ModelState.UserUID != null)
                        Assert.AreEqual(resourceError, error.ModelState.UserUID[0].ToString());
                    else if (error.ModelState.CustomerUID != null)
                        Assert.AreEqual(resourceError, error.ModelState.CustomerUID[0].ToString());
                    else if (error.ModelState.AssetUID != null)
                        Assert.AreEqual(resourceError, error.ModelState.AssetUID[0].ToString());
                    else if (error.ModelState.ActionUTC != null)
                        Assert.AreEqual(resourceError, error.ModelState.ActionUTC[0].ToString());
                    Assert.AreEqual(MasterDataMessages.ResourceManager.GetString("ERR_Invalid"), error.Message);
                }
                else
                    StringAssert.Contains(ResponseString, resourceError);

            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While DeSerializing JSON Object", e);
                throw new Exception("Got Error While DeSerializing JSON Object");
            }
        }
        #endregion
    }
}
