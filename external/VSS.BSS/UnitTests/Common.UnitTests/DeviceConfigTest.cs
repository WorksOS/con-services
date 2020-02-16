using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.MTSMessages;
using VSS.UnitTest.Common;

namespace UnitTests
{
  [TestClass]
  public class DeviceConfigTest : UnitTestBase
  {
    private const string GPS_DEVICE_ID = "LBL00003532RM";

    [DatabaseTest]
    [TestMethod]
    public void ConfigureDeviceForServicePlan_Downgrade_OneMinutePlan_TO_CATUTIL_Test()
    {
      SetupDeviceForServicePlanHelper(TestData.TestMTS523);

      //Upgrade to CATUTIL
      DeviceConfig.ConfigureDeviceForServicePlan(Ctx.OpContext, GPS_DEVICE_ID,
                                                                DeviceTypeEnum.Series523, true, ServiceTypeEnum.CATUtilization,
                                                                GenerateCATServicePlanHelper());

      //Downgrade back to VLCORE
      var expected = DeviceConfig.ConfigureDeviceForServicePlan(Ctx.OpContext, GPS_DEVICE_ID, 
                                                                DeviceTypeEnum.Series523, false, ServiceTypeEnum.Essentials, 
                                                                GenerateCoreServicePlanHelper());

      Assert.IsTrue(expected, "ServicePlan Downgrade did not succeed.");

      CheckMTSDeviceInterval(GPS_DEVICE_ID);
    } 

    [DatabaseTest]
    [TestMethod]
    public void ConfigureDeviceForServicePlan_Downgrade_CATUTIL_TO_CORE_Test()
    {
      SetupDeviceForServicePlanHelper(TestData.TestMTS523);

      //Upgrade to ONEMINUTE
      DeviceConfig.ConfigureDeviceForServicePlan(Ctx.OpContext, GPS_DEVICE_ID,
                                                                DeviceTypeEnum.Series523, true, ServiceTypeEnum.e1minuteUpdateRateUpgrade,
                                                                GenerateOneMinuteServicePlanHelper());

      //Downgrade back to CATUTIL
      var expected = DeviceConfig.ConfigureDeviceForServicePlan(Ctx.OpContext, GPS_DEVICE_ID,
                                                                DeviceTypeEnum.Series523, false, ServiceTypeEnum.CATUtilization,
                                                                GenerateCATServicePlanHelper());

      Assert.IsTrue(expected, "ServicePlan Downgrade did not succeed.");

      CheckMTSDeviceInterval(GPS_DEVICE_ID, ServiceTypeEnum.CATUtilization);

    }

    [DatabaseTest]
    [TestMethod]
    public void ConfigureDeviceForServicePlan_ModifyVLCORE_With_EmptyServicePlanIDs_Test()
    {
      SetupDeviceForServicePlanHelper(TestData.TestMTS523);
      var expected = DeviceConfig.ConfigureDeviceForServicePlan(Ctx.OpContext, GPS_DEVICE_ID,
                                                                DeviceTypeEnum.Series523, false, ServiceTypeEnum.Essentials,
                                                                new List<DeviceConfig.ServicePlanIDs>());

      Assert.IsFalse(expected, "Cannot delete service plans that do not exist.");
    }

    [DatabaseTest]
    [TestMethod]
    public void ConfigureDeviceForServicePlan_UpgradeCORE_To_CATUTIL_Test()
    {
      SetupDeviceForServicePlanHelper(TestData.TestMTS523);
      DeviceConfig.ConfigureDeviceForServicePlan(Ctx.OpContext, GPS_DEVICE_ID,
                                                                DeviceTypeEnum.Series523, true, ServiceTypeEnum.CATUtilization,
                                                                GenerateCATServicePlanHelper());
      CheckMTSDeviceInterval(GPS_DEVICE_ID, ServiceTypeEnum.CATUtilization);
    }

    [DatabaseTest]
    [TestMethod]
    public void ConfigureDeviceForServicePlan_UpgradeCATUTIL_To_ONEMINUTEPLAN_Test()
    {
      SetupDeviceForServicePlanHelper(TestData.TestMTS523);
      DeviceConfig.ConfigureDeviceForServicePlan(Ctx.OpContext, GPS_DEVICE_ID,
                                                                DeviceTypeEnum.Series523, true, ServiceTypeEnum.CATUtilization,
                                                                new List<DeviceConfig.ServicePlanIDs>());

      DeviceConfig.ConfigureDeviceForServicePlan(Ctx.OpContext, GPS_DEVICE_ID,
                                                                DeviceTypeEnum.Series523, true, ServiceTypeEnum.e1minuteUpdateRateUpgrade,
                                                                GenerateOneMinuteServicePlanHelper());

      CheckMTSDeviceInterval(GPS_DEVICE_ID, ServiceTypeEnum.e1minuteUpdateRateUpgrade);
    }

    [DatabaseTest]
    [TestMethod]
    public void ConfigureDeviceForServicePlan_UpgradeCORE_To_ONEMINUTEPLAN_Test()
    {
      SetupDeviceForServicePlanHelper(TestData.TestMTS523);
      DeviceConfig.ConfigureDeviceForServicePlan(Ctx.OpContext, GPS_DEVICE_ID,
                                                                DeviceTypeEnum.Series523, true, ServiceTypeEnum.e1minuteUpdateRateUpgrade,
                                                                GenerateOneMinuteServicePlanHelper());

      CheckMTSDeviceInterval(GPS_DEVICE_ID, ServiceTypeEnum.e1minuteUpdateRateUpgrade);
    }

    [DatabaseTest]
    [TestMethod]
    public void ConfigureDeviceForServicePlan_AddCorePlan_Series521_Test()
    {
      ConfigureDeviceForServicePlan_AddCorePlan_MTS_TestHelper(TestData.TestMTS521, DeviceTypeEnum.Series521, 99, 0, 1, 30, 150, 3600, false, 0.2, 30, true);
    }

    [DatabaseTest]
    [TestMethod]
    public void ConfigureDeviceForServicePlan_AddCorePlan_Series522_Test()
    {
      ConfigureDeviceForServicePlan_AddCorePlan_MTS_TestHelper(TestData.TestMTS522, DeviceTypeEnum.Series522, 99, 0, 1, 30, 150, 3600, false, 0.2, 30, true);
    }

    [DatabaseTest]
    [TestMethod]
    public void ConfigureDeviceForServicePlan_AddCorePlan_Series523_Test()
    {
      ConfigureDeviceForServicePlan_AddCorePlan_MTS_TestHelper(TestData.TestMTS523, DeviceTypeEnum.Series523, 99, 0, 1, 30, 150, 3600, false, 0.2, 30, true);
    }

    [DatabaseTest]
    [TestMethod]
    public void ConfigureDeviceForServicePlan_AddCorePlan_SNM940_Test()
    {
      ConfigureDeviceForServicePlan_AddCorePlan_MTS_TestHelper(TestData.TestSNM940, DeviceTypeEnum.SNM940, 99, 1, 1, 30, 10, 4, false, 1, 30, true);
    }

    [DatabaseTest]
    [TestMethod]
    public void ConfigureDeviceForServicePlan_AddCorePlan_PL420_Test()
    {
      ConfigureDeviceForServicePlan_AddCorePlan_MTS_TestHelper(TestData.TestPL420, DeviceTypeEnum.PL420, 99, 0, 1, 300, 10, 4, false, 0.1, 120, true);
    }

    [DatabaseTest]
    [TestMethod]
    public void ConfigureDeviceForServicePlan_AddCorePlan_PL421_Test()
    {
      ConfigureDeviceForServicePlan_AddCorePlan_MTS_TestHelper(TestData.TestPL421, DeviceTypeEnum.PL421, 99, 0, 1, 30, 10, 4, false, 0.2, 30, true);
    }

    [DatabaseTest]
    [TestMethod]
    public void ConfigureDeviceForServicePlan_AddCorePlan_MANUALDEVICE_Test()
    {
      
    }

    [DatabaseTest]
    [TestMethod]
    public void ConfigureDeviceForServicePlan_AddCorePlan_PL121_Test()
    {

    }

    [DatabaseTest]
    [TestMethod]
    public void ConfigureDeviceForServicePlan_AddCorePlan_PL321_Test()
    {

    }

    [DatabaseTest]
    [TestMethod]
    public void ConfigureDeviceForServicePlan_AddCorePlan_CrossCheck_Test()
    {

    }

    [DatabaseTest]
    [TestMethod]
    public void ConfigureDeviceForServicePlan_AddCorePlan_TrimTrac_Test()
    {

    }

    [DatabaseTest]
    [TestMethod]
    public void ConfigureDeviceToDefaults_PL420_Test()
    {
      List<DeviceConfig.ServicePlanIDs> currentPlanIDs = SetupDeviceForServicePlanHelper(TestData.TestPL420);
      var result = DeviceConfig.ConfigureDeviceToDefaults(Ctx.OpContext, GPS_DEVICE_ID, DeviceTypeEnum.PL420, currentPlanIDs);
      Assert.IsTrue(result, "Failed to configure Device defaults for PL420.");

      // verify the device default vaules for PL420
      var mtsOut = (from m in Ctx.RawContext.MTSOutReadOnly
                    where m.TypeID == DeviceConfigurationBaseUserDataMessage.kPacketID &&
                          m.SubTypeID == (int)DeviceConfigurationBaseUserDataMessage.ConfigType.StoppedNotificationConfiguration &&
                          m.PacketID == UserDataBaseMessage.kPacketID &&
                          m.SerialNumber == GPS_DEVICE_ID &&
                          m.DeviceType == (int)DeviceTypeEnum.PL420
                    select m);

      int messageCount = mtsOut.Count<MTSOut>();
      Assert.AreEqual(1, messageCount, "there should have been MTSOut record created for stopped threshold");
      byte[] message = mtsOut.FirstOrDefault().Payload;
      Assert.IsNotNull(message, "Payload Should not be null");

      UserDataBaseMessage StoppedNotificationUserData = PlatformMessage.HydratePlatformMessage(message, true, false) as UserDataBaseMessage;

      Assert.IsNotNull(StoppedNotificationUserData, "Payload should be UserDataBase Message");
      DeviceConfigurationBaseUserDataMessage stopped = StoppedNotificationUserData.Message as DeviceConfigurationBaseUserDataMessage;

      Assert.IsNotNull(stopped.SpeedingReporting, "SpeedingReporting should not be null");
      Assert.AreEqual(120, stopped.SpeedingReporting.DurationThreshold, "Stopped speeding reporting duration threshold is not equal");
      Assert.AreEqual(1, stopped.SpeedingReporting.SpeedThreshold, "Stopped speeding reporting speed threshold is not equal");
    }

    [DatabaseTest]
    [TestMethod]
    public void ConfigureDeviceToDefaults_PL421_Test()
    {
      List<DeviceConfig.ServicePlanIDs> currentPlanIDs = SetupDeviceForServicePlanHelper(TestData.TestPL421);
      var result = DeviceConfig.ConfigureDeviceToDefaults(Ctx.OpContext, GPS_DEVICE_ID, DeviceTypeEnum.PL421, currentPlanIDs);
      Assert.IsTrue(result, "Failed to configure Device defaults for PL421.");

      // verify the device default vaules for PL421
      var mtsOut = (from m in Ctx.RawContext.MTSOutReadOnly
                    where m.TypeID == DeviceConfigurationBaseUserDataMessage.kPacketID &&
                          m.SubTypeID == (int)DeviceConfigurationBaseUserDataMessage.ConfigType.StoppedNotificationConfiguration &&
                          m.PacketID == UserDataBaseMessage.kPacketID &&
                          m.SerialNumber == GPS_DEVICE_ID &&
                          m.DeviceType == (int)DeviceTypeEnum.PL421
                    select m);

      int messageCount = mtsOut.Count<MTSOut>();
      Assert.AreEqual(1, messageCount, "there should have been MTSOut record created for stopped threshold");
      byte[] message = mtsOut.FirstOrDefault().Payload;
      Assert.IsNotNull(message, "Payload Should not be null");

      UserDataBaseMessage StoppedNotificationUserData = PlatformMessage.HydratePlatformMessage(message, true, false) as UserDataBaseMessage;

      Assert.IsNotNull(StoppedNotificationUserData, "Payload should be UserDataBase Message");
      DeviceConfigurationBaseUserDataMessage stopped = StoppedNotificationUserData.Message as DeviceConfigurationBaseUserDataMessage;

      Assert.IsNotNull(stopped.SpeedingReporting, "SpeedingReporting should not be null");
      Assert.AreEqual(30, stopped.SpeedingReporting.DurationThreshold, "Stopped speeding reporting duration threshold is not equal");
      Assert.AreEqual(2, stopped.SpeedingReporting.SpeedThreshold, "Stopped speeding reporting speed threshold is not equal");
    }

    [TestMethod]
    public void UserBaseMessageConfigureFirmwareMessage_Test()
    {
      //Write code to ascertain the new Config Type Message.
      //First failure condition should be that the message type FirmwareVersionConfig doesn't exist
      //Type = 0x15 and SubType = 0x25
      DeviceConfigurationBaseUserDataMessage message = new DeviceConfigurationBaseUserDataMessage();
      Assert.AreEqual(0x15, message.PacketID, "Message is not of the type of 0x15");
      Assert.IsNotNull(Enum.GetName(typeof(DeviceConfigurationBaseUserDataMessage.ConfigType), 0x25), "Message of sub type 0x25(FirmwareVersionConfig) not fount");

     // DeviceConfigurationBaseUserDataMessage 
    }
   
    [DatabaseTest]
    [TestMethod]
    public void ConfigurePL42xForVocationalTruck_Test()
    {
      AssetBuilder assetBuilder = new AssetBuilder();
      Device _device = TestData.TestPL421;
      List<DeviceConfig.ServicePlanIDs> currentPlanIDs = SetupDeviceForServicePlanHelper(_device);
      //The Asset serial number should conform to the rules of CAT serialnumbers
      var assetVoc = assetBuilder.WithDevice(_device).ProductFamily("Vocational Truck").SerialNumberVin("TSW12345").Save();               
      var result = DeviceConfig.ConfigureDeviceToDefaults(Ctx.OpContext, _device.GpsDeviceID, DeviceTypeEnum.PL421, currentPlanIDs);
      //check if the default configuration has been sent successfully
      Assert.IsTrue(result, "Failed to configure Device defaults for PL421.");

      //check if the asset's product family is set to ONHT
      Assert.AreEqual("Vocational Truck", assetVoc.ProductFamilyName, "The asset's product family is not vocational truck");

      //Check if the appropriate firmware config message has been sent to the device on configuration
      var mtsOut = (from m in Ctx.RawContext.MTSOutReadOnly
                    where m.TypeID == DeviceConfigurationBaseUserDataMessage.kPacketID &&
                          m.SubTypeID == (int)DeviceConfigurationBaseUserDataMessage.ConfigType.AssetBasedFirmwareVersionConfig &&
                          m.PacketID == UserDataBaseMessage.kPacketID &&
                          m.SerialNumber == _device.GpsDeviceID &&
                          m.DeviceType == (int)DeviceTypeEnum.PL421
                    select m);

      Assert.AreEqual(1, mtsOut.Count<MTSOut>(), "There is no firmwareconfig message sent");
      byte[] message = mtsOut.FirstOrDefault().Payload;      
      Assert.IsNotNull(message, "Payload Should not be null");

      UserDataBaseMessage assetBasedFirmwareConfig = PlatformMessage.HydratePlatformMessage(message, true, false) as UserDataBaseMessage;
      Assert.IsNotNull(assetBasedFirmwareConfig, "Payload should be UserDataBase Message");
      DeviceConfigurationBaseUserDataMessage config = assetBasedFirmwareConfig.Message as DeviceConfigurationBaseUserDataMessage;

      Assert.IsNotNull(config.firmwareVersionConfig, "the firmware configuration should not be null");
      Assert.AreEqual((byte)DeviceConfigurationBaseUserDataMessage.AssetBasedFirmwareConfiguration.PL420VocationalTrucks, (byte)config.firmwareVersionConfig.FirmWareConfiguration, "firmware configuration value should be 1 for vocational truck");      
    }   

    [DatabaseTest]
    [TestMethod]
    public void ConfigurePL42xForBCP_Test()
    {
      List<DeviceConfig.ServicePlanIDs> currentPlanIDs = SetupDeviceForServicePlanHelper(TestData.TestPL421);
      var result = DeviceConfig.ConfigureDeviceToDefaults(Ctx.OpContext, GPS_DEVICE_ID, DeviceTypeEnum.PL421, currentPlanIDs);
      //check if the default configuration has been sent successfully
      Assert.IsTrue(result, "Failed to configure Device defaults for PL421.");

      //Check if the appropriate firmware config message has been sent to the device on configuration
      var mtsOut = (from m in Ctx.RawContext.MTSOutReadOnly
                    where m.TypeID == DeviceConfigurationBaseUserDataMessage.kPacketID &&
                          m.SubTypeID == (int)DeviceConfigurationBaseUserDataMessage.ConfigType.AssetBasedFirmwareVersionConfig &&
                          m.PacketID == UserDataBaseMessage.kPacketID &&
                          m.SerialNumber == GPS_DEVICE_ID &&
                          m.DeviceType == (int)DeviceTypeEnum.PL421
                    select m);

      Assert.AreEqual(1, mtsOut.Count<MTSOut>(), "There is no firmwareconfig message sent");
      byte[] message = mtsOut.FirstOrDefault().Payload;
      Assert.IsNotNull(message, "Payload Should not be null");

      UserDataBaseMessage assetBasedFirmwareConfig = PlatformMessage.HydratePlatformMessage(message, true, false) as UserDataBaseMessage;
      Assert.IsNotNull(assetBasedFirmwareConfig, "Payload should be UserDataBase Message");
      DeviceConfigurationBaseUserDataMessage config = assetBasedFirmwareConfig.Message as DeviceConfigurationBaseUserDataMessage;

      Assert.IsNotNull(config.firmwareVersionConfig, "the firmware configuration should not be null");
      Assert.AreEqual((byte)DeviceConfigurationBaseUserDataMessage.AssetBasedFirmwareConfiguration.PL421BCP, (byte)config.firmwareVersionConfig.FirmWareConfiguration, "firmware configuration value should be 5 for types other than genset and voc truck");
    }

    [DatabaseTest]
    [TestMethod]
    public void ConfigureDeviceToDefaults_PL420ConfigJ1939_Test()
    {
      List<DeviceConfig.ServicePlanIDs> currentPlanIDs = SetupDeviceForServicePlanHelper(TestData.TestPL420);
      var result = DeviceConfig.ConfigureDeviceToDefaults(Ctx.OpContext, GPS_DEVICE_ID, DeviceTypeEnum.PL420, currentPlanIDs);
      Assert.IsTrue(result, "Failed to configure Device defaults for PL420.");

      // verify the device default vaules for PL420
      var mtsOut = (from m in Ctx.RawContext.MTSOutReadOnly
                    where m.TypeID == DeviceConfigurationBaseUserDataMessage.kPacketID &&
                          m.SubTypeID == (int)DeviceConfigurationBaseUserDataMessage.ConfigType.ConfigureJ1939Reporting &&
                          m.PacketID == UserDataBaseMessage.kPacketID &&
                          m.SerialNumber == GPS_DEVICE_ID &&
                          m.DeviceType == (int)DeviceTypeEnum.PL420
                    select m);

      int messageCount = mtsOut.Count<MTSOut>();
      Assert.AreEqual(2, messageCount, "there should have been MTSOut record created for stopped threshold");
      int count = 0;
      foreach (MTSOut m in mtsOut)
      {
        byte[] message = m.Payload;
        Assert.IsNotNull(message, "Payload Should not be null");

        UserDataBaseMessage J1939UserData = PlatformMessage.HydratePlatformMessage(message, true, false) as UserDataBaseMessage;

        Assert.IsNotNull(J1939UserData, "Payload should be UserDataBase Message");
        DeviceConfigurationBaseUserDataMessage msg = J1939UserData.Message as DeviceConfigurationBaseUserDataMessage;

        Assert.IsNotNull(msg.J1939Reporting, "J1939Reporting should not be null");
        if (msg.J1939Reporting.reportType == DeviceConfigurationBaseUserDataMessage.ReportType.Periodic)
        {
          count++;
          Assert.AreEqual(2, msg.J1939Reporting.parameter.Count(), "incorrect number of parameters");
        }
        else if (msg.J1939Reporting.reportType == DeviceConfigurationBaseUserDataMessage.ReportType.Fault)
        {
          count++;
          Assert.IsNull(msg.J1939Reporting.parameter, "incorrect number of parameters");
        }
      }
      Assert.AreEqual(2, count, "Incorrrect parameters");
    }

    [DatabaseTest]
    [TestMethod]
    public void ConfigureDeviceToDefaults_PL421ConfigJ1939_Test()
    {
      List<DeviceConfig.ServicePlanIDs> currentPlanIDs = SetupDeviceForServicePlanHelper(TestData.TestPL421);
      var result = DeviceConfig.ConfigureDeviceToDefaults(Ctx.OpContext, GPS_DEVICE_ID, DeviceTypeEnum.PL421, currentPlanIDs);
      Assert.IsTrue(result, "Failed to configure Device defaults for PL421.");

      // verify the device default vaules for PL421
      var mtsOut = (from m in Ctx.RawContext.MTSOutReadOnly
                    where m.TypeID == DeviceConfigurationBaseUserDataMessage.kPacketID &&
                          m.SubTypeID == (int)DeviceConfigurationBaseUserDataMessage.ConfigType.ConfigureJ1939Reporting &&
                          m.PacketID == UserDataBaseMessage.kPacketID &&
                          m.SerialNumber == GPS_DEVICE_ID &&
                          m.DeviceType == (int)DeviceTypeEnum.PL421
                    select m);

      int messageCount = mtsOut.Count<MTSOut>();
      Assert.AreEqual(2, messageCount, "there should have been MTSOut record created for stopped threshold");
      int count = 0;
      foreach (MTSOut m in mtsOut)
      {
        byte[] message = m.Payload;
        Assert.IsNotNull(message, "Payload Should not be null");

        UserDataBaseMessage J1939UserData = PlatformMessage.HydratePlatformMessage(message, true, false) as UserDataBaseMessage;

        Assert.IsNotNull(J1939UserData, "Payload should be UserDataBase Message");
        DeviceConfigurationBaseUserDataMessage msg = J1939UserData.Message as DeviceConfigurationBaseUserDataMessage;

        Assert.IsNotNull(msg.J1939Reporting, "J1939Reporting should not be null");
        if (msg.J1939Reporting.reportType == DeviceConfigurationBaseUserDataMessage.ReportType.Periodic)
        {
          count++;
          Assert.AreEqual(4, msg.J1939Reporting.parameter.Count(), "incorrect number of parameters");
        }
        else if (msg.J1939Reporting.reportType == DeviceConfigurationBaseUserDataMessage.ReportType.Fault)
        {
          count++;
          Assert.IsNull(msg.J1939Reporting.parameter, "incorrect number of parameters");
        }
      }
      Assert.AreEqual(2, count, "Incorrrect parameters");
    }

    [DatabaseTest]
    [TestMethod]
    public void ConfigureDeviceToDefaults_PL421ConfigAlternateDatasource_Test()
    {
      List<DeviceConfig.ServicePlanIDs> currentPlanIDs = SetupDeviceForServicePlanHelper(TestData.TestPL421);
      var result = DeviceConfig.ConfigureDeviceToDefaults(Ctx.OpContext, GPS_DEVICE_ID, DeviceTypeEnum.PL421, currentPlanIDs);
      Assert.IsTrue(result, "Failed to configure Device defaults for PL421.");

      // verify the device default vaules for PL421
      var mtsOut = (from m in Ctx.RawContext.MTSOutReadOnly
                    where m.TypeID == DeviceConfigurationBaseUserDataMessage.kPacketID &&
                          m.SubTypeID == (int)DeviceConfigurationBaseUserDataMessage.ConfigType.ConfigureMachineEventHeader &&
                          m.PacketID == UserDataBaseMessage.kPacketID &&
                          m.SerialNumber == GPS_DEVICE_ID &&
                          m.DeviceType == (int)DeviceTypeEnum.PL421
                    select m);

      int messageCount = mtsOut.Count<MTSOut>();
      Assert.AreEqual(1, messageCount, "there should have been MTSOut record created for MachineEvenHeader ");
      foreach (MTSOut m in mtsOut)
      {
        byte[] message = m.Payload;
        Assert.IsNotNull(message, "Payload Should not be null");

        UserDataBaseMessage ConfigureMachineEvent = PlatformMessage.HydratePlatformMessage(message, true, false) as UserDataBaseMessage;

        Assert.IsNotNull(ConfigureMachineEvent, "Payload should be UserDataBase Message");
        DeviceConfigurationBaseUserDataMessage msg = ConfigureMachineEvent.Message as DeviceConfigurationBaseUserDataMessage;

        Assert.IsNotNull(msg.machineEventHeader, "Machine event header config should not be null");
        Assert.AreEqual(msg.machineEventHeader.DataSource,PrimaryDataSourceEnum.J1939, "Primary Data Source should be J1939");
      }
    }

    [DatabaseTest]
    [TestMethod]
    public void ConfigureDeviceToDefaults_500Series_Bug12886_Test()
    {
      List<DeviceConfig.ServicePlanIDs> currentPlanIDs = SetupDeviceForServicePlanHelper(TestData.TestMTS522);
      var result = DeviceConfig.ConfigureDeviceToDefaults(Ctx.OpContext, GPS_DEVICE_ID, DeviceTypeEnum.Series522, currentPlanIDs);
      Assert.IsTrue(result, "Failed to configure Device defaults for MTS 522.");

      // verify the device default vaules for MTS 522
      var mtsOut = (from m in Ctx.RawContext.MTSOutReadOnly
                    where m.TypeID == ConfigureGatewayMessage.kPacketID &&                          
                          m.PacketID == UserDataBaseMessage.kPacketID &&
                          m.SerialNumber == GPS_DEVICE_ID &&
                          m.DeviceType == (int)DeviceTypeEnum.Series522
                    select m);

      int messageCount = mtsOut.Count<MTSOut>();
      Assert.AreEqual(1, messageCount, "there should have been MTSOut record created for stopped threshold");
      byte[] message = mtsOut.FirstOrDefault().Payload;
      Assert.IsNotNull(message, "Payload Should not be null");

      UserDataBaseMessage gatewayUserData = PlatformMessage.HydratePlatformMessage(message, true, false) as UserDataBaseMessage;

      Assert.IsNotNull(gatewayUserData, "Payload should be UserDataBase Message");
      ConfigureGatewayMessage gateway = gatewayUserData.Message as ConfigureGatewayMessage;

      Assert.AreEqual(new TimeSpan(0, 0, 5), gateway.GetDigSwitchConfig(FieldID.DigitalInput1Config, -1, null, MessageStatusEnum.Sent).DelayTime, "Delay Time for Switch 1 should be 5 seconds");
      Assert.AreEqual(new TimeSpan(0, 0, 5), gateway.GetDigSwitchConfig(FieldID.DigitalInput2Config, -1, null, MessageStatusEnum.Sent).DelayTime, "Delay Time for Switch 2 should be 5 seconds");
      Assert.AreEqual(new TimeSpan(0, 0, 5), gateway.GetDigSwitchConfig(FieldID.DigitalInput3Config, -1, null, MessageStatusEnum.Sent).DelayTime, "Delay Time for Switch 3 should be 5 seconds");
      Assert.AreEqual(new TimeSpan(0, 0, 5), gateway.GetDigSwitchConfig(FieldID.DigitalInput4Config, -1, null, MessageStatusEnum.Sent).DelayTime, "Delay Time for Switch 4 should be 5 seconds");      
    }

    [DatabaseTest]
    [TestMethod]
    public void ConfigureDeviceForServicePlan_500Series_Bug12886_Test()
    {
      SetupDeviceForServicePlanHelper(TestData.TestMTS522);
      var result = DeviceConfig.ConfigureDeviceForServicePlan(Ctx.OpContext, GPS_DEVICE_ID, DeviceTypeEnum.Series522, true, ServiceTypeEnum.Essentials, GenerateCATServicePlanHelper());
      Assert.IsTrue(result, "Failed to configure MTS 522 device for VLCORE service plan.");

      // verify the device default vaules for MTS 522
      var mtsOut = (from m in Ctx.RawContext.MTSOutReadOnly
                    where m.TypeID == ConfigureGatewayMessage.kPacketID &&
                          m.PacketID == UserDataBaseMessage.kPacketID &&
                          m.SerialNumber == GPS_DEVICE_ID &&
                          m.DeviceType == (int)DeviceTypeEnum.Series522
                    select m);

      int messageCount = mtsOut.Count<MTSOut>();
      Assert.AreEqual(1, messageCount, "there should have been MTSOut record created for configuration of digital switches");
      byte[] message = mtsOut.FirstOrDefault().Payload;
      Assert.IsNotNull(message, "Payload Should not be null");

      UserDataBaseMessage gatewayUserData = PlatformMessage.HydratePlatformMessage(message, true, false) as UserDataBaseMessage;

      Assert.IsNotNull(gatewayUserData, "Payload should be UserDataBase Message");
      ConfigureGatewayMessage gateway = gatewayUserData.Message as ConfigureGatewayMessage;
      Assert.IsNotNull(gateway, "Expected a ConfigureGatewayMessage");
      Assert.AreEqual(new TimeSpan(0, 0, 5), gateway.GetDigSwitchConfig(FieldID.DigitalInput1Config, -1, null, MessageStatusEnum.Sent).DelayTime, "Delay Time for Switch 1 should be 5 seconds");
      Assert.AreEqual(new TimeSpan(0, 0, 5), gateway.GetDigSwitchConfig(FieldID.DigitalInput2Config, -1, null, MessageStatusEnum.Sent).DelayTime, "Delay Time for Switch 2 should be 5 seconds");
      Assert.AreEqual(new TimeSpan(0, 0, 5), gateway.GetDigSwitchConfig(FieldID.DigitalInput3Config, -1, null, MessageStatusEnum.Sent).DelayTime, "Delay Time for Switch 3 should be 5 seconds");
      Assert.AreEqual(new TimeSpan(0, 0, 5), gateway.GetDigSwitchConfig(FieldID.DigitalInput4Config, -1, null, MessageStatusEnum.Sent).DelayTime, "Delay Time for Switch 4 should be 5 seconds");
    }

    [DatabaseTest]
    [TestMethod]
    public void ConfigureDeviceForServicePlan_500Series_AddUtilization()
    {
      SetupDeviceForServicePlanHelper(TestData.TestMTS522);
      var result = DeviceConfig.ConfigureDeviceForServicePlan(Ctx.OpContext, GPS_DEVICE_ID, DeviceTypeEnum.Series522, true, ServiceTypeEnum.CATUtilization, GenerateCATServicePlanHelper());
      Assert.IsTrue(result, "Failed to configure MTS 522 device to add Utilization plan.");

      // verify that no message was written to the MTSOut table to configure switches
      var mtsOut = (from m in Ctx.RawContext.MTSOutReadOnly
                    where m.TypeID == ConfigureGatewayMessage.kPacketID &&
                          m.PacketID == UserDataBaseMessage.kPacketID &&
                          m.SerialNumber == GPS_DEVICE_ID &&
                          m.DeviceType == (int)DeviceTypeEnum.Series522
                    select m);

      int messageCount = mtsOut.Count<MTSOut>();
      Assert.AreEqual(0, messageCount, "There should not have been a MTSOut record created for digital switch configuration");
    }

    [TestMethod]
    public void TestIsEnvironmentProd_True_ReturnsTrue()
    {
      ConfigurationManager.AppSettings["IsProductionBSS"] = "true";
      DeviceConfig.ResetEnvironmentFlag();
      Assert.IsTrue(DeviceConfig.IsEnvironmentProd(), "Expected DeviceConfig.IsEnvironmentProd() to be true the first time");
      Assert.IsTrue(DeviceConfig.IsEnvironmentProd(), "Expected DeviceConfig.IsEnvironmentProd() to be true the second time");
    }

    [TestMethod]
    public void TestIsEnvironmentProd_False_ReturnsFalse()
    {
      ConfigurationManager.AppSettings["IsProductionBSS"] = "false";
      DeviceConfig.ResetEnvironmentFlag();
      Assert.IsFalse(DeviceConfig.IsEnvironmentProd(), "Expected DeviceConfig.IsEnvironmentProd() to be false the first time");
      Assert.IsFalse(DeviceConfig.IsEnvironmentProd(), "Expected DeviceConfig.IsEnvironmentProd() to be false the second time");
    }

    [TestMethod]
    public void TestIsEnvironmentProd_NoSetting_ReturnsFalse()
    {
      ConfigurationManager.AppSettings["IsProductionBSS"] = null;
      DeviceConfig.ResetEnvironmentFlag();
      Assert.IsFalse(DeviceConfig.IsEnvironmentProd(), "Expected DeviceConfig.IsEnvironmentProd() to be false the first time");
      Assert.IsFalse(DeviceConfig.IsEnvironmentProd(), "Expected DeviceConfig.IsEnvironmentProd() to be false the second time");
    }

    [TestMethod]
    public void TestIsEnvironmentProd_InvalidSetting_ReturnsFalse()
    {
      ConfigurationManager.AppSettings["IsProductionBSS"] = "nottoday";
      DeviceConfig.ResetEnvironmentFlag();
      Assert.IsFalse(DeviceConfig.IsEnvironmentProd(), "Expected DeviceConfig.IsEnvironmentProd() to be false the first time");
      Assert.IsFalse(DeviceConfig.IsEnvironmentProd(), "Expected DeviceConfig.IsEnvironmentProd() to be false the second time");
    }

    #region Private Members

    #region Helpers

    //test mts device defaults upon adding essentials service plan
    private void ConfigureDeviceForServicePlan_AddCorePlan_MTS_TestHelper(
      Device device, DeviceTypeEnum deviceType, 
      byte siteEntrySpeedMPH, byte siteExitSpeedMPH, byte siteHysteresisSeconds, 
      ushort movingConfigRadius, 
      double speedingThreshold, short speedingDurationSec, bool speedingReportingEnabled, 
      double stopThreshold, short stopDurationSec, bool stopReportingEnabled)
    {
      SetupDeviceForServicePlanHelper(device);

      var result = DeviceConfig.ConfigureDeviceForServicePlan(Ctx.OpContext, GPS_DEVICE_ID, deviceType, true, ServiceTypeEnum.Essentials, GenerateCoreServicePlanHelper());
      Assert.IsTrue(result, "Failed to configure " + deviceType.ToString() + " for essentials service plan.");

      CheckMTSDeviceInterval(GPS_DEVICE_ID);

      //Device Shutdown delay is 900, MDT Shutdown delay is 60, alwaysOnDevice is false
      var generalDeviceConfiguration = GetDeviceConfigurationBaseUserDataMessageHelper(DeviceConfigurationBaseUserDataMessage.kPacketID, (int)DeviceConfigurationBaseUserDataMessage.ConfigType.GeneralDeviceConfiguration, deviceType);
      Assert.AreEqual(900, generalDeviceConfiguration.GeneralDevice.DeviceShutdownDelay, "Failed to properly configure " + deviceType.ToString() + " for essentials service plan, device shutdown delay should be 900.");
      Assert.AreEqual(60, generalDeviceConfiguration.GeneralDevice.MDTShutdownDelay, "Failed to properly configure " + deviceType.ToString() + " for essentials service plan, mdt shutdown delay should be 60.");
      Assert.IsFalse(generalDeviceConfiguration.GeneralDevice.AlwaysOnDevice, "Failed to properly configure " + deviceType.ToString() + " for essentials service plan, alwaysOnDevice should be false.");

      //Ignition reporting is enabled
      var ignitionReportingConfiguration = GetDeviceConfigurationBaseUserDataMessageHelper(DeviceConfigurationBaseUserDataMessage.kPacketID, (int)DeviceConfigurationBaseUserDataMessage.ConfigType.IgnitionReportingConfiguration, deviceType);
      Assert.IsTrue(ignitionReportingConfiguration.IgnitionReportingConfig.IgnitionReportingEnabled, "Failed to properly configure " + deviceType.ToString() + " for essentials service plan, ignition reporting should be true.");
      
      //Runtime Mileage is 0
      var setDeviceMileageRunTimeCounters = GetBaseUserDataMessageHelper(SetDeviceMileageRunTimeCountersBaseUserDataMessage.kPacketID, deviceType) as SetDeviceMileageRunTimeCountersBaseUserDataMessage;
      Assert.AreEqual(0, setDeviceMileageRunTimeCounters.Mileage, "Failed to properly configure " + deviceType.ToString() + " for essentials service plan, runtime mileage should be 0.");

      //Runtime Hours is 0
      Assert.AreEqual(0, setDeviceMileageRunTimeCounters.RunTimeCounterHours, "Failed to properly configure " + deviceType.ToString() + " for essentials service plan, runtime hours should be 0.");

      //All sites purged
      MTSOut mtsOut = (from m in Ctx.RawContext.MTSOutReadOnly
                       where m.PacketID == SitePurgeBaseMessage.kPacketID &&
                             m.SerialNumber == GPS_DEVICE_ID
                       select m).FirstOrDefault();
      var sitePurge = PlatformMessage.HydratePlatformMessage(mtsOut.Payload, true, false) as SitePurgeBaseMessage;
      Assert.AreEqual(0xFFFFFFFF, sitePurge.SiteID, "Failed to properly configure " + deviceType.ToString() + " for essentials service plan, sites SiteID should be 0xFFFFFFFF.");

      //Digital switch sensors are on
      var configureDiscreteInputs = GetBaseUserDataMessageHelper(ConfigureDiscreteInputsBaseUserDataMessage.kPacketID, deviceType) as ConfigureDiscreteInputsBaseUserDataMessage;
      Assert.IsNotNull(configureDiscreteInputs, "Expected to get a ConfigureDiscreteInputsBaseUserDataMessage.");
      Assert.IsTrue(configureDiscreteInputs.EnableDiscreteInput1, "Failed to properly configure " + deviceType.ToString() + " for essentials service plan, sensor 1 should be enabled.");
      Assert.IsTrue(configureDiscreteInputs.IgnitionRequiredInput1, "Failed to properly configure " + deviceType.ToString() + " for essentials service plan, sensor 1 ignition required should be true.");
      Assert.AreEqual(4, configureDiscreteInputs.DiscreteInput1HysteresisHalfSeconds, "Failed to properly configure " + deviceType.ToString() + " for essentials service plan, sensor 1 hysteresis half seconds should be 0.");
      Assert.IsFalse(configureDiscreteInputs.DiscreteInput1HighOne, "Failed to properly configure " + deviceType.ToString() + " for essentials service plan, sensor 1 hasPosPolarity should be false.");
      if ((deviceType == DeviceTypeEnum.PL420) || (deviceType == DeviceTypeEnum.PL421))
      {
        // Sensor 2 is for the tamper switch for these two device types.  It should not be enabled by default.
        Assert.IsFalse(configureDiscreteInputs.EnableDiscreteInput2, "Failed to properly configure " + deviceType.ToString() + " for essentials service plan, sensor 2 should not be enabled.");
      }
      else
      {
        // Otherwise switch 2 should be enabled.
        Assert.IsTrue(configureDiscreteInputs.EnableDiscreteInput2, "Failed to properly configure " + deviceType.ToString() + " for essentials service plan, sensor 2 should be enabled.");
      }
      Assert.IsFalse(configureDiscreteInputs.IgnitionRequiredInput2, "Failed to properly configure " + deviceType.ToString() + " for essentials service plan, sensor 2 ignition required should be false.");
      Assert.AreEqual(4, configureDiscreteInputs.DiscreteInput2HysteresisHalfSeconds, "Failed to properly configure " + deviceType.ToString() + " for essentials service plan, sensor 2 hysteresis half seconds should be 0.");
      Assert.IsFalse(configureDiscreteInputs.DiscreteInput2HighOne, "Failed to properly configure " + deviceType.ToString() + " for essentials service plan, sensor 2 hasPosPolarity should be false.");
      Assert.IsTrue(configureDiscreteInputs.EnableDiscreteInput3, "Failed to properly configure " + deviceType.ToString() + " for essentials service plan, sensor 3 should be enabled.");
      Assert.IsFalse(configureDiscreteInputs.IgnitionRequiredInput3, "Failed to properly configure " + deviceType.ToString() + " for essentials service plan, sensor 3 ignition required should be false.");
      Assert.AreEqual(4, configureDiscreteInputs.DiscreteInput3HysteresisHalfSeconds, "Failed to properly configure " + deviceType.ToString() + " for essentials service plan, sensor 3 hysteresis half seconds should be 0.");
      Assert.IsFalse(configureDiscreteInputs.DiscreteInput3HighOne, "Failed to properly configure " + deviceType.ToString() + " for essentials service plan, sensor 3 hasPosPolarity should be false.");

      //Site Entry speed in MPH, Site Exit speed in MPH, Site Hysterisis in sec
      var zoneLogicConfiguration = GetDeviceConfigurationBaseUserDataMessageHelper(DeviceConfigurationBaseUserDataMessage.kPacketID, (int)DeviceConfigurationBaseUserDataMessage.ConfigType.ZoneLogicConfiguration, deviceType);
      Assert.AreEqual(siteEntrySpeedMPH, zoneLogicConfiguration.ZoneLogic.HomeSiteEntrySpeed, "Failed to properly configure " + deviceType.ToString() + " for essentials service plan, site entry speed should be " + siteEntrySpeedMPH + ".");
      Assert.AreEqual(siteExitSpeedMPH, zoneLogicConfiguration.ZoneLogic.HomeSiteExitSpeed, "Failed to properly configure " + deviceType.ToString() + " for essentials service plan, site exit speed should be " + siteExitSpeedMPH + ".");
      Assert.AreEqual(siteHysteresisSeconds, zoneLogicConfiguration.ZoneLogic.HomeSiteHysteresisSeconds, "Failed to properly configure " + deviceType.ToString() + " for essentials service plan, site hysterisis should be " + siteHysteresisSeconds + ".");

      //Moving radius in feet
      var movingConfiguration = GetDeviceConfigurationBaseUserDataMessageHelper(DeviceConfigurationBaseUserDataMessage.kPacketID, (int)DeviceConfigurationBaseUserDataMessage.ConfigType.MovingConfiguration, deviceType);
      Assert.AreEqual(movingConfigRadius, movingConfiguration.MovingConfig.Radius, "Failed to properly configure " + deviceType.ToString() + " for essentials service plan, moving radius should be " + movingConfigRadius + ".");

      //Speeding threshold in MPH, Speeding duration in secs, Speeding reporting enabled
      var speedingReportingConfiguration = GetDeviceConfigurationBaseUserDataMessageHelper(DeviceConfigurationBaseUserDataMessage.kPacketID, (int)DeviceConfigurationBaseUserDataMessage.ConfigType.SpeedingReportingConfiguration, deviceType);
      Assert.AreEqual((byte)(int)speedingThreshold, speedingReportingConfiguration.SpeedingReporting.SpeedThreshold, "Failed to properly configure " + deviceType.ToString() + " for essentials service plan, speeding threshold should be " + speedingThreshold + ".");
      Assert.AreEqual(speedingDurationSec, speedingReportingConfiguration.SpeedingReporting.DurationThreshold, "Failed to properly configure " + deviceType.ToString() + " for essentials service plan, speeding duration should be " + speedingDurationSec + ".");
      Assert.AreEqual(speedingReportingEnabled, speedingReportingConfiguration.SpeedingReporting.ConfigurationFlag, "Failed to properly configure " + deviceType.ToString() + " for essentials service plan, speeding reporting enabled should be " + speedingReportingEnabled + ".");

      //Stop threshold in MPH, Stop duration in sec, Stop reporting enabled
      var stoppedNotificationConfiguration = GetDeviceConfigurationBaseUserDataMessageHelper(DeviceConfigurationBaseUserDataMessage.kPacketID, (int)DeviceConfigurationBaseUserDataMessage.ConfigType.StoppedNotificationConfiguration, deviceType);
      //divide the StoppedThreshold (which is stored in SpeedThreshold) by 10 because the formatter multiplies by 10 for storage in db
      Assert.AreEqual((byte)(int)stopThreshold, stoppedNotificationConfiguration.SpeedingReporting.SpeedThreshold / 10, "Failed to properly configure " + deviceType.ToString() + " for essentials service plan, stop threshold should be " + stopThreshold + ".");
      Assert.AreEqual(stopDurationSec, stoppedNotificationConfiguration.SpeedingReporting.DurationThreshold, "Failed to properly configure " + deviceType.ToString() + " for essentials service plan, stop duration should be " + stopDurationSec + ".");
      Assert.AreEqual(stopReportingEnabled, stoppedNotificationConfiguration.SpeedingReporting.ConfigurationFlag, "Failed to properly configure " + deviceType.ToString() + " for essentials service plan, stop reporting enabled should be " + stopReportingEnabled + ".");

      if (deviceType == DeviceTypeEnum.PL420)
      {
        //Main power loss reporting enabled
        var mainPowerLossReporting = GetDeviceConfigurationBaseUserDataMessageHelper(DeviceConfigurationBaseUserDataMessage.kPacketID, (int)DeviceConfigurationBaseUserDataMessage.ConfigType.MainPowerLossReporting, deviceType);
        Assert.IsTrue(mainPowerLossReporting.MainPowerLossReportingConfig.isEnabled, "Failed to properly configure " + deviceType.ToString() + " for essentials service plan, main power loss reporting enabled should be true.");

        //Suspicious Move reporting enabled
        var suspiciousMoveReporting = GetDeviceConfigurationBaseUserDataMessageHelper(DeviceConfigurationBaseUserDataMessage.kPacketID, (int)DeviceConfigurationBaseUserDataMessage.ConfigType.SuspiciousMoveReporting, deviceType);
        Assert.IsTrue(suspiciousMoveReporting.SuspiciousMoveReportingConfig.isEnabled, "Failed to properly configure " + deviceType.ToString() + " for essentials service plan, suspicious move reporting enabled should be true.");
      }
    }

    private List<DeviceConfig.ServicePlanIDs> SetupDeviceForServicePlanHelper(Device device)
    {
      device.GpsDeviceID = GPS_DEVICE_ID;
      Asset asset = Entity.Asset.WithDevice(device).SerialNumberVin(GPS_DEVICE_ID).WithCoreService().Save();
      Helpers.NHRaw.AddDeviceToRawDevice(device);
      int utcNowKeyDate = DateTime.UtcNow.KeyDate();
      var currentViews = (from view in Ctx.OpContext.ServiceViewReadOnly
                          join service in Ctx.OpContext.ServiceReadOnly on view.fk_ServiceID equals service.ID
                          join st in Ctx.OpContext.ServiceTypeReadOnly on service.fk_ServiceTypeID equals st.ID
                          where view.fk_AssetID == asset.AssetID
                                && view.StartKeyDate <= utcNowKeyDate
                                && view.EndKeyDate > utcNowKeyDate
                                && view.Customer.fk_CustomerTypeID != (int)CustomerTypeEnum.Corporate
                          orderby st.ID
                          select new DeviceConfig.ServicePlanIDs
                          {
                            PlanID = st.ID,
                            IsCore = st.IsCore,
                          }).ToList();
      return currentViews;
    }

    private DeviceConfigurationBaseUserDataMessage GetDeviceConfigurationBaseUserDataMessageHelper(int? typeId, int? subTypeId, DeviceTypeEnum deviceType, string serialNumber = GPS_DEVICE_ID)
    {
      MTSOut mtsOut = (from m in Ctx.RawContext.MTSOutReadOnly
                       where m.TypeID == typeId &&
                             m.SubTypeID == subTypeId &&
                             m.SerialNumber == serialNumber &&
                             m.DeviceType == (int)deviceType
                       select m).FirstOrDefault();
      return ((PlatformMessage.HydratePlatformMessage(mtsOut.Payload, true, false) as UserDataBaseMessage).Message) as DeviceConfigurationBaseUserDataMessage;
    }

    private BaseUserDataMessage GetBaseUserDataMessageHelper(int? typeId, DeviceTypeEnum deviceType, string serialNumber = GPS_DEVICE_ID)
    {
      MTSOut mtsOut = (from m in Ctx.RawContext.MTSOutReadOnly
                       where m.TypeID == typeId &&
                             m.SerialNumber == serialNumber &&
                             m.DeviceType == (int)deviceType
                       select m).FirstOrDefault();
      return (PlatformMessage.HydratePlatformMessage(mtsOut.Payload, true, false) as UserDataBaseMessage).Message;
    }

    #endregion

    #region ServicePlan Helpers

    private static List<DeviceConfig.ServicePlanIDs> GenerateCoreServicePlanHelper()
    {
      return new List<DeviceConfig.ServicePlanIDs>
               {
                 new DeviceConfig.ServicePlanIDs
                   {
                     IsCore = true,
                     PlanID = (int) ServiceTypeEnum.Essentials
                   }
               };
    }

    private static List<DeviceConfig.ServicePlanIDs> GenerateCATServicePlanHelper()
    {
      var servicePlans = GenerateCoreServicePlanHelper();
      servicePlans.Add(new DeviceConfig.ServicePlanIDs
                         {
                           IsCore = false,
                           PlanID = (int) ServiceTypeEnum.CATUtilization
                         });

      return servicePlans;
    }

    private static List<DeviceConfig.ServicePlanIDs> GenerateOneMinuteServicePlanHelper()
    {
      var servicePlans = GenerateCATServicePlanHelper();
      servicePlans.Add(new DeviceConfig.ServicePlanIDs
                         {
                           IsCore = false,
                           PlanID = (int) ServiceTypeEnum.e1minuteUpdateRateUpgrade
                         });

      return servicePlans;
    }

    #endregion

    #region Interval Check

    private void CheckMTSDeviceInterval(string gpsDeviceId, ServiceTypeEnum serviceType = ServiceTypeEnum.Essentials)
    {
      var mtsDevice = (Ctx.RawContext.MTSDeviceReadOnly.Where(m => m.SerialNumber == gpsDeviceId)).SingleOrDefault();
      IntervalMatchHelper(mtsDevice, serviceType);
    }

    private static void IntervalMatchHelper(MTSDevice mtsDevice, ServiceTypeEnum serviceTypeEnum)
    {
      switch (serviceTypeEnum)
      {
        case ServiceTypeEnum.Essentials:
          MatchVLCore(mtsDevice);
          break;
        case ServiceTypeEnum.CATUtilization:
          MatchCATUTIL(mtsDevice);
          break;
        case ServiceTypeEnum.e1minuteUpdateRateUpgrade:
          MatchONEMINUTERATE(mtsDevice);
          break;
      }
    }

    private static void MatchVLCore(MTSDevice mtsDevice)
    {
      const int samplingInterval = 21600;
      const int reportingInterval = 21600;

      Assert.AreEqual(samplingInterval, mtsDevice.SampleRate, "VLCore plans sampling interval is incorrect.");
      Assert.AreEqual(reportingInterval, mtsDevice.UpdateRate, "VLCore Reporting interval is incorrect.");
    }

    private static void MatchCATUTIL(MTSDevice mtsDevice)
    {
      const int samplingInterval = 3600;
      const int reportingInterval = 3600;

      Assert.AreEqual(samplingInterval, mtsDevice.SampleRate, "CATUTIL plans sampling interval is incorrect.");
      Assert.AreEqual(reportingInterval, mtsDevice.UpdateRate, "CATUTIL Reporting interval is incorrect.");
    }

    private static void MatchONEMINUTERATE(MTSDevice mtsDevice)
    {
      const int samplingInterval = 60;
      const int reportingInterval = 600;

      Assert.AreEqual(samplingInterval, mtsDevice.SampleRate, "ONEMINRATE plans sampling interval is incorrect.");
      Assert.AreEqual(reportingInterval, mtsDevice.UpdateRate, "ONEMINRATE Reporting interval is incorrect.");
    }

    #endregion

    #endregion
  }
}