using System;
using System.IO;
using System.Net;
using VSS.MasterData.Asset.AcceptanceTests.Utils.Config;
using VSS.MasterData.Asset.AcceptanceTests.Utils.Features.Classes;
using VSS.MasterData.Asset.AcceptanceTests.Utils.Features.Classes.AssetService;
using AutomationCore.API.Framework.Common;
using AutomationCore.API.Framework.Library;
using AutomationCore.Shared.Library;
using Newtonsoft.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.MasterData.Asset.AcceptanceTests.Resources;
using System.Threading.Tasks;
using System.Threading;
using AutomationCore.API.Framework.Common.Features.TPaaS;
using AutomationCore.API.Framework.Common.Config.TPaaSServicesConfig;
using System.Collections.Generic;
using VSS.Kafka.Factory.Interface;
using VSS.Kafka.Factory.Model;
using System.Linq;

namespace VSS.MasterData.Asset.AcceptanceTests.Scenarios.AssetService
{
  public class AssetServiceSupport : IHandler
  {

    # region Variables

    private static Random random = new Random();
    private static Log4Net Log = new Log4Net(typeof(AssetServiceSupport));

    public CreateAssetEvent CreateAssetModel = new CreateAssetEvent();
    public UpdateAssetEvent UpdateAssetModel = new UpdateAssetEvent();
    public DeleteAssetEvent DeleteAssetModel = new DeleteAssetEvent();
    public InValidCreateAssetEvent InValidCreateAssetModel = new InValidCreateAssetEvent();
    public InValidUpdateAssetEvent InValidUpdateAssetModel = new InValidUpdateAssetEvent();
    public InValidDeleteAssetEvent InValidDeleteAssetModel = new InValidDeleteAssetEvent();

    public string ResponseString = string.Empty;
    public string ErrorString = string.Empty;

    public static string MinValue;
    public static string MaxValue;
    public static string Valid;


    public CreateAssetModel assetServiceCreateResponse = null;
    public UpdateAssetModel assetServiceUpdateResponse = null;
    public DeleteAssetModel assetServiceDeleteResponse = null;

    #endregion

    #region Constructors

    public AssetServiceSupport(Log4Net myLog)
    {
      AssetServiceConfig.SetupEnvironment();
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


    public long RandomLongNumber()
    {
      Random random = new Random();
      long randomNumber = Math.Abs((long)((random.NextDouble() * 2.0 - 1.0) * long.MaxValue));
      return randomNumber;
    }

    
    public static string RandomString(int length)
    {
      const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
      return new string(Enumerable.Repeat(chars, length)
        .Select(s => s[random.Next(s.Length)]).ToArray());
    }
    #endregion

    #region Post Methods

    public void PostValidCreateRequestToService()
    {
      string requestString = JsonConvert.SerializeObject(CreateAssetModel);

      try
      {
        string accessToken = AssetServiceConfig.GetValidUserAccessToken();
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
        ResponseString = RestClientUtil.DoHttpRequest(AssetServiceConfig.AssetServiceEndpoint, HeaderSettings.PostMethod, accessToken,
           HeaderSettings.JsonMediaType, requestString, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);
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
        string accessToken = AssetServiceConfig.GetValidUserAccessToken();
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
        ResponseString = RestClientUtil.DoInvalidHttpRequest(AssetServiceConfig.AssetServiceEndpoint, HeaderSettings.PostMethod, accessToken,
           contentType, requestString, actualResponse, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);
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
        string accessToken = AssetServiceConfig.GetValidUserAccessToken();
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
        ResponseString = RestClientUtil.DoHttpRequest(AssetServiceConfig.AssetServiceEndpoint, HeaderSettings.PutMethod, accessToken,
           HeaderSettings.JsonMediaType, requestString, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);

      }
      catch (Exception e)
      {
        LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Asset Service", e);
        throw new Exception(e + " Got Error While Posting Data To Asset Service");
      }

    }

    public void GetValidResponseFromService(string accessToken, string queryString = null)
    {
      try
      {
        LogResult.Report(Log, "log_ForInfo", "Get the response with Valid Values: ");
        ResponseString = RestClientUtil.DoHttpRequest(AssetServiceConfig.AssetServiceEndpoint + "/list" + "?" + queryString, HeaderSettings.GetMethod, accessToken,
           HeaderSettings.JsonMediaType, null, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);

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
        string accessToken = AssetServiceConfig.GetValidUserAccessToken();
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
        ResponseString = RestClientUtil.DoInvalidHttpRequest(AssetServiceConfig.AssetServiceEndpoint, HeaderSettings.PutMethod, accessToken,
           contentType, requestString, actualResponse, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);
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
        string accessToken = AssetServiceConfig.GetValidUserAccessToken();
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + assetUid);
        ResponseString = RestClientUtil.DoHttpRequest(string.Format("{0}?Assetuid={1}&ActionUTC={2}", AssetServiceConfig.AssetServiceEndpoint, assetUID, actionUtcString), HeaderSettings.DeleteMethod, accessToken,
           HeaderSettings.JsonMediaType, null, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);
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
        string accessToken = AssetServiceConfig.GetValidUserAccessToken();
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + assetUid);
        ResponseString = RestClientUtil.DoInvalidHttpRequest(string.Format("{0}?Assetuid={1}&ActionUTC={2}", AssetServiceConfig.AssetServiceEndpoint, assetUid, actionUtc), HeaderSettings.DeleteMethod, accessToken,
           contentType, null, actualResponse, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);
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
        string resourceError = AssetServiceMessages.ResourceManager.GetString(ErrorMessage);
        if (error.ModelState != null)
        {
          if (error.ModelState.SerialNumber != null)
            Assert.AreEqual(resourceError, error.ModelState.SerialNumber[0].ToString());
          else if (error.ModelState.MakeCode != null)
            Assert.AreEqual(resourceError, error.ModelState.MakeCode[0].ToString());
          else if (error.ModelState.ModelYear != null)
            Assert.AreEqual(resourceError, error.ModelState.ModelYear[0].ToString());
          else if (error.ModelState.IconKey != null)
            Assert.AreEqual(resourceError, error.ModelState.IconKey[0].ToString());
          else if (error.ModelState.ActionUTC != null)
            Assert.AreEqual(resourceError, error.ModelState.ActionUTC[0].ToString());
          else
            Assert.AreEqual(AssetServiceMessages.ResourceManager.GetString("ERR_Invalid"), error.Message);

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

    #region Response Verification

    public void VerifyAssetServiceCreateResponse()
    {
      try
      {
        WaitForKafkaResponseAfterCreate();

        if (CreateAssetModel.AssetName != null)
          Assert.AreEqual(CreateAssetModel.AssetName, assetServiceCreateResponse.CreateAssetEvent.AssetName);
        if (CreateAssetModel.SerialNumber != null)
          Assert.AreEqual(CreateAssetModel.SerialNumber, assetServiceCreateResponse.CreateAssetEvent.SerialNumber);
        if (CreateAssetModel.MakeCode != null)
          Assert.AreEqual(CreateAssetModel.MakeCode, assetServiceCreateResponse.CreateAssetEvent.MakeCode);
        if (CreateAssetModel.ModelYear != null)
          Assert.AreEqual(CreateAssetModel.ModelYear, assetServiceCreateResponse.CreateAssetEvent.ModelYear);
        if (CreateAssetModel.Model != null)
          Assert.AreEqual(CreateAssetModel.Model, assetServiceCreateResponse.CreateAssetEvent.Model);
        if (CreateAssetModel.ActionUTC != null)
          Assert.AreEqual(CreateAssetModel.ActionUTC.ToString("yyyyMMddhhmmss"), assetServiceCreateResponse.CreateAssetEvent.ActionUTC.ToString("yyyyMMddhhmmss"));
        if (CreateAssetModel.AssetUID != null)
          Assert.AreEqual(CreateAssetModel.AssetUID, assetServiceCreateResponse.CreateAssetEvent.AssetUID);
        if (CreateAssetModel.AssetType != null)
          Assert.AreEqual(CreateAssetModel.AssetType, assetServiceCreateResponse.CreateAssetEvent.AssetType);
        if (CreateAssetModel.EquipmentVIN != null)
          Assert.AreEqual(CreateAssetModel.EquipmentVIN, assetServiceCreateResponse.CreateAssetEvent.EquipmentVIN);
        if (CreateAssetModel.IconKey != null)
          Assert.AreEqual(CreateAssetModel.IconKey, assetServiceCreateResponse.CreateAssetEvent.IconKey);
        if (CreateAssetModel.LegacyAssetID != null)
          Assert.AreEqual(CreateAssetModel.LegacyAssetID, assetServiceCreateResponse.CreateAssetEvent.LegacyAssetID);
        if (CreateAssetModel.OwningCustomerUID != null)
          Assert.AreEqual(CreateAssetModel.OwningCustomerUID, assetServiceCreateResponse.CreateAssetEvent.OwningCustomerUID);

        assetServiceCreateResponse = null; // Reassigning to null
      }
      catch (Exception e)
      {
        LogResult.Report(Log, "log_ForError", "Got Error While Verifying Response", e);
        Assert.Fail("Did not find the event in kafka topic");
      }
    }

    public void VerifyAssetServiceUpdateResponse()
    {
      try
      {
        WaitForKafkaResponseAfterUpdate();

        if (UpdateAssetModel.AssetName != null)
          Assert.AreEqual(UpdateAssetModel.AssetName, assetServiceUpdateResponse.UpdateAssetEvent.AssetName);
        if (UpdateAssetModel.ModelYear != null)
          Assert.AreEqual(UpdateAssetModel.ModelYear, assetServiceUpdateResponse.UpdateAssetEvent.ModelYear);
        if (UpdateAssetModel.Model != null)
          Assert.AreEqual(UpdateAssetModel.Model, assetServiceUpdateResponse.UpdateAssetEvent.Model);
        if (UpdateAssetModel.ActionUTC != null)
          Assert.AreEqual(UpdateAssetModel.ActionUTC.ToString("yyyyMMddhhmmss"), assetServiceUpdateResponse.UpdateAssetEvent.ActionUTC.ToString("yyyyMMddhhmmss"));
        if (UpdateAssetModel.AssetUID != null)
          Assert.AreEqual(UpdateAssetModel.AssetUID, assetServiceUpdateResponse.UpdateAssetEvent.AssetUID);
        if (UpdateAssetModel.AssetType != null)
          Assert.AreEqual(UpdateAssetModel.AssetType, assetServiceUpdateResponse.UpdateAssetEvent.AssetType);
        if (UpdateAssetModel.EquipmentVIN != null)
          Assert.AreEqual(UpdateAssetModel.EquipmentVIN, assetServiceUpdateResponse.UpdateAssetEvent.EquipmentVIN);
        if (UpdateAssetModel.IconKey != null)
          Assert.AreEqual(UpdateAssetModel.IconKey, assetServiceUpdateResponse.UpdateAssetEvent.IconKey);
        if (UpdateAssetModel.LegacyAssetID != null)
          Assert.AreEqual(UpdateAssetModel.LegacyAssetID, assetServiceUpdateResponse.UpdateAssetEvent.LegacyAssetID);

        assetServiceUpdateResponse = null; // Reassigning to null
      }
      catch (Exception e)
      {
        LogResult.Report(Log, "log_ForError", "Got Error While Verifying Response", e);
        Assert.Fail("Did not find the event in kafka topic");
      }

    }

    public void VerifyAssetServiceDeleteResponse()
    {
      try
      {
        WaitForKafkaResponseAfterDelete();

        if (DeleteAssetModel.ActionUTC != null)
          Assert.AreEqual(DeleteAssetModel.ActionUTC.ToString("yyyyMMddhhmmss"), assetServiceDeleteResponse.DeleteAssetEvent.ActionUTC.ToString("yyyyMMddhhmmss"));
        if (DeleteAssetModel.AssetUID != null)
          Assert.AreEqual(DeleteAssetModel.AssetUID, assetServiceDeleteResponse.DeleteAssetEvent.AssetUID);

        assetServiceDeleteResponse = null; // Reassigning to null
      }
      catch (Exception e)
      {
        LogResult.Report(Log, "log_ForError", "Got Error While Verifying Response", e);
        Assert.Fail("Did not find the event in kafka topic");
      }
    }

    #endregion

    #region IKVM
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

        if (CreateAssetModel != null && CreateAssetModel.ActionUTC != null)
        {
          if (CreateAssetModel.ActionUTC.ToString() != null && message.Value.Contains(CreateAssetModel.ActionUTC.ToString("yyyy-MM-ddThh:mm:ss"))
              && CreateAssetModel.AssetUID.ToString() != null && message.Value.Contains(CreateAssetModel.AssetUID.ToString()))
            assetServiceCreateResponse = JsonConvert.DeserializeObject<CreateAssetModel>(message.Value);
          LogResult.Report(Log, "log_ForInfo", string.Format("Response Received With Offset {0}: {1}", message.OffSet, message.Value));

          if (UpdateAssetModel != null && UpdateAssetModel.ActionUTC != null && UpdateAssetModel.AssetUID != Guid.Empty)
          {
            if (UpdateAssetModel.ActionUTC.ToString() != null && message.Value.Contains(UpdateAssetModel.ActionUTC.ToString("yyyy-MM-ddThh:mm:ss"))
                && UpdateAssetModel.AssetUID.ToString() != null && message.Value.Contains(UpdateAssetModel.AssetUID.ToString()))
              assetServiceUpdateResponse = JsonConvert.DeserializeObject<UpdateAssetModel>(message.Value);
            LogResult.Report(Log, "log_ForInfo", string.Format("Response Received With Offset {0}: {1}", message.OffSet, message.Value));
          }

          if (DeleteAssetModel != null && DeleteAssetModel.ActionUTC != null && DeleteAssetModel.AssetUID != Guid.Empty)
          {
            if (DeleteAssetModel.ActionUTC.ToString() != null && message.Value.Contains(DeleteAssetModel.ActionUTC.ToString("yyyy-MM-ddThh:mm:ss"))
                && DeleteAssetModel.AssetUID.ToString() != null && message.Value.Contains(DeleteAssetModel.AssetUID.ToString()))
              assetServiceDeleteResponse = JsonConvert.DeserializeObject<DeleteAssetModel>(message.Value);
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
    private void WaitForKafkaResponseAfterCreate(bool isPositiveCase = true)
    {
      int i = 0;
      if (!isPositiveCase)
        LogResult.Report(Log, "log_ForInfo", "Expecting No Response From Kafka");
      else
        LogResult.Report(Log, "log_ForInfo", "Waiting " + KafkaServicesConfig.InitialWaitingTimeForReceivingResponseInSeconds + " seconds For Kafka Response");
      for (i = 0; i < KafkaServicesConfig.InitialWaitingTimeForReceivingResponseInSeconds; i++)
      {
        if (CreateAssetModel.AssetUID != Guid.Empty)
        {
          if (assetServiceCreateResponse != null)
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

    private void WaitForKafkaResponseAfterUpdate(bool isPositiveCase = true)
    {
      int i = 0;
      if (!isPositiveCase)
        LogResult.Report(Log, "log_ForInfo", "Expecting No Response From Kafka");
      else
        LogResult.Report(Log, "log_ForInfo", "Waiting " + KafkaServicesConfig.InitialWaitingTimeForReceivingResponseInSeconds + " seconds For Kafka Response");
      for (i = 0; i < KafkaServicesConfig.InitialWaitingTimeForReceivingResponseInSeconds; i++)
      {
        if (UpdateAssetModel.AssetUID != Guid.Empty)
        {
          if (assetServiceUpdateResponse != null)
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

    private void WaitForKafkaResponseAfterDelete(bool isPositiveCase = true)
    {
      int i = 0;
      if (!isPositiveCase)
        LogResult.Report(Log, "log_ForInfo", "Expecting No Response From Kafka");
      else
        LogResult.Report(Log, "log_ForInfo", "Waiting " + KafkaServicesConfig.InitialWaitingTimeForReceivingResponseInSeconds + " seconds For Kafka Response");
      for (i = 0; i < KafkaServicesConfig.InitialWaitingTimeForReceivingResponseInSeconds; i++)
      {
        if (DeleteAssetModel.AssetUID != Guid.Empty)
        {
          if (assetServiceDeleteResponse != null)
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
