using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading;
using TestUtility;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;
using ContractExecutionStatesEnum = VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling.ContractExecutionStatesEnum;

namespace WebApiTests
{
  [TestClass]
  public class AssetWebTests
  {
    private readonly Msg msg = new Msg();
    [TestMethod]
    public void Createadevicetype3Andassetwithradioserial()
    {
      msg.Title("Asset WebTest 1", "Inject device type3 with assetId (radio serial), call webAPI to get asset Id");
      var ts = new TestSupport {IsPublishToKafka = false};
      var legacyAssetId = ts.SetLegacyAssetId();
      var eventArray = new[] {
       "| TableName | EventDate   | AssetUID      | LegacyAssetID   | Name            | MakeCode | SerialNumber | Model | IconKey | AssetType  | OwningCustomerUID |",
      $"| Asset     | 0d+09:00:00 | {ts.AssetUid} | {legacyAssetId} | Asset WebTest 1 | CAT      | XAT1         | 345D  | 10      | Excavators |                   |"};
      ts.PublishEventCollection(eventArray);

      var deviceUid = Guid.NewGuid();
      var deviceEventArray = new[] {
       "| TableName   | EventDate   | DeviceSerialNumber | DeviceState | DeviceType              | DeregisteredUTC | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | fk_AssetUID   | fk_DeviceUID |",
      $"| Device      | 0d+09:00:00 | {deviceUid}        | Subscribed  | {DeviceTypeEnum.SNM940} | 0d+09:00:00     | {deviceUid} | CDMA         | Asset WebTest 1           |               |              |",
      $"| AssetDevice | 0d+09:05:00 |                    |             |                         |                 |             |              |                           | {ts.AssetUid} | {deviceUid}  |"};
      ts.PublishEventCollection(deviceEventArray);
      var actualResult = CallWebApiGetAssetId(ts,-1,6,deviceUid.ToString());
      Assert.AreEqual(legacyAssetId, actualResult.assetId, " Legacy asset id's do not match");
      Assert.AreEqual(true, actualResult.Result , " result of request doesn't match expected");
    }

    [TestMethod]
    public void CallwebApItogetassetIdfornonexistentradioserial()
    {
      msg.Title("Asset WebTest 2", "Call webAPI to get asset Id for non existent rado serial");
      var ts = new TestSupport { IsPublishToKafka = false };
      var deviceUid = Guid.NewGuid();
      var actualResult = CallWebApiGetAssetId(ts,-1,6,deviceUid.ToString());
      Assert.AreEqual(-1, actualResult.assetId, " Legacy asset id's do not match");
      Assert.AreEqual(false, actualResult.Result , " result of request doesn't match expected");
    }

    [TestMethod]
    public void Createadevicetype3Andassetwithradioserialwithownercustomerandman3Dsubscription()
    {
      msg.Title("Asset WebTest 3", "Inject Man3D PM sub, device type3 with assetId(radio serial) WITH OWNERCUSTOMER, call webAPI to get asset Id");
      var ts = new TestSupport { IsPublishToKafka = false };
      var legacyAssetId = ts.SetLegacyAssetId();
      var customerUid = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDate = ts.FirstEventDate.ToString("yyyy-MM-dd");
      var endDate = new DateTime(9999, 12, 31).ToString("yyyy-MM-dd");
      var deviceUid = Guid.NewGuid();
      // Write events to database 
      var eventsArray = new[] {
        "| TableName    | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | ",
       $"| Customer     | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                | 15               |             |                | ",
       $"| Subscription | 0d+09:00:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 15               | {startDate} | {endDate}      | "};
      ts.PublishEventCollection(eventsArray);
      var assetEventArray = new[] {
       "| TableName | EventDate   | AssetUID      | LegacyAssetID   | Name           | MakeCode | SerialNumber | Model | IconKey | AssetType  | OwningCustomerUID |",
      $"| Asset     | 0d+09:00:00 | {ts.AssetUid} | {legacyAssetId} | AssetWebTest3  | CAT      | XAT1         | 345D  | 10      | Excavators | {customerUid}     |"};
      ts.PublishEventCollection(assetEventArray);
      var deviceEventArray = new[] {
       "| TableName   | EventDate   | DeviceSerialNumber | DeviceState | DeviceType              | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | fk_AssetUID   | fk_DeviceUID |",
      $"| Device      | 0d+09:00:00 | {deviceUid}        | Subscribed  | {DeviceTypeEnum.SNM940} | {deviceUid} | CDMA         | Asset WebTest 3           |               |              |",
      $"| AssetDevice | 0d+09:20:00 |                    |             |                         |             |              |                           | {ts.AssetUid} | {deviceUid}  |"};
      ts.PublishEventCollection(deviceEventArray);
      //Call Web api
      var actualResult = CallWebApiGetAssetId(ts,-1,6,deviceUid.ToString());
      Assert.AreEqual(legacyAssetId, actualResult.assetId, " Legacy asset id's do not match");
      // Note : 15 ng machine level but 18 is current gen machine level
      Assert.AreEqual(18, actualResult.machineLevel, " Machine levels do not match ");
      Assert.AreEqual(true, actualResult.Result , " result of request doesn't match expected");
    }

    [TestMethod]
    public void Createadevicetype3AndassetwithradioserialwithNoownercustomerandman3Dsubscription()
    {
      msg.Title("Asset WebTest 4", "Inject Man3D PM sub, device type3 with assetId(radio serial) NO OWNERCUSTOMER, call webAPI to get asset Id");
      var ts = new TestSupport { IsPublishToKafka = false };
      var legacyAssetId = ts.SetLegacyAssetId();
      var customerUid = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDate = ts.FirstEventDate.ToString("yyyy-MM-dd");
      var endDate = new DateTime(9999, 12, 31).ToString("yyyy-MM-dd");
      var deviceUid = Guid.NewGuid();
      // Write events to database 
      var eventsArray = new[] {
        "| TableName         | EventDate   | fk_AssetUID   | fk_SubscriptionUID | EffectiveDate | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | ",
       $"| Subscription      | 0d+09:00:00 |               |                    |               | {subscriptionUid} | {customerUid}  | 15               | {startDate} | {endDate}      | "};
      ts.PublishEventCollection(eventsArray);
      var assetEventArray = new[] {
       "| TableName | EventDate   | AssetUID      | LegacyAssetID   | Name           | MakeCode | SerialNumber | Model | IconKey | AssetType  | OwningCustomerUID |",
      $"| Asset     | 0d+09:00:00 | {ts.AssetUid} | {legacyAssetId} | AssetWebTest4  | CAT      | XAT1         | 345D  | 10      | Excavators |                   |"};
      ts.PublishEventCollection(assetEventArray);
      var deviceEventArray = new[] {
       "| TableName   | EventDate   | DeviceSerialNumber | DeviceState | DeviceType               | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | fk_AssetUID   | fk_DeviceUID |",
      $"| Device      | 0d+09:00:00 | {deviceUid}        | Subscribed  | {DeviceTypeEnum.SNM940}  | {deviceUid} | CDMA         | Asset WebTest 4           |               |              |",
      $"| AssetDevice | 0d+09:20:00 |                    |             |                          |             |              |                           | {ts.AssetUid} | {deviceUid}  |"};
      ts.PublishEventCollection(deviceEventArray);
      //Call Web api
      var actualResult = CallWebApiGetAssetId(ts,-1,6,deviceUid.ToString());
      Assert.AreEqual(legacyAssetId, actualResult.assetId, " Legacy asset id's do not match");
      Assert.AreEqual(0, actualResult.machineLevel, " Machine levels do not match ");
      Assert.AreEqual(true, actualResult.Result , " result of request doesn't match expected");
    }

    [TestMethod]
    public void Inject3Dpmsubdevicetype3WithassetIdradioserialcallwebApItogetassetId()
    {
      msg.Title("Asset WebTest 5", "Inject 3D PM sub, device type3 with assetId (radio serial), call webAPI to get asset Id");
      var ts = new TestSupport { IsPublishToKafka = false };
      var legacyAssetId = ts.SetLegacyAssetId();
      var customerUid = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDate = ts.FirstEventDate.ToString("yyyy-MM-dd");
      var endDate = new DateTime(9999, 12, 31).ToString("yyyy-MM-dd");
      var deviceUid = Guid.NewGuid();
      // Write events to database 
      var eventsArray = new[] {
        "| TableName         | EventDate   | fk_AssetUID   | fk_SubscriptionUID | EffectiveDate | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | ",
       $"| AssetSubscription | 0d+09:00:00 | {ts.AssetUid} | {subscriptionUid}  | {startDate}   |                   |                | 13               |             |                | ",
       $"| Subscription      | 0d+09:00:00 |               |                    |               | {subscriptionUid} | {customerUid}  | 13               | {startDate} | {endDate}      | "};
      ts.PublishEventCollection(eventsArray);
      var assetEventArray = new[] {
       "| TableName | EventDate   | AssetUID      | LegacyAssetID   | Name           | MakeCode | SerialNumber | Model | IconKey | AssetType  | OwningCustomerUID |",
      $"| Asset     | 0d+09:00:00 | {ts.AssetUid} | {legacyAssetId} | AssetWebTest5  | CAT      | XAT1         | 345D  | 10      | Excavators |                   |"};
      ts.PublishEventCollection(assetEventArray);
      var deviceEventArray = new[] {
       "| TableName   | EventDate   | DeviceSerialNumber | DeviceState | DeviceType              | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | fk_AssetUID   | fk_DeviceUID |",
      $"| Device      | 0d+09:00:00 | {deviceUid}        | Subscribed  | {DeviceTypeEnum.SNM940} | {deviceUid} | CDMA         | Asset WebTest 4           |               |              |",
      $"| AssetDevice | 0d+09:20:00 |                    |             |                         |             |              |                           | {ts.AssetUid} | {deviceUid}  |"};
      ts.PublishEventCollection(deviceEventArray);
      //Call Web api
      var actualResult = CallWebApiGetAssetId(ts,-1,6,deviceUid.ToString());
      Assert.AreEqual(legacyAssetId, actualResult.assetId, " Legacy asset id's do not match");
      Assert.AreEqual(16, actualResult.machineLevel, " Machine levels do not match ");
      Assert.AreEqual(true, actualResult.Result , " result of request doesn't match expected");
    }

    [TestMethod]
    public void Inject3DpmAndMaunual3DSubdevicetype3WithassetIdradioserialcallwebApItogetassetId()
    {
      msg.Title("Asset WebTest 6", "Inject 3D PM sub and a Manual 3D subscription, device type3 with assetId (radio serial), call webAPI to get asset Id");
      var ts = new TestSupport { IsPublishToKafka = false };
      var legacyAssetId = ts.SetLegacyAssetId();
      var customerUid = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var subscriptionUid2 = Guid.NewGuid();
      var startDate = ts.FirstEventDate.ToString("yyyy-MM-dd");
      var endDate = new DateTime(9999, 12, 31).ToString("yyyy-MM-dd");
      var deviceUid = Guid.NewGuid();
      // Write events to database 
      var eventsArray = new[] {
        "| TableName         | EventDate   | fk_AssetUID   | fk_SubscriptionUID | EffectiveDate | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | ",
       $"| AssetSubscription | 0d+09:00:00 | {ts.AssetUid} | {subscriptionUid}  | {startDate}   |                   |                | 13               |             |                | ",
       $"| Subscription      | 0d+09:00:00 |               |                    |               | {subscriptionUid} | {customerUid}  | 13               | {startDate} | {endDate}      | ",
       $"| Subscription      | 0d+09:01:00 |               |                    |               | {subscriptionUid2}| {customerUid}  | 15               | {startDate} | {endDate}      | "};

      ts.PublishEventCollection(eventsArray);
      var assetEventArray = new[] {
       "| TableName | EventDate   | AssetUID      | LegacyAssetID   | Name           | MakeCode | SerialNumber | Model | IconKey | AssetType  | OwningCustomerUID |",
      $"| Asset     | 0d+09:00:00 | {ts.AssetUid} | {legacyAssetId} | AssetWebTest6  | CAT      | XAT1         | 345D  | 10      | Excavators |                   |"};
      ts.PublishEventCollection(assetEventArray);
      var deviceEventArray = new[] {
       "| TableName   | EventDate   | DeviceSerialNumber | DeviceState | DeviceType              | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | fk_AssetUID   | fk_DeviceUID |",
      $"| Device      | 0d+09:00:00 | {deviceUid}        | Subscribed  | {DeviceTypeEnum.SNM940} | {deviceUid} | CDMA         | Asset WebTest 4           |               |              |",
      $"| AssetDevice | 0d+09:20:00 |                    |             |                         |             |              |                           | {ts.AssetUid} | {deviceUid}  |"};
      ts.PublishEventCollection(deviceEventArray);
      //Call Web api
      var actualResult = CallWebApiGetAssetId(ts,-1,6,deviceUid.ToString());
      Assert.AreEqual(legacyAssetId, actualResult.assetId, " Legacy asset id's do not match");
      Assert.AreEqual(16, actualResult.machineLevel, " Machine levels do not match ");
      Assert.AreEqual(true, actualResult.Result , " result of request doesn't match expected");
    }

    [TestMethod]
    public void Inject3DpMsubMan3DpMand3Dpmsubdevicetype3WithassetIdradioserialcallwebApItogetassetId()
    {
      msg.Title("Asset WebTest 7", "Inject Project with no asset, device or subscription call webAPI with project id but no device to get asset Id");
      var ts = new TestSupport { IsPublishToKafka = false };
      var legacyProjectId = ts.SetLegacyProjectId();
      var projectUid = Guid.NewGuid();
      var startDate = ts.FirstEventDate.ToString("yyyy-MM-dd");
      var endDate = new DateTime(9999, 12, 31).ToString("yyyy-MM-dd");
      var geometryWKT = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      // Write events to database 
      var assetEventArray = new[] {
       "| TableName | EventDate   | ProjectUID   | LegacyProjectID   | Name          | fk_ProjectTypeID | ProjectTimeZone           | LandfillTimeZone | StartDate   | EndDate   | GeometryWKT   |",
      $"| Project   | 0d+09:00:00 | {projectUid} | {legacyProjectId} | AssetWebTest7 | 2                | New Zealand Standard Time | Pacific/Auckland | {startDate} | {endDate} | {geometryWKT} |" };
      ts.PublishEventCollection(assetEventArray);
      //Call Web api
      var actualResult = CallWebApiGetAssetId(ts,legacyProjectId,0,"");
      Assert.AreEqual(-1, actualResult.assetId, " Legacy asset id's do not match");
      Assert.AreEqual(0, actualResult.machineLevel, " Machine levels do not match ");
      Assert.AreEqual(false, actualResult.Result , " result of request doesn't match expected");
    }

    [TestMethod]
    public void InjectProjectandcustomernoassetdeviceorsubscriptioncallwebApIwithprojectidbutnodevicetogetassetId()
    {
      msg.Title("Asset WebTest 8", "Inject Project and customer. no asset, device or subscription call webAPI with project id but no device to get asset Id");
      var ts = new TestSupport { IsPublishToKafka = false };
      var legacyProjectId = ts.SetLegacyProjectId();
      var projectUid = Guid.NewGuid();
      var customerUid = Guid.NewGuid();
      var startDate = ts.FirstEventDate.ToString("yyyy-MM-dd");
      var endDate = new DateTime(9999, 12, 31).ToString("yyyy-MM-dd");
      var geometryWKT = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      // Write events to database 
      var projectEventArray = new[] {
       "| TableName | EventDate   | ProjectUID   | LegacyProjectID   | Name          | fk_ProjectTypeID | ProjectTimeZone           | LandfillTimeZone | StartDate   | EndDate   | GeometryWKT   |",
      $"| Project   | 0d+09:00:00 | {projectUid} | {legacyProjectId} | AssetWebTest8 | 2                | New Zealand Standard Time | Pacific/Auckland | {startDate} | {endDate} | {geometryWKT} |" };
      ts.PublishEventCollection(projectEventArray);
      var eventsArray = new[] {
        "| TableName       | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID |",
       $"| Customer        | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                | 15               |             |                |               |",
       $"| CustomerProject | 0d+09:20:00 |               |           |                   |                   | {customerUid}  |                  |             |                | {projectUid}  |"};
      ts.PublishEventCollection(eventsArray);
      //Call Web api
      var actualResult = CallWebApiGetAssetId(ts,legacyProjectId,0,"");
      Assert.AreEqual(-1, actualResult.assetId, " Legacy asset id's do not match");
      Assert.AreEqual(0, actualResult.machineLevel, " Machine levels do not match ");
      Assert.AreEqual(false, actualResult.Result , " result of request doesn't match expected");
    }


    [TestMethod]
    public void InjectProjectCustomerandManual3DpmSubscriptionNoassetdevicecallwebApIwithprojectidbutnodevicetogetassetId()
    {
      msg.Title("Asset WebTest 9", "Inject Project, Customer and Manual 3D pm Subscription. No asset, device. call webAPI with project id but no device to get asset Id");
      var ts = new TestSupport { IsPublishToKafka = false };
      var legacyProjectId = ts.SetLegacyProjectId();
      var projectUid = Guid.NewGuid();
      var customerUid = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDate = ts.FirstEventDate.ToString("yyyy-MM-dd");
      var endDate = new DateTime(9999, 12, 31).ToString("yyyy-MM-dd");
      var geometryWKT = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      // Write events to database 
      var projectEventArray = new[] {
       "| TableName | EventDate   | ProjectUID   | LegacyProjectID   | Name          | fk_ProjectTypeID | ProjectTimeZone           | LandfillTimeZone | StartDate   | EndDate   | GeometryWKT   |",
      $"| Project   | 0d+09:00:00 | {projectUid} | {legacyProjectId} | AssetWebTest9 | 2                | New Zealand Standard Time | Pacific/Auckland | {startDate} | {endDate} | {geometryWKT} |" };
      ts.PublishEventCollection(projectEventArray);
      var eventsArray = new[] {
        "| TableName       | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID |",
       $"| Customer        | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                | 15               |             |                |               |",
       $"| Subscription    | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 15               | {startDate} | {endDate}      |               |",
       $"| CustomerProject | 0d+09:20:00 |               |           |                   |                   | {customerUid}  |                  |             |                | {projectUid}  |"};
      ts.PublishEventCollection(eventsArray);
      //Call Web api
      var actualResult = CallWebApiGetAssetId(ts,legacyProjectId,0,"");
      Assert.AreEqual(-1, actualResult.assetId, " Legacy asset id's do not match");
      Assert.AreEqual(18, actualResult.machineLevel, " Machine levels do not match ");
      Assert.AreEqual(true, actualResult.Result , " result of request doesn't match expected");
    }

    [TestMethod]
    public void InjectProjectCustomerManual3DpmSubscriptionAssetanddevicecallwebApIwithprojectid()
    {
      msg.Title("Asset WebTest 10", "Inject Project, Customer,Manual 3D pm Subscription,Asset and device. call webAPI with project id.");
      var ts = new TestSupport { IsPublishToKafka = false };
      var legacyProjectId = ts.SetLegacyProjectId();
      var legacyAssetId = ts.SetLegacyAssetId();
      var projectUid = Guid.NewGuid();
      var customerUid = Guid.NewGuid();
      var deviceUid = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDate = ts.FirstEventDate.ToString("yyyy-MM-dd");
      var endDate = new DateTime(9999, 12, 31).ToString("yyyy-MM-dd");
      var geometryWKT = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      // Write events to database 
      var projectEventArray = new[] {
       "| TableName | EventDate   | ProjectUID   | LegacyProjectID   | Name          | fk_ProjectTypeID | ProjectTimeZone           | LandfillTimeZone | StartDate   | EndDate   | GeometryWKT   |",
      $"| Project   | 0d+09:00:00 | {projectUid} | {legacyProjectId} | AssetWebTest10| 2                | New Zealand Standard Time | Pacific/Auckland | {startDate} | {endDate} | {geometryWKT} |" };
      ts.PublishEventCollection(projectEventArray);
      var eventsArray = new[] {
        "| TableName       | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID |",
       $"| Customer        | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                | 15               |             |                |               |",
       $"| Subscription    | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 15               | {startDate} | {endDate}      |               |",
       $"| CustomerProject | 0d+09:20:00 |               |           |                   |                   | {customerUid}  |                  |             |                | {projectUid}  |"};
      ts.PublishEventCollection(eventsArray);
      var assetEventArray = new[] {
       "| TableName | EventDate   | AssetUID      | LegacyAssetID   | Name           | MakeCode | SerialNumber | Model | IconKey | AssetType  | OwningCustomerUID |",
      $"| Asset     | 0d+09:00:00 | {ts.AssetUid} | {legacyAssetId} | AssetWebTest4  | CAT      | XAT1         | 345D  | 10      | Excavators | {customerUid}     |"};
      ts.PublishEventCollection(assetEventArray);
      var deviceEventArray = new[] {
       "| TableName   | EventDate   | DeviceSerialNumber | DeviceState | DeviceType              | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | fk_AssetUID   | fk_DeviceUID |",
      $"| Device      | 0d+09:00:00 | {deviceUid}        | Subscribed  | {DeviceTypeEnum.SNM940} | {deviceUid} | CDMA         | Asset WebTest 10          |               |              |",
      $"| AssetDevice | 0d+09:20:00 |                    |             |                         |             |              |                           | {ts.AssetUid} | {deviceUid}  |"};
      ts.PublishEventCollection(deviceEventArray);
      //Call Web api
      var actualResult = CallWebApiGetAssetId(ts,legacyProjectId,6,deviceUid.ToString());
      Assert.AreEqual(legacyAssetId, actualResult.assetId, " Legacy asset id's do not match");
      Assert.AreEqual(18, actualResult.machineLevel, " Machine levels do not match ");
      Assert.AreEqual(true, actualResult.Result , " result of request doesn't match expected");
    }

    [TestMethod]
    public void ValidateAssetIdMessages1()
    {
      msg.Title("Asset WebTest 11", "Validate error message : Must have assetId and or projectID ");
      var ts = new TestSupport { IsPublishToKafka = false };
      var legacyProjectId = ts.SetLegacyProjectId();
      var projectUid = Guid.NewGuid();
      var customerUid = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDate = ts.FirstEventDate.ToString("yyyy-MM-dd");
      var endDate = new DateTime(9999, 12, 31).ToString("yyyy-MM-dd");
      var geometryWKT = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      var projectEventArray = new[] {
       "| TableName | EventDate   | ProjectUID   | LegacyProjectID   | Name          | fk_ProjectTypeID | ProjectTimeZone           | LandfillTimeZone | StartDate   | EndDate   | GeometryWKT   |",
      $"| Project   | 0d+09:00:00 | {projectUid} | {legacyProjectId} | AssetWebTest11| 2                | New Zealand Standard Time | Pacific/Auckland | {startDate} | {endDate} | {geometryWKT} |" };
      ts.PublishEventCollection(projectEventArray);
      var eventsArray = new[] {
        "| TableName       | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID |",
       $"| Customer        | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                | 15               |             |                |               |",
       $"| Subscription    | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 15               | {startDate} | {endDate}      |               |",
       $"| CustomerProject | 0d+09:20:00 |               |           |                   |                   | {customerUid}  |                  |             |                | {projectUid}  |"};
      ts.PublishEventCollection(eventsArray);

      var request = GetAssetIdRequest.CreateGetAssetIdRequest(0,6,"");
      var requestJson = JsonConvert.SerializeObject(request, ts.jsonSettings);
      var restClient = new RestClient();
      var uri = ts.GetBaseUri() + "api/v1/asset/getId";
      var method = HttpMethod.Post.ToString();
      var response = restClient.DoHttpRequest(uri, method, requestJson, System.Net.HttpStatusCode.BadRequest);
      var actualResult = JsonConvert.DeserializeObject<GetAssetIdResult>(response, ts.jsonSettings);
      Assert.AreEqual("Must have assetId and/or projectID", actualResult.Message, " result message from web api does not match expected");
      Assert.AreEqual(ContractExecutionStatesEnum.ValidationError, actualResult.Code, "code from web api does not match expected");
    }

    [TestMethod]
    public void ValidateAssetIdMessages2()
    {
      msg.Title("Asset WebTest 12", "Validate error message :AssetId must have valid deviceType ");
      var ts = new TestSupport { IsPublishToKafka = false };
      var legacyProjectId = ts.SetLegacyProjectId();
      var projectUid = Guid.NewGuid();
      var customerUid = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDate = ts.FirstEventDate.ToString("yyyy-MM-dd");
      var endDate = new DateTime(9999, 12, 31).ToString("yyyy-MM-dd");
      var geometryWKT = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      var projectEventArray = new[] {
       "| TableName | EventDate   | ProjectUID   | LegacyProjectID   | Name           | fk_ProjectTypeID | ProjectTimeZone           | LandfillTimeZone | StartDate   | EndDate   | GeometryWKT   |",
      $"| Project   | 0d+09:00:00 | {projectUid} | {legacyProjectId} | AssetWebTest12 | 2                | New Zealand Standard Time | Pacific/Auckland | {startDate} | {endDate} | {geometryWKT} |" };
      ts.PublishEventCollection(projectEventArray);
      var eventsArray = new[] {
        "| TableName       | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID |",
       $"| Customer        | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                | 13               |             |                |               |",
       $"| Subscription    | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 13               | {startDate} | {endDate}      |               |",
       $"| CustomerProject | 0d+09:20:00 |               |           |                   |                   | {customerUid}  |                  |             |                | {projectUid}  |"};
      ts.PublishEventCollection(eventsArray);

      var request = GetAssetIdRequest.CreateGetAssetIdRequest(legacyProjectId,1500, "AAA");
      var requestJson = JsonConvert.SerializeObject(request, ts.jsonSettings);
      var restClient = new RestClient();
      var uri = ts.GetBaseUri() + "api/v1/asset/getId";
      var method = HttpMethod.Post.ToString();
      var response = restClient.DoHttpRequest(uri, method, requestJson, System.Net.HttpStatusCode.BadRequest);
      var actualResult = JsonConvert.DeserializeObject<GetAssetIdResult>(response, ts.jsonSettings);
      Assert.AreEqual("AssetId must have valid deviceType", actualResult.Message, " result message from web api does not match expected");
      Assert.AreEqual(ContractExecutionStatesEnum.ValidationError, actualResult.Code, "code from web api does not match expected");
    }


    /// <summary>
    /// Call the web api and return the response
    /// </summary>
    /// <param name="ts">testsupport instance</param>
    /// <param name="projectId">project id</param>
    /// <param name="deviceType">device type number</param>
    /// <param name="radioSerial">device UID</param>
    /// <returns></returns>
    private GetAssetIdResult CallWebApiGetAssetId(TestSupport ts,long projectId, int deviceType, string radioSerial)
    {
      Thread.Sleep(500);
      var request = GetAssetIdRequest.CreateGetAssetIdRequest(projectId,deviceType, radioSerial);
      var requestJson = JsonConvert.SerializeObject(request, ts.jsonSettings);
      var restClient = new RestClient();
      var uri = ts.GetBaseUri() + "api/v1/asset/getId";
      var method = HttpMethod.Post.ToString();
      var response = restClient.DoHttpRequest(uri, method, requestJson);
      var actualResult = JsonConvert.DeserializeObject<GetAssetIdResult>(response, ts.jsonSettings);
      return actualResult;
    }
  }
}
