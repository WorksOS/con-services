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
using VSS.VisionLink.MasterData.AcceptanceTests.Helpers;
using VSS.VisionLink.MasterData.AcceptanceTests.Resources;
using VSS.VisionLink.MasterData.AcceptanceTests.Utils.Config;
using VSS.VisionLink.MasterData.AcceptanceTests.Utils.Features.Classes;
using VSS.VisionLink.MasterData.AcceptanceTests.Utils.Features.Classes.GeofenceService;

namespace VSS.VisionLink.MasterData.AcceptanceTests.Scenarios.GeofenceService
{
  class GeofenceServiceSupport
  {
    # region Variables

    private static Log4Net Log = new Log4Net(typeof(GeofenceServiceSupport));
    public static string UserName = "";
    public static string PassWord = "";

    public string ResponseString = string.Empty;

    public CreateGeofenceEvent CreateGeofenceModel = new CreateGeofenceEvent();
    public UpdateGeofenceEvent UpdateGeofenceModel = new UpdateGeofenceEvent();
    public DeleteGeofenceEvent DeleteGeofenceModel = new DeleteGeofenceEvent();
    public InvalidCreateGeofenceEvent InvalidCreateGeofenceModel = new InvalidCreateGeofenceEvent();
    public InvalidUpdateGeofenceEvent InvalidUpdateGeofenceModel = new InvalidUpdateGeofenceEvent();
    public InvalidDeleteGeofenceEvent InvalidDeleteGeofenceModel = new InvalidDeleteGeofenceEvent();
    public ConsumerWrapper _consumerWrapper;
    public CheckForGeofenceCreateHandler _checkForGeoFenceCreateHandler;
    public CheckForGeofenceUpdateHandler _checkForGeoFenceUpdateHandler;
    public CheckForGeofenceDeletetHandler _checkForGeoFenceDeleteHandler;

    #endregion

    #region Constructors

    public GeofenceServiceSupport(Log4Net myLog)
    {
      MasterDataConfig.SetupEnvironment();
      Log = myLog;
    }

    #endregion

    #region Utility Methods

    public int RandomNumber()
    {
      Random random = new Random();
      int randomNumber = random.Next(0000000, 2147483647);
      return randomNumber;
    }


    #endregion

    #region Post Methods

    public void PostValidCreateRequestToService()
    {
      string requestString = JsonConvert.SerializeObject(CreateGeofenceModel);

      try
      {
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
        RestClientUtil.DoHttpRequest(MasterDataConfig.GeofenceServiceEndpoint, HeaderSettings.PostMethod, UserName, PassWord, HeaderSettings.JsonMediaType, requestString, HttpStatusCode.OK);

      }
      catch (Exception e)
      {
        LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Geofence Service", e);
        throw new Exception(e + " Got Error While Posting Data To Geofence Service");
      }
    }

    public void PostValidUpdateRequestToService()
    {
      string requestString = JsonConvert.SerializeObject(UpdateGeofenceModel);

      try
      {
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
        RestClientUtil.DoHttpRequest(MasterDataConfig.GeofenceServiceEndpoint, HeaderSettings.PutMethod, UserName, PassWord, HeaderSettings.JsonMediaType, requestString, HttpStatusCode.OK);

      }
      catch (Exception e)
      {
        LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Asset Service", e);
        throw new Exception(e + " Got Error While Posting Data To Asset Service");
      }
    }

    public void PostValidDeleteRequestToService(Guid geofenceUid, Guid userUid, DateTime actionUtc)
    {

      string actionUtcString = actionUtc.ToString("yyyy-MM-ddThh:mm:ss");
      string geofenceUID = geofenceUid.ToString();
      string userUID = userUid.ToString();
      try
      {
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + userUID + geofenceUID);
        RestClientUtil.DoHttpRequest(string.Format("{0}?Geofenceuid={1}&useruid={2}&ActionUTC={3}", MasterDataConfig.GeofenceServiceEndpoint, geofenceUID, userUID, actionUtcString),
          HeaderSettings.DeleteMethod, UserName, PassWord, HeaderSettings.JsonMediaType, null, HttpStatusCode.OK);
      }
      catch (Exception e)
      {
        LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Asset Service", e);
        throw new Exception(e + " Got Error While Posting Data To Asset Service");

      }

    }

    public void PostInValidCreateRequestToService(string contentType, HttpStatusCode actualResponse)
    {
      string requestString = JsonConvert.SerializeObject(InvalidCreateGeofenceModel);

      try
      {
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
        ResponseString = RestClientUtil.DoInvalidHttpRequest(MasterDataConfig.GeofenceServiceEndpoint, HeaderSettings.PostMethod, UserName, PassWord, contentType, requestString, actualResponse);

      }
      catch (Exception e)
      {
        LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Asset Service", e);
        throw new Exception(e + " Got Error While Posting Data To Asset Service");
      }
    }

    public void PostInValidUpdateRequestToService(string contentType, HttpStatusCode actualResponse)
    {
      string requestString = JsonConvert.SerializeObject(InvalidUpdateGeofenceModel);

      try
      {
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
        ResponseString = RestClientUtil.DoInvalidHttpRequest(MasterDataConfig.GeofenceServiceEndpoint, HeaderSettings.PutMethod, UserName, PassWord, contentType, requestString, actualResponse);

      }
      catch (Exception e)
      {
        LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Asset Service", e);
        throw new Exception(e + " Got Error While Posting Data To Asset Service");
      }
    }

    public void PostInValidDeleteRequestToService(string geofenceUid, string userUid, string actionUtc, string contentType, HttpStatusCode actualResponse)
    {
      try
      {
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + userUid + geofenceUid);
        RestClientUtil.DoHttpRequest(string.Format("{0}?Geofenceuid={1}&useruid={2}&ActionUTC={3}", MasterDataConfig.GeofenceServiceEndpoint, geofenceUid, userUid, actionUtc),
          HeaderSettings.DeleteMethod, UserName, PassWord, HeaderSettings.JsonMediaType, null, HttpStatusCode.OK);
      }
      catch (Exception e)
      {
        LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Asset Service", e);
        throw new Exception(e + " Got Error While Posting Data To Asset Service");

      }

    }
    #endregion

    #region ErrorResponse Verification
    public void VerifyErrorResponse(string ErrorMessage)
    {
      try
      {
        GeofenceServiceErrorResponseModel error = JsonConvert.DeserializeObject<GeofenceServiceErrorResponseModel>(ResponseString);
        string resourceError = MasterDataMessages.ResourceManager.GetString(ErrorMessage);
        if (error.ModelState != null)
        {
          if (error.ModelState.GeofenceName != null)
            Assert.AreEqual(resourceError, error.ModelState.GeofenceName[0].ToString());
          else if (error.ModelState.Description != null)
            Assert.AreEqual(resourceError, error.ModelState.Description[0].ToString());
          else if (error.ModelState.GeofenceType != null)

            Assert.AreEqual(resourceError, error.ModelState.GeofenceType[0].ToString());
          else if (error.ModelState.GeofenceType != null)
            Assert.AreEqual(resourceError, error.ModelState.GeofenceType[0].ToString());
          else if (error.ModelState.GeometryWKT != null)
            Assert.AreEqual(resourceError, error.ModelState.GeometryWKT[0].ToString());
          else if (error.ModelState.ActionUTC != null)
            Assert.AreEqual(resourceError, error.ModelState.ActionUTC[0].ToString());
          else if (error.ModelState.FillColor != null)
            Assert.AreEqual(resourceError, error.ModelState.FillColor[0].ToString());
          else if (error.ModelState.IsTransparent != null)
            Assert.AreEqual(resourceError, error.ModelState.IsTransparent[0].ToString());
          else if (error.ModelState.CustomerUID != null)
            Assert.AreEqual(resourceError, error.ModelState.CustomerUID[0].ToString());
          else if (error.ModelState.UserUID != null)
            Assert.AreEqual(resourceError, error.ModelState.UserUID[0].ToString());
          else if (error.ModelState.GeofenceUID != null)
            Assert.AreEqual(resourceError, error.ModelState.GeofenceUID[0].ToString());
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
    #region Response Verification

    public void VerifyGeofenceServiceCreateResponse(CreateGeofenceModel kafkaresponse)
    {
      if (kafkaresponse != null)
      {
        if (kafkaresponse.CreateGeofenceEvent.GeofenceName != null)
          Assert.AreEqual(CreateGeofenceModel.GeofenceName, kafkaresponse.CreateGeofenceEvent.GeofenceName);
        if (kafkaresponse.CreateGeofenceEvent.Description != null)
          Assert.AreEqual(CreateGeofenceModel.Description, kafkaresponse.CreateGeofenceEvent.Description);
        if (kafkaresponse.CreateGeofenceEvent.GeofenceType != null)
          Assert.AreEqual(CreateGeofenceModel.GeofenceType, kafkaresponse.CreateGeofenceEvent.GeofenceType);
        if (kafkaresponse.CreateGeofenceEvent.GeometryWKT != null)
          Assert.AreEqual(CreateGeofenceModel.GeometryWKT, kafkaresponse.CreateGeofenceEvent.GeometryWKT);
        if (kafkaresponse.CreateGeofenceEvent.FillColor != null)
          Assert.AreEqual(CreateGeofenceModel.FillColor, kafkaresponse.CreateGeofenceEvent.FillColor);
        if (kafkaresponse.CreateGeofenceEvent.ActionUTC != null)
          Assert.AreEqual(CreateGeofenceModel.ActionUTC, kafkaresponse.CreateGeofenceEvent.ActionUTC);
        if (kafkaresponse.CreateGeofenceEvent.IsTransparent != null)
          Assert.AreEqual(CreateGeofenceModel.IsTransparent, kafkaresponse.CreateGeofenceEvent.IsTransparent);
        if (kafkaresponse.CreateGeofenceEvent.CustomerUID != null)
          Assert.AreEqual(CreateGeofenceModel.CustomerUID, kafkaresponse.CreateGeofenceEvent.CustomerUID);
        if (kafkaresponse.CreateGeofenceEvent.UserUID != null)
          Assert.AreEqual(CreateGeofenceModel.UserUID, kafkaresponse.CreateGeofenceEvent.UserUID);
        if (kafkaresponse.CreateGeofenceEvent.GeofenceUID != null)
          Assert.AreEqual(CreateGeofenceModel.GeofenceUID, kafkaresponse.CreateGeofenceEvent.GeofenceUID);
        if (kafkaresponse.CreateGeofenceEvent.ActionUTC != null)
          Assert.AreEqual(CreateGeofenceModel.ActionUTC, kafkaresponse.CreateGeofenceEvent.ActionUTC);
      }
    }

    public void VerifyGeofenceServiceUpdateResponse(UpdateGeofenceModel kafkaresponse)
    {
      if (kafkaresponse != null)
      {
        if (kafkaresponse.UpdateGeofenceEvent.GeofenceName != null)
          Assert.AreEqual(UpdateGeofenceModel.GeofenceName, kafkaresponse.UpdateGeofenceEvent.GeofenceName);
        if (kafkaresponse.UpdateGeofenceEvent.Description != null)
          Assert.AreEqual(UpdateGeofenceModel.Description, kafkaresponse.UpdateGeofenceEvent.Description);
        if (kafkaresponse.UpdateGeofenceEvent.GeofenceType != null)
          Assert.AreEqual(UpdateGeofenceModel.GeofenceType, kafkaresponse.UpdateGeofenceEvent.GeofenceType);
        if (kafkaresponse.UpdateGeofenceEvent.GeometryWKT != null)
          Assert.AreEqual(UpdateGeofenceModel.GeometryWKT, kafkaresponse.UpdateGeofenceEvent.GeometryWKT);
        if (kafkaresponse.UpdateGeofenceEvent.FillColor != null)
          Assert.AreEqual(UpdateGeofenceModel.FillColor, kafkaresponse.UpdateGeofenceEvent.FillColor);
        if (kafkaresponse.UpdateGeofenceEvent.IsTransparent != null)
          Assert.AreEqual(UpdateGeofenceModel.IsTransparent, kafkaresponse.UpdateGeofenceEvent.IsTransparent);
        if (kafkaresponse.UpdateGeofenceEvent.UserUID != null)
          Assert.AreEqual(UpdateGeofenceModel.UserUID, kafkaresponse.UpdateGeofenceEvent.UserUID);
        if (kafkaresponse.UpdateGeofenceEvent.GeofenceUID != null)
          Assert.AreEqual(UpdateGeofenceModel.GeofenceUID, kafkaresponse.UpdateGeofenceEvent.GeofenceUID);
        if (kafkaresponse.UpdateGeofenceEvent.ActionUTC != null)
          Assert.AreEqual(UpdateGeofenceModel.ActionUTC, kafkaresponse.UpdateGeofenceEvent.ActionUTC);
      }
    }

    public void VerifyGeofenceServiceDeleteResponse(DeleteGeofenceModel kafkaresponse)
    {
      if (kafkaresponse != null)
      {
        if (kafkaresponse.DeleteGeofenceEvent.UserUID != null)
          Assert.AreEqual(DeleteGeofenceModel.UserUID, kafkaresponse.DeleteGeofenceEvent.UserUID);
        if (kafkaresponse.DeleteGeofenceEvent.GeofenceUID != null)
          Assert.AreEqual(DeleteGeofenceModel.GeofenceUID, kafkaresponse.DeleteGeofenceEvent.GeofenceUID);
        if (kafkaresponse.DeleteGeofenceEvent.ActionUTC != null)
          Assert.AreEqual(DeleteGeofenceModel.ActionUTC, kafkaresponse.DeleteGeofenceEvent.ActionUTC);
      }
    }
    #endregion

    public void SetupCreateGeofenceKafkaConsumer(Guid userUidToLookFor, Guid geofenceUidToLookFor, DateTime actionUtc)
    {
      _checkForGeoFenceCreateHandler = new CheckForGeofenceCreateHandler(userUidToLookFor, geofenceUidToLookFor, actionUtc);
      SubscribeAndConsumeFromKafka(_checkForGeoFenceCreateHandler);
    }

    public void SetupUpdateGeofenceKafkaConsumer(Guid userUidToLookFor, Guid geofenceUidToLookFor, DateTime actionUtc)
    {
      _checkForGeoFenceUpdateHandler = new CheckForGeofenceUpdateHandler(userUidToLookFor, geofenceUidToLookFor, actionUtc);
      SubscribeAndConsumeFromKafka(_checkForGeoFenceUpdateHandler);
    }

    public void SetupDeleteGeofenceKafkaConsumer(Guid userUidToLookFor, Guid geofenceUidToLookFor, DateTime actionUtc)
    {
      _checkForGeoFenceDeleteHandler = new CheckForGeofenceDeletetHandler(userUidToLookFor, geofenceUidToLookFor, actionUtc);
      SubscribeAndConsumeFromKafka(_checkForGeoFenceDeleteHandler);
    }

    private void SubscribeAndConsumeFromKafka(CheckForGeofenceHandler geoFenceHandler)
    {
      var eventAggregator = new EventAggregator();
      eventAggregator.Subscribe(geoFenceHandler);
      _consumerWrapper = new ConsumerWrapper(eventAggregator,
          new KafkaConsumerParams("GeofenceServiceAcceptanceTest", MasterDataConfig.GeofenceServiceKafkaUri,
              MasterDataConfig.GeofenceServiceTopic));
      //new Thread(()=>_consumerWrapper.Consume(fetchFromTail: true)){ Priority = ThreadPriority.Highest }.Start();      
      Task.Factory.StartNew(() => _consumerWrapper.ReadOffset(fetchFromTail: true));
      Thread.Sleep(new TimeSpan(0, 0, 10));
    }

  }
}
