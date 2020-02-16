using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Hosted.VLCommon;

namespace UnitTests
{
  // TODO: Refactor setup for these tests to reduce the amount of code to set up these cases.
  [TestClass]
  public class A5N2ConfigDataTest
  {
    [TestMethod]
    public void A5N2ConfigData_Switches_SuccessWithAllData()
    {
      A5N2ConfigData data = new A5N2ConfigData();
      data.CurrentDigitalSwitches = new List<A5N2ConfigData.DigitalSwitchConfig>();

      var now = DateTime.UtcNow;

      data.CurrentDigitalSwitches.Add(
        new A5N2ConfigData.DigitalSwitchConfig()
        {
          SwitchNumber = 1,
          DelayTime = new TimeSpan(0, 0, 0, 0, 100),
          Enabled = true,
          MessageSourceID = 1,
          MonitoringCondition = DigitalInputMonitoringConditions.Always,
          SentUTC = now
        });

      data.CurrentDigitalSwitches.Add(
        new A5N2ConfigData.DigitalSwitchConfig()
        {
          SwitchNumber = 2,
          DelayTime = new TimeSpan(0, 0, 0, 0, 200),
          Enabled = true,
          MessageSourceID = 2,
          MonitoringCondition = DigitalInputMonitoringConditions.KeyOnEngineOn,
          SentUTC = now
        });

      XElement dataElement = data.ToXElement();
      A5N2ConfigData data2 = new A5N2ConfigData(dataElement);
      Assert.AreEqual(dataElement.ToString(), data2.ToXElement().ToString(), "XML Does Not Equal");
    }


    [TestMethod]
    public void A5N2ConfigData_Switches_Update_Success()
    {
      A5N2ConfigData data = new A5N2ConfigData();
      //data.CurrentDailyReport = new A5N2ConfigData.DailyReportConfig();
      data.CurrentDigitalSwitches = new List<A5N2ConfigData.DigitalSwitchConfig>();
      data.CurrentDigitalSwitches.Add(new A5N2ConfigData.DigitalSwitchConfig() { SwitchNumber = 1, DelayTime = new TimeSpan(0, 0, 0, 0, 100), Enabled = true, MessageSourceID = 1, MonitoringCondition = DigitalInputMonitoringConditions.Always, SentUTC = DateTime.UtcNow });

      var newConfig = new A5N2ConfigData.DigitalSwitchConfig()
      {
        SwitchNumber = 1,
        DelayTime = new TimeSpan(0, 0, 0, 0, 200),
        Enabled = true,
        MessageSourceID = 2,
        MonitoringCondition = DigitalInputMonitoringConditions.KeyOnEngineOn,
        SentUTC = DateTime.UtcNow
      };

      data.Update(newConfig);

      Assert.AreEqual(newConfig.MessageSourceID, data.CurrentDigitalSwitches[0].MessageSourceID, "current should have the new config");
      //Assert.IsNotNull(data.PendingDailyReport, "pending should not be null");

      Assert.AreEqual(1, data.CurrentDigitalSwitches.Count, "Should only have 1 switch configured");
    }

    [TestMethod]
    public void A5N2ConfigData_Switches_DiscreteInputConfig_Success()
    {
      A5N2ConfigData data = new A5N2ConfigData();
      data.CurrentDigitalSwitches = new List<A5N2ConfigData.DigitalSwitchConfig>();

      var now = DateTime.UtcNow;

      data.CurrentDigitalSwitches.Add(
        new A5N2ConfigData.DigitalSwitchConfig()
        {
          SwitchNumber = 1,
          DelayTime = new TimeSpan(0, 0, 0, 0, 100),
          Enabled = true,
          MessageSourceID = 1,
          MonitoringCondition = DigitalInputMonitoringConditions.Always,
          SentUTC = now
        });

      data.CurrentDigitalSwitches.Add(
        new A5N2ConfigData.DigitalSwitchConfig()
        {
          SwitchNumber = 2,
          DelayTime = new TimeSpan(0, 0, 0, 0, 200),
          Enabled = true,
          MessageSourceID = 2,
          MonitoringCondition = DigitalInputMonitoringConditions.KeyOnEngineOn,
          SentUTC = now
        });

      XElement dataElement = data.ToXElement();
      A5N2ConfigData data2 = new A5N2ConfigData(dataElement);

      int switchIndex = 0;
      Assert.AreEqual(1, data.CurrentDigitalSwitches[switchIndex].SwitchNumber, "SwitchNumbers should match");
      Assert.AreEqual(new TimeSpan(0, 0, 0, 0, 100), data.CurrentDigitalSwitches[switchIndex].DelayTime, "Delay should match");
      Assert.AreEqual(true, data.CurrentDigitalSwitches[switchIndex].Enabled, "Enabled should match");
      Assert.AreEqual(1, data.CurrentDigitalSwitches[switchIndex].MessageSourceID, "Message Source ID should match");
      Assert.AreEqual((int)DigitalInputMonitoringConditions.Always, (int)data.CurrentDigitalSwitches[switchIndex].MonitoringCondition, "Monitoring Condition should match");

      switchIndex = 1;
      Assert.AreEqual(2, data.CurrentDigitalSwitches[switchIndex].SwitchNumber, "SwitchNumbers should match");
      Assert.AreEqual(new TimeSpan(0, 0, 0, 0, 200), data.CurrentDigitalSwitches[switchIndex].DelayTime, "Delay should match");
      Assert.AreEqual(true, data.CurrentDigitalSwitches[switchIndex].Enabled, "Enabled should match");
      Assert.AreEqual(2, data.CurrentDigitalSwitches[switchIndex].MessageSourceID, "Message Source ID should match");
      Assert.AreEqual((int)DigitalInputMonitoringConditions.KeyOnEngineOn, (int)data.CurrentDigitalSwitches[switchIndex].MonitoringCondition, "Monitoring Condition should match");
    }


    [TestMethod]
    public void A5N2ConfigData_AuditConfigChanges_DataInFlow_UpdateSuccess()
    {
      var nhOpContextMock = new Mock<INH_OP>();
      var assetSecurityIncidentMock = new Mock<IObjectSet<AssetSecurityIncident>>();

      nhOpContextMock.SetupGet(x => x.AssetSecurityIncident).Returns(assetSecurityIncidentMock.Object);
      var stubAsset = new Asset
      {
        SerialNumberVIN = "ABC123",
        fk_MakeCode = "CAT",
        Device = new Device
        {
          fk_DeviceTypeID = 16
        }
      };
      var tamper = CreateTamperBlock();
      var config = new A5N2ConfigData { CurrentAssetSecurityTamperLevel = tamper, IsAuditRequired = true};
      var testConfig = new A5N2ConfigData.AssetSecurityTamperLevel
      {
        Status = MessageStatusEnum.Acknowledged
      };
      config.AuditConfigChanges(nhOpContextMock.Object, stubAsset, testConfig);

      assetSecurityIncidentMock.Verify(t => t.AddObject(It.IsAny<AssetSecurityIncident>()));
    }

    [TestMethod]
    public void A5N2ConfigData_Update_ForDataInTamperBlock_CurrentTamperLevelIsUpdated()
    {
      var config = new A5N2ConfigData();
      config.Update(CreateTamperBlock());
      AssertsForTamperBlock(config);
    }

    [TestMethod]
    public void A5N2ConfigData_Update_ForDataInStartModeBlock_CurrentStartModeIsUpdated()
    {
      var config = new A5N2ConfigData();
      config.Update(CreateStartModeBlock());
      AssertsForStartModeBlock(config);
    }

    [TestMethod]
    public void A5N2ConfigData_Update_ForDataInStartModeAndTamperBlock_CurrentTamperLevelAndStartModeIsUpdated()
    {
      var config = new A5N2ConfigData();
      config.Update(CreateAssetSecurityStatusBlock());
      AssertsForStartModeBlock(config);
      AssertsForTamperBlock(config);
    }

    [TestMethod]
    public void A5N2ConfigData_Update_ForDataInPendingStartModeAndTamperBlock_CurrentTamperLevelAndPendingStartModeIsUpdated()
    {
      var config = new A5N2ConfigData();
      config.Update(CreateAssetSecurityPendingStatusBlock());
      AssertsForPendingStartModeBlock(config);
      AssertsForTamperBlock(config);
    }

    private static void AssertsForTamperBlock(A5N2ConfigData config)
    {
      Assert.IsNotNull(config.CurrentAssetSecurityTamperLevel);
      Assert.IsNull(config.LastSentAssetSecurityTamperLevel);
      Assert.IsTrue(config.CurrentAssetSecurityTamperLevel.TamperLevel == TamperResistanceStatus.TamperResistanceLevel1);
      Assert.IsTrue(config.CurrentAssetSecurityTamperLevel.TamperConfigurationSource ==
                    TamperResistanceModeConfigurationSource.CatElectronicsTechnician);
    }

    private static void AssertsForStartModeBlock(A5N2ConfigData config)
    {
      Assert.IsNotNull(config.CurrentAssetSecurityConfig);
      Assert.IsNull(config.LastSentAssetSecurityConfig);
      Assert.IsTrue(config.CurrentAssetSecurityConfig.MachineStartStatus == MachineStartStatus.Disabled);
      Assert.IsTrue(config.CurrentAssetSecurityConfig.MachineStartStatusTrigger == MachineStartStatusTrigger.OTACommand);
    }

    private static void AssertsForPendingStartModeBlock(A5N2ConfigData config)
    {
      Assert.IsNotNull(config.PendingAssetSecurityConfig);
      Assert.IsTrue(config.PendingAssetSecurityConfig.MachineStartStatus == MachineStartStatus.Disabled);
      Assert.IsTrue(config.PendingAssetSecurityConfig.MachineStartModeConfigSource == MachineStartModeConfigurationSource.CatElectronicsTechnician);
    }

    private static A5N2ConfigData.AssetSecurityTamperLevel CreateTamperBlock()
    {
      var tamper = new A5N2ConfigData.AssetSecurityTamperLevel
      {
        TamperConfigurationSource = TamperResistanceModeConfigurationSource.CatElectronicsTechnician,
        TamperLevel = TamperResistanceStatus.TamperResistanceLevel1,
        Status = MessageStatusEnum.Acknowledged
      };
      return tamper;
    }

    private static A5N2ConfigData.AssetSecurityStartStatus CreateStartModeBlock()
    {
      var startStatus = new A5N2ConfigData.AssetSecurityStartStatus()
      {
        MachineStartStatus = MachineStartStatus.Disabled,
        MachineStartStatusTrigger = MachineStartStatusTrigger.OTACommand,
        Status = MessageStatusEnum.Acknowledged
      };
      return startStatus;
    }

    private static A5N2ConfigData.AssetSecurityPendingStartStatus CreatePendingStartModeBlock()
    {
      var startStatus = new A5N2ConfigData.AssetSecurityPendingStartStatus
      {
        MachineStartStatus = MachineStartStatus.Disabled,
        MachineStartModeConfigSource = MachineStartModeConfigurationSource.CatElectronicsTechnician,
        Status = MessageStatusEnum.Acknowledged
      };
      return startStatus;
    }

    private static A5N2ConfigData.AssetSecurityStatus CreateAssetSecurityStatusBlock()
    {
      var assetSecurityStatus = new A5N2ConfigData.AssetSecurityStatus()
      {
        AssetSecurityStartStatus = CreateStartModeBlock(),
        AssetSecurityTamperLevel = CreateTamperBlock(),
        Status = MessageStatusEnum.Acknowledged
      };
      return assetSecurityStatus;
    }

    private static A5N2ConfigData.AssetSecurityPendingStatus CreateAssetSecurityPendingStatusBlock()
    {
      var assetSecurityStatus = new A5N2ConfigData.AssetSecurityPendingStatus()
      {
        AssetSecurityPendingStartStatus = CreatePendingStartModeBlock(),
        AssetSecurityTamperLevel = CreateTamperBlock(),
        Status = MessageStatusEnum.Acknowledged
      };
      return assetSecurityStatus;
    }
  }
}
