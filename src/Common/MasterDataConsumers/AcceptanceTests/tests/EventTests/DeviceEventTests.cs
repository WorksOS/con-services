using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtility;

namespace EventTests
{
  [TestClass]
  public class DeviceEventTests
  {

    [TestMethod]
    public void Inject_A_Minimum_Create_Device_Event()
    {
      var msg = new Msg();
      var ts = new TestSupport { IsPublishToKafka = true };
      var mysql = new MySqlHelper();
      var deviceUid = Guid.NewGuid();
      msg.Title("Device Event 1", "Create Device Event ");
      var eventArray = new[] {
         "| EventType         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID   |",
        $"| CreateDeviceEvent | 0d+09:00:00 | {deviceUid}        | Subscribed  | SNM940     | {deviceUid} | "};

      ts.PublishEventCollection(eventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Device", "DeviceUID", 1, deviceUid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Device", "DeviceUID", "DeviceSerialNumber, DeviceState, DeviceType, DeviceUID", $"{deviceUid},Subscribed,SNM940,{deviceUid}", deviceUid);
    }

    [TestMethod]
    public void Inject_A_Full_Create_Device_Event()
    {
      var msg = new Msg();
      var ts = new TestSupport { IsPublishToKafka = true };
      var mysql = new MySqlHelper();
      var deviceUid = Guid.NewGuid();
      var deregisteredDate = ts.ConvertTimeStampAndDayOffSetToDateTime("1d+12:00:00", ts.FirstEventDate);
      msg.Title("Device Event 2", "Create Device Event ");
      var eventArray = new[] {
         "| EventType         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID   | DeregisteredUTC    | DataLinkType | GatewayFirmwarePartNumber | MainboardSoftwareVersion | ModuleType | RadioFirmwarePartNumber |",
        $"| CreateDeviceEvent | 0d+09:00:00 | {deviceUid}        | Subscribed  | SNM940     | {deviceUid} | {deregisteredDate} | CDMA         | Device Event 2            | 3.54                     | modtyp     | 88                      |"};

      ts.PublishEventCollection(eventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Device", "DeviceUID", 1, deviceUid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Device", "DeviceUID", "DeviceSerialNumber, DeviceState, DeviceType, DeviceUID,  DeregisteredUTC, DataLinkType, GatewayFirmwarePartNumber, MainboardSoftwareVersion, ModuleType, RadioFirmwarePartNumber", 
        $"{deviceUid},Subscribed,SNM940,{deviceUid},{deregisteredDate},CDMA,Device Event 2,3.54,modtyp,88",deviceUid);
    }

    [TestMethod]
    public void Create_Then_Update_Device_State()
    {
      var msg = new Msg();
      var ts = new TestSupport { IsPublishToKafka = true };
      var mysql = new MySqlHelper();
      var deviceUid = Guid.NewGuid();
      msg.Title("Device Event 3", "Create Device Event ");
      var eventArray = new[] {
         "| EventType         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | MainboardSoftwareVersion | RadioFirmwarePartNumber |",
        $"| CreateDeviceEvent | 0d+09:00:00 | {deviceUid}        | Subscribed  | SNM940     | {deviceUid} | CDMA         | Device Event 3            | 3.54                     | 88                      |"};

      ts.PublishEventCollection(eventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Device", "DeviceUID", 1, deviceUid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Device", "DeviceUID", "DeviceSerialNumber, DeviceState, DeviceType, DeviceUID,  DataLinkType, GatewayFirmwarePartNumber, MainboardSoftwareVersion, RadioFirmwarePartNumber",
        $"{deviceUid},Subscribed,SNM940,{deviceUid},CDMA,Device Event 3,3.54,88", deviceUid);

      var updateEventArray = new[] {
         "| EventType         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | MainboardSoftwareVersion | RadioFirmwarePartNumber |",
        $"| UpdateDeviceEvent | 0d+09:00:00 | {deviceUid}        | Deregistered| SNM940     | {deviceUid} | CDMA         | Device Event 3            | 3.54                     | 88                      |"};

      ts.PublishEventCollection(updateEventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Device", "DeviceUID", 1, deviceUid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Device", "DeviceUID", "DeviceSerialNumber, DeviceState, DeviceType, DeviceUID,  DataLinkType, GatewayFirmwarePartNumber, MainboardSoftwareVersion, RadioFirmwarePartNumber",
        $"{deviceUid},Deregistered,SNM940,{deviceUid},CDMA,Device Event 3,3.54,88", deviceUid);
    }

    [TestMethod]
    public void Create_Then_Update_Device_Type()
    {
      var msg = new Msg();
      var ts = new TestSupport { IsPublishToKafka = true };
      var mysql = new MySqlHelper();
      var deviceUid = Guid.NewGuid();
      msg.Title("Device Event 4", "Create Device Event ");
      var eventArray = new[] {
         "| EventType         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | MainboardSoftwareVersion | RadioFirmwarePartNumber |",
        $"| CreateDeviceEvent | 0d+09:00:00 | {deviceUid}        | Subscribed  | SNM940     | {deviceUid} | CDMA         | Device Event 4            | 3.54                     |88                       |"};

      ts.PublishEventCollection(eventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Device", "DeviceUID", 1, deviceUid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Device", "DeviceUID", "DeviceSerialNumber, DeviceState, DeviceType, DeviceUID, DataLinkType, GatewayFirmwarePartNumber, MainboardSoftwareVersion, RadioFirmwarePartNumber",
        $"{deviceUid},Subscribed,SNM940,{deviceUid},CDMA,Device Event 4,3.54,88", deviceUid);

      var updateEventArray = new[] {
         "| EventType         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | MainboardSoftwareVersion | RadioFirmwarePartNumber |",
        $"| UpdateDeviceEvent | 0d+09:00:00 | {deviceUid}        | Subscribed  | SNM941     | {deviceUid} | CDMA         | Device Event 4            | 3.54                     | 88                      |"};

      ts.PublishEventCollection(updateEventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Device", "DeviceUID", 1, deviceUid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Device", "DeviceUID", "DeviceSerialNumber, DeviceState, DeviceType, DeviceUID, DataLinkType, GatewayFirmwarePartNumber, MainboardSoftwareVersion, RadioFirmwarePartNumber",
        $"{deviceUid},Subscribed,SNM941,{deviceUid},CDMA,Device Event 4,3.54,88", deviceUid);
    }


    [TestMethod]
    public void Create_Then_Update_Device_UID()
    {
      var msg = new Msg();
      var ts = new TestSupport { IsPublishToKafka = true };
      var mysql = new MySqlHelper();
      var deviceUid = Guid.NewGuid();
      msg.Title("Device Event 5", "Create Device Event ");
      var eventArray = new[] {
         "| EventType         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | MainboardSoftwareVersion | RadioFirmwarePartNumber |",
        $"| CreateDeviceEvent | 0d+09:00:00 | {deviceUid}        | Subscribed  | SNM940     | {deviceUid} | CDMA         | Device Event 5            | 3.54                     | 88                      |"};

      ts.PublishEventCollection(eventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Device", "DeviceUID", 1, deviceUid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Device", "DeviceUID", "DeviceSerialNumber, DeviceState, DeviceType, DeviceUID,  DataLinkType, GatewayFirmwarePartNumber, MainboardSoftwareVersion, RadioFirmwarePartNumber",
        $"{deviceUid},Subscribed,SNM940,{deviceUid},CDMA,Device Event 5,3.54,88", deviceUid);

      var deviceUid2 = Guid.NewGuid();
      var updateEventArray = new[] {
         "| EventType         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID    | DataLinkType | GatewayFirmwarePartNumber | MainboardSoftwareVersion | RadioFirmwarePartNumber |",
        $"| UpdateDeviceEvent | 0d+09:00:00 | {deviceUid}        | Subscribed  | SNM940     | {deviceUid2} | CDMA         | Device Event 5            | 3.54                     | 88                      |"};

      ts.PublishEventCollection(updateEventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Device", "DeviceUID", 1, deviceUid2);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Device", "DeviceUID", "DeviceSerialNumber, DeviceState, DeviceType, DeviceUID,  DataLinkType, GatewayFirmwarePartNumber, MainboardSoftwareVersion, RadioFirmwarePartNumber",
        $"{deviceUid},Subscribed,SNM940,{deviceUid2},CDMA,Device Event 5,3.54,88", deviceUid2);
    }

    [TestMethod]
    public void Create_Then_Update_Device_Deregister()
    {
      var msg = new Msg();
      var ts = new TestSupport { IsPublishToKafka = true };
      var mysql = new MySqlHelper();
      var deviceUid = Guid.NewGuid();
      var deregisteredDate = ts.ConvertTimeStampAndDayOffSetToDateTime("1d+12:00:00", ts.FirstEventDate);
      msg.Title("Device Event 6", "Create Device Event ");
      var eventArray = new[] {
         "| EventType         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | MainboardSoftwareVersion | RadioFirmwarePartNumber |",
        $"| CreateDeviceEvent | 0d+09:00:00 | {deviceUid}        | Subscribed  | SNM940     | {deviceUid} | CDMA         | Device Event 6            | 3.54                     | 88                      |"};

      ts.PublishEventCollection(eventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Device", "DeviceUID", 1, deviceUid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Device", "DeviceUID", "DeviceSerialNumber, DeviceState, DeviceType, DeviceUID,  DeregisteredUTC, DataLinkType, GatewayFirmwarePartNumber, MainboardSoftwareVersion, RadioFirmwarePartNumber",
        $"{deviceUid},Subscribed,SNM940,{deviceUid},,CDMA,Device Event 6 ,3.54,88", deviceUid);

      var updateEventArray = new[] {
         "| EventType         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID   | DeregisteredUTC    | DataLinkType | GatewayFirmwarePartNumber | MainboardSoftwareVersion | RadioFirmwarePartNumber |",
        $"| UpdateDeviceEvent | 0d+09:00:00 | {deviceUid}        | Subscribed  | SNM940     | {deviceUid} | {deregisteredDate} | CDMA         | Device Event 6            | 3.54                     | 88                      |"};

      ts.PublishEventCollection(updateEventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Device", "DeviceUID", 1, deviceUid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Device", "DeviceUID", "DeviceSerialNumber, DeviceState, DeviceType, DeviceUID,  DeregisteredUTC, DataLinkType, GatewayFirmwarePartNumber, MainboardSoftwareVersion, RadioFirmwarePartNumber",
        $"{deviceUid},Subscribed,SNM940,{deviceUid},{deregisteredDate},CDMA,Device Event 6 ,3.54,88", deviceUid);
    }


    [TestMethod]
    public void Create_Then_Update_Device_DataLinkType()
    {
      var msg = new Msg();
      var ts = new TestSupport { IsPublishToKafka = true };
      var mysql = new MySqlHelper();
      var deviceUid = Guid.NewGuid();
      msg.Title("Device Event 7", "Create Device Event ");
      var eventArray = new[] {
         "| EventType         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | MainboardSoftwareVersion | RadioFirmwarePartNumber |",
        $"| CreateDeviceEvent | 0d+09:00:00 | {deviceUid}        | Subscribed  | SNM940     | {deviceUid} | CDMA         | Device Event 7            | 3.54                     | 88                      |"};

      ts.PublishEventCollection(eventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Device", "DeviceUID", 1, deviceUid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Device", "DeviceUID", "DeviceSerialNumber, DeviceState, DeviceType, DeviceUID, DataLinkType, GatewayFirmwarePartNumber, MainboardSoftwareVersion, RadioFirmwarePartNumber",
        $"{deviceUid},Subscribed,SNM940,{deviceUid},CDMA,Device Event 7,3.54,88", deviceUid);

      var updateEventArray = new[] {
         "| EventType         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | MainboardSoftwareVersion | RadioFirmwarePartNumber |",
        $"| UpdateDeviceEvent | 0d+09:00:00 | {deviceUid}        | Subscribed  | SNM940     | {deviceUid} | 4G           | Device Event 7            | 3.54                     | 88                      |"};

      ts.PublishEventCollection(updateEventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Device", "DeviceUID", 1, deviceUid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Device", "DeviceUID", "DeviceSerialNumber, DeviceState, DeviceType, DeviceUID,  DataLinkType, GatewayFirmwarePartNumber, MainboardSoftwareVersion, RadioFirmwarePartNumber",
        $"{deviceUid},Subscribed,SNM940,{deviceUid},4G,Device Event 7,3.54,88", deviceUid);
    }


    [TestMethod]
    public void Create_Then_Update_Device_GateFirmwarePartNumber()
    {
      var msg = new Msg();
      var ts = new TestSupport { IsPublishToKafka = true };
      var mysql = new MySqlHelper();
      var deviceUid = Guid.NewGuid();
      msg.Title("Device Event 8", "Create Device Event ");
      var eventArray = new[] {
         "| EventType         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | MainboardSoftwareVersion | RadioFirmwarePartNumber |",
        $"| CreateDeviceEvent | 0d+09:00:00 | {deviceUid}        | Subscribed  | SNM940     | {deviceUid} | CDMA         | Device Event 8            | 3.54                     | 88                      |"};

      ts.PublishEventCollection(eventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Device", "DeviceUID", 1, deviceUid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Device", "DeviceUID", "DeviceSerialNumber, DeviceState, DeviceType, DeviceUID, DataLinkType, GatewayFirmwarePartNumber, MainboardSoftwareVersion, RadioFirmwarePartNumber",
        $"{deviceUid},Subscribed,SNM940,{deviceUid},CDMA,Device Event 8,3.54,88", deviceUid);


      var updateEventArray = new[] {
         "| EventType         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | MainboardSoftwareVersion | RadioFirmwarePartNumber |",
        $"| UpdateDeviceEvent | 0d+09:00:00 | {deviceUid}        | Subscribed  | SNM940     | {deviceUid} | CDMA         | Device Event 8 - Update   | 3.54                     | 88                      |"};

      ts.PublishEventCollection(updateEventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Device", "DeviceUID", 1, deviceUid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Device", "DeviceUID", "DeviceSerialNumber, DeviceState, DeviceType, DeviceUID, DataLinkType, GatewayFirmwarePartNumber, MainboardSoftwareVersion, RadioFirmwarePartNumber",
        $"{deviceUid},Subscribed,SNM940,{deviceUid},CDMA,Device Event 8 - Update,3.54,88", deviceUid);
    }

    [TestMethod]
    public void Create_Then_Update_Device_MainBoardSoftWareVersion()
    {
      var msg = new Msg();
      var ts = new TestSupport { IsPublishToKafka = true };
      var mysql = new MySqlHelper();
      var deviceUid = Guid.NewGuid();
      msg.Title("Device Event 9", "Create Device Event ");
      var eventArray = new[] {
         "| EventType         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | MainboardSoftwareVersion | RadioFirmwarePartNumber |",
        $"| CreateDeviceEvent | 0d+09:00:00 | {deviceUid}        | Subscribed  | SNM940     | {deviceUid} | CDMA         | Device Event 9            | 3.54                     | 88                      |"};

      ts.PublishEventCollection(eventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Device", "DeviceUID", 1, deviceUid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Device", "DeviceUID", "DeviceSerialNumber, DeviceState, DeviceType, DeviceUID, DataLinkType, GatewayFirmwarePartNumber, MainboardSoftwareVersion, RadioFirmwarePartNumber",
        $"{deviceUid},Subscribed,SNM940,{deviceUid},CDMA,Device Event 9,3.54,88", deviceUid);

      var updateEventArray = new[] {
         "| EventType         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | MainboardSoftwareVersion | RadioFirmwarePartNumber |",
        $"| UpdateDeviceEvent | 0d+09:00:00 | {deviceUid}        | Subscribed  | SNM940     | {deviceUid} | CDMA         | Device Event 9            | 500                      |  88                      |"};
       
      ts.PublishEventCollection(updateEventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Device", "DeviceUID", 1, deviceUid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Device", "DeviceUID", "DeviceSerialNumber, DeviceState, DeviceType, DeviceUID, DataLinkType, GatewayFirmwarePartNumber, MainboardSoftwareVersion, RadioFirmwarePartNumber",
        $"{deviceUid},Subscribed,SNM940,{deviceUid},CDMA,Device Event 9,500,88", deviceUid);
    }

    [TestMethod]
    public void Create_Then_Update_Device_ModuleType()
    {
      var msg = new Msg();
      var ts = new TestSupport { IsPublishToKafka = true };
      var mysql = new MySqlHelper();
      var deviceUid = Guid.NewGuid();
      msg.Title("Device Event 10", "Create Device Event ");
      var eventArray = new[] {
         "| EventType         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | MainboardSoftwareVersion | ModuleType | RadioFirmwarePartNumber |",
        $"| CreateDeviceEvent | 0d+09:00:00 | {deviceUid}        | Subscribed  | SNM940     | {deviceUid} | CDMA         | Device Event 10           | 3.54                     | modtyp     | 88                      |"};

      ts.PublishEventCollection(eventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Device", "DeviceUID", 1, deviceUid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Device", "DeviceUID", "DeviceSerialNumber, DeviceState, DeviceType, DeviceUID, DataLinkType, GatewayFirmwarePartNumber, MainboardSoftwareVersion, ModuleType, RadioFirmwarePartNumber",
        $"{deviceUid},Subscribed,SNM940,{deviceUid},CDMA,Device Event 10,3.54,modtyp,88", deviceUid);

      var updateEventArray = new[] {
         "| EventType         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | MainboardSoftwareVersion | ModuleType | RadioFirmwarePartNumber |",
        $"| UpdateDeviceEvent | 0d+09:00:00 | {deviceUid}        | Subscribed  | SNM940     | {deviceUid} | CDMA         | Device Event 10           | 3.54                     | updmod     | 88                      |"};

      ts.PublishEventCollection(updateEventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Device", "DeviceUID", 1, deviceUid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Device", "DeviceUID", "DeviceSerialNumber, DeviceState, DeviceType, DeviceUID, DataLinkType, GatewayFirmwarePartNumber, MainboardSoftwareVersion, ModuleType, RadioFirmwarePartNumber",
        $"{deviceUid},Subscribed,SNM940,{deviceUid},CDMA,Device Event 10,3.54,updmod,88", deviceUid);
    }

    [TestMethod]
    public void Create_Then_Update_Device_RadioFirmwarePartNumber()
    {
      var msg = new Msg();
      var ts = new TestSupport { IsPublishToKafka = true };
      var mysql = new MySqlHelper();
      var deviceUid = Guid.NewGuid();
      msg.Title("Device Event 11", "Create Device Event ");
      var eventArray = new[] {
         "| EventType         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | MainboardSoftwareVersion | RadioFirmwarePartNumber |",
        $"| CreateDeviceEvent | 0d+09:00:00 | {deviceUid}        | Subscribed  | SNM940     | {deviceUid} | CDMA         | Device Event 11           | 3.54                     | 88                      |"};

      ts.PublishEventCollection(eventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Device", "DeviceUID", 1, deviceUid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Device", "DeviceUID", "DeviceSerialNumber, DeviceState, DeviceType, DeviceUID, DataLinkType, GatewayFirmwarePartNumber, MainboardSoftwareVersion, RadioFirmwarePartNumber",
        $"{deviceUid},Subscribed,SNM940,{deviceUid},CDMA,Device Event 11,3.54,88", deviceUid);

      var updateEventArray = new[] {
         "| EventType         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | MainboardSoftwareVersion | RadioFirmwarePartNumber |",
        $"| UpdateDeviceEvent | 0d+09:00:00 | {deviceUid}        | Subscribed  | SNM940     | {deviceUid} | CDMA         | Device Event 11           | 3.54                     | 99                      |"};

      ts.PublishEventCollection(updateEventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Device", "DeviceUID", 1, deviceUid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Device", "DeviceUID", "DeviceSerialNumber, DeviceState, DeviceType, DeviceUID, DataLinkType, GatewayFirmwarePartNumber, MainboardSoftwareVersion,RadioFirmwarePartNumber",
        $"{deviceUid},Subscribed,SNM940,{deviceUid},CDMA,Device Event 11,3.54,99", deviceUid);
    }

    [TestMethod]
    public void Try_Create_Different_Device_Same_Guid()
    {
      var msg = new Msg();
      var ts = new TestSupport { IsPublishToKafka = true };
      var mysql = new MySqlHelper();
      var deviceUid = Guid.NewGuid();
      msg.Title("Device Event 12", "Create Device Event ");
      var eventArray = new[] {
         "| EventType         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | MainboardSoftwareVersion | RadioFirmwarePartNumber |",
        $"| CreateDeviceEvent | 0d+09:00:00 | {deviceUid}        | Subscribed  | SNM940     | {deviceUid} | CDMA         | Device Event 12           | 3.54                     | 88                      |",
        $"| CreateDeviceEvent | 0d+09:30:00 | {deviceUid}        | Subscribed  | SNM941     | {deviceUid} | 3G           | Device Event 12 - Part 2  | 800                      | 77                      |"};

      ts.PublishEventCollection(eventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Device", "DeviceUID", 1,deviceUid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Device", "DeviceUID", "DeviceSerialNumber, DeviceState, DeviceType, DeviceUID, DataLinkType, GatewayFirmwarePartNumber, MainboardSoftwareVersion, RadioFirmwarePartNumber",
        $"{deviceUid},Subscribed,SNM941,{deviceUid},3G,Device Event 12 - Part 2,800,77",deviceUid);
    }


    [TestMethod]
    public void Create_Asset_Device_And_Associate()
    {
      var msg = new Msg();
      var ts = new TestSupport { IsPublishToKafka = true };
      var mysql = new MySqlHelper();
      var deviceUid = Guid.NewGuid();
      msg.Title("Device Event 13", "Associate Device Asset Event ");
      var deviceEventArray = new[] {
         "| EventType         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | MainboardSoftwareVersion | RadioFirmwarePartNumber |",
        $"| CreateDeviceEvent | 0d+09:00:00 | {deviceUid}        | Subscribed  | SNM940     | {deviceUid} | CDMA         | Device Event 13           | 3.54                     | 88                      |"};

      ts.PublishEventCollection(deviceEventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Device", "DeviceUID", 1, deviceUid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Device", "DeviceUID", "DeviceSerialNumber, DeviceState, DeviceType, DeviceUID, DataLinkType, GatewayFirmwarePartNumber, MainboardSoftwareVersion, RadioFirmwarePartNumber",
        $"{deviceUid},Subscribed,SNM940,{deviceUid},CDMA,Device Event 13,3.54,88", deviceUid);

      var assetEventArray = new[] {
         "| EventType        | EventDate   | AssetUID      | AssetName | Make | SerialNumber | Model | IconKey | AssetType  | LastActionedUTC |",
        $"| CreateAssetEvent | 0d+09:00:00 | {ts.AssetUid} | AssetE1   | CAT  | XAT1         | 374D  | 10      | Excavators | 0d+09:00:00     |"};

      ts.PublishEventCollection(assetEventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Asset", "AssetUID", 1, new Guid(ts.AssetUid));
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Asset", "AssetUID", "Name,MakeCode,SerialNumber,Model,IconKey,AssetType", "AssetE1,CAT,XAT1,374D,10,Excavators", new Guid(ts.AssetUid));


      var associateAssetDevice = new [] {
         "| EventType                 | EventDate   | AssetUID      | DeviceUID   |",
        $"| AssociateDeviceAssetEvent | 0d+09:00:00 | {ts.AssetUid} | {deviceUid} |"};
      ts.PublishEventCollection(associateAssetDevice);
      mysql.VerifyTestResultDatabaseRecordCount("AssetDevice", "fk_DeviceUID", 1, deviceUid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("AssetDevice", "fk_AssetUID", "fk_DeviceUID", $"{deviceUid}", new Guid(ts.AssetUid));
    }


    [TestMethod]
    public void Create_Asset_Device_And_Associate_Then_Dissociate()
    {
      var msg = new Msg();
      msg.Title("Device Event 14", "Associate Device Asset Event ");
      var ts = new TestSupport { IsPublishToKafka = true };
      var mysql = new MySqlHelper();
      var deviceUid = Guid.NewGuid();
      var deviceEventArray = new[] {
         "| EventType         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | MainboardSoftwareVersion | RadioFirmwarePartNumber |",
        $"| CreateDeviceEvent | 0d+09:00:00 | {deviceUid}        | Subscribed  | SNM940     | {deviceUid} | CDMA         | Device Event 14           | 3.54                     | 88                      |"};

      ts.PublishEventCollection(deviceEventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Device", "DeviceUID", 1, deviceUid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Device", "DeviceUID", "DeviceSerialNumber, DeviceState, DeviceType, DeviceUID, DataLinkType, GatewayFirmwarePartNumber, MainboardSoftwareVersion, RadioFirmwarePartNumber",
        $"{deviceUid},Subscribed,SNM940,{deviceUid},CDMA,Device Event 14,3.54,88", deviceUid);

      var assetEventArray = new[] {
         "| EventType        | EventDate   | AssetUID      | AssetName | Make | SerialNumber | Model | IconKey | AssetType  | LastActionedUTC |",
        $"| CreateAssetEvent | 0d+09:00:00 | {ts.AssetUid} | AssetE1   | CAT  | XAT1         | 374D  | 10      | Excavators | 0d+09:00:00     |"};

      ts.PublishEventCollection(assetEventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Asset", "AssetUID", 1, new Guid(ts.AssetUid));
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Asset", "AssetUID", "Name,MakeCode,SerialNumber,Model,IconKey,AssetType", "AssetE1,CAT,XAT1,374D,10,Excavators", Guid.Parse(ts.AssetUid));


      var associateAssetDevice = new[] {
         "| EventType                 | EventDate   | AssetUID      | DeviceUID   |",
        $"| AssociateDeviceAssetEvent | 0d+09:10:00 | {ts.AssetUid} | {deviceUid} |"};
      ts.PublishEventCollection(associateAssetDevice);
      mysql.VerifyTestResultDatabaseRecordCount("AssetDevice", "fk_DeviceUID", 1, deviceUid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("AssetDevice", "fk_AssetUID", "fk_DeviceUID", $"{deviceUid}", Guid.Parse(ts.AssetUid));


      var dissociateAssetDevice = new[] {
         "| EventType                  | EventDate   | AssetUID      | DeviceUID   |",
        $"| DissociateDeviceAssetEvent | 0d+09:30:00 | {ts.AssetUid} | {deviceUid} |"};
      ts.PublishEventCollection(dissociateAssetDevice);
      mysql.VerifyTestResultDatabaseRecordCount("AssetDevice", "fk_DeviceUID", 0, deviceUid);
    }
  }
}
