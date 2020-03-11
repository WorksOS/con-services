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
using VSS.MasterData.Customer.AcceptanceTests.Utils.Features.Classes.CustomerService;
using VSS.MasterData.Asset.AcceptanceTests.Scenarios.AssetSearchService;
using VSS.MasterData.Asset.AcceptanceTests.Scenarios.AssetService;
using VSS.MasterData.Asset.AcceptanceTests.Utils.Config;
using VSS.MasterData.Asset.AcceptanceTests.Utils.Features.Classes.AssetDetail;
using VSS.MasterData.Asset.AcceptanceTests.Utils.Features.Classes.AssetService;
using VSS.MasterData.Asset.AcceptanceTests.Utils.Features.Classes.AssetSubscription;
using VSS.MasterData.Asset.AcceptanceTests.Utils.Features.Classes.DeviceService;

namespace VSS.MasterData.Asset.AcceptanceTests.Scenarios.AssetDetailsService
{
    public class AssetDetailsServiceSupport
    {

        private static Log4Net Log = new Log4Net(typeof(AssetDetailsServiceSupport));
        public string OldResponseString = string.Empty;
        public string ResponseString = string.Empty;
        public AssetSubscriptionModel FirstAssetSubscriptionModel = new AssetSubscriptionModel();
        public AssetSubscriptionModel AssetSubscriptionModel = new AssetSubscriptionModel();
        public CreateCustomerEvent DealerModel = new CreateCustomerEvent();
        public CreateCustomerEvent FirstDealerModel = new CreateCustomerEvent();
        public CreateCustomerEvent CustomerModel = new CreateCustomerEvent();
        public CreateCustomerEvent FirstCustomerModel = new CreateCustomerEvent();
        public CreateAssetEvent FirstAssetModel = new CreateAssetEvent();
        public CreateDeviceModel FirstDeviceModel = new CreateDeviceModel();

        #region Constructors

        public AssetDetailsServiceSupport(Log4Net myLog)
        {
            AssetServiceConfig.SetupEnvironment();
            Log = myLog;
        }

        #endregion

        public void PostValidSubscriptionRequestToService()
        {
            string requestString = JsonConvert.SerializeObject(AssetSubscriptionModel);

            try
            {
                string accessToken = AssetServiceConfig.GetValidUserAccessToken();
                LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
                ResponseString = RestClientUtil.DoHttpRequest(AssetServiceConfig.AssetSubscriptionEndpoint, HeaderSettings.PostMethod, accessToken,
                   HeaderSettings.JsonMediaType, requestString, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);
            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Asset Subscription Service", e);
                throw new Exception(e + " Got Error While Posting Data To Asset Subscription Service");
            }
        }

        public void GetAssetDetails(string assetUID = null, string deviceUID = null)
        {
            string requestType = string.Empty;
            string parameters = string.Empty;

            if (assetUID != null && deviceUID == null)
            {
                parameters = "?" + AssetServiceConfig.AssetUID + "=" + assetUID;
            }
            else if (assetUID == null && deviceUID != null)
            {
                parameters = "?" + AssetServiceConfig.DeviceUID + "=" + deviceUID;
            }
            else if (assetUID != null && deviceUID != null)
            {
                parameters = "?" + AssetServiceConfig.AssetUID + "=" + assetUID + "&" + AssetServiceConfig.DeviceUID + "=" + deviceUID;
            }
            
            if (assetUID == "EMPTY" && deviceUID == "EMPTY")
            {
                requestType = "Invalid";
                parameters = "";
            }
            else if (assetUID == "EMPTY" && deviceUID == "Invalid")
            {
                requestType = "Invalid";
                parameters = "?" + AssetServiceConfig.DeviceUID + "=" + deviceUID;
            }
            else if (assetUID == "Invalid" && deviceUID == "EMPTY")
            {
                requestType = "Invalid";
                parameters = "?" + AssetServiceConfig.AssetUID + "=" + assetUID;
            }
            else if (assetUID == "Invalid" && deviceUID == "Invalid")
            {
                requestType = "Invalid";
                parameters = "?" + AssetServiceConfig.AssetUID + "=" + assetUID + "&" + AssetServiceConfig.DeviceUID + "=" + deviceUID;
            }

            try
            {
                string accessToken = AssetServiceConfig.GetValidUserAccessToken();
                LogResult.Report(Log, "log_ForInfo", "Getting the AssetDetail Values:");
                if (requestType == "Invalid")
                {
                    ResponseString = RestClientUtil.DoInvalidHttpRequest(AssetServiceConfig.AssetDetailEndpoint + parameters, HeaderSettings.GetMethod, accessToken,
                       HeaderSettings.JsonMediaType, null, HttpStatusCode.BadRequest, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);
                }
                else
                {
                    ResponseString = RestClientUtil.DoHttpRequest(AssetServiceConfig.AssetDetailEndpoint + parameters, HeaderSettings.GetMethod, accessToken,
                       HeaderSettings.JsonMediaType, null, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);
                }
            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While Getting Data from Asset Detail Service", e);
                throw new Exception(e + " Got Error While Getting Data from Asset Detail Service");
            }
        }

        public void GetAssetDetails_Association(string oldAssetUID, string assetUID)
        {
            string requestType = string.Empty;

            try
            {
                string accessToken = AssetServiceConfig.GetValidUserAccessToken();
                LogResult.Report(Log, "log_ForInfo", "Getting the AssetDetail Values:");
                OldResponseString = RestClientUtil.DoHttpRequest(AssetServiceConfig.AssetDetailEndpoint + "?" + AssetServiceConfig.AssetUID + "=" + oldAssetUID, HeaderSettings.GetMethod, accessToken,
                    HeaderSettings.JsonMediaType, null, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);

                ResponseString = RestClientUtil.DoHttpRequest(AssetServiceConfig.AssetDetailEndpoint + "?" + AssetServiceConfig.AssetUID + "=" + assetUID, HeaderSettings.GetMethod, accessToken,
                    HeaderSettings.JsonMediaType, null, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);
            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While Getting Data from Asset Detail Service", e);
                throw new Exception(e + " Got Error While Getting Data from Asset Detail Service");
            }
        }

        public void VerifyResponse(string verifyParameter)
        {
            List<AssetDetailResponseModel> responseList = new List<AssetDetailResponseModel>();
            AssetDetailResponseModel response = new AssetDetailResponseModel();

            try
            {
                responseList = JsonConvert.DeserializeObject<List<AssetDetailResponseModel>>(ResponseString);
            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While DeSerializing JSON Object", e);
                throw new Exception("Got Error While DeSerializing JSON Object");
            }

            Assert.AreEqual(1, responseList.Count());
            response = responseList[0];

            //Verify Asset Info
            Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetName, response.AssetInfo.AssetName);
            Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.SerialNumber, response.AssetInfo.SerialNumber);
            Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.MakeCode, response.AssetInfo.MakeCode);
            Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.Model, response.AssetInfo.Model);
            Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetType, response.AssetInfo.AssetType);
            Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.ModelYear, response.AssetInfo.ModelYear);
            Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetUID, response.AssetInfo.AssetUID);

            //Verify Device Info
            Assert.AreEqual(AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.CreateDeviceModel.DeviceSerialNumber, response.DeviceInfo.DeviceSerialNumber);
            Assert.AreEqual(AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.CreateDeviceModel.DeviceType, response.DeviceInfo.DeviceType);
            Assert.AreEqual(AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.CreateDeviceModel.DeviceState, response.DeviceInfo.DeviceState);
            Assert.AreEqual(AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.CreateDeviceModel.DeviceUID, response.DeviceInfo.DeviceUID);

            switch (verifyParameter)
            {
                case "HappyPath":
                    //Verify Account Info
                    Assert.AreEqual(2, response.AccountInfo.Count());
                    foreach (AccountInfo accountInfo in response.AccountInfo)
                    {
                        if (accountInfo.CustomerType == "Customer")
                        {
                            Assert.AreEqual(CustomerModel.CustomerUID, accountInfo.CustomerUID);
                            Assert.AreEqual(CustomerModel.CustomerName, accountInfo.CustomerName);
                            Assert.AreEqual(DealerModel.CustomerUID, accountInfo.ParentCustomerUID);
                            Assert.AreEqual(DealerModel.CustomerName, accountInfo.ParentName);
                            Assert.AreEqual(DealerModel.CustomerType, accountInfo.ParentCustomerType);
                        }
                        else if (accountInfo.CustomerType == "Dealer")
                        {
                            Assert.AreEqual(DealerModel.CustomerUID, accountInfo.CustomerUID);
                            Assert.AreEqual(DealerModel.CustomerName, accountInfo.CustomerName);
                            Assert.AreEqual(DealerModel.CustomerUID, accountInfo.ParentCustomerUID);
                            Assert.AreEqual(DealerModel.CustomerName, accountInfo.ParentName);
                            Assert.AreEqual(DealerModel.CustomerType, accountInfo.ParentCustomerType);
                        }
                    }
                    //Verify Subscription Info
                    Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetUID, response.Subscription.AssetUID);
                    Assert.AreEqual("Active", response.Subscription.SubscriptionStatus);
                    Assert.AreEqual(1, response.Subscription.OwnersVisibility.Count());

                    foreach (OwnersVisibility ownersVisibility in response.Subscription.OwnersVisibility)
                    {
                        Assert.AreEqual(AssetSubscriptionModel.CustomerUID, ownersVisibility.CustomerUID);
                        if (AssetSubscriptionModel.CustomerUID == DealerModel.CustomerUID)
                        {
                            Assert.AreEqual(DealerModel.CustomerType, ownersVisibility.CustomerType);
                        }
                        else
                        {
                            Assert.AreEqual(CustomerModel.CustomerType, ownersVisibility.CustomerType);
                        }
                        Assert.AreEqual(AssetSubscriptionModel.SubscriptionUID, ownersVisibility.SubscriptionUID);
                        Assert.AreEqual(AssetSubscriptionModel.SubscriptionType, ownersVisibility.SubscriptionName);
                        Assert.AreEqual("Active", ownersVisibility.SubscriptionStatus);
                        Assert.AreEqual(AssetSubscriptionModel.StartDate.ToString(), ownersVisibility.SubscriptionStartDate.ToString());
                        Assert.AreEqual(AssetSubscriptionModel.EndDate.ToString(), ownersVisibility.SubscriptionEndDate.ToString());
                    }
                    break;

                case "WithoutAccountAndSubscription":
                    //Verify Account Info
                    Assert.AreEqual(0, response.AccountInfo.Count());
                    //Verify Subscription Info
                    Assert.AreEqual(null, response.Subscription);
                    break;

                case "WithoutSubscription":
                    //Verify Account Info
                    Assert.AreEqual(2, response.AccountInfo.Count());
                    foreach (AccountInfo accountInfo in response.AccountInfo)
                    {
                        if (accountInfo.CustomerType == "Customer")
                        {
                            Assert.AreEqual(CustomerModel.CustomerUID, accountInfo.CustomerUID);
                            Assert.AreEqual(CustomerModel.CustomerName, accountInfo.CustomerName);
                            Assert.AreEqual(DealerModel.CustomerUID, accountInfo.ParentCustomerUID);
                            Assert.AreEqual(DealerModel.CustomerName, accountInfo.ParentName);
                            Assert.AreEqual(DealerModel.CustomerType, accountInfo.ParentCustomerType);
                        }
                        else if (accountInfo.CustomerType == "Dealer")
                        {
                            Assert.AreEqual(DealerModel.CustomerUID, accountInfo.CustomerUID);
                            Assert.AreEqual(DealerModel.CustomerName, accountInfo.CustomerName);
                            Assert.AreEqual(DealerModel.CustomerUID, accountInfo.ParentCustomerUID);
                            Assert.AreEqual(DealerModel.CustomerName, accountInfo.ParentName);
                            Assert.AreEqual(DealerModel.CustomerType, accountInfo.ParentCustomerType);
                        }
                    }
                    //Verify Subscription Info
                    Assert.AreEqual(null, response.Subscription);
                    break;

                default:
                    break;
            }
        }

        public void VerifyResponse_Association(string verifyParameter)
        {

            List<AssetDetailResponseModel> responseList = new List<AssetDetailResponseModel>();
            AssetDetailResponseModel response = new AssetDetailResponseModel();

            switch (verifyParameter)
            {
                case "CustomerWithSameDealer":
                    try
                    {
                        responseList = JsonConvert.DeserializeObject<List<AssetDetailResponseModel>>(OldResponseString);
                    }
                    catch (Exception e)
                    {
                        LogResult.Report(Log, "log_ForError", "Got Error While DeSerializing JSON Object", e);
                        throw new Exception("Got Error While DeSerializing JSON Object");
                    }
                    Assert.AreEqual(1, responseList.Count());
                    response = responseList[0];

                    //Verify Asset Info
                    Assert.AreEqual(FirstAssetModel.AssetName, response.AssetInfo.AssetName);
                    Assert.AreEqual(FirstAssetModel.SerialNumber, response.AssetInfo.SerialNumber);
                    Assert.AreEqual(FirstAssetModel.MakeCode, response.AssetInfo.MakeCode);
                    Assert.AreEqual(FirstAssetModel.Model, response.AssetInfo.Model);
                    Assert.AreEqual(FirstAssetModel.AssetType, response.AssetInfo.AssetType);
                    Assert.AreEqual(FirstAssetModel.ModelYear, response.AssetInfo.ModelYear);
                    Assert.AreEqual(FirstAssetModel.AssetUID, response.AssetInfo.AssetUID);

                    //Verify Device Info
                    Assert.AreEqual(FirstDeviceModel.DeviceSerialNumber, response.DeviceInfo.DeviceSerialNumber);
                    Assert.AreEqual(FirstDeviceModel.DeviceType, response.DeviceInfo.DeviceType);
                    Assert.AreEqual(FirstDeviceModel.DeviceState, response.DeviceInfo.DeviceState);
                    Assert.AreEqual(FirstDeviceModel.DeviceUID, response.DeviceInfo.DeviceUID);

                    //Verify Account Info
                    Assert.AreEqual(2, response.AccountInfo.Count());
                    foreach (AccountInfo accountInfo in response.AccountInfo)
                    {
                        if (accountInfo.CustomerType == "Customer")
                        {
                            Assert.AreEqual(FirstCustomerModel.CustomerUID, accountInfo.CustomerUID);
                            Assert.AreEqual(FirstCustomerModel.CustomerName, accountInfo.CustomerName);
                            Assert.AreEqual(DealerModel.CustomerUID, accountInfo.ParentCustomerUID);
                            Assert.AreEqual(DealerModel.CustomerName, accountInfo.ParentName);
                            Assert.AreEqual(DealerModel.CustomerType, accountInfo.ParentCustomerType);
                        }
                        else if (accountInfo.CustomerType == "Dealer")
                        {
                            Assert.AreEqual(DealerModel.CustomerUID, accountInfo.CustomerUID);
                            Assert.AreEqual(DealerModel.CustomerName, accountInfo.CustomerName);
                            Assert.AreEqual(DealerModel.CustomerUID, accountInfo.ParentCustomerUID);
                            Assert.AreEqual(DealerModel.CustomerName, accountInfo.ParentName);
                            Assert.AreEqual(DealerModel.CustomerType, accountInfo.ParentCustomerType);
                        }
                    }

                     try
                    {
                        responseList = JsonConvert.DeserializeObject<List<AssetDetailResponseModel>>(ResponseString);
                    }
                    catch (Exception e)
                    {
                        LogResult.Report(Log, "log_ForError", "Got Error While DeSerializing JSON Object", e);
                        throw new Exception("Got Error While DeSerializing JSON Object");
                    }
                    Assert.AreEqual(1, responseList.Count());
                    response = responseList[0];

                    //Verify Asset Info
                    Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetName, response.AssetInfo.AssetName);
                    Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.SerialNumber, response.AssetInfo.SerialNumber);
                    Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.MakeCode, response.AssetInfo.MakeCode);
                    Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.Model, response.AssetInfo.Model);
                    Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetType, response.AssetInfo.AssetType);
                    Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.ModelYear, response.AssetInfo.ModelYear);
                    Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetUID, response.AssetInfo.AssetUID);

                    //Verify Device Info
                    Assert.AreEqual(AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.CreateDeviceModel.DeviceSerialNumber, response.DeviceInfo.DeviceSerialNumber);
                    Assert.AreEqual(AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.CreateDeviceModel.DeviceType, response.DeviceInfo.DeviceType);
                    Assert.AreEqual(AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.CreateDeviceModel.DeviceState, response.DeviceInfo.DeviceState);
                    Assert.AreEqual(AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.CreateDeviceModel.DeviceUID, response.DeviceInfo.DeviceUID);

                    //Verify Account Info
                    Assert.AreEqual(2, response.AccountInfo.Count());
                    foreach (AccountInfo accountInfo in response.AccountInfo)
                    {
                        if (accountInfo.CustomerType == "Customer")
                        {
                            Assert.AreEqual(CustomerModel.CustomerUID, accountInfo.CustomerUID);
                            Assert.AreEqual(CustomerModel.CustomerName, accountInfo.CustomerName);
                            Assert.AreEqual(DealerModel.CustomerUID, accountInfo.ParentCustomerUID);
                            Assert.AreEqual(DealerModel.CustomerName, accountInfo.ParentName);
                            Assert.AreEqual(DealerModel.CustomerType, accountInfo.ParentCustomerType);
                        }
                        else if (accountInfo.CustomerType == "Dealer")
                        {
                            Assert.AreEqual(DealerModel.CustomerUID, accountInfo.CustomerUID);
                            Assert.AreEqual(DealerModel.CustomerName, accountInfo.CustomerName);
                            Assert.AreEqual(DealerModel.CustomerUID, accountInfo.ParentCustomerUID);
                            Assert.AreEqual(DealerModel.CustomerName, accountInfo.ParentName);
                            Assert.AreEqual(DealerModel.CustomerType, accountInfo.ParentCustomerType);
                        }
                    }
                    break;

                case "CustomerDealer":
                     try
                    {
                        responseList = JsonConvert.DeserializeObject<List<AssetDetailResponseModel>>(ResponseString);
                    }
                    catch (Exception e)
                    {
                        LogResult.Report(Log, "log_ForError", "Got Error While DeSerializing JSON Object", e);
                        throw new Exception("Got Error While DeSerializing JSON Object");
                    }
                    Assert.AreEqual(1, responseList.Count());
                    response = responseList[0];

                    //Verify Asset Info
                    Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetName, response.AssetInfo.AssetName);
                    Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.SerialNumber, response.AssetInfo.SerialNumber);
                    Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.MakeCode, response.AssetInfo.MakeCode);
                    Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.Model, response.AssetInfo.Model);
                    Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetType, response.AssetInfo.AssetType);
                    Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.ModelYear, response.AssetInfo.ModelYear);
                    Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetUID, response.AssetInfo.AssetUID);

                    //Verify Device Info
                    Assert.AreEqual(AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.CreateDeviceModel.DeviceSerialNumber, response.DeviceInfo.DeviceSerialNumber);
                    Assert.AreEqual(AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.CreateDeviceModel.DeviceType, response.DeviceInfo.DeviceType);
                    Assert.AreEqual(AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.CreateDeviceModel.DeviceState, response.DeviceInfo.DeviceState);
                    Assert.AreEqual(AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.CreateDeviceModel.DeviceUID, response.DeviceInfo.DeviceUID);

                    //Verify Account Info
                    Assert.AreEqual(2, response.AccountInfo.Count());
                    foreach (AccountInfo accountInfo in response.AccountInfo)
                    {
                        if (accountInfo.CustomerType == "Customer")
                        {
                            Assert.AreEqual(CustomerModel.CustomerUID, accountInfo.CustomerUID);
                            Assert.AreEqual(CustomerModel.CustomerName, accountInfo.CustomerName);
                            Assert.AreEqual(DealerModel.CustomerUID, accountInfo.ParentCustomerUID);
                            Assert.AreEqual(DealerModel.CustomerName, accountInfo.ParentName);
                            Assert.AreEqual(DealerModel.CustomerType, accountInfo.ParentCustomerType);
                        }
                        else if (accountInfo.CustomerType == "Dealer")
                        {
                            Assert.AreEqual(DealerModel.CustomerUID, accountInfo.CustomerUID);
                            Assert.AreEqual(DealerModel.CustomerName, accountInfo.CustomerName);
                            Assert.AreEqual(DealerModel.CustomerUID, accountInfo.ParentCustomerUID);
                            Assert.AreEqual(DealerModel.CustomerName, accountInfo.ParentName);
                            Assert.AreEqual(DealerModel.CustomerType, accountInfo.ParentCustomerType);
                        }
                    }
                    break;

                case "Customer":
                     try
                    {
                        responseList = JsonConvert.DeserializeObject<List<AssetDetailResponseModel>>(ResponseString);
                    }
                    catch (Exception e)
                    {
                        LogResult.Report(Log, "log_ForError", "Got Error While DeSerializing JSON Object", e);
                        throw new Exception("Got Error While DeSerializing JSON Object");
                    }
                    Assert.AreEqual(1, responseList.Count());
                    response = responseList[0];

                    //Verify Asset Info
                    Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetName, response.AssetInfo.AssetName);
                    Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.SerialNumber, response.AssetInfo.SerialNumber);
                    Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.MakeCode, response.AssetInfo.MakeCode);
                    Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.Model, response.AssetInfo.Model);
                    Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetType, response.AssetInfo.AssetType);
                    Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.ModelYear, response.AssetInfo.ModelYear);
                    Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetUID, response.AssetInfo.AssetUID);

                    //Verify Device Info
                    Assert.AreEqual(AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.CreateDeviceModel.DeviceSerialNumber, response.DeviceInfo.DeviceSerialNumber);
                    Assert.AreEqual(AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.CreateDeviceModel.DeviceType, response.DeviceInfo.DeviceType);
                    Assert.AreEqual(AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.CreateDeviceModel.DeviceState, response.DeviceInfo.DeviceState);
                    Assert.AreEqual(AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.CreateDeviceModel.DeviceUID, response.DeviceInfo.DeviceUID);

                    //Verify Account Info
                    Assert.AreEqual(1, response.AccountInfo.Count());
                    foreach (AccountInfo accountInfo in response.AccountInfo)
                    {
                        Assert.AreEqual(CustomerModel.CustomerUID, accountInfo.CustomerUID);
                        Assert.AreEqual(CustomerModel.CustomerName, accountInfo.CustomerName);
                        Assert.AreEqual(CustomerModel.CustomerType, accountInfo.CustomerType);
                        Assert.AreEqual(null, accountInfo.ParentCustomerUID);
                        Assert.AreEqual(null, accountInfo.ParentName);
                        Assert.AreEqual(null, accountInfo.ParentCustomerType);
                    }
                    break;

                case "Dealer":
                     try
                    {
                        responseList = JsonConvert.DeserializeObject<List<AssetDetailResponseModel>>(ResponseString);
                    }
                    catch (Exception e)
                    {
                        LogResult.Report(Log, "log_ForError", "Got Error While DeSerializing JSON Object", e);
                        throw new Exception("Got Error While DeSerializing JSON Object");
                    }
                    Assert.AreEqual(1, responseList.Count());
                    response = responseList[0];

                    //Verify Asset Info
                    Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetName, response.AssetInfo.AssetName);
                    Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.SerialNumber, response.AssetInfo.SerialNumber);
                    Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.MakeCode, response.AssetInfo.MakeCode);
                    Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.Model, response.AssetInfo.Model);
                    Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetType, response.AssetInfo.AssetType);
                    Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.ModelYear, response.AssetInfo.ModelYear);
                    Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetUID, response.AssetInfo.AssetUID);

                    //Verify Device Info
                    Assert.AreEqual(AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.CreateDeviceModel.DeviceSerialNumber, response.DeviceInfo.DeviceSerialNumber);
                    Assert.AreEqual(AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.CreateDeviceModel.DeviceType, response.DeviceInfo.DeviceType);
                    Assert.AreEqual(AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.CreateDeviceModel.DeviceState, response.DeviceInfo.DeviceState);
                    Assert.AreEqual(AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.CreateDeviceModel.DeviceUID, response.DeviceInfo.DeviceUID);

                    //Verify Account Info
                    Assert.AreEqual(1, response.AccountInfo.Count());
                    foreach (AccountInfo accountInfo in response.AccountInfo)
                    {
                        Assert.AreEqual(DealerModel.CustomerUID, accountInfo.CustomerUID);
                        Assert.AreEqual(DealerModel.CustomerName, accountInfo.CustomerName);
                        Assert.AreEqual(DealerModel.CustomerType, accountInfo.CustomerType);
                        Assert.AreEqual(DealerModel.CustomerUID, accountInfo.ParentCustomerUID);
                        Assert.AreEqual(DealerModel.CustomerName, accountInfo.ParentName);
                        Assert.AreEqual(DealerModel.CustomerType, accountInfo.ParentCustomerType);
                    }
                    break;

                default:
                    break;
            }
        }

        public void VerifyResponse_Subscription(string verifyParameter)
        {
            List<AssetDetailResponseModel> responseList = new List<AssetDetailResponseModel>();
            AssetDetailResponseModel response = new AssetDetailResponseModel();

            try
            {
                responseList = JsonConvert.DeserializeObject<List<AssetDetailResponseModel>>(ResponseString);
            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While DeSerializing JSON Object", e);
                throw new Exception("Got Error While DeSerializing JSON Object");
            }

            Assert.AreEqual(1, responseList.Count());
            response = responseList[0];

            //Verify Asset Info
            Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetName, response.AssetInfo.AssetName);
            Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.SerialNumber, response.AssetInfo.SerialNumber);
            Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.MakeCode, response.AssetInfo.MakeCode);
            Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.Model, response.AssetInfo.Model);
            Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetType, response.AssetInfo.AssetType);
            Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.ModelYear, response.AssetInfo.ModelYear);
            Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetUID, response.AssetInfo.AssetUID);

            //Verify Device Info
            Assert.AreEqual(AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.CreateDeviceModel.DeviceSerialNumber, response.DeviceInfo.DeviceSerialNumber);
            Assert.AreEqual(AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.CreateDeviceModel.DeviceType, response.DeviceInfo.DeviceType);
            Assert.AreEqual(AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.CreateDeviceModel.DeviceState, response.DeviceInfo.DeviceState);
            Assert.AreEqual(AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.CreateDeviceModel.DeviceUID, response.DeviceInfo.DeviceUID);

            //Verify Account Info
            Assert.AreEqual(2, response.AccountInfo.Count());
            foreach (AccountInfo accountInfo in response.AccountInfo)
            {
                if (accountInfo.CustomerType == "Customer")
                {
                    Assert.AreEqual(CustomerModel.CustomerUID, accountInfo.CustomerUID);
                    Assert.AreEqual(CustomerModel.CustomerName, accountInfo.CustomerName);
                    Assert.AreEqual(DealerModel.CustomerUID, accountInfo.ParentCustomerUID);
                    Assert.AreEqual(DealerModel.CustomerName, accountInfo.ParentName);
                    Assert.AreEqual(DealerModel.CustomerType, accountInfo.ParentCustomerType);
                }
                else if (accountInfo.CustomerType == "Dealer")
                {
                    Assert.AreEqual(DealerModel.CustomerUID, accountInfo.CustomerUID);
                    Assert.AreEqual(DealerModel.CustomerName, accountInfo.CustomerName);
                    Assert.AreEqual(DealerModel.CustomerUID, accountInfo.ParentCustomerUID);
                    Assert.AreEqual(DealerModel.CustomerName, accountInfo.ParentName);
                    Assert.AreEqual(DealerModel.CustomerType, accountInfo.ParentCustomerType);
                }
            }

            switch (verifyParameter)
            {
                case "OneActive_Customer":
                    //Verify Subscription Info
                    Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetUID, response.Subscription.AssetUID);
                    Assert.AreEqual("Active", response.Subscription.SubscriptionStatus);
                    Assert.AreEqual(1, response.Subscription.OwnersVisibility.Count());

                    foreach (OwnersVisibility ownersVisibility in response.Subscription.OwnersVisibility)
                    {
                        Assert.AreEqual(AssetDetailsServiceSteps.defaultValidAssetSubscriptionModel.CustomerUID, ownersVisibility.CustomerUID);
                        Assert.AreEqual(CustomerModel.CustomerName, ownersVisibility.CustomerName);
                        Assert.AreEqual(CustomerModel.CustomerType, ownersVisibility.CustomerType);
                        Assert.AreEqual(AssetDetailsServiceSteps.defaultValidAssetSubscriptionModel.SubscriptionUID, ownersVisibility.SubscriptionUID);
                        Assert.AreEqual(AssetDetailsServiceSteps.defaultValidAssetSubscriptionModel.SubscriptionType, ownersVisibility.SubscriptionName);
                        Assert.AreEqual("Active", ownersVisibility.SubscriptionStatus);
                        Assert.AreEqual(AssetDetailsServiceSteps.defaultValidAssetSubscriptionModel.StartDate.ToString(), ownersVisibility.SubscriptionStartDate.ToString());
                        Assert.AreEqual(AssetDetailsServiceSteps.defaultValidAssetSubscriptionModel.EndDate.ToString(), ownersVisibility.SubscriptionEndDate.ToString());
                    }
                    break;

                case "OneActive_Dealer":
                    //Verify Subscription Info
                    Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetUID, response.Subscription.AssetUID);
                    Assert.AreEqual("Active", response.Subscription.SubscriptionStatus);
                    Assert.AreEqual(1, response.Subscription.OwnersVisibility.Count());

                    foreach (OwnersVisibility ownersVisibility in response.Subscription.OwnersVisibility)
                    {
                        Assert.AreEqual(AssetDetailsServiceSteps.defaultValidAssetSubscriptionModel.CustomerUID, ownersVisibility.CustomerUID);
                        Assert.AreEqual(DealerModel.CustomerName, ownersVisibility.CustomerName);
                        Assert.AreEqual(DealerModel.CustomerType, ownersVisibility.CustomerType);
                        Assert.AreEqual(AssetDetailsServiceSteps.defaultValidAssetSubscriptionModel.SubscriptionUID, ownersVisibility.SubscriptionUID);
                        Assert.AreEqual(AssetDetailsServiceSteps.defaultValidAssetSubscriptionModel.SubscriptionType, ownersVisibility.SubscriptionName);
                        Assert.AreEqual("Active", ownersVisibility.SubscriptionStatus);
                        Assert.AreEqual(AssetDetailsServiceSteps.defaultValidAssetSubscriptionModel.StartDate.ToString(), ownersVisibility.SubscriptionStartDate.ToString());
                        Assert.AreEqual(AssetDetailsServiceSteps.defaultValidAssetSubscriptionModel.EndDate.ToString(), ownersVisibility.SubscriptionEndDate.ToString());
                    }
                    break;

                case "OneInactive_Customer":
                    //Verify Subscription Info
                    Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetUID, response.Subscription.AssetUID);
                    Assert.AreEqual("InActive", response.Subscription.SubscriptionStatus);
                    Assert.AreEqual(1, response.Subscription.OwnersVisibility.Count());

                    foreach (OwnersVisibility ownersVisibility in response.Subscription.OwnersVisibility)
                    {
                        Assert.AreEqual(AssetDetailsServiceSteps.defaultValidAssetSubscriptionModel.CustomerUID, ownersVisibility.CustomerUID);
                        Assert.AreEqual(CustomerModel.CustomerName, ownersVisibility.CustomerName);
                        Assert.AreEqual(CustomerModel.CustomerType, ownersVisibility.CustomerType);
                        Assert.AreEqual(AssetDetailsServiceSteps.defaultValidAssetSubscriptionModel.SubscriptionUID, ownersVisibility.SubscriptionUID);
                        Assert.AreEqual(AssetDetailsServiceSteps.defaultValidAssetSubscriptionModel.SubscriptionType, ownersVisibility.SubscriptionName);
                        Assert.AreEqual("InActive", ownersVisibility.SubscriptionStatus);
                        Assert.AreEqual(AssetDetailsServiceSteps.defaultValidAssetSubscriptionModel.StartDate.ToString(), ownersVisibility.SubscriptionStartDate.ToString());
                        Assert.AreEqual(AssetDetailsServiceSteps.defaultValidAssetSubscriptionModel.EndDate.ToString(), ownersVisibility.SubscriptionEndDate.ToString());
                    }
                    break;

                case "OneInactive_Dealer":
                    //Verify Subscription Info
                    Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetUID, response.Subscription.AssetUID);
                    Assert.AreEqual("InActive", response.Subscription.SubscriptionStatus);
                    Assert.AreEqual(1, response.Subscription.OwnersVisibility.Count());

                    foreach (OwnersVisibility ownersVisibility in response.Subscription.OwnersVisibility)
                    {
                        Assert.AreEqual(AssetDetailsServiceSteps.defaultValidAssetSubscriptionModel.CustomerUID, ownersVisibility.CustomerUID);
                        Assert.AreEqual(DealerModel.CustomerName, ownersVisibility.CustomerName);
                        Assert.AreEqual(DealerModel.CustomerType, ownersVisibility.CustomerType);
                        Assert.AreEqual(AssetDetailsServiceSteps.defaultValidAssetSubscriptionModel.SubscriptionUID, ownersVisibility.SubscriptionUID);
                        Assert.AreEqual(AssetDetailsServiceSteps.defaultValidAssetSubscriptionModel.SubscriptionType, ownersVisibility.SubscriptionName);
                        Assert.AreEqual("InActive", ownersVisibility.SubscriptionStatus);
                        Assert.AreEqual(AssetDetailsServiceSteps.defaultValidAssetSubscriptionModel.StartDate.ToString(), ownersVisibility.SubscriptionStartDate.ToString());
                        Assert.AreEqual(AssetDetailsServiceSteps.defaultValidAssetSubscriptionModel.EndDate.ToString(), ownersVisibility.SubscriptionEndDate.ToString());
                    }
                    break;

                case "TwoActive":
                    //Verify Subscription Info
                    Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetUID, response.Subscription.AssetUID);
                    Assert.AreEqual("Active", response.Subscription.SubscriptionStatus);
                    Assert.AreEqual(2, response.Subscription.OwnersVisibility.Count());

                    foreach (OwnersVisibility ownersVisibility in response.Subscription.OwnersVisibility)
                    {
                        if (ownersVisibility.CustomerType == "Customer")
                        {
                            Assert.AreEqual(FirstAssetSubscriptionModel.CustomerUID, ownersVisibility.CustomerUID);
                            Assert.AreEqual(CustomerModel.CustomerName, ownersVisibility.CustomerName);
                            Assert.AreEqual(FirstAssetSubscriptionModel.SubscriptionUID, ownersVisibility.SubscriptionUID);
                            Assert.AreEqual(FirstAssetSubscriptionModel.SubscriptionType, ownersVisibility.SubscriptionName);
                            Assert.AreEqual("Active", ownersVisibility.SubscriptionStatus);
                            Assert.AreEqual(FirstAssetSubscriptionModel.StartDate.ToString(), ownersVisibility.SubscriptionStartDate.ToString());
                            Assert.AreEqual(FirstAssetSubscriptionModel.EndDate.ToString(), ownersVisibility.SubscriptionEndDate.ToString());
                        }
                        else if (ownersVisibility.CustomerType == "Dealer")
                        {
                            Assert.AreEqual(AssetSubscriptionModel.CustomerUID, ownersVisibility.CustomerUID);
                            Assert.AreEqual(DealerModel.CustomerName, ownersVisibility.CustomerName);
                            Assert.AreEqual(AssetSubscriptionModel.SubscriptionUID, ownersVisibility.SubscriptionUID);
                            Assert.AreEqual(AssetSubscriptionModel.SubscriptionType, ownersVisibility.SubscriptionName);
                            Assert.AreEqual("Active", ownersVisibility.SubscriptionStatus);
                            Assert.AreEqual(AssetSubscriptionModel.StartDate.ToString(), ownersVisibility.SubscriptionStartDate.ToString());
                            Assert.AreEqual(AssetSubscriptionModel.EndDate.ToString(), ownersVisibility.SubscriptionEndDate.ToString());
                        }
                    }
                    break;

                case "TwoInactive":
                    //Verify Subscription Info
                    Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetUID, response.Subscription.AssetUID);
                    Assert.AreEqual("InActive", response.Subscription.SubscriptionStatus);
                    Assert.AreEqual(2, response.Subscription.OwnersVisibility.Count());

                    foreach (OwnersVisibility ownersVisibility in response.Subscription.OwnersVisibility)
                    {
                        if (ownersVisibility.CustomerType == "Customer")
                        {
                            Assert.AreEqual(FirstAssetSubscriptionModel.CustomerUID, ownersVisibility.CustomerUID);
                            Assert.AreEqual(CustomerModel.CustomerName, ownersVisibility.CustomerName);
                            Assert.AreEqual(FirstAssetSubscriptionModel.SubscriptionUID, ownersVisibility.SubscriptionUID);
                            Assert.AreEqual(FirstAssetSubscriptionModel.SubscriptionType, ownersVisibility.SubscriptionName);
                            Assert.AreEqual("InActive", ownersVisibility.SubscriptionStatus);
                            Assert.AreEqual(FirstAssetSubscriptionModel.StartDate.ToString(), ownersVisibility.SubscriptionStartDate.ToString());
                            Assert.AreEqual(FirstAssetSubscriptionModel.EndDate.ToString(), ownersVisibility.SubscriptionEndDate.ToString());
                        }
                        else if (ownersVisibility.CustomerType == "Dealer")
                        {
                            Assert.AreEqual(AssetSubscriptionModel.CustomerUID, ownersVisibility.CustomerUID);
                            Assert.AreEqual(DealerModel.CustomerName, ownersVisibility.CustomerName);
                            Assert.AreEqual(AssetSubscriptionModel.SubscriptionUID, ownersVisibility.SubscriptionUID);
                            Assert.AreEqual(AssetSubscriptionModel.SubscriptionType, ownersVisibility.SubscriptionName);
                            Assert.AreEqual("InActive", ownersVisibility.SubscriptionStatus);
                            Assert.AreEqual(AssetSubscriptionModel.StartDate.ToString(), ownersVisibility.SubscriptionStartDate.ToString());
                            Assert.AreEqual(AssetSubscriptionModel.EndDate.ToString(), ownersVisibility.SubscriptionEndDate.ToString());
                        }
                    }
                    break;

                case "OneActiveOneInactive":
                    //Verify Subscription Info
                    Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetUID, response.Subscription.AssetUID);
                    Assert.AreEqual("Active", response.Subscription.SubscriptionStatus);
                    Assert.AreEqual(2, response.Subscription.OwnersVisibility.Count());

                    foreach (OwnersVisibility ownersVisibility in response.Subscription.OwnersVisibility)
                    {
                        if (ownersVisibility.CustomerType == "Customer")
                        {
                            Assert.AreEqual(FirstAssetSubscriptionModel.CustomerUID, ownersVisibility.CustomerUID);
                            Assert.AreEqual(CustomerModel.CustomerName, ownersVisibility.CustomerName);
                            Assert.AreEqual(FirstAssetSubscriptionModel.SubscriptionUID, ownersVisibility.SubscriptionUID);
                            Assert.AreEqual(FirstAssetSubscriptionModel.SubscriptionType, ownersVisibility.SubscriptionName);
                            Assert.AreEqual("Active", ownersVisibility.SubscriptionStatus);
                            Assert.AreEqual(FirstAssetSubscriptionModel.StartDate.ToString(), ownersVisibility.SubscriptionStartDate.ToString());
                            Assert.AreEqual(FirstAssetSubscriptionModel.EndDate.ToString(), ownersVisibility.SubscriptionEndDate.ToString());
                        }
                        else if (ownersVisibility.CustomerType == "Dealer")
                        {
                            Assert.AreEqual(AssetSubscriptionModel.CustomerUID, ownersVisibility.CustomerUID);
                            Assert.AreEqual(DealerModel.CustomerName, ownersVisibility.CustomerName);
                            Assert.AreEqual(AssetSubscriptionModel.SubscriptionUID, ownersVisibility.SubscriptionUID);
                            Assert.AreEqual(AssetSubscriptionModel.SubscriptionType, ownersVisibility.SubscriptionName);
                            Assert.AreEqual("InActive", ownersVisibility.SubscriptionStatus);
                            Assert.AreEqual(AssetSubscriptionModel.StartDate.ToString(), ownersVisibility.SubscriptionStartDate.ToString());
                            Assert.AreEqual(AssetSubscriptionModel.EndDate.ToString(), ownersVisibility.SubscriptionEndDate.ToString());
                        }
                    }
                    break;

                default:
                    break;
            }
        }

        public void VerifyErrorResponse(string verifyParameter)
        {
            AssetDetailErrorResponseModel response = new AssetDetailErrorResponseModel();
            try
            {
                response = JsonConvert.DeserializeObject<AssetDetailErrorResponseModel>(ResponseString);
            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While DeSerializing JSON Object", e);
                throw new Exception("Got Error While DeSerializing JSON Object");
            }

            switch(verifyParameter)
            {
                case "ERR_Message_EMPTY":
                    Assert.AreEqual("AssetUID/DeviceUID has not been provided", response.Message);
                    break;

                case "ERR_Message_DeviceUID":
                    Assert.AreEqual("The request is invalid.", response.Message);
                    Assert.AreEqual("The value 'Invalid' is not valid for Nullable`1.", response.ModelState.deviceUID[0]);
                    break;

                case "ERR_Message_AssetUID":
                    Assert.AreEqual("The request is invalid.", response.Message);
                    Assert.AreEqual("The value 'Invalid' is not valid for Nullable`1.", response.ModelState.assetUID[0]);
                    break;

                case "ERR_Message_AssetDeviceUID":
                    Assert.AreEqual("The request is invalid.", response.Message);
                    Assert.AreEqual("The value 'Invalid' is not valid for Nullable`1.", response.ModelState.assetUID[0]);
                    Assert.AreEqual("The value 'Invalid' is not valid for Nullable`1.", response.ModelState.deviceUID[0]);
                    break;

                default:
                    break;
            }
        }

        public void VerifyResponse_AssetAndDevice()
        {
            List<AssetDetailResponseModel> response = new List<AssetDetailResponseModel>();

            try
            {
                response = JsonConvert.DeserializeObject<List<AssetDetailResponseModel>>(ResponseString);
            }
            catch (Exception e)
            {
                LogResult.Report(Log, "log_ForError", "Got Error While DeSerializing JSON Object", e);
                throw new Exception("Got Error While DeSerializing JSON Object");
            }

            Assert.AreEqual(2, response.Count());

            foreach (AssetDetailResponseModel assetDetailResponse in response)
            {

                if (assetDetailResponse.AssetInfo.AssetUID == AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetUID)
                {
                    //Verify Asset Info
                    Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetName, assetDetailResponse.AssetInfo.AssetName);
                    Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.SerialNumber, assetDetailResponse.AssetInfo.SerialNumber);
                    Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.MakeCode, assetDetailResponse.AssetInfo.MakeCode);
                    Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.Model, assetDetailResponse.AssetInfo.Model);
                    Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetType, assetDetailResponse.AssetInfo.AssetType);
                    Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.ModelYear, assetDetailResponse.AssetInfo.ModelYear);
                    Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetUID, assetDetailResponse.AssetInfo.AssetUID);

                    //Verify Device Info
                    Assert.AreEqual(AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.CreateDeviceModel.DeviceSerialNumber, assetDetailResponse.DeviceInfo.DeviceSerialNumber);
                    Assert.AreEqual(AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.CreateDeviceModel.DeviceType, assetDetailResponse.DeviceInfo.DeviceType);
                    Assert.AreEqual(AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.CreateDeviceModel.DeviceState, assetDetailResponse.DeviceInfo.DeviceState);
                    Assert.AreEqual(AssetDeviceSearchServiceSteps.assetDeviceSearchServiceSupport.CreateDeviceModel.DeviceUID, assetDetailResponse.DeviceInfo.DeviceUID);

                    //Verify Account Info
                    Assert.AreEqual(2, assetDetailResponse.AccountInfo.Count());
                    foreach (AccountInfo accountInfo in assetDetailResponse.AccountInfo)
                    {
                        if (accountInfo.CustomerType == "Customer")
                        {
                            Assert.AreEqual(CustomerModel.CustomerUID, accountInfo.CustomerUID);
                            Assert.AreEqual(CustomerModel.CustomerName, accountInfo.CustomerName);
                            Assert.AreEqual(DealerModel.CustomerUID, accountInfo.ParentCustomerUID);
                            Assert.AreEqual(DealerModel.CustomerName, accountInfo.ParentName);
                            Assert.AreEqual(DealerModel.CustomerType, accountInfo.ParentCustomerType);
                        }
                        else if (accountInfo.CustomerType == "Dealer")
                        {
                            Assert.AreEqual(DealerModel.CustomerUID, accountInfo.CustomerUID);
                            Assert.AreEqual(DealerModel.CustomerName, accountInfo.CustomerName);
                            Assert.AreEqual(DealerModel.CustomerUID, accountInfo.ParentCustomerUID);
                            Assert.AreEqual(DealerModel.CustomerName, accountInfo.ParentName);
                            Assert.AreEqual(DealerModel.CustomerType, accountInfo.ParentCustomerType);
                        }
                    }

                    //Verify Subscription Info
                    Assert.AreEqual(AssetServiceSteps.assetServiceSupport.CreateAssetModel.AssetUID, assetDetailResponse.Subscription.AssetUID);
                    Assert.AreEqual("Active", assetDetailResponse.Subscription.SubscriptionStatus);
                    Assert.AreEqual(1, assetDetailResponse.Subscription.OwnersVisibility.Count());

                    foreach (OwnersVisibility ownersVisibility in assetDetailResponse.Subscription.OwnersVisibility)
                    {
                        Assert.AreEqual(AssetSubscriptionModel.CustomerUID, ownersVisibility.CustomerUID);
                        if (AssetSubscriptionModel.CustomerUID == DealerModel.CustomerUID)
                        {
                            Assert.AreEqual(DealerModel.CustomerType, ownersVisibility.CustomerType);
                        }
                        else
                        {
                            Assert.AreEqual(CustomerModel.CustomerType, ownersVisibility.CustomerType);
                        }
                        Assert.AreEqual(AssetSubscriptionModel.SubscriptionUID, ownersVisibility.SubscriptionUID);
                        Assert.AreEqual(AssetSubscriptionModel.SubscriptionType, ownersVisibility.SubscriptionName);
                        Assert.AreEqual("Active", ownersVisibility.SubscriptionStatus);
                        Assert.AreEqual(AssetSubscriptionModel.StartDate.ToString(), ownersVisibility.SubscriptionStartDate.ToString());
                        Assert.AreEqual(AssetSubscriptionModel.EndDate.ToString(), ownersVisibility.SubscriptionEndDate.ToString());
                    }
                }
                else
                {
                    //Verify Asset Info
                    Assert.AreEqual(FirstAssetModel.AssetName, assetDetailResponse.AssetInfo.AssetName);
                    Assert.AreEqual(FirstAssetModel.SerialNumber, assetDetailResponse.AssetInfo.SerialNumber);
                    Assert.AreEqual(FirstAssetModel.MakeCode, assetDetailResponse.AssetInfo.MakeCode);
                    Assert.AreEqual(FirstAssetModel.Model, assetDetailResponse.AssetInfo.Model);
                    Assert.AreEqual(FirstAssetModel.AssetType, assetDetailResponse.AssetInfo.AssetType);
                    Assert.AreEqual(FirstAssetModel.ModelYear, assetDetailResponse.AssetInfo.ModelYear);
                    Assert.AreEqual(FirstAssetModel.AssetUID, assetDetailResponse.AssetInfo.AssetUID);

                    //Verify Device Info
                    Assert.AreEqual(FirstDeviceModel.DeviceSerialNumber, assetDetailResponse.DeviceInfo.DeviceSerialNumber);
                    Assert.AreEqual(FirstDeviceModel.DeviceType, assetDetailResponse.DeviceInfo.DeviceType);
                    Assert.AreEqual(FirstDeviceModel.DeviceState, assetDetailResponse.DeviceInfo.DeviceState);
                    Assert.AreEqual(FirstDeviceModel.DeviceUID, assetDetailResponse.DeviceInfo.DeviceUID);

                    //Verify Account Info
                    Assert.AreEqual(2, assetDetailResponse.AccountInfo.Count());
                    foreach (AccountInfo accountInfo in assetDetailResponse.AccountInfo)
                    {
                        if (accountInfo.CustomerType == "Customer")
                        {
                            Assert.AreEqual(FirstCustomerModel.CustomerUID, accountInfo.CustomerUID);
                            Assert.AreEqual(FirstCustomerModel.CustomerName, accountInfo.CustomerName);
                            Assert.AreEqual(FirstDealerModel.CustomerUID, accountInfo.ParentCustomerUID);
                            Assert.AreEqual(FirstDealerModel.CustomerName, accountInfo.ParentName);
                            Assert.AreEqual(FirstDealerModel.CustomerType, accountInfo.ParentCustomerType);
                        }
                        else if (accountInfo.CustomerType == "Dealer")
                        {
                            Assert.AreEqual(FirstDealerModel.CustomerUID, accountInfo.CustomerUID);
                            Assert.AreEqual(FirstDealerModel.CustomerName, accountInfo.CustomerName);
                            Assert.AreEqual(FirstDealerModel.CustomerUID, accountInfo.ParentCustomerUID);
                            Assert.AreEqual(FirstDealerModel.CustomerName, accountInfo.ParentName);
                            Assert.AreEqual(FirstDealerModel.CustomerType, accountInfo.ParentCustomerType);
                        }
                    }

                    //Verify Subscription Info
                    Assert.AreEqual(FirstAssetSubscriptionModel.AssetUID, assetDetailResponse.Subscription.AssetUID);
                    Assert.AreEqual("Active", assetDetailResponse.Subscription.SubscriptionStatus);
                    Assert.AreEqual(1, assetDetailResponse.Subscription.OwnersVisibility.Count());

                    foreach (OwnersVisibility ownersVisibility in assetDetailResponse.Subscription.OwnersVisibility)
                    {
                        Assert.AreEqual(FirstAssetSubscriptionModel.CustomerUID, ownersVisibility.CustomerUID);
                        if (FirstAssetSubscriptionModel.CustomerUID == FirstDealerModel.CustomerUID)
                        {
                            Assert.AreEqual(FirstDealerModel.CustomerType, ownersVisibility.CustomerType);
                        }
                        else
                        {
                            Assert.AreEqual(FirstCustomerModel.CustomerType, ownersVisibility.CustomerType);
                        }
                        Assert.AreEqual(FirstAssetSubscriptionModel.SubscriptionUID, ownersVisibility.SubscriptionUID);
                        Assert.AreEqual(FirstAssetSubscriptionModel.SubscriptionType, ownersVisibility.SubscriptionName);
                        Assert.AreEqual("Active", ownersVisibility.SubscriptionStatus);
                        Assert.AreEqual(FirstAssetSubscriptionModel.StartDate.ToString(), ownersVisibility.SubscriptionStartDate.ToString());
                        Assert.AreEqual(FirstAssetSubscriptionModel.EndDate.ToString(), ownersVisibility.SubscriptionEndDate.ToString());
                    }
                }
            }
        }
    }
}