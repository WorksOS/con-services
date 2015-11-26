using System;
using System.IO;
using System.Net;
using VSS.VisionLink.MasterData.AcceptanceTests.Utils.Config;
using VSS.VisionLink.MasterData.AcceptanceTests.Utils.Features.Classes;
using VSS.VisionLink.MasterData.AcceptanceTests.Utils.Features.Classes.AssetService;
using AutomationCore.API.Framework.Common;
using AutomationCore.API.Framework.Library;
using AutomationCore.Shared.Library;
using Newtonsoft.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.MasterData.AcceptanceTests.Resources;
using VSS.VisionLink.MasterData.AcceptanceTests.Helpers;
using VSS.KafkaWrapper;
using System.Threading.Tasks;
using System.Threading;
using VSS.KafkaWrapper.Models;

namespace VSS.VisionLink.MasterData.AcceptanceTests.Scenarios.AssetService
{
  public class AssetServiceSupport
  {

    # region Variables

    private static Log4Net Log = new Log4Net(typeof(AssetServiceSupport));

    public CreateAssetEvent CreateAssetModel = new CreateAssetEvent();
    public UpdateAssetEvent UpdateAssetModel = new UpdateAssetEvent();
    public DeleteAssetEvent DeleteAssetModel = new DeleteAssetEvent();
    public InValidCreateAssetEvent InValidCreateAssetModel = new InValidCreateAssetEvent();
    public InValidUpdateAssetEvent InValidUpdateAssetModel = new InValidUpdateAssetEvent();
    public InValidDeleteAssetEvent InValidDeleteAssetModel = new InValidDeleteAssetEvent();

    public static string AssetServiceURI = "";
    public static string UserName = "";
    public static string PassWord = "";

    public string ResponseString = string.Empty;
    public string ErrorString = string.Empty;


    public ConsumerWrapper _consumerWrapper;
    public CheckForAssetCreateHandler _checkForAssetCreateHandler;
    public CheckForAssetUpdateHandler _checkForAssetUpdateHandler;
    public CheckForAssetDeleteHandler _checkForAssetDeleteHandler;

    #endregion

    #region Constructors

    public AssetServiceSupport(Log4Net myLog)
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
      string requestString = JsonConvert.SerializeObject(CreateAssetModel);

      try
      {
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
        RestClientUtil.DoHttpRequest( MasterDataConfig.AssetServiceEndpoint, HeaderSettings.PostMethod, UserName, PassWord, HeaderSettings.JsonMediaType, requestString, HttpStatusCode.OK);

      }
      catch (Exception e)
      {
        LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Asset Service", e);
        throw new Exception(e + " Got Error While Posting Data To Asset Service");
      }

    }

    public void PostInValidCreateRequestToService(string contentType, HttpStatusCode actualResponse)
    {
      string requestString = JsonConvert.SerializeObject(InValidCreateAssetModel);

      try
      {
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
        ResponseString = RestClientUtil.DoInvalidHttpRequest(MasterDataConfig.AssetServiceEndpoint, HeaderSettings.PostMethod, UserName, PassWord, contentType, requestString, actualResponse);
      }
      catch (Exception e)
      {
        LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Asset Service", e);
        throw new Exception(e + " Got Error While Posting Data To Asset Service");
      }

    }


    public void PostValidUpdateRequestToService()
    {
      string requestString = JsonConvert.SerializeObject(UpdateAssetModel);

      try
      {
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
        RestClientUtil.DoHttpRequest(MasterDataConfig.AssetServiceEndpoint, HeaderSettings.PutMethod, UserName, PassWord, HeaderSettings.JsonMediaType, requestString, HttpStatusCode.OK);

      }
      catch (Exception e)
      {
        LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Asset Service", e);
        throw new Exception(e + " Got Error While Posting Data To Asset Service");
      }

    }

    public void PostInValidUpdateRequestToService(string contentType, HttpStatusCode actualResponse)
    {
      string requestString = JsonConvert.SerializeObject(InValidUpdateAssetModel);

      try
      {
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
        ResponseString = RestClientUtil.DoInvalidHttpRequest(MasterDataConfig.AssetServiceEndpoint, HeaderSettings.PutMethod, UserName, PassWord, contentType, requestString, actualResponse);
      }
      catch (Exception e)
      {
        LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Asset Service", e);
        throw new Exception(e + " Got Error While Posting Data To Asset Service");
      }

    }


    public void PostValidDeleteRequestToService(Guid assetUid, DateTime actionUtc)
    {

      string actionUtcString = actionUtc.ToString("yyyy-MM-ddThh:mm:ss");
      string assetUID = assetUid.ToString();
      try
      {
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + assetUid);
        RestClientUtil.DoHttpRequest(string.Format("{0}?Assetuid={1}&ActionUTC={2}", MasterDataConfig.AssetServiceEndpoint, assetUID, actionUtcString),
          HeaderSettings.DeleteMethod, UserName, PassWord, HeaderSettings.JsonMediaType, null, HttpStatusCode.OK);
      }
      catch (Exception e)
      {
        LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Asset Service", e);
        throw new Exception(e + " Got Error While Posting Data To Asset Service");

      }

    }

    public void PostInValidDeleteRequestToService(string assetUid, string actionUtc, string contentType, HttpStatusCode actualResponse)
    {
      try
      {
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + assetUid);
        ResponseString = RestClientUtil.DoInvalidHttpRequest(string.Format("{0}?Assetuid={1}&ActionUTC={2}", MasterDataConfig.AssetServiceEndpoint, assetUid, actionUtc),
          HeaderSettings.DeleteMethod, UserName, PassWord, contentType, null, actualResponse);
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
        ErrorResponseModel error = JsonConvert.DeserializeObject<ErrorResponseModel>(ResponseString);
        string resourceError = MasterDataMessages.ResourceManager.GetString(ErrorMessage);
        if (error.ModelState != null)
        {
          if (error.ModelState.SerialNumber != null)
            StringAssert.Equals(resourceError, error.ModelState.SerialNumber[0].ToString());
          else if (error.ModelState.MakeCode != null)
            StringAssert.Equals(resourceError, error.ModelState.MakeCode[0].ToString());
          else if (error.ModelState.ModelYear != null)
            StringAssert.Equals(resourceError, error.ModelState.ModelYear[0].ToString());
          else if (error.ModelState.IconKey != null)
            StringAssert.Equals(resourceError, error.ModelState.IconKey[0].ToString());
          else if (error.ModelState.ActionUTC != null)
            StringAssert.Equals(resourceError, error.ModelState.ActionUTC[0].ToString());
          else
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

    public void VerifyAssetServiceCreateResponse(CreateAssetModel kafkaresponse)
    {
    if (kafkaresponse != null)
      {
        if (kafkaresponse.CreateAssetEvent.AssetName != null)
          Assert.AreEqual(CreateAssetModel.AssetName, kafkaresponse.CreateAssetEvent.AssetName);
        if (kafkaresponse.CreateAssetEvent.SerialNumber != null)
          Assert.AreEqual(CreateAssetModel.SerialNumber, kafkaresponse.CreateAssetEvent.SerialNumber);
        if (kafkaresponse.CreateAssetEvent.MakeCode != null)
          Assert.AreEqual(CreateAssetModel.MakeCode, kafkaresponse.CreateAssetEvent.MakeCode);
        if (kafkaresponse.CreateAssetEvent.ModelYear != null)
          Assert.AreEqual(CreateAssetModel.ModelYear, kafkaresponse.CreateAssetEvent.ModelYear);
        if (kafkaresponse.CreateAssetEvent.Model != null)
          Assert.AreEqual(CreateAssetModel.Model, kafkaresponse.CreateAssetEvent.Model);
        if (kafkaresponse.CreateAssetEvent.ActionUTC != null)
          Assert.AreEqual(CreateAssetModel.ActionUTC, kafkaresponse.CreateAssetEvent.ActionUTC);
        if (kafkaresponse.CreateAssetEvent.AssetUID != null)
          Assert.AreEqual(CreateAssetModel.AssetUID, kafkaresponse.CreateAssetEvent.AssetUID);
        if (kafkaresponse.CreateAssetEvent.AssetType != null)
          Assert.AreEqual(CreateAssetModel.AssetType, kafkaresponse.CreateAssetEvent.AssetType);
        if (kafkaresponse.CreateAssetEvent.EquipmentVIN != null)
          Assert.AreEqual(CreateAssetModel.EquipmentVIN, kafkaresponse.CreateAssetEvent.EquipmentVIN);
        if (kafkaresponse.CreateAssetEvent.IconKey != null)
          Assert.AreEqual(CreateAssetModel.IconKey, kafkaresponse.CreateAssetEvent.IconKey);
      }
    }

    public void VerifyAssetServiceUpdateResponse(UpdateAssetModel kafkaresponse)
    {
      if (kafkaresponse != null)
      {
        if (kafkaresponse.UpdateAssetEvent.AssetName != null)
          Assert.AreEqual(UpdateAssetModel.AssetName, kafkaresponse.UpdateAssetEvent.AssetName);
        if (kafkaresponse.UpdateAssetEvent.ModelYear != null)
          Assert.AreEqual(UpdateAssetModel.ModelYear, kafkaresponse.UpdateAssetEvent.ModelYear);
        if (kafkaresponse.UpdateAssetEvent.Model != null)
          Assert.AreEqual(UpdateAssetModel.Model, kafkaresponse.UpdateAssetEvent.Model);
        if (kafkaresponse.UpdateAssetEvent.ActionUTC != null)
          Assert.AreEqual(UpdateAssetModel.ActionUTC, kafkaresponse.UpdateAssetEvent.ActionUTC);
        if (kafkaresponse.UpdateAssetEvent.AssetUID != null)
          Assert.AreEqual(UpdateAssetModel.AssetUID, kafkaresponse.UpdateAssetEvent.AssetUID);
        if (kafkaresponse.UpdateAssetEvent.AssetType != null)
          Assert.AreEqual(UpdateAssetModel.AssetType, kafkaresponse.UpdateAssetEvent.AssetType);
        if (kafkaresponse.UpdateAssetEvent.EquipmentVIN != null)
          Assert.AreEqual(UpdateAssetModel.EquipmentVIN, kafkaresponse.UpdateAssetEvent.EquipmentVIN);
        if (kafkaresponse.UpdateAssetEvent.IconKey != null)
          Assert.AreEqual(UpdateAssetModel.IconKey, kafkaresponse.UpdateAssetEvent.IconKey);
      }
    }

    public void VerifyAssetServiceDeleteResponse(DeleteAssetModel kafkaresponse)
    {
      if (kafkaresponse != null)
      {
        if (kafkaresponse.DeleteAssetEvent.ActionUTC != null)
          Assert.AreEqual(DeleteAssetModel.ActionUTC.ToString("yyyyMMddhhmmss"), kafkaresponse.DeleteAssetEvent.ActionUTC.ToString("yyyyMMddhhmmss"));
        if (kafkaresponse.DeleteAssetEvent.AssetUID != null)
          Assert.AreEqual(DeleteAssetModel.AssetUID, kafkaresponse.DeleteAssetEvent.AssetUID);
      }
    }

    #endregion

    public void SetupCreateAssetKafkaConsumer(Guid assetUidToLookFor, DateTime actionUtc)
    {
      _checkForAssetCreateHandler = new CheckForAssetCreateHandler(assetUidToLookFor, actionUtc);
      SubscribeAndConsumeFromKafka(_checkForAssetCreateHandler);
    }

    public void SetupUpdateAssetKafkaConsumer(Guid assetUidToLookFor, DateTime actionUtc)
    {
      _checkForAssetUpdateHandler = new CheckForAssetUpdateHandler(assetUidToLookFor, actionUtc);
      SubscribeAndConsumeFromKafka(_checkForAssetUpdateHandler);
    }

    public void SetupDeleteAssetKafkaConsumer(Guid assetUidToLookFor, DateTime actionUtc)
    {
      _checkForAssetDeleteHandler = new CheckForAssetDeleteHandler(assetUidToLookFor, actionUtc);
      SubscribeAndConsumeFromKafka(_checkForAssetDeleteHandler);
    }

    public void SubscribeAndConsumeFromKafka(CheckForAssetHandler assetHandler)
    {
      var eventAggregator = new EventAggregator();
      eventAggregator.Subscribe(assetHandler);
      _consumerWrapper = new ConsumerWrapper(eventAggregator,
          new KafkaConsumerParams("AssetServiceAcceptanceTest", MasterDataConfig.AssetServiceKafkaUri,
              MasterDataConfig.AssetServiceTopic));
      //new Thread(()=>_consumerWrapper.Consume(fetchFromTail: true)){ Priority = ThreadPriority.Highest }.Start();      
      Task.Factory.StartNew(() => _consumerWrapper.ReadOffset(fetchFromTail: true));
      Thread.Sleep(new TimeSpan(0, 0, 10));
    }

  }
}
