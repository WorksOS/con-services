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
using System.Threading.Tasks;
using VSS.MasterData.Asset.AcceptanceTests.Scenarios.AssetService;
using VSS.MasterData.Asset.AcceptanceTests.Utils.Config;
using VSS.MasterData.Asset.AcceptanceTests.Utils.Features.Classes.AssetSearchService;
using VSS.MasterData.Asset.AcceptanceTests.Utils.Features.Classes.AssetService;
using VSS.MasterData.Asset.AcceptanceTests.Utils.Features.Classes.DeviceService;

namespace VSS.MasterData.Asset.AcceptanceTests.Scenarios.AssetSearchService
{
    public class AssetDeviceSearchServiceSupport
    {

        private static Log4Net Log = new Log4Net(typeof(AssetDeviceSearchServiceSupport));
        public string ResponseString = string.Empty;
        public CreateAssetEvent createAssetEvent;
        public CreateDeviceModel CreateDeviceModel = new CreateDeviceModel();
        public DeviceAssetAssociationModel DeviceAssetAssociationModel = new DeviceAssetAssociationModel();
        public DeviceAssetDissociationModel DeviceAssetDissociationModel = new DeviceAssetDissociationModel();
        string validPageNo = "2";
        string validPageSize = "15";
        string validSearchString = "AutoTestAPICreateAssetSerial";
        string wrongPageNo = "19000";
        string wrongPageSize = "185000";
        string invalidPageNo = "-1";
        string invalidPageSize = "-1";
        string invalidPageNoZero = "0";
        string invalidPageSizeZero = "0";
        string wrongSearchString = "WrongSearchString";

        #region Constructors

        public AssetDeviceSearchServiceSupport(Log4Net myLog)
        {
            AssetServiceConfig.SetupEnvironment();
            Log = myLog;
        }

        #endregion

        public void GetAssetDeviceList(string searchType = null, string pageNo = null, string pageSize = null)
        {
            createAssetEvent = AssetServiceSteps.assetServiceSupport.CreateAssetModel;
            string searchString = string.Empty;
            string requestType = string.Empty;

            if (searchType == "AssetSN")
            {
                searchString = createAssetEvent.SerialNumber;
            }
            else if (searchType == "AssetName")
            {
                searchString = createAssetEvent.AssetName;
            }
            else if (searchType == "DeviceSN")
            {
                searchString = CreateDeviceModel.DeviceSerialNumber;
            }
            else if (searchType == "AssetSNSorting" || searchType == "Valid")
            {
                searchString = validSearchString;
            }
            else if (searchType == "Wrong")
            {
                searchString = wrongSearchString;
            }
            else
            {
                searchString = searchType;
            }

            if (pageNo == "Valid")
            {
                pageNo = validPageNo;
            }
            else if (pageNo == "Invalid")
            {
                pageNo = invalidPageNo;
                requestType = "Invalid";
            }
            else if (pageNo == "Wrong")
            {
                pageNo = wrongPageNo;
            }
            else if (pageNo == "0" || pageNo == "-1")
            {
                requestType = "Invalid";
            }

            if (pageSize == "Valid")
            {
                pageSize = validPageSize;
            }
            else if (pageSize == "Invalid")
            {
                pageSize = invalidPageSize;
                requestType = "Invalid";
            }
            else if (pageSize == "Wrong")
            {
                pageSize = wrongPageSize;
            }
            else if (pageSize == "0" || pageSize == "-1")
            {
                requestType = "Invalid";
            }


            string parameters = string.Empty;

            if (searchString != null && pageNo == null && pageSize == null)
            {
                parameters = "?" + AssetServiceConfig.SearchString + "=" + searchString;
            }
            else if (searchString == null && pageNo != null && pageSize == null)
            {
                parameters = "?" + AssetServiceConfig.PageNo + "=" + pageNo;
            }
            else if (searchString == null && pageNo == null && pageSize != null)
            {
                parameters = "?" + AssetServiceConfig.PageSize + "=" + pageSize;
            }
            else if (searchString != null && pageNo != null && pageSize == null)
            {
                parameters = "?" + AssetServiceConfig.SearchString + "=" + searchString + "&" + AssetServiceConfig.PageNo + "=" + pageNo;
            }
            else if (searchString != null && pageNo == null && pageSize != null)
            {
                parameters = "?" + AssetServiceConfig.SearchString + "=" + searchString + "&" + AssetServiceConfig.PageSize + "=" + pageSize;
            }
            else if (searchString == null && pageNo != null && pageSize != null)
            {
                parameters = "?" + AssetServiceConfig.PageNo + "=" + pageNo + "&" + AssetServiceConfig.PageSize + "=" + pageSize;
            }
            else if (searchString != null && pageNo != null && pageSize != null)
            {
                parameters = "?" + AssetServiceConfig.SearchString + "=" + searchString + "&" + AssetServiceConfig.PageNo + "=" + pageNo + "&" + AssetServiceConfig.PageSize + "=" + pageSize;
            }
            else if (searchString == null && pageNo == null && pageSize == null)
            {
                parameters = "";
            }

            try
            {
                string accessToken = AssetServiceConfig.GetValidUserAccessToken();
                LogResult.Report(Log, "log_ForInfo", "Getting the AssetDetail Values:");
                if (requestType == "Invalid")
                {
                    ResponseString = RestClientUtil.DoInvalidHttpRequest(AssetServiceConfig.AssetSearchEndpoint + parameters, HeaderSettings.GetMethod, accessToken,
                       HeaderSettings.JsonMediaType, null, HttpStatusCode.BadRequest, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);
                }
                else
                {
                    ResponseString = RestClientUtil.DoHttpRequest(AssetServiceConfig.AssetSearchEndpoint + parameters, HeaderSettings.GetMethod, accessToken,
                       HeaderSettings.JsonMediaType, null, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);
                }
            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While Getting Data from Asset Device Search Service", e);
                throw new Exception(e + " Got Error While Getting Data from Asset Device Search Service");
            }
        }

        //device model
        public void PostValidCreateRequestToService()
        {
            string requestString = JsonConvert.SerializeObject(CreateDeviceModel);

            try
            {
                string accessToken = AssetServiceConfig.GetValidUserAccessToken();
                LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
                ResponseString = RestClientUtil.DoHttpRequest(AssetServiceConfig.DeviceServiceEndpoint, HeaderSettings.PostMethod, accessToken,
                   HeaderSettings.JsonMediaType, requestString, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);
            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Device Service", e);
                throw new Exception(e + " Got Error While Posting Data To Device Service");
            }

        }

        public void PostValidDeviceAssetAssociationRequestToService()
        {
            string requestString = JsonConvert.SerializeObject(DeviceAssetAssociationModel);

            try
            {
                string accessToken = AssetServiceConfig.GetValidUserAccessToken();
                LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
                ResponseString = RestClientUtil.DoHttpRequest(AssetServiceConfig.DeviceAssetAssociationEndpoint, HeaderSettings.PostMethod, accessToken,
                   HeaderSettings.JsonMediaType, requestString, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);
            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To DeviceAsset Association Service", e);
                throw new Exception(e + " Got Error While Posting Data To DeviceAsset Association Service");
            }

        }

        public void PostValidDeviceAssetDissociationRequestToService()
        {
            string requestString = JsonConvert.SerializeObject(DeviceAssetDissociationModel);

            try
            {
                string accessToken = AssetServiceConfig.GetValidUserAccessToken();
                LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
                ResponseString = RestClientUtil.DoHttpRequest(AssetServiceConfig.DeviceAssetDissociationEndpoint, HeaderSettings.PostMethod, accessToken,
                   HeaderSettings.JsonMediaType, requestString, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);
            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To DeviceAsset Dissociation Service", e);
                throw new Exception(e + " Got Error While Posting Data To DeviceAsset Dissociation Service");
            }

        }

        public void VerifyResponse(string verifyParameter)
        {
            AssetDeviceResponseModel response;
            try
            {
                response = JsonConvert.DeserializeObject<AssetDeviceResponseModel>(ResponseString);
            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While DeSerializing JSON Object", e);
                throw new Exception("Got Error While DeSerializing JSON Object");
            }
            switch (verifyParameter)
            {
                case "Asset":
                    Assert.AreEqual(1, response.TotalNumberOfPages);
                    Assert.AreEqual(1, response.PageNumber);
                    foreach (AssetDevice assetdevice in response.AssetDevices)
                    {
                        Assert.AreEqual(createAssetEvent.AssetUID.ToString(), assetdevice.AssetUID.ToLower());
                        Assert.AreEqual(createAssetEvent.AssetName, assetdevice.AssetName);
                        Assert.AreEqual(createAssetEvent.SerialNumber, assetdevice.AssetSerialNumber);
                        Assert.AreEqual(createAssetEvent.MakeCode, assetdevice.AssetMakeCode);
                        Assert.AreEqual(" - ", assetdevice.DeviceSerialNumber);
                        Assert.AreEqual(" - ", assetdevice.DeviceType);
                        Assert.AreEqual(null, assetdevice.DeviceUID);
                    }
                    break;

                case "Device":
                    Assert.AreEqual(1, response.TotalNumberOfPages);
                    Assert.AreEqual(1, response.PageNumber);
                    foreach (AssetDevice assetdevice in response.AssetDevices)
                    {
                        Assert.AreEqual(null, assetdevice.AssetUID);
                        Assert.AreEqual(" - ", assetdevice.AssetName);
                        Assert.AreEqual(" - ", assetdevice.AssetSerialNumber);
                        Assert.AreEqual(" - ", assetdevice.AssetMakeCode);
                        Assert.AreEqual(CreateDeviceModel.DeviceSerialNumber, assetdevice.DeviceSerialNumber);
                        Assert.AreEqual(CreateDeviceModel.DeviceType, assetdevice.DeviceType);
                        Assert.AreEqual(CreateDeviceModel.DeviceUID.ToString(), assetdevice.DeviceUID);
                    }
                    break;

                case "DeviceAsset":
                    Assert.AreEqual(1, response.TotalNumberOfPages);
                    Assert.AreEqual(1, response.PageNumber);
                    foreach (AssetDevice assetdevice in response.AssetDevices)
                    {
                        Assert.AreEqual(createAssetEvent.AssetUID.ToString(), assetdevice.AssetUID.ToLower());
                        Assert.AreEqual(createAssetEvent.AssetName, assetdevice.AssetName);
                        Assert.AreEqual(createAssetEvent.SerialNumber, assetdevice.AssetSerialNumber);
                        Assert.AreEqual(createAssetEvent.MakeCode, assetdevice.AssetMakeCode);
                        Assert.AreEqual(CreateDeviceModel.DeviceSerialNumber, assetdevice.DeviceSerialNumber);
                        Assert.AreEqual(CreateDeviceModel.DeviceType, assetdevice.DeviceType);
                        Assert.AreEqual(CreateDeviceModel.DeviceUID.ToString(), assetdevice.DeviceUID);
                    }
                    break;

                case "PageNo":
                    Assert.AreEqual(Convert.ToInt32(validPageNo), response.PageNumber);
                    break;

                case "PageSize":
                    Assert.AreEqual(Convert.ToInt32(validPageSize), response.AssetDevices.Count());
                    break;

                case "SearchStringPageNo":
                    Assert.AreEqual(Convert.ToInt32(validPageNo), response.PageNumber);
                    foreach (AssetDevice assetdevice in response.AssetDevices)
                    {
                        Assert.IsTrue(assetdevice.AssetSerialNumber.Contains(validSearchString) || assetdevice.AssetName.Contains(validSearchString) || assetdevice.DeviceSerialNumber.Contains(validSearchString),"Verify search string");
                    }
                    break;

                case "SearchStringPageSize":
                    Assert.AreEqual(Convert.ToInt32(validPageSize), response.AssetDevices.Count());
                    foreach (AssetDevice assetdevice in response.AssetDevices)
                    {
                        Assert.IsTrue(assetdevice.AssetSerialNumber.Contains(validSearchString) || assetdevice.AssetName.Contains(validSearchString) || assetdevice.DeviceSerialNumber.Contains(validSearchString), "Verify search string");
                    }
                    break;

                case "PageNoPageSize":
                    Assert.AreEqual(Convert.ToInt32(validPageNo), response.PageNumber);
                    Assert.AreEqual(Convert.ToInt32(validPageSize), response.AssetDevices.Count());
                    break;

                case "SearchStringPageNoPageSize":
                    Assert.AreEqual(Convert.ToInt32(validPageNo), response.PageNumber);
                    Assert.AreEqual(Convert.ToInt32(validPageSize), response.AssetDevices.Count());
                    foreach (AssetDevice assetdevice in response.AssetDevices)
                    {
                        Assert.IsTrue(assetdevice.AssetSerialNumber.Contains(validSearchString) || assetdevice.AssetName.Contains(validSearchString) || assetdevice.DeviceSerialNumber.Contains(validSearchString), "Verify search string");
                    }
                    break;

                case "InvalidPageSize":
                    Assert.IsTrue((response.AssetDevices.Count() < Convert.ToInt32(invalidPageSize) || response.AssetDevices.Count() > 0), "Verify invalid page size");
                    break;

                default:
                    break;
            }
        }

        public void VerifyErrorResponse()
        {
            AssetDeviceErrorResponseModel response;
            try
            {
                response = JsonConvert.DeserializeObject<AssetDeviceErrorResponseModel>(ResponseString);
            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While DeSerializing JSON Object", e);
                throw new Exception("Got Error While DeSerializing JSON Object");
            }
            Assert.AreEqual("Invalid input", response.Message);
        }

        public void VerifyResponseZeroAssetDeviceList()
        {
            AssetDeviceResponseModel response;
            try
            {
                response = JsonConvert.DeserializeObject<AssetDeviceResponseModel>(ResponseString);
            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While DeSerializing JSON Object", e);
                throw new Exception("Got Error While DeSerializing JSON Object");
            }
            Assert.AreEqual(0, response.TotalNumberOfPages);
            Assert.AreEqual(0, response.PageNumber);
            Assert.AreEqual(null, response.AssetDevices);
        }

        public void VerifyResponseSortedByAssetSN()
        {
            List<string> responseSerialNumbers = new List<string>();
            List<string> sortedSerialNumbers = new List<string>();
            AssetDeviceResponseModel response;

            try
            {
                response = JsonConvert.DeserializeObject<AssetDeviceResponseModel>(ResponseString);
            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While DeSerializing JSON Object", e);
                throw new Exception("Got Error While DeSerializing JSON Object");
            }

            Assert.AreEqual(1, response.PageNumber);

            foreach (AssetDevice assetdevice in response.AssetDevices)
            {
                responseSerialNumbers.Add(assetdevice.AssetSerialNumber);
                sortedSerialNumbers.Add(assetdevice.AssetSerialNumber);
            }

            sortedSerialNumbers.Sort();

            Assert.IsTrue(responseSerialNumbers.SequenceEqual(sortedSerialNumbers), "Verify the Asset list is sorted based on Asset Serial Number");
        }
    }
}
