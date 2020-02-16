using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon.MTSMessages;
using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;

namespace UnitTests
{
    
    
    /// <summary>
    ///This is a test class for MachineEventMessageTest and is intended
    ///to contain all MachineEventMessageTest Unit Tests
    ///</summary>
  [TestClass()]
  public class MachineEventMessageTest : UnitTestBase
  {
    [TestMethod]
    public void UnknownDistanceTraveled()
    {
      MachineEventMessage machineEventMessage = new MachineEventMessage();
      machineEventMessage.DevicePacketSequenceID = 2;
      machineEventMessage.UtcDateTime = DateTime.UtcNow;
      machineEventMessage.Latitude = 1;
      machineEventMessage.Latitude = 12;
      machineEventMessage.SpeedMPH = 1;
      machineEventMessage.ServiceMeterHours = 12 / 3600;
      machineEventMessage.MilesTraveled = null;
      uint bitPosition = 0;
      byte[] bytes = PlatformMessage.SerializePlatformMessage(machineEventMessage, null, ref bitPosition, true);
      MachineEventMessage msg = PlatformMessage.HydratePlatformMessage(bytes, true, true) as MachineEventMessage;
      Assert.IsNotNull(msg, "message should be machineEvent");
      Assert.IsNull(msg.MilesTraveled, "MilesTraveled should be null");
      machineEventMessage.MilesTraveled = 15 * 0.062137119;
      bitPosition = 0;
      bytes = PlatformMessage.SerializePlatformMessage(machineEventMessage, null, ref bitPosition, true);
      msg = PlatformMessage.HydratePlatformMessage(bytes, true, true) as MachineEventMessage;
      Assert.IsNotNull(msg, "message should be machineEvent");
      Assert.AreEqual(machineEventMessage.MilesTraveled, msg.MilesTraveled, "MilesTraveled is incorrect");
    }

    [TestMethod]
    public void SerializeFuelEngineReportTest()
    {
      MachineEventMessage machineEventMessage = new MachineEventMessage();
      machineEventMessage.DevicePacketSequenceID = 2;
      machineEventMessage.UtcDateTime = DateTime.UtcNow;
      machineEventMessage.Latitude = 1;
      machineEventMessage.Latitude = 12;
      machineEventMessage.SpeedMPH = 1;
      machineEventMessage.ServiceMeterHours = 12 / 3600;
      machineEventMessage.Blocks = new MachineEventBlock[1];
      FuelEnginePayloadCycleCountBlock fepc = new FuelEnginePayloadCycleCountBlock();
      fepc.SubType = 0x00;
      FuelEngineReport fuelEngine = new FuelEngineReport();
      fuelEngine.TransactionVersion = 0x01;
      fuelEngine.ReportingECMs = new ReportingECM[1];
      fuelEngine.ReportingECMs[0] = new ReportingECM();
      fuelEngine.ReportingECMs[0].ECMIdentifier = 27;
      fuelEngine.ReportingECMs[0].FuelConsumption = 12;
      fuelEngine.ReportingECMs[0].FuelLevel = 15;
      fuelEngine.ReportingECMs[0].NumberEngineStarts = 2;
      fuelEngine.ReportingECMs[0].TotalEngineIdleTime = TimeSpan.FromHours(2);
      fuelEngine.ReportingECMs[0].TotalEngineRevolutions = 12;
      fuelEngine.ReportingECMs[0].TotalIdleFuel = 1;
      fuelEngine.ReportingECMs[0].TotalMachineIdleFuel = 2;
      fuelEngine.ReportingECMs[0].TotalMachineIdleTime = TimeSpan.FromHours(15);
      fuelEngine.ReportingECMs[0].TotalMaximumFuelGallons = 100;
      fepc.Message = fuelEngine;
      MachineEventBlock block1 = new MachineEventBlock();
      block1.DeltaTime = TimeSpan.FromMinutes(-2);
      block1.GatewayData = fepc;
      machineEventMessage.Blocks[0] = block1;

      uint bitPosition = 0;
      byte[] bytes = PlatformMessage.SerializePlatformMessage(machineEventMessage, null, ref bitPosition, true);
      MachineEventMessage msg = PlatformMessage.HydratePlatformMessage(bytes, true, true) as MachineEventMessage;
      Assert.IsNotNull(msg, "message should be machineEvent");
      Assert.IsNotNull(msg.Blocks, "message should have machine event block");
      Assert.AreEqual(1, msg.Blocks.Length, "Message should only have 1 block");
      Assert.IsNotNull(msg.Blocks[0].GatewayData, "message should have Machine Event Block Data");
      FuelEnginePayloadCycleCountBlock fepcActual = msg.Blocks[0].GatewayData as FuelEnginePayloadCycleCountBlock;
      Assert.IsNotNull(fepcActual, "Data Should be FuelEnginePayloadCycleCountBlock");
      FuelEngineReport fuelEngineActual = fepcActual.Message as FuelEngineReport;
      Assert.IsNotNull(fuelEngineActual, "msg block data should be FuelEngineReport");
      Assert.AreEqual(fuelEngine.TransactionVersion, fuelEngineActual.TransactionVersion, "TransactionVersions do not equal");
      Assert.IsNotNull(fuelEngineActual.ReportingECMs, "Reporting ECMs should not be null");
      Assert.IsTrue(fuelEngineActual.ReportingECMs.Length == 1, "Reporting ECMs Count should be 1");
      Assert.AreEqual(fuelEngine.ReportingECMs[0].TotalMachineIdleTime, fuelEngineActual.ReportingECMs[0].TotalMachineIdleTime, "TotalMachineIdleTimes do not equal");
      Assert.AreEqual(fuelEngine.ReportingECMs[0].TotalMachineIdleFuel, fuelEngineActual.ReportingECMs[0].TotalMachineIdleFuel, "TotalMachineIdleFuel do not equal");
      Assert.AreEqual(fuelEngine.ReportingECMs[0].TotalMaximumFuelGallons, fuelEngineActual.ReportingECMs[0].TotalMaximumFuelGallons, "TotalMaximumFuelGallons do not equal");
      Assert.AreEqual(fuelEngine.ReportingECMs[0].TotalIdleFuel, fuelEngineActual.ReportingECMs[0].TotalIdleFuel, "TotalIdleFuel do not equal");
      Assert.AreEqual(fuelEngine.ReportingECMs[0].TotalEngineRevolutions, fuelEngineActual.ReportingECMs[0].TotalEngineRevolutions, "TotalEngineRevolutions do not equal");
      Assert.AreEqual(fuelEngine.ReportingECMs[0].TotalEngineIdleTime, fuelEngineActual.ReportingECMs[0].TotalEngineIdleTime, "TotalEngineIdleTime do not equal");
      Assert.AreEqual(fuelEngine.ReportingECMs[0].NumberEngineStarts, fuelEngineActual.ReportingECMs[0].NumberEngineStarts, "NumberEngineStarts do not equal");
      Assert.AreEqual(fuelEngine.ReportingECMs[0].FuelLevel, fuelEngineActual.ReportingECMs[0].FuelLevel, "FuelLevel do not equal");
      Assert.AreEqual(fuelEngine.ReportingECMs[0].FuelConsumption, fuelEngineActual.ReportingECMs[0].FuelConsumption, "FuelConsumption do not equal");
      Assert.AreEqual(fuelEngine.ReportingECMs[0].ECMIdentifier, fuelEngineActual.ReportingECMs[0].ECMIdentifier, "ECMIdentifier do not equal");
    }

    [TestMethod]
    public void SerializePayloadAndCycleCountReportTest()
    {
      MachineEventMessage machineEventMessage = new MachineEventMessage();
      machineEventMessage.DevicePacketSequenceID = 2;
      machineEventMessage.UtcDateTime = DateTime.UtcNow;
      machineEventMessage.Latitude = 1;
      machineEventMessage.Latitude = 12;
      machineEventMessage.SpeedMPH = 1;
      machineEventMessage.ServiceMeterHours = 12 / 3600;
      machineEventMessage.Blocks = new MachineEventBlock[1];
      FuelEnginePayloadCycleCountBlock fepc = new FuelEnginePayloadCycleCountBlock();
      fepc.SubType = 0x03;
      PayloadAndCycleCountReport payloadCycleCount = new PayloadAndCycleCountReport();
      payloadCycleCount.TransactionVersion = 0x00;
      payloadCycleCount.NumberOfECMs = 1;
      payloadCycleCount.PayloadCycleCountECMs = new PayloadCycleCountECM[1];
      payloadCycleCount.PayloadCycleCountECMs[0] = new PayloadCycleCountECM();
      payloadCycleCount.PayloadCycleCountECMs[0].ECMIdentifier = 27;
      payloadCycleCount.PayloadCycleCountECMs[0].TotalPayload = 1024;
      payloadCycleCount.PayloadCycleCountECMs[0].TotalCycles = 42;
      fepc.Message = payloadCycleCount;
      MachineEventBlock block1 = new MachineEventBlock();
      block1.DeltaTime = TimeSpan.FromMinutes(-2);
      block1.GatewayData = fepc;
      machineEventMessage.Blocks[0] = block1;

      uint bitPosition = 0;
      byte[] bytes = PlatformMessage.SerializePlatformMessage(machineEventMessage, null, ref bitPosition, true);
      MachineEventMessage msg = PlatformMessage.HydratePlatformMessage(bytes, true, true) as MachineEventMessage;
      Assert.IsNotNull(msg, "message should be machineEvent");
      Assert.IsNotNull(msg.Blocks, "message should have machine event block");
      Assert.AreEqual(1, msg.Blocks.Length, "Message should only have 1 block");
      Assert.IsNotNull(msg.Blocks[0].GatewayData, "message should have Machine Event Block Data");
      FuelEnginePayloadCycleCountBlock fepcActual = msg.Blocks[0].GatewayData as FuelEnginePayloadCycleCountBlock;
      Assert.IsNotNull(fepcActual, "Data Should be FuelEnginePayloadCycleCountBlock");
      PayloadAndCycleCountReport payloadCycleCountActual = fepcActual.Message as PayloadAndCycleCountReport;
      Assert.IsNotNull(payloadCycleCountActual, "msg block data should be PayloadAndCycleCountReport");
      Assert.AreEqual(payloadCycleCount.TransactionVersion, payloadCycleCountActual.TransactionVersion, "TransactionVersions do not equal");
      Assert.IsNotNull(payloadCycleCountActual.PayloadCycleCountECMs, "Reporting ECMs should not be null");
      Assert.IsTrue(payloadCycleCountActual.PayloadCycleCountECMs.Length == 1, "Reporting ECMs Count should be 1");
      Assert.AreEqual(payloadCycleCount.PayloadCycleCountECMs[0].ECMIdentifier, payloadCycleCountActual.PayloadCycleCountECMs[0].ECMIdentifier, "ECMIdentifier do not equal");
      Assert.AreEqual(payloadCycleCount.PayloadCycleCountECMs[0].TotalPayload, payloadCycleCountActual.PayloadCycleCountECMs[0].TotalPayload, "TotalPayload do not equal");
      Assert.AreEqual(payloadCycleCount.PayloadCycleCountECMs[0].TotalCycles, payloadCycleCountActual.PayloadCycleCountECMs[0].TotalCycles, "TotalCycles do not equal");
    }

    [TestMethod]
    public void SerializePayloadAndCycleCountReportTest_SetNumberOfECMsToFF()
    {
      MachineEventMessage machineEventMessage = new MachineEventMessage();
      machineEventMessage.DevicePacketSequenceID = 2;
      machineEventMessage.UtcDateTime = DateTime.UtcNow;
      machineEventMessage.Latitude = 1;
      machineEventMessage.Latitude = 12;
      machineEventMessage.SpeedMPH = 1;
      machineEventMessage.ServiceMeterHours = 12 / 3600;
      machineEventMessage.Blocks = new MachineEventBlock[1];
      FuelEnginePayloadCycleCountBlock fepc = new FuelEnginePayloadCycleCountBlock();
      fepc.SubType = 0x03;
      PayloadAndCycleCountReport payloadCycleCount = new PayloadAndCycleCountReport();
      payloadCycleCount.TransactionVersion = 0x00;
      payloadCycleCount.NumberOfECMs = byte.MaxValue;
      fepc.Message = payloadCycleCount;
      MachineEventBlock block1 = new MachineEventBlock();
      block1.DeltaTime = TimeSpan.FromMinutes(-2);
      block1.GatewayData = fepc;
      machineEventMessage.Blocks[0] = block1;

      uint bitPosition = 0;
      byte[] bytes = PlatformMessage.SerializePlatformMessage(machineEventMessage, null, ref bitPosition, true);
      MachineEventMessage msg = PlatformMessage.HydratePlatformMessage(bytes, true, true) as MachineEventMessage;
      Assert.IsNotNull(msg, "message should be machineEvent");
      Assert.IsNotNull(msg.Blocks, "message should have machine event block");
      Assert.AreEqual(1, msg.Blocks.Length, "Message should only have 1 block");
      Assert.IsNotNull(msg.Blocks[0].GatewayData, "message should have Machine Event Block Data");
      FuelEnginePayloadCycleCountBlock fepcActual = msg.Blocks[0].GatewayData as FuelEnginePayloadCycleCountBlock;
      Assert.IsNotNull(fepcActual, "Data Should be FuelEnginePayloadCycleCountBlock");
      PayloadAndCycleCountReport payloadCycleCountActual = fepcActual.Message as PayloadAndCycleCountReport;
      Assert.IsNotNull(payloadCycleCountActual, "msg block data should be PayloadAndCycleCountReport");
      Assert.AreEqual(payloadCycleCount.TransactionVersion, payloadCycleCountActual.TransactionVersion, "TransactionVersions do not equal");

      Assert.AreEqual(byte.MaxValue, payloadCycleCountActual.NumberOfECMsUnConverted, "Expected NumberOfECMsUnConverted to be 0xFF");
      Assert.IsNull(payloadCycleCountActual.NumberOfECMs, "NumberOfECMs should be null");
      Assert.IsNull(payloadCycleCountActual.PayloadCycleCountECMs, "payloadCycleCountECMs should be null");
    }

    [TestMethod]
    public void SerializePayloadAndCycleCountReportTest_SetPayloadAndCyclesToFF()
    {
      MachineEventMessage machineEventMessage = new MachineEventMessage();
      machineEventMessage.DevicePacketSequenceID = 2;
      machineEventMessage.UtcDateTime = DateTime.UtcNow;
      machineEventMessage.Latitude = 1;
      machineEventMessage.Latitude = 12;
      machineEventMessage.SpeedMPH = 1;
      machineEventMessage.ServiceMeterHours = 12 / 3600;
      machineEventMessage.Blocks = new MachineEventBlock[1];
      FuelEnginePayloadCycleCountBlock fepc = new FuelEnginePayloadCycleCountBlock();
      fepc.SubType = 0x03;
      PayloadAndCycleCountReport payloadCycleCount = new PayloadAndCycleCountReport();
      payloadCycleCount.TransactionVersion = 0x00;
      payloadCycleCount.NumberOfECMs = 1;
      payloadCycleCount.PayloadCycleCountECMs = new PayloadCycleCountECM[1];
      payloadCycleCount.PayloadCycleCountECMs[0] = new PayloadCycleCountECM();
      payloadCycleCount.PayloadCycleCountECMs[0].ECMIdentifier = 27;
      payloadCycleCount.PayloadCycleCountECMs[0].TotalPayload = uint.MaxValue;
      payloadCycleCount.PayloadCycleCountECMs[0].TotalCycles = uint.MaxValue;
      fepc.Message = payloadCycleCount;
      MachineEventBlock block1 = new MachineEventBlock();
      block1.DeltaTime = TimeSpan.FromMinutes(-2);
      block1.GatewayData = fepc;
      machineEventMessage.Blocks[0] = block1;

      uint bitPosition = 0;
      byte[] bytes = PlatformMessage.SerializePlatformMessage(machineEventMessage, null, ref bitPosition, true);
      MachineEventMessage msg = PlatformMessage.HydratePlatformMessage(bytes, true, true) as MachineEventMessage;
      Assert.IsNotNull(msg, "message should be machineEvent");
      Assert.IsNotNull(msg.Blocks, "message should have machine event block");
      Assert.AreEqual(1, msg.Blocks.Length, "Message should only have 1 block");
      Assert.IsNotNull(msg.Blocks[0].GatewayData, "message should have Machine Event Block Data");
      FuelEnginePayloadCycleCountBlock fepcActual = msg.Blocks[0].GatewayData as FuelEnginePayloadCycleCountBlock;
      Assert.IsNotNull(fepcActual, "Data Should be FuelEnginePayloadCycleCountBlock");
      PayloadAndCycleCountReport payloadCycleCountActual = fepcActual.Message as PayloadAndCycleCountReport;
      Assert.IsNotNull(payloadCycleCountActual, "msg block data should be PayloadAndCycleCountReport");
      Assert.AreEqual(payloadCycleCount.TransactionVersion, payloadCycleCountActual.TransactionVersion, "TransactionVersions do not equal");
      Assert.IsNotNull(payloadCycleCountActual.PayloadCycleCountECMs, "Reporting ECMs should not be null");
      Assert.IsTrue(payloadCycleCountActual.PayloadCycleCountECMs.Length == 1, "Reporting ECMs Count should be 1");
      Assert.AreEqual(payloadCycleCount.PayloadCycleCountECMs[0].ECMIdentifier, payloadCycleCountActual.PayloadCycleCountECMs[0].ECMIdentifier, "ECMIdentifier do not equal");
      Assert.AreEqual(uint.MaxValue, payloadCycleCountActual.PayloadCycleCountECMs[0].TotalPayloadUnConverted, "TotalPayloadUnConverted should be 0xFF");
      Assert.IsNull(payloadCycleCountActual.PayloadCycleCountECMs[0].TotalPayload, "TotalPayload should be null");
      Assert.AreEqual(uint.MaxValue, payloadCycleCountActual.PayloadCycleCountECMs[0].TotalCyclesUnConverted, "TotalCyclesUnConverted should be 0xFF");
      Assert.IsNull(payloadCycleCountActual.PayloadCycleCountECMs[0].TotalCycles, "TotalCycles should be null");
    }

    [TestMethod]
    public void EcmInformationMessageWithDatalinkTypeCDLAndJ1939Test()
    {
      MachineEventMessage expectedMachineEventMessage = SetUpMachineEventMessage(DeviceIDData.DataLinkType.CDLAndJ1939);
      AssertMachineEventMessage(expectedMachineEventMessage);
    }

    [TestMethod]
    public void EcmInformationMessageTestWithDatalinkTypeJ1939()
    {
      MachineEventMessage expectedMachineEventMessage = SetUpMachineEventMessage(DeviceIDData.DataLinkType.J1939);
      AssertMachineEventMessage(expectedMachineEventMessage);
    }

    [TestMethod]
    public void EcmInformationMessageTestWithDatalinkTypeCDL()
    {
      MachineEventMessage expectedMachineEventMessage = SetUpMachineEventMessage(DeviceIDData.DataLinkType.CDL);
      AssertMachineEventMessage(expectedMachineEventMessage);
    }
    
    [TestMethod]
    public void ECMInformationMessageVersion2Test()
    {
      string msgPayload = "02700007C05939AE00000000000000000000000000000010000001FFFF0053005102022276657273696F6E363838387473616D0076657273696F6E3638386A73616D0000012D6203810DC90161AFE1A198080F2AA2383036383938362D303038303134423030394C51323334373935362D3032B4";
      uint bitPosition = 0;
      //<Block Type="ECMInfo">
      //                              <version>2</version>
      //                              <EventUTC>2014-07-11 07:14:00.000</EventUTC>
      //                              <DeltaTime>-1</DeltaTime>
      //                              <Engine>version6,888tsam</Engine>
      //                              <Transmission>version6,88jsam</Transmission>                                   
      //                                  <ECM>
      //                                  <Datalink>J1939</Datalink>
      //                                  <ActingMasterECM>Yes</ActingMasterECM>
      //                                  <SyncSMUClock>Yes</SyncSMUClock>
      //                                  <EventProtocolVersion>0</EventProtocolVersion>
      //                                  <DiagProtocolVersion>1</DiagProtocolVersion>
      //                                  <mid2>866</mid2>
      //                                  <ToolSupportLevel2>3457</ToolSupportLevel2>
      //                                  <ApplicationLevel2>457</ApplicationLevel2>             
      //                                  <sourceaddress>97</sourceaddress>                                                                                                                                              
      //                                  <SoftwarePart>8068986-00</SoftwarePart>                                                                                                                                                                              
      //                                  <SerialNumber>8014B009LQ</SerialNumber>
      //                                  <hardwarepartnumber>2347956-02</hardwarepartnumber>     
      //  <ArbiraryAddressCapable>Y</ArbiraryAddressCapable>
      //  <IndustryGroup>2</IndustryGroup>
      //  <VehicleSystemInstance>2</VehicleSystemInstance>
      //  <VehicleSystem>21</VehicleSystem>
      //  <Function>15</Function>
      //  <FunctionInstance>1</FunctionInstance>
      //  <ECUInstance>0</ECUInstance>
      //  <ManufacturerCode>1221</ManufacturerCode>
      //  <IdentityNumber>123311</IdentityNumber>										
      //                              </ECM>                               
                                       
      //                        </Block>
      MachineEventMessage msg = PlatformMessage.HydratePlatformMessage(HexDump.HexStringToBytes(msgPayload), ref bitPosition, true, true) as MachineEventMessage;
      Assert.IsNotNull(msg, "message should be machineEvent");
      Assert.IsNotNull(msg.Blocks, "message should have machine event block");
      Assert.AreEqual(1, msg.Blocks.Length, "Message should only have 1 block");
      Assert.IsNotNull(msg.Blocks[0].GatewayData, "message should have Machine Event Block Data");
      ECMInformationMessage ECMMsg = msg.Blocks[0].GatewayData as ECMInformationMessage;
      Assert.IsNotNull(ECMMsg, "msg block data should be ECMInformationMessage");

      Assert.AreEqual(ECMMsg.DeviceData[0].ECMType.ToString(), "J1939", "ECMType do not equal");
      Assert.IsTrue(ECMMsg.DeviceData[0].ActingMasterECM, "ActingMasterECM do not equal");
      Assert.IsTrue(ECMMsg.DeviceData[0].SyncronizedSMUClockStrategySupported, "SyncronizedSMUClockStrategySupported do not match");
      Assert.AreEqual(ECMMsg.DeviceData[0].EventProtocolVersion, 0, "EventProtocolVersion do not equal");
      Assert.AreEqual(ECMMsg.DeviceData[0].DiagnosticProtocolVersion, 1, "DiagnosticProtocolVersion do not equal");
      Assert.AreEqual(ECMMsg.DeviceData[0].ModuleID1, 866, "ModuleID1 do not equal");
      Assert.AreEqual(ECMMsg.DeviceData[0].Module1ServiceToolSupportChangeLevel, 3457, "Module1ServiceToolSupportChangeLevel do not equal");
      Assert.AreEqual(ECMMsg.DeviceData[0].Module1ApplicationLevel, 457, "Module1ApplicationLevel do not equal");
      Assert.AreEqual(ECMMsg.DeviceData[0].ECMSourceAddress, 97, "ECMSourceAddress do not equal");
      Assert.AreEqual(ECMMsg.DeviceData[0].ECMSoftwarePartNumber, "8068986-00", "ECMSoftwarePartNumber do not equal");
      Assert.AreEqual(ECMMsg.DeviceData[0].ECMSerialNumber, "8014B009LQ", "ECMSerialNumber do not equal");
      Assert.AreEqual(ECMMsg.DeviceData[0].ECMHardwarePartNumber, "2347956-02", "ECMHardwarePartNumber do not equal");
      Assert.AreEqual(ECMMsg.DeviceData[0].ArbitraryAddressCapable, true, "ArbitraryAddressCapable do not equal");
      Assert.AreEqual(ECMMsg.DeviceData[0].IndustryGroup, (byte)2, "IndustryGroup do not equal");
      Assert.AreEqual(ECMMsg.DeviceData[0].VehicleSystemInstance, (byte)2, "VehicleSystemInstance do not equal");
      Assert.AreEqual(ECMMsg.DeviceData[0].VehicleSystem, (byte)21, "VehicleSystem do not equal");
      Assert.AreEqual(ECMMsg.DeviceData[0].FunctionInstance, (byte)1, "FunctionInstance do not equal");
      Assert.AreEqual(ECMMsg.DeviceData[0].Function, (byte)15, "Function do not equal");
      Assert.AreEqual(ECMMsg.DeviceData[0].ECUInstance, (byte)0, "ECUInstance do not equal");
      Assert.AreEqual(ECMMsg.DeviceData[0].ManufacturerCode, (ushort) 1221, "ManufacturerCode do not equal");
      Assert.AreEqual(ECMMsg.DeviceData[0].IdentityNumber, 123311, "IdentityNumber do not equal");
    }

    [TestMethod]
    public void GatewayAdminMessageDigitalInputsTest()
    {
      MachineEventMessage machineEventMessage = new MachineEventMessage();
      machineEventMessage.DevicePacketSequenceID = 2;
      machineEventMessage.UtcDateTime = DateTime.UtcNow;
      machineEventMessage.Latitude = 1;
      machineEventMessage.Latitude = 12;
      machineEventMessage.SpeedMPH = 1;
      machineEventMessage.ServiceMeterHours = 12 / 3600;
      machineEventMessage.Blocks = new MachineEventBlock[1];
      GatewayAdministrationMessage admin = new GatewayAdministrationMessage();
      admin.SubType = 0x00;
      DigitalInputsAdminInformation inputs = new DigitalInputsAdminInformation();
      inputs.Input1DelayTime = TimeSpan.FromHours(3);
      inputs.Input2DelayTime = TimeSpan.FromHours(2);
      inputs.Input3DelayTime = TimeSpan.FromHours(1);
      inputs.Input4DelayTime = TimeSpan.FromHours(4);
      inputs.Input1Description = "Input1";
      inputs.Input2Description = "Input2";
      inputs.Input3Description = "Input3";
      inputs.Input4Description = "Input4";
      inputs.Input1MonitoringCondition = DigitalInputMonitoringConditions.Always;
      inputs.Input2MonitoringCondition = DigitalInputMonitoringConditions.KeyOffEngineOff;
      inputs.Input3MonitoringCondition = DigitalInputMonitoringConditions.KeyOnEngineOff;
      inputs.Input4MonitoringCondition = DigitalInputMonitoringConditions.KeyOnEngineOn;
      inputs.inputConfig1 = InputConfig.NormallyClosed;
      inputs.inputConfig2 = InputConfig.NotInstalled;
      inputs.inputConfig3 = InputConfig.NotConfigured;
      inputs.inputConfig4 = InputConfig.NormallyOpen;
      inputs.IsDigitalInput1Configured = true;
      inputs.IsDigitalInput2Configured = false;
      inputs.IsDigitalInput3Configured = true;
      inputs.IsDigitalInput4Configured = false;
      admin.Message = inputs;
      MachineEventBlock block1 = new MachineEventBlock();
      block1.DeltaTime = TimeSpan.FromMinutes(-2);
      block1.GatewayData = admin;
      machineEventMessage.Blocks[0] = block1;
      uint bitPosition = 0;
      byte[] bytes = PlatformMessage.SerializePlatformMessage(machineEventMessage, null, ref bitPosition, true);
      MachineEventMessage msg = PlatformMessage.HydratePlatformMessage(bytes, true, true) as MachineEventMessage;
      Assert.IsNotNull(msg, "message should be machineEvent");
      Assert.IsNotNull(msg.Blocks, "message should have machine event block");
      Assert.AreEqual(1, msg.Blocks.Length, "Message should only have 1 block");
      Assert.IsNotNull(msg.Blocks[0].GatewayData, "message should have Machine Event Block Data");
      
      GatewayAdministrationMessage adminActual = msg.Blocks[0].GatewayData as GatewayAdministrationMessage;

      Assert.IsNotNull(adminActual, "Should Return a gateway admin Message");

      DigitalInputsAdminInformation inputsActual = adminActual.Message as DigitalInputsAdminInformation;

      Assert.IsNotNull(inputsActual, "Should Return a DigitalInputsAdminInformation Inner Message");
      Assert.IsTrue(inputsActual.IsDigitalInput1Configured, "IsDigitalInput1Configured should be true");
      Assert.IsFalse(inputsActual.IsDigitalInput2Configured, "IsDigitalInput2Configured should be false");
      Assert.IsTrue(inputsActual.IsDigitalInput3Configured, "IsDigitalInput3Configured should be true");
      Assert.IsFalse(inputsActual.IsDigitalInput4Configured, "IsDigitalInput4Configured should be false");
      Assert.AreEqual(inputs.inputConfig1, inputsActual.inputConfig1, "inputConfig1 should Equal");
      Assert.AreEqual(null, inputsActual.inputConfig2, "inputConfig2 should be null");
      Assert.AreEqual(inputs.inputConfig3, inputsActual.inputConfig3, "inputConfig3 should Equal");
      Assert.AreEqual(null, inputsActual.inputConfig4, "inputConfig4 should be null");
      Assert.AreEqual(inputs.Input1MonitoringCondition, inputsActual.Input1MonitoringCondition, "Input1MonitoringCondition should Equal");
      Assert.AreEqual(null, inputsActual.Input2MonitoringCondition, "Input2MonitoringCondition should be null");
      Assert.AreEqual(inputs.Input3MonitoringCondition, inputsActual.Input3MonitoringCondition, "Input3MonitoringCondition should Equal");
      Assert.AreEqual(null, inputsActual.Input4MonitoringCondition, "Input4MonitoringCondition should be null");
      Assert.AreEqual(inputs.Input1Description, inputsActual.Input1Description, "Input1Description should equal");
      Assert.AreEqual(null, inputsActual.Input2Description, "Input2Description should be null");
      Assert.AreEqual(inputs.Input3Description, inputsActual.Input3Description, "Input3Description should equal");
      Assert.AreEqual(null, inputsActual.Input4Description, "Input4Description should be null");
      Assert.AreEqual(inputs.Input1DelayTime, inputsActual.Input1DelayTime, "Input1DelayTime should equal");
      Assert.AreEqual(null, inputsActual.Input2DelayTime, "Input2DelayTime should be null");
      Assert.AreEqual(inputs.Input3DelayTime, inputsActual.Input3DelayTime, "Input3DelayTime should equal");
      Assert.AreEqual(null, inputsActual.Input4DelayTime, "Input4DelayTime should be null");
      inputs.IsDigitalInput2Configured = true;
      inputs.IsDigitalInput4Configured = true;
      bitPosition = 0;
      admin.Message = inputs;
      block1.GatewayData = admin;
      machineEventMessage.Blocks[0] = block1;
      bytes = PlatformMessage.SerializePlatformMessage(machineEventMessage, null, ref bitPosition, true);
      msg = PlatformMessage.HydratePlatformMessage(bytes, true, true) as MachineEventMessage;
      adminActual = msg.Blocks[0].GatewayData as GatewayAdministrationMessage;
      inputsActual = adminActual.Message as DigitalInputsAdminInformation;
      Assert.AreEqual(inputs.inputConfig2, inputsActual.inputConfig2, "Config2 should equal");
      Assert.AreEqual(inputs.inputConfig4, inputsActual.inputConfig4, "Config4 should equal");
    }

    [TestMethod]
    public void GatewayAdminMessageMaintTest()
    {
      MachineEventMessage machineEventMessage = new MachineEventMessage();
      machineEventMessage.DevicePacketSequenceID = 2;
      machineEventMessage.UtcDateTime = DateTime.UtcNow;
      machineEventMessage.Latitude = 1;
      machineEventMessage.Latitude = 12;
      machineEventMessage.SpeedMPH = 1;
      machineEventMessage.ServiceMeterHours = 12 / 3600;
      machineEventMessage.Blocks = new MachineEventBlock[1];
      GatewayAdministrationMessage admin = new GatewayAdministrationMessage();
      admin.SubType = 0x01;
      MaintenanceAdministrationInformation maint = new MaintenanceAdministrationInformation();
      maint.MaintenanceModeEnabled = true;
      maint.MaintenanceModeDuration = TimeSpan.FromHours(2);
      maint.TransactionVersion = 0x00;
      admin.Message = maint;
      MachineEventBlock block1 = new MachineEventBlock();
      block1.DeltaTime = TimeSpan.FromMinutes(-2);
      block1.GatewayData = admin;
      machineEventMessage.Blocks[0] = block1;
      uint bitPosition = 0;
      byte[] bytes = PlatformMessage.SerializePlatformMessage(machineEventMessage, null, ref bitPosition, true);
      MachineEventMessage msg = PlatformMessage.HydratePlatformMessage(bytes, true, true) as MachineEventMessage;
      Assert.IsNotNull(msg, "message should be machineEvent");
      Assert.IsNotNull(msg.Blocks, "message should have machine event block");
      Assert.AreEqual(1, msg.Blocks.Length, "Message should only have 1 block");
      Assert.IsNotNull(msg.Blocks[0].GatewayData, "message should have Machine Event Block Data");

      GatewayAdministrationMessage adminActual = msg.Blocks[0].GatewayData as GatewayAdministrationMessage;

      Assert.IsNotNull(adminActual, "Should Return a gateway admin Message");

      MaintenanceAdministrationInformation maintActual = adminActual.Message as MaintenanceAdministrationInformation;
      Assert.IsNotNull(maintActual, "Should Return a MaintenanceAdministrationInformation inner Message");
      Assert.AreEqual(maint.TransactionVersion, maintActual.TransactionVersion, "TransactionVersions do not equal");
      Assert.AreEqual(maint.MaintenanceModeEnabled, maintActual.MaintenanceModeEnabled, "MaintenanceModeEnabled do not equal");
      Assert.AreEqual(maint.MaintenanceModeDuration, maintActual.MaintenanceModeDuration, "MaintenanceModeDuration do not equal");
    }

    [TestMethod]
    public void GatewayAdminMessageFailedTest()
    {
      MachineEventMessage machineEventMessage = new MachineEventMessage();
      machineEventMessage.DevicePacketSequenceID = 2;
      machineEventMessage.UtcDateTime = DateTime.UtcNow;
      machineEventMessage.Latitude = 1;
      machineEventMessage.Latitude = 12;
      machineEventMessage.SpeedMPH = 1;
      machineEventMessage.ServiceMeterHours = 12 / 3600;
      machineEventMessage.Blocks = new MachineEventBlock[1];
      GatewayAdministrationMessage admin = new GatewayAdministrationMessage();
      admin.SubType = 0xFF;
      AdministrationFailedDelivery failed = new AdministrationFailedDelivery();
      failed.MessageSequenceID = 15;
      failed.Reason = AdministrationFailedDelivery.FailureReason.IncorrectParameterValue;
      admin.Message = failed;
      MachineEventBlock block1 = new MachineEventBlock();
      block1.DeltaTime = TimeSpan.FromMinutes(-2);
      block1.GatewayData = admin;
      machineEventMessage.Blocks[0] = block1;
      uint bitPosition = 0;
      byte[] bytes = PlatformMessage.SerializePlatformMessage(machineEventMessage, null, ref bitPosition, true);
      MachineEventMessage msg = PlatformMessage.HydratePlatformMessage(bytes, true, true) as MachineEventMessage;
      Assert.IsNotNull(msg, "message should be machineEvent");
      Assert.IsNotNull(msg.Blocks, "message should have machine event block");
      Assert.AreEqual(1, msg.Blocks.Length, "Message should only have 1 block");
      Assert.IsNotNull(msg.Blocks[0].GatewayData, "message should have Machine Event Block Data");

      GatewayAdministrationMessage adminActual = msg.Blocks[0].GatewayData as GatewayAdministrationMessage;

      Assert.IsNotNull(adminActual, "Should Return a gateway admin Message");

      AdministrationFailedDelivery failedActual = adminActual.Message as AdministrationFailedDelivery;
      Assert.IsNotNull(failedActual, "Should Return a AdministrationFailedDelivery inner Message");
      Assert.AreEqual(failed.MessageSequenceID, failedActual.MessageSequenceID, "incorrect Message id");
      Assert.AreEqual(failed.Reason, failedActual.Reason, "incorrect Reason");
    }

    [TestMethod]
    public void MSSKeyIDReportTest()
    {
      MachineEventMessage machineEventMessage = new MachineEventMessage();
      machineEventMessage.DevicePacketSequenceID = 2;
      machineEventMessage.UtcDateTime = DateTime.UtcNow;
      machineEventMessage.Latitude = 1;
      machineEventMessage.Latitude = 12;
      machineEventMessage.SpeedMPH = 1;
      machineEventMessage.ServiceMeterHours = 12 / 3600;
      machineEventMessage.Blocks = new MachineEventBlock[1];
      MachineActivityEventBlock ma = new MachineActivityEventBlock();
      ma.SubType = 0x05;
      MSSKeyIDReport mss = new MSSKeyIDReport();
      mss.TransactionVersion = 0x00;
      mss.MSSKeyID = 1234567890L;
      ma.Message = mss;
      MachineEventBlock block1 = new MachineEventBlock();
      block1.DeltaTime = TimeSpan.FromMinutes(-2);
      block1.GatewayData = ma;
      machineEventMessage.Blocks[0] = block1;
      uint bitPosition = 0;
      byte[] bytes = PlatformMessage.SerializePlatformMessage(machineEventMessage, null, ref bitPosition, true);
      MachineEventMessage msg = PlatformMessage.HydratePlatformMessage(bytes, true, true) as MachineEventMessage;
      Assert.IsNotNull(msg, "message should be machineEvent");
      Assert.IsNotNull(msg.Blocks, "message should have machine event block");
      Assert.AreEqual(1, msg.Blocks.Length, "Message should only have 1 block");
      Assert.IsNotNull(msg.Blocks[0].GatewayData, "message should have Machine Event Block Data");

      MachineActivityEventBlock maActual = msg.Blocks[0].GatewayData as MachineActivityEventBlock;
      Assert.IsNotNull(maActual, "Data Should be MachineActivityEventBlock");
      MSSKeyIDReport mssActual = maActual.Message as MSSKeyIDReport;
      Assert.IsNotNull(mssActual, "Data Should be MachineActivityEventBlock");
      Assert.AreEqual(mss.MSSKeyID, mssActual.MSSKeyID, "MSSKeyID should be equal");
    }

    [TestMethod]
    public void SMHAdjusmentTest()
    {
      MachineEventMessage machineEventMessage = new MachineEventMessage();
      machineEventMessage.DevicePacketSequenceID = 2;
      machineEventMessage.UtcDateTime = DateTime.UtcNow;
      machineEventMessage.Latitude = 1;
      machineEventMessage.Latitude = 12;
      machineEventMessage.SpeedMPH = 1;
      machineEventMessage.ServiceMeterHours = 12 / 3600;
      machineEventMessage.Blocks = new MachineEventBlock[1];
      SMHAdjustmentMessage smh = new SMHAdjustmentMessage();
      smh.TransactionVersion = 0x00;
      smh.SubType = 0x00;
      smh.SMUBeforeAdj = TimeSpan.FromHours(22789.53);
      smh.SMUAfterAdj = TimeSpan.FromHours(256899.99);
      MachineEventBlock block1 = new MachineEventBlock();
      block1.DeltaTime = TimeSpan.FromMinutes(-2);
      block1.GatewayData = smh;
      machineEventMessage.Blocks[0] = block1;
      uint bitPosition = 0;
      byte[] bytes = PlatformMessage.SerializePlatformMessage(machineEventMessage, null, ref bitPosition, true);
      MachineEventMessage msg = PlatformMessage.HydratePlatformMessage(bytes, true, true) as MachineEventMessage;
      Assert.IsNotNull(msg, "message should be machineEvent");
      Assert.IsNotNull(msg.Blocks, "message should have machine event block");
      Assert.AreEqual(1, msg.Blocks.Length, "Message should only have 1 block");
      Assert.IsNotNull(msg.Blocks[0].GatewayData, "message should have Machine Event Block Data");
      SMHAdjustmentMessage smhActual = msg.Blocks[0].GatewayData as SMHAdjustmentMessage;
      Assert.IsNotNull(smhActual, "Data Should be SMHAdjustmentMessage");
      Assert.AreEqual(smh.SMUAfterAdj, smhActual.SMUAfterAdj, "SMUAfterAdj should equal");
      Assert.AreEqual(smh.SMUBeforeAdj, smhActual.SMUBeforeAdj, "SMUBeforeAdj should equal");
    }

    [TestMethod]
    public void ServiceOutageMessageTest()
    {
      ServiceOutageMessage serviceOutage = new ServiceOutageMessage();
      serviceOutage.OutageDescription = "Test Outage";
      serviceOutage.OutageDescriptionCode = 1;
      serviceOutage.ServiceOutageCatergory = ServiceOutageMessage.OutageCategory.GPS;
      serviceOutage.ServiceOutageLevel = ServiceOutageMessage.OutageLevel.Fatal;
      serviceOutage.UtcDateTime = DateTime.UtcNow;
      serviceOutage.DevicePacketSequenceID = 15;
      uint bitPosition = 0;
      byte[] actual = PlatformMessage.SerializePlatformMessage(serviceOutage, null, ref bitPosition, true);
      ServiceOutageMessage msg = PlatformMessage.HydratePlatformMessage(actual, true, true) as ServiceOutageMessage;
      Assert.IsNotNull(msg, "Message should be a Service outage Message");
      Assert.AreEqual(serviceOutage.OutageDescription, msg.OutageDescription, "Outage Descriptions should Equal");
    }

    [TestMethod]
    public void VehicleBusAddressClaimMessageTest()
    {
      MachineEventMessage machineEventMessage = new MachineEventMessage();
      machineEventMessage.DevicePacketSequenceID = 2;
      machineEventMessage.UtcDateTime = DateTime.UtcNow;
      machineEventMessage.Latitude = 1;
      machineEventMessage.Latitude = 12;
      machineEventMessage.SpeedMPH = 1;
      machineEventMessage.ServiceMeterHours = 12 / 3600;
      machineEventMessage.Blocks = new MachineEventBlock[1];

      MachineEventBlock block1 = new MachineEventBlock();
      block1.DeltaTime = TimeSpan.FromMinutes(-2);
      block1.Source = MachineEventSourceEnum.VehicleBus;
      block1.IsVehicleBusTrimbleAbstraction = true;
      block1.Protocol = 0;

      VehicleBusAddressClaimMessage addressClaim = new VehicleBusAddressClaimMessage();
      addressClaim.DeviceECMs = new VehicleBusECMAddressClaim[1];
      addressClaim.DeviceECMs[0] = new VehicleBusECMAddressClaim();
      addressClaim.DeviceECMs[0].ArbitraryAddressCapable = false;
      addressClaim.DeviceECMs[0].ECUInstance = 7;
      addressClaim.DeviceECMs[0].Function = 25;
      addressClaim.DeviceECMs[0].FunctionInstance = 31;
      addressClaim.DeviceECMs[0].IdentityNumber = 123;
      addressClaim.DeviceECMs[0].IndustryGroup = 6;
      addressClaim.DeviceECMs[0].ManufacturerCode = 456;
      addressClaim.DeviceECMs[0].SourceAddress = 12;
      addressClaim.DeviceECMs[0].VehicleSystem = 111;
      addressClaim.DeviceECMs[0].VehicleSystemInstance = 10;

      block1.VehicleBusData = addressClaim;
      machineEventMessage.Blocks[0] = block1;

      uint bitPosition = 0;
      byte[] actual = PlatformMessage.SerializePlatformMessage(machineEventMessage, null, ref bitPosition, true);
      MachineEventMessage msg = PlatformMessage.HydratePlatformMessage(actual, true, true) as MachineEventMessage;
      Assert.IsNotNull(msg, "Message should be a machine event");
      Assert.AreEqual(1, msg.Blocks.Length, "Should have 1 block in the machine event");
      Assert.IsNotNull(msg.Blocks[0].VehicleBusData, "Data Should be vehicle bus type");
      VehicleBusAddressClaimMessage actualClaim = msg.Blocks[0].VehicleBusData as VehicleBusAddressClaimMessage;
      Assert.IsNotNull(actualClaim, "Should have been an address claim message");
      Assert.IsFalse(actualClaim.DeviceECMs[0].ArbitraryAddressCapable, "ArbitraryAddressCapable should be false");
      Assert.AreEqual(addressClaim.DeviceECMs[0].ECUInstance, actualClaim.DeviceECMs[0].ECUInstance, "ECUInstance does not equal");
      Assert.AreEqual(addressClaim.DeviceECMs[0].Function, actualClaim.DeviceECMs[0].Function, "Function does not equal");
      Assert.AreEqual(addressClaim.DeviceECMs[0].FunctionInstance, actualClaim.DeviceECMs[0].FunctionInstance, "FunctionInstance does not equal");
      Assert.AreEqual(addressClaim.DeviceECMs[0].IdentityNumber, actualClaim.DeviceECMs[0].IdentityNumber, "IdentityNumber does not equal");
      Assert.AreEqual(addressClaim.DeviceECMs[0].IndustryGroup, actualClaim.DeviceECMs[0].IndustryGroup, "IndustryGroup does not equal");
      Assert.AreEqual(addressClaim.DeviceECMs[0].ManufacturerCode, actualClaim.DeviceECMs[0].ManufacturerCode, "ManufacturerCode does not equal");
      Assert.AreEqual(addressClaim.DeviceECMs[0].SourceAddress, actualClaim.DeviceECMs[0].SourceAddress, "SourceAddress does not equal");
      Assert.AreEqual(addressClaim.DeviceECMs[0].VehicleSystem, actualClaim.DeviceECMs[0].VehicleSystem, "VehicleSystem does not equal");
      Assert.AreEqual(addressClaim.DeviceECMs[0].VehicleSystemInstance, actualClaim.DeviceECMs[0].VehicleSystemInstance, "VehicleSystemInstance does not equal");
    }

    [TestMethod]
    public void VehicleBusParametersReportMessageTest()
    {
      MachineEventMessage parametersMachineEvent = new MachineEventMessage();
      parametersMachineEvent.DevicePacketSequenceID = 2;
      parametersMachineEvent.UtcDateTime = DateTime.UtcNow;
      parametersMachineEvent.Latitude = 1;
      parametersMachineEvent.Latitude = 12;
      parametersMachineEvent.SpeedMPH = 1;
      parametersMachineEvent.ServiceMeterHours = 12 / 3600;
      parametersMachineEvent.Blocks = new MachineEventBlock[1];

      MachineEventBlock block1 = new MachineEventBlock();
      block1.DeltaTime = TimeSpan.FromMinutes(-2);
      block1.Source = MachineEventSourceEnum.VehicleBus;
      block1.IsVehicleBusTrimbleAbstraction = true;
      block1.Protocol = 0;

      VehicleBusJ1939ParametersReportMessage parametersReportMessage = new VehicleBusJ1939ParametersReportMessage();
      parametersReportMessage.ReportType = VehicleBusJ1939ParametersReportTypeEnum.Periodic;
      parametersReportMessage.ParametersReportBlocks = new VehicleBusJ1939ParametersReportBlock[1];
      parametersReportMessage.ParametersReportBlocks[0] = new VehicleBusJ1939ParametersReportBlock();
      parametersReportMessage.ParametersReportBlocks[0].CANBusInstance = 0x99;
      parametersReportMessage.ParametersReportBlocks[0].ProtectStatus = 1;
      parametersReportMessage.ParametersReportBlocks[0].AmberWarningStatus = 2;
      parametersReportMessage.ParametersReportBlocks[0].RedStopLampStatus = 3;
      parametersReportMessage.ParametersReportBlocks[0].MalfunctionIndicatorLampStatus = 0;
      parametersReportMessage.ParametersReportBlocks[0].SourceAddress = 0x0;
      parametersReportMessage.ParametersReportBlocks[0].PGN = 0xF004;
      parametersReportMessage.ParametersReportBlocks[0].SPN = 0xBE;
      parametersReportMessage.ParametersReportBlocks[0].parameter = new VehicleBusJ1939ParametersReportBlock.J1939Parameter();
      parametersReportMessage.ParametersReportBlocks[0].parameter.DoubleValue = 45.3f;
      block1.VehicleBusData = parametersReportMessage;
      parametersMachineEvent.Blocks[0] = block1;

      uint bitPosition = 0;
      byte[] actual = PlatformMessage.SerializePlatformMessage(parametersMachineEvent, null, ref bitPosition, true);
      MachineEventMessage msg = PlatformMessage.HydratePlatformMessage(actual, true, true) as MachineEventMessage;
      Assert.IsNotNull(msg, "Message should be a machine event");
      Assert.AreEqual(1, msg.Blocks.Length, "Should have 1 block in the machine event");
      Assert.IsNotNull(msg.Blocks[0].VehicleBusData, "Data Should be vehicle bus type");
      VehicleBusJ1939ParametersReportMessage actualParametersReportMessage = msg.Blocks[0].VehicleBusData as VehicleBusJ1939ParametersReportMessage;
      Assert.IsNotNull(actualParametersReportMessage, "Should have been a VehicleBusJ1939ParametersReportMessage");
      Assert.AreEqual(parametersReportMessage.ReportType, actualParametersReportMessage.ReportType, "ReportType does not match");
      Assert.AreEqual(parametersReportMessage.ParametersReportBlocks.Length, actualParametersReportMessage.ParametersReportBlocks.Length, "There should be 1 ParametersReportBlocks");
      Assert.AreEqual(parametersReportMessage.ParametersReportBlocks[0].AmberWarningStatus, actualParametersReportMessage.ParametersReportBlocks[0].AmberWarningStatus, "AmberWarningStatus does not match");
      Assert.AreEqual(parametersReportMessage.ParametersReportBlocks[0].ProtectStatus, actualParametersReportMessage.ParametersReportBlocks[0].ProtectStatus, "ProtectStatus does not match");
      Assert.AreEqual(parametersReportMessage.ParametersReportBlocks[0].RedStopLampStatus, actualParametersReportMessage.ParametersReportBlocks[0].RedStopLampStatus, "RedStopLampStatus does not match");
      Assert.AreEqual(parametersReportMessage.ParametersReportBlocks[0].MalfunctionIndicatorLampStatus, actualParametersReportMessage.ParametersReportBlocks[0].MalfunctionIndicatorLampStatus, "MalfunctionIndicatorLampStatus does not match");
      Assert.AreEqual(parametersReportMessage.ParametersReportBlocks[0].SourceAddress, actualParametersReportMessage.ParametersReportBlocks[0].SourceAddress, "SourceAddress does not match");
      Assert.AreEqual(parametersReportMessage.ParametersReportBlocks[0].PGN, actualParametersReportMessage.ParametersReportBlocks[0].PGN, "PGN does not match");
      Assert.AreEqual(parametersReportMessage.ParametersReportBlocks[0].SPN, actualParametersReportMessage.ParametersReportBlocks[0].SPN, "SPN does not match");
      Assert.AreEqual(45.3f, BitConverter.ToSingle(actualParametersReportMessage.ParametersReportBlocks[0].ParameterPayload,0), "ParameterPayload does not match");
    }

    [TestMethod]
    public void VehicleBusStatisticsReportMessageTest()
    {
      MachineEventMessage machineEventMessage = new MachineEventMessage();
      machineEventMessage.DevicePacketSequenceID = 2;
      machineEventMessage.UtcDateTime = DateTime.UtcNow;
      machineEventMessage.Latitude = 1;
      machineEventMessage.Latitude = 12;
      machineEventMessage.SpeedMPH = 1;
      machineEventMessage.ServiceMeterHours = 12 / 3600;
      machineEventMessage.Blocks = new MachineEventBlock[1];

      MachineEventBlock block1 = new MachineEventBlock();
      block1.DeltaTime = TimeSpan.FromMinutes(-2);
      block1.Source = MachineEventSourceEnum.VehicleBus;
      block1.IsVehicleBusTrimbleAbstraction = true;
      block1.Protocol = 0;

      VehicleBusJ1939StatisticsReportMessage StatisticsReportMessage = new VehicleBusJ1939StatisticsReportMessage();
      StatisticsReportMessage.StatisticsReportBlocks = new VehicleBusJ1939StatisticsReportBlock[1];
      StatisticsReportMessage.StatisticsReportBlocks[0] = new VehicleBusJ1939StatisticsReportBlock();
      StatisticsReportMessage.StatisticsReportBlocks[0].SourceAddress = 0x11;
      StatisticsReportMessage.StatisticsReportBlocks[0].PGN = 0x1111;
      StatisticsReportMessage.StatisticsReportBlocks[0].SPN = 0x111111;
      StatisticsReportMessage.StatisticsReportBlocks[0].UTCDelta = 0x2222;
      StatisticsReportMessage.StatisticsReportBlocks[0].SMUDelta = 0x3333;
      StatisticsReportMessage.StatisticsReportBlocks[0].Minimum = 0x44444444;
      StatisticsReportMessage.StatisticsReportBlocks[0].Maximum = 0x55555555;
      StatisticsReportMessage.StatisticsReportBlocks[0].Average = 0x66666666;
      StatisticsReportMessage.StatisticsReportBlocks[0].StandardDeviation = 0x77777777;
      StatisticsReportMessage.StatisticsReportBlocks[0].ScaleFactorExponent = 0x88;
      block1.VehicleBusData = StatisticsReportMessage;
      machineEventMessage.Blocks[0] = block1;

      uint bitPosition = 0;
      byte[] actual = PlatformMessage.SerializePlatformMessage(machineEventMessage, null, ref bitPosition, true);
      MachineEventMessage msg = PlatformMessage.HydratePlatformMessage(actual, true, true) as MachineEventMessage;
      Assert.IsNotNull(msg, "Message should be a machine event");
      Assert.AreEqual(1, msg.Blocks.Length, "Should have 1 block in the machine event");
      Assert.IsNotNull(msg.Blocks[0].VehicleBusData, "Data Should be vehicle bus type");
      VehicleBusJ1939StatisticsReportMessage actualStatisticsReportMessage = msg.Blocks[0].VehicleBusData as VehicleBusJ1939StatisticsReportMessage;
      Assert.IsNotNull(actualStatisticsReportMessage, "Should have been a VehicleBusJ1939StatisticsReportMessage");
      Assert.AreEqual(StatisticsReportMessage.StatisticsReportBlocks.Length, actualStatisticsReportMessage.StatisticsReportBlocks.Length, "There should be 1 StatisticsReportBlocks");
      Assert.AreEqual(StatisticsReportMessage.StatisticsReportBlocks[0].UTCDelta, actualStatisticsReportMessage.StatisticsReportBlocks[0].UTCDelta, "UTCDelta does not match");
      Assert.AreEqual(StatisticsReportMessage.StatisticsReportBlocks[0].SMUDelta, actualStatisticsReportMessage.StatisticsReportBlocks[0].SMUDelta, "SMUDelta does not match");
      Assert.AreEqual(StatisticsReportMessage.StatisticsReportBlocks[0].Minimum, actualStatisticsReportMessage.StatisticsReportBlocks[0].Minimum, "Minimum does not match");
      Assert.AreEqual(StatisticsReportMessage.StatisticsReportBlocks[0].Maximum, actualStatisticsReportMessage.StatisticsReportBlocks[0].Maximum, "Maximum does not match");
      Assert.AreEqual(StatisticsReportMessage.StatisticsReportBlocks[0].SourceAddress, actualStatisticsReportMessage.StatisticsReportBlocks[0].SourceAddress, "SourceAddress does not match");
      Assert.AreEqual(StatisticsReportMessage.StatisticsReportBlocks[0].PGN, actualStatisticsReportMessage.StatisticsReportBlocks[0].PGN, "PGN does not match");
      Assert.AreEqual(StatisticsReportMessage.StatisticsReportBlocks[0].SPN, actualStatisticsReportMessage.StatisticsReportBlocks[0].SPN, "SPN does not match");
      Assert.AreEqual(StatisticsReportMessage.StatisticsReportBlocks[0].Average, actualStatisticsReportMessage.StatisticsReportBlocks[0].Average, "Average does not match");
      Assert.AreEqual(StatisticsReportMessage.StatisticsReportBlocks[0].StandardDeviation, actualStatisticsReportMessage.StatisticsReportBlocks[0].StandardDeviation, "StandardDeviation does not match");
      Assert.AreEqual(StatisticsReportMessage.StatisticsReportBlocks[0].ScaleFactorExponent, actualStatisticsReportMessage.StatisticsReportBlocks[0].ScaleFactorExponent, "ScaleFactorExponent does not match");
    }

    #region Vehicle Bus Payload Report

    [TestMethod]
    public void VehicleBusPayloadReportMessageTest()
    {
      byte ecmCount = 1;
      uint totalPayload = 1024;
      uint totalCycles = 42;

      MachineEventMessage machineEventMessage = CreateMachineEventMessage_VehicleBusPayloadReport(ecmCount,totalPayload,totalCycles);
      
      uint bitPosition = 0;
      byte[] actual = PlatformMessage.SerializePlatformMessage(machineEventMessage, null, ref bitPosition, true);
      MachineEventMessage msg = PlatformMessage.HydratePlatformMessage(actual, true, true) as MachineEventMessage;
      Assert.IsNotNull(msg, "Message should be a machine event");
      Assert.AreEqual(1, msg.Blocks.Length, "Should have 1 block in the machine event");
      Assert.IsNotNull(msg.Blocks[0].VehicleBusData, "Data Should be vehicle bus type");
      VehicleBusPayloadReport actualPayloadReportMessage = msg.Blocks[0].VehicleBusData as VehicleBusPayloadReport;
      Assert.IsNotNull(actualPayloadReportMessage, "Should have been a VehicleBusPayloadReport");
      Assert.AreEqual(((VehicleBusPayloadReport)machineEventMessage.Blocks[0].VehicleBusData).ECMCount, actualPayloadReportMessage.ECMCount);
      Assert.AreEqual(((VehicleBusPayloadReport)machineEventMessage.Blocks[0].VehicleBusData).PayloadCycleCountECMs[0].CANBusInstance, actualPayloadReportMessage.PayloadCycleCountECMs[0].CANBusInstance);
      Assert.AreEqual(((VehicleBusPayloadReport)machineEventMessage.Blocks[0].VehicleBusData).PayloadCycleCountECMs[0].ECMSourceAddress, actualPayloadReportMessage.PayloadCycleCountECMs[0].ECMSourceAddress);
      Assert.AreEqual(((VehicleBusPayloadReport)machineEventMessage.Blocks[0].VehicleBusData).PayloadCycleCountECMs[0].TotalPayload, actualPayloadReportMessage.PayloadCycleCountECMs[0].TotalPayload);
      Assert.AreEqual(((VehicleBusPayloadReport)machineEventMessage.Blocks[0].VehicleBusData).PayloadCycleCountECMs[0].TotalCycles, actualPayloadReportMessage.PayloadCycleCountECMs[0].TotalCycles);

    }

    [TestMethod]
    public void VehicleBusPayloadReportTest_SetNumberOfECMsToFF()
    {

      byte ecmCount = byte.MaxValue;
      
      MachineEventMessage machineEventMessage = CreateMachineEventMessage_VehicleBusPayloadReport(ecmCount);
      
      uint bitPosition = 0;
      byte[] actual = PlatformMessage.SerializePlatformMessage(machineEventMessage, null, ref bitPosition, true);
      MachineEventMessage msg = PlatformMessage.HydratePlatformMessage(actual, true, true) as MachineEventMessage;
      Assert.IsNotNull(msg, "Message should be a machine event");
      Assert.AreEqual(1, msg.Blocks.Length, "Should have 1 block in the machine event");
      Assert.IsNotNull(msg.Blocks[0].VehicleBusData, "Data Should be vehicle bus type");
      VehicleBusPayloadReport actualPayloadReportMessage = msg.Blocks[0].VehicleBusData as VehicleBusPayloadReport;
      Assert.IsNotNull(actualPayloadReportMessage, "Should have been a VehicleBusPayloadReport");

      Assert.AreEqual(byte.MaxValue, actualPayloadReportMessage.ECMCountUnConverted, "Expected NumberOfECMsUnConverted to be 0xFF");
      Assert.IsNull(actualPayloadReportMessage.ECMCount, "NumberOfECMs should be null");
      Assert.IsNull(actualPayloadReportMessage.PayloadCycleCountECMs, "payloadCycleCountECMs should be null");
    }

    [TestMethod]
    public void VehicleBusPayloadReportTest_SetPayloadAndCyclesToFF()
    {
      byte ecmCount = 1;
      uint totalPayload = uint.MaxValue;
      uint totalCycles = uint.MaxValue;

      MachineEventMessage machineEventMessage = CreateMachineEventMessage_VehicleBusPayloadReport(ecmCount, totalPayload, totalCycles);

      uint bitPosition = 0;
      byte[] actual = PlatformMessage.SerializePlatformMessage(machineEventMessage, null, ref bitPosition, true);
      MachineEventMessage msg = PlatformMessage.HydratePlatformMessage(actual, true, true) as MachineEventMessage;
      Assert.IsNotNull(msg, "Message should be a machine event");
      Assert.AreEqual(1, msg.Blocks.Length, "Should have 1 block in the machine event");
      Assert.IsNotNull(msg.Blocks[0].VehicleBusData, "Data Should be vehicle bus type");
      VehicleBusPayloadReport actualPayloadReportMessage = msg.Blocks[0].VehicleBusData as VehicleBusPayloadReport;
      Assert.IsNotNull(actualPayloadReportMessage, "Should have been a VehicleBusPayloadReport");
      Assert.AreEqual(((VehicleBusPayloadReport)machineEventMessage.Blocks[0].VehicleBusData).ECMCount , actualPayloadReportMessage.ECMCount);
      Assert.AreEqual(((VehicleBusPayloadReport)machineEventMessage.Blocks[0].VehicleBusData).PayloadCycleCountECMs[0].CANBusInstance, actualPayloadReportMessage.PayloadCycleCountECMs[0].CANBusInstance);
      Assert.AreEqual(((VehicleBusPayloadReport)machineEventMessage.Blocks[0].VehicleBusData).PayloadCycleCountECMs[0].ECMSourceAddress, actualPayloadReportMessage.PayloadCycleCountECMs[0].ECMSourceAddress);
      Assert.AreEqual(uint.MaxValue, actualPayloadReportMessage.PayloadCycleCountECMs[0].TotalPayloadUnConverted);
      Assert.AreEqual(uint.MaxValue, actualPayloadReportMessage.PayloadCycleCountECMs[0].TotalCyclesUnConverted);
      Assert.IsNull(actualPayloadReportMessage.PayloadCycleCountECMs[0].TotalPayload, "Total Payload should be null");
      Assert.IsNull(actualPayloadReportMessage.PayloadCycleCountECMs[0].TotalCycles, "Total Cycles should be null");
    }

    private MachineEventMessage CreateMachineEventMessage_VehicleBusPayloadReport(byte ecmCount, uint totalPayload, uint totalCycles)
    {
      MachineEventMessage machineEventMessage = new MachineEventMessage()
      {
        DevicePacketSequenceID = 2,
        UtcDateTime = DateTime.UtcNow,
        Latitude = 1,
        Longitude = 12,
        SpeedMPH = 1,
        ServiceMeterHours = 12 / 3600,
        Blocks = new MachineEventBlock[1]
        {
          new MachineEventBlock()
          {
            DeltaTime = TimeSpan.FromMinutes(-2),
            Source = MachineEventSourceEnum.VehicleBus,
            IsVehicleBusTrimbleAbstraction = true,
            Protocol = 0,
            VehicleBusData=  new VehicleBusPayloadReport()
            {
              ECMCount = ecmCount,
              PayloadCycleCountECMs = new VehicleBusPayloadCycleCountECM[1]
              {
                new VehicleBusPayloadCycleCountECM()
                {
                  CANBusInstance = 0x99,
                  ECMSourceAddress = 0x11,
                  TotalPayload = totalPayload,
                  TotalCycles = totalCycles
                }
              }
            }
          }
        }
      };
      return machineEventMessage;
    }

    private MachineEventMessage CreateMachineEventMessage_VehicleBusPayloadReport(byte ecmCount)
    {
      MachineEventMessage machineEventMessage = new MachineEventMessage()
      {
        DevicePacketSequenceID = 2,
        UtcDateTime = DateTime.UtcNow,
        Latitude = 1,
        Longitude = 12,
        SpeedMPH = 1,
        ServiceMeterHours = 12 / 3600,
        Blocks = new MachineEventBlock[1]
        {
          new MachineEventBlock()
          {
            DeltaTime = TimeSpan.FromMinutes(-2),
            Source = MachineEventSourceEnum.VehicleBus,
            IsVehicleBusTrimbleAbstraction = true,
            Protocol = 0,
            VehicleBusData=  new VehicleBusPayloadReport()
            {
              ECMCount = ecmCount,
            }
          }
        }
      };
      return machineEventMessage;
    }

    # endregion

    #region "Privates"

    private void AssertMachineEventMessage(MachineEventMessage expectedMachineEventMessage)
    {
      ECMInformationMessage expectedECMInformationMessage = expectedMachineEventMessage.Blocks[0].GatewayData as ECMInformationMessage;
      uint bitPosition = 0;
      byte[] bytes = PlatformMessage.SerializePlatformMessage(expectedMachineEventMessage, null, ref bitPosition, true);
      MachineEventMessage msg = PlatformMessage.HydratePlatformMessage(bytes, true, true) as MachineEventMessage;
      Assert.IsNotNull(msg, "message should be machineEvent");
      Assert.IsNotNull(msg.Blocks, "message should have machine event block");
      Assert.AreEqual(1, msg.Blocks.Length, "Message should only have 1 block");
      Assert.IsNotNull(msg.Blocks[0].GatewayData, "message should have Machine Event Block Data");
      ECMInformationMessage actualEcmMsg = msg.Blocks[0].GatewayData as ECMInformationMessage;
      Assert.IsNotNull(actualEcmMsg, "msg block data should be ECMInformationMessage");
      Assert.IsNotNull(expectedECMInformationMessage != null, "expectedECMInformationMessage != null");
      Assert.AreEqual(expectedECMInformationMessage.EngineSerialNumbers.Length, actualEcmMsg.EngineSerialNumbers.Length, "Number of Engine serial Numbers Do Not Equal");
      Assert.AreEqual(expectedECMInformationMessage.TransmissionSerialNumbers.Length, actualEcmMsg.TransmissionSerialNumbers.Length, "Number of Transmission serial Numbers count Do Not Equal");
      for (int i = 0; i < expectedECMInformationMessage.EngineSerialNumbers.Length; i++)
      {
        Assert.AreEqual(expectedECMInformationMessage.EngineSerialNumbers[i], actualEcmMsg.EngineSerialNumbers[i], "Engine Serial Numbers are not the same");
      }
      for (int i = 0; i < expectedECMInformationMessage.TransmissionSerialNumbers.Length; i++)
      {
        Assert.AreEqual(expectedECMInformationMessage.TransmissionSerialNumbers[i], actualEcmMsg.TransmissionSerialNumbers[i], "Transmission Serial Numbers are not the same");
      }
      Assert.AreEqual(expectedECMInformationMessage.DeviceData.Length, actualEcmMsg.DeviceData.Length, "Device Data Count Do Not Equal");
      Assert.AreEqual(expectedECMInformationMessage.DeviceData[0].ActingMasterECM, actualEcmMsg.DeviceData[0].ActingMasterECM, "ActingMasterECM do not equal");
      Assert.AreEqual(expectedECMInformationMessage.DeviceData[0].SyncronizedSMUClockStrategySupported, actualEcmMsg.DeviceData[0].SyncronizedSMUClockStrategySupported, "SyncronizedSMUClockStrategySupported do not equal");
      Assert.AreEqual(expectedECMInformationMessage.DeviceData[0].DiagnosticProtocolVersion, actualEcmMsg.DeviceData[0].DiagnosticProtocolVersion, "DiagnosticProtocolVersion do not equal");
      Assert.AreEqual(expectedECMInformationMessage.DeviceData[0].EventProtocolVersion, actualEcmMsg.DeviceData[0].EventProtocolVersion, "EventProtocolVersion do not equal");
      Assert.AreEqual(expectedECMInformationMessage.DeviceData[0].ECMType, actualEcmMsg.DeviceData[0].ECMType, "ECMType do not equal");

      if (expectedECMInformationMessage.DeviceData[0].ECMType == DeviceIDData.DataLinkType.CDL ||
          expectedECMInformationMessage.DeviceData[0].ECMType == DeviceIDData.DataLinkType.CDLAndJ1939
        || expectedECMInformationMessage.DeviceData[0].ECMType == DeviceIDData.DataLinkType.J1939)
      {
        Assert.AreEqual(expectedECMInformationMessage.DeviceData[0].ModuleID1, actualEcmMsg.DeviceData[0].ModuleID1, "ModuleID1 do not equal");
        Assert.AreEqual(expectedECMInformationMessage.DeviceData[0].Module1ServiceToolSupportChangeLevel, actualEcmMsg.DeviceData[0].Module1ServiceToolSupportChangeLevel, "Module1ServiceToolSupportChangeLevel do not equal");
        Assert.AreEqual(expectedECMInformationMessage.DeviceData[0].Module1ApplicationLevel, actualEcmMsg.DeviceData[0].Module1ApplicationLevel, "Module1ApplicationLevel do not equal");
      }

      if (expectedECMInformationMessage.DeviceData[0].ECMType == DeviceIDData.DataLinkType.CDLAndJ1939)
      {
        Assert.AreEqual(expectedECMInformationMessage.DeviceData[0].ModuleID2, actualEcmMsg.DeviceData[0].ModuleID2, "ModuleID2 do not equal");
        Assert.AreEqual(expectedECMInformationMessage.DeviceData[0].Module2ServiceToolSupportChangeLevel, actualEcmMsg.DeviceData[0].Module2ServiceToolSupportChangeLevel, "Module2ServiceToolSupportChangeLevel do not equal");
        Assert.AreEqual(expectedECMInformationMessage.DeviceData[0].Module2ApplicationLevel, actualEcmMsg.DeviceData[0].Module2ApplicationLevel, "Module2ApplicationLevel do not equal");
      }
      Assert.AreEqual(expectedECMInformationMessage.DeviceData[0].ECMSoftwarePartNumber, actualEcmMsg.DeviceData[0].ECMSoftwarePartNumber, "ECMSoftwarePartNumber do not equal");
      Assert.AreEqual(expectedECMInformationMessage.DeviceData[0].ECMSerialNumber, actualEcmMsg.DeviceData[0].ECMSerialNumber, "ECMSerialNumber do not equal");
    }

    private static MachineEventMessage SetUpMachineEventMessage(DeviceIDData.DataLinkType datalink)
    {
      MachineEventMessage machineEventMessage = new MachineEventMessage();
      machineEventMessage.DevicePacketSequenceID = 2;
      machineEventMessage.UtcDateTime = DateTime.UtcNow;
      machineEventMessage.Latitude = 1;
      machineEventMessage.Latitude = 12;
      machineEventMessage.SpeedMPH = 1;
      machineEventMessage.ServiceMeterHours = 12 / 3600;
      machineEventMessage.Blocks = new MachineEventBlock[1];
      ECMInformationMessage ecm = new ECMInformationMessage();
      ecm.EngineSerialNumbers = new string[2];
      ecm.EngineSerialNumbers[0] = "1s23";
      ecm.EngineSerialNumbers[1] = "2s23";
      ecm.TransmissionSerialNumbers = new string[3];
      ecm.TransmissionSerialNumbers[0] = "14t2";
      ecm.TransmissionSerialNumbers[1] = "15t2";
      ecm.TransmissionSerialNumbers[2] = "16t2";
      ecm.DeviceData = new DeviceIDData[1];
      ecm.DeviceData[0] = new DeviceIDData();
      ecm.DeviceData[0].ActingMasterECM = true;
      ecm.DeviceData[0].DiagnosticProtocolVersion = 1;
      ecm.DeviceData[0].EventProtocolVersion = 1;
      ecm.DeviceData[0].SyncronizedSMUClockStrategySupported = true;
      ecm.DeviceData[0].ECMType = datalink;
      if (datalink == DeviceIDData.DataLinkType.CDL || datalink == DeviceIDData.DataLinkType.CDLAndJ1939 || datalink == DeviceIDData.DataLinkType.J1939 || datalink == DeviceIDData.DataLinkType.SAEJI939AndJ1939 || datalink == DeviceIDData.DataLinkType.SAEJI939AndCDL)
      {
        ecm.DeviceData[0].ModuleID1 = 23;
      }
      if (datalink == DeviceIDData.DataLinkType.CDLAndJ1939)
      {
        ecm.DeviceData[0].ModuleID2 = 27;
        ecm.DeviceData[0].Module2ServiceToolSupportChangeLevel = 27;
      }
      ecm.DeviceData[0].ECMSerialNumber = "15";
      ecm.DeviceData[0].ECMSoftwarePartNumber = "1234567-00";
      MachineEventBlock block1 = new MachineEventBlock();
      block1.DeltaTime = TimeSpan.FromMinutes(-2);
      block1.GatewayData = ecm;
      machineEventMessage.Blocks[0] = block1;
      return machineEventMessage;
    }

    #endregion

    #region Tamper Secuirty Status Information Message

    [TestMethod]
    public void TamperSecurityStatusReport_AllNullTest()
    {
      TamperSecurityStatusReportTest();
    }

    [TestMethod]
    public void TamperSecurityStatusReport_AllNullExceptMachineStartStatusTriggerTest()
    {
      TamperSecurityStatusReportTest(machineStartStatus: MachineStartStatus.NormalOperation);
    }

    [TestMethod]
    public void TamperSecurityStatusReport_AllNullExceptMachineStartStatusTest()
    {
      TamperSecurityStatusReportTest(machineStartStatusTrigger: MachineStartStatusTrigger.InvalidMSSKey);
    }

    [TestMethod]
    public void TamperSecurityStatusReport_AllNotNullTest()
    {
      TamperSecurityStatusReportTest(machineStartStatus: MachineStartStatus.DisabledPending, machineStartStatusTrigger: MachineStartStatusTrigger.OTACommand);
    }
    
    private void TamperSecurityStatusReportTest(MachineStartStatus? machineStartStatus = null, MachineStartStatusTrigger? machineStartStatusTrigger = null)
    {
      MachineEventMessage machineEventMessage = new MachineEventMessage();
      machineEventMessage.DevicePacketSequenceID = 2;
      machineEventMessage.UtcDateTime = DateTime.UtcNow;
      machineEventMessage.Latitude = 1;
      machineEventMessage.Latitude = 12;
      machineEventMessage.SpeedMPH = 1;
      machineEventMessage.ServiceMeterHours = 12 / 3600;
      machineEventMessage.Blocks = new MachineEventBlock[1];
      MachineActivityEventBlock ma = new MachineActivityEventBlock();
      ma.SubType = 0x06;
      TamperSecurityStatusInformationMessage mss = new TamperSecurityStatusInformationMessage();
      mss.TransactionVersion = 0x00;
      if(machineStartStatusTrigger.HasValue)
        mss.StartStatusTrigger = machineStartStatusTrigger.Value;
      if(machineStartStatus.HasValue)
        mss.StartStatus = machineStartStatus.Value;
      ma.Message = mss;
      MachineEventBlock block1 = new MachineEventBlock();
      block1.DeltaTime = TimeSpan.FromMinutes(-2);
      block1.GatewayData = ma;
      machineEventMessage.Blocks[0] = block1;
      uint bitPosition = 0;
      byte[] bytes = PlatformMessage.SerializePlatformMessage(machineEventMessage, null, ref bitPosition, true);
      MachineEventMessage msg = PlatformMessage.HydratePlatformMessage(bytes, true, true) as MachineEventMessage;
      Assert.IsNotNull(msg, "message should be machineEvent");
      Assert.IsNotNull(msg.Blocks, "message should have machine event block");
      Assert.AreEqual(1, msg.Blocks.Length, "Message should only have 1 block");
      Assert.IsNotNull(msg.Blocks[0].GatewayData, "message should have Machine Event Block Data");

      MachineActivityEventBlock maActual = msg.Blocks[0].GatewayData as MachineActivityEventBlock;
      Assert.IsNotNull(maActual, "Data Should be MachineActivityEventBlock");
      TamperSecurityStatusInformationMessage mssActual = maActual.Message as TamperSecurityStatusInformationMessage;
      Assert.IsNotNull(mssActual, "Data Should be MachineActivityEventBlock");
      Assert.AreEqual(mss.StartStatus, mssActual.StartStatus, "Start Status should be equal");
      Assert.AreEqual(mss.StartStatusTrigger, mssActual.StartStatusTrigger, "Start Status Trigger should be equal");
    }
    #endregion

    # region Fuel Engine Version 3
    
    [TestMethod]
    public void FuelEngineMessgeVersion3_Success()
    {
      //Payload from real device
      string hexMessageActual = "02590047F70DF99A4070110010DF8BE23A689AF11600000040080300000202000100000000250045000302001B0000022F002400007D0004A33D00015777001132490FF806A3C6F000005B990000000B00460500000000000000000075";
      MachineEventMessage msgActual = PlatformMessage.HydratePlatformMessage(HexDump.HexStringToBytes(hexMessageActual), true, true) as MachineEventMessage;

      MachineEventBlock block1 = new MachineEventBlock();
      block1 = msgActual.Blocks[1];

      FuelEnginePayloadCycleCountBlock fepc = new FuelEnginePayloadCycleCountBlock();
      fepc = (FuelEnginePayloadCycleCountBlock)block1.GatewayData;
      fepc.SubType = 0x00;
      FuelEngineReport innerMsg = new FuelEngineReport();
      innerMsg = (FuelEngineReport)fepc.Message;

      FuelEngineReport fuelEngine = new FuelEngineReport(); //expected Message
      fuelEngine.TransactionVersion = 0x03;
      fuelEngine.ReportingECMsVersion3 = new ReportingECMVersion3[2];
      fuelEngine.ReportingECMsVersion3[0] = new ReportingECMVersion3();
      fuelEngine.ReportingECMsVersion3[1] = new ReportingECMVersion3();

      fuelEngine.ReportingECMsVersion3[0].ECMIdentifier = 27;
      fuelEngine.ReportingECMsVersion3[0].FuelLevel = 47;
      fuelEngine.ReportingECMsVersion3[0].isFuelLevelAvailable = true;



      fuelEngine.ReportingECMsVersion3[1].ECMIdentifier = 36;
      fuelEngine.ReportingECMsVersion3[1].isFuelConsumptionAvailable = true;
      fuelEngine.ReportingECMsVersion3[1].isNumberOfEngineStartsAvailable = true;
      fuelEngine.ReportingECMsVersion3[1].isTotalEngineIdleTimeAvailable = true;
      fuelEngine.ReportingECMsVersion3[1].isTotalEngineRevolutionsAvailable = true;
      fuelEngine.ReportingECMsVersion3[1].isTotalIdleFuelAvailable = true;
      fuelEngine.ReportingECMsVersion3[1].isTotalMaximumFuelAvailable = true;
      fuelEngine.ReportingECMsVersion3[1].FuelConsumption = 37991.625;
      fuelEngine.ReportingECMsVersion3[1].NumberEngineStarts = 4088;
      fuelEngine.ReportingECMsVersion3[1].TotalEngineIdleTime = TimeSpan.FromHours(4396.36);
      fuelEngine.ReportingECMsVersion3[1].TotalEngineRevolutions = 445586368;
      fuelEngine.ReportingECMsVersion3[1].TotalIdleFuel = 2931.125;
      fuelEngine.ReportingECMsVersion3[1].TotalMaximumFuelGallons = 140873.125;

      Assert.AreEqual(fuelEngine.ReportingECMsVersion3[0].isFuelLevelAvailable, innerMsg.ReportingECMsVersion3[0].isFuelLevelAvailable, "Fuel level value should match");
      Assert.AreEqual(fuelEngine.ReportingECMsVersion3[0].FuelLevel.Value, innerMsg.ReportingECMsVersion3[0].FuelLevel.Value, "Fuel level value should match");

      Assert.AreEqual(fuelEngine.ReportingECMsVersion3[1].isFuelConsumptionAvailable, innerMsg.ReportingECMsVersion3[1].isFuelConsumptionAvailable, "Fuel consumption value should match");
      Assert.AreEqual(fuelEngine.ReportingECMsVersion3[1].isNumberOfEngineStartsAvailable, innerMsg.ReportingECMsVersion3[1].isNumberOfEngineStartsAvailable, "Number Of Engine Starts value should match");
      Assert.AreEqual(fuelEngine.ReportingECMsVersion3[1].isTotalEngineIdleTimeAvailable, innerMsg.ReportingECMsVersion3[1].isTotalEngineIdleTimeAvailable, "Total Engine Idle Time value should match");
      Assert.AreEqual(fuelEngine.ReportingECMsVersion3[1].isTotalEngineRevolutionsAvailable, innerMsg.ReportingECMsVersion3[1].isTotalEngineRevolutionsAvailable, "Total Engine revolutions value should match");
      Assert.AreEqual(fuelEngine.ReportingECMsVersion3[1].isTotalIdleFuelAvailable, innerMsg.ReportingECMsVersion3[1].isTotalIdleFuelAvailable, "Total Idle Fuel value should match");
      Assert.AreEqual(fuelEngine.ReportingECMsVersion3[1].isTotalMaximumFuelAvailable, innerMsg.ReportingECMsVersion3[1].isTotalMaximumFuelAvailable, "Total Max fuel Gallons value should match");
      Assert.AreEqual(fuelEngine.ReportingECMsVersion3[1].FuelConsumption.Value, innerMsg.ReportingECMsVersion3[1].FuelConsumption.Value, "Fuel consumption value should match");
      Assert.AreEqual(fuelEngine.ReportingECMsVersion3[1].NumberEngineStarts.Value, innerMsg.ReportingECMsVersion3[1].NumberEngineStarts.Value, "Number Of Engine Starts value should match");
      Assert.AreEqual(fuelEngine.ReportingECMsVersion3[1].TotalEngineIdleTime.Value, innerMsg.ReportingECMsVersion3[1].TotalEngineIdleTime.Value, "Total Engine Idle Time value should match");
      Assert.AreEqual(fuelEngine.ReportingECMsVersion3[1].TotalEngineRevolutions.Value, innerMsg.ReportingECMsVersion3[1].TotalEngineRevolutions.Value, "Total Engine revolutions value should match");
      Assert.AreEqual(fuelEngine.ReportingECMsVersion3[1].TotalIdleFuel.Value, innerMsg.ReportingECMsVersion3[1].TotalIdleFuel.Value, "Total Idle Fuel value should match");
      Assert.AreEqual(fuelEngine.ReportingECMsVersion3[1].TotalMaximumFuelGallons.Value, innerMsg.ReportingECMsVersion3[1].TotalMaximumFuelGallons.Value, "Total Max fuel Gallons value should match");
      
    }
    # endregion

    #region Radio Machine Secuirty Status Information Message

    [DatabaseTest]
    [TestMethod]
    public void RadioDeviceMachineSecurityReportingStatusMessageTest_AllNullTest()
    {
      RadioDeviceMachineSecurityReportingStatusMessageTest();
    }

    [DatabaseTest]
    [TestMethod]
    public void RadioDeviceMachineSecurityReportingStatusMessageTest_AllNullExceptMachineStartStatusTest()
    {
      RadioDeviceMachineSecurityReportingStatusMessageTest(latestmachineStartStatus: MachineSecurityModeSetting.NormalOperationWithMachineSecurityFeatureEnabled,
          currentmachineStartStatus: MachineSecurityModeSetting.NormalOperationWithMachineSecurityFeatureEnabled);
    }

    [TestMethod]
    public void RadioDeviceMachineSecurityReportingStatusMessageTest_AllNotNullTest()
    {
      RadioDeviceMachineSecurityReportingStatusMessageTest(latestmachineStartStatus: MachineSecurityModeSetting.NormalOperationWithMachineSecurityFeatureEnabled,
          currentmachineStartStatus: MachineSecurityModeSetting.NormalOperationWithMachineSecurityFeatureEnabled,
        tamperResistanceStatus: TamperResistanceStatus.TamperResistanceLevel2,
        deviceSecurityModeReceivingStatus: DeviceSecurityModeReceivingStatus.ModechangeRequestImplemented,
        sourceSecurityModeConfiguration: SourceSecurityModeConfiguration.VisionLink);
    }

    private void RadioDeviceMachineSecurityReportingStatusMessageTest(MachineSecurityModeSetting? latestmachineStartStatus = null,
                MachineSecurityModeSetting? currentmachineStartStatus = null, TamperResistanceStatus? tamperResistanceStatus = null,
                DeviceSecurityModeReceivingStatus? deviceSecurityModeReceivingStatus = null,
                    SourceSecurityModeConfiguration? sourceSecurityModeConfiguration = null)
    {
      MachineEventMessage receivedMessage = new MachineEventMessage();
      receivedMessage.LocationAge = new TimeSpan(0, 1, 0, 0, 0);
      receivedMessage.Latitude = 15;
      receivedMessage.Longitude = 12;
      receivedMessage.MilesTraveled = 10;
      receivedMessage.ServiceMeterHours = 15;
      receivedMessage.SpeedMPH = 10;
      receivedMessage.Track = 32;
      receivedMessage.UtcDateTime = DateTime.UtcNow;
      receivedMessage.LocationUncertaintyUnit = LocationUncertaintyUnitEnum.Meters;
      receivedMessage.LocationUncertainty = 25;
      receivedMessage.LocationAgeUnit = LocationAgeUnitEnum.Hours;
      receivedMessage.DevicePacketSequenceID = 17;
      receivedMessage.Blocks = new MachineEventBlock[1];

      MachineEventBlock block1 = new MachineEventBlock();
      block1.DeltaTime = new TimeSpan(-1, 0, 0);
      block1.Source = MachineEventSourceEnum.Radio;

      DeviceMachineSecurityReportingStatusMessage rptStatusMessage = new DeviceMachineSecurityReportingStatusMessage();
      rptStatusMessage.byteCount = 8;
      if (latestmachineStartStatus.HasValue)
        rptStatusMessage.LatestMachineSecurityModeconfiguration = latestmachineStartStatus.Value;
      if (currentmachineStartStatus.HasValue)
        rptStatusMessage.CurrentMachineSecurityModeconfiguration = currentmachineStartStatus.Value;
      if (tamperResistanceStatus.HasValue)
        rptStatusMessage.TamperResistanceMode = tamperResistanceStatus.Value;
      if (deviceSecurityModeReceivingStatus.HasValue)
        rptStatusMessage.DeviceSecurityModeReceivingStatus = deviceSecurityModeReceivingStatus.Value;
      if (sourceSecurityModeConfiguration.HasValue)
        rptStatusMessage.SourceSecurityModeConfiguration = sourceSecurityModeConfiguration.Value;

      block1.RadioData = rptStatusMessage;

      receivedMessage.Blocks[0] = block1;

      byte[] originalMsg = PlatformMessage.SerializePlatformMessage(receivedMessage);

      MachineEventMessage msg = PlatformMessage.HydratePlatformMessage(originalMsg, true, true) as MachineEventMessage;
      Assert.IsNotNull(msg, "Message should have been hydrated successfully");
      Assert.AreEqual(1, msg.Blocks.Length, "There should only be one message for this device");

      foreach (MachineEventBlock block in msg.Blocks)
      {
        Assert.IsNotNull(block.RadioData, "Radio Machine Security Information record should have been hyderated successfully");
        if (block.RadioData is DeviceMachineSecurityReportingStatusMessage)
        {
          DeviceMachineSecurityReportingStatusMessage objRec = block.RadioData as DeviceMachineSecurityReportingStatusMessage;
          if (latestmachineStartStatus.HasValue)
            Assert.AreEqual(rptStatusMessage.LatestMachineSecurityModeconfiguration, objRec.LatestMachineSecurityModeconfiguration, "Machine Latest Start Status aren't equal");
          if (currentmachineStartStatus.HasValue)
            Assert.AreEqual(rptStatusMessage.CurrentMachineSecurityModeconfiguration, objRec.CurrentMachineSecurityModeconfiguration, "Machine Current Start Status aren't equal");
          if (tamperResistanceStatus.HasValue)
            Assert.AreEqual(rptStatusMessage.TamperResistanceMode, objRec.TamperResistanceMode, "Tamper Resistance Statuses aren't equal");
          if (deviceSecurityModeReceivingStatus.HasValue)
            Assert.AreEqual(rptStatusMessage.DeviceSecurityModeReceivingStatus, objRec.DeviceSecurityModeReceivingStatus, "Device SecurityMode Receiving Status aren't equal");
          if (sourceSecurityModeConfiguration.HasValue)
            Assert.AreEqual(rptStatusMessage.SourceSecurityModeConfiguration, objRec.SourceSecurityModeConfiguration, "Source SecurityMode Configuration Status aren't equal");
        }
      }

    }
    #endregion


    # region TMS Message

    [TestMethod]
    public void TMSInformationGatewayBlockTest()
    {
      DateTime receivedUTC = DateTime.UtcNow;
      MachineEventMessage receivedMessage = new MachineEventMessage();
      receivedMessage.LocationAge = new TimeSpan(0, 1, 0, 0, 0);
      receivedMessage.Latitude = 15;
      receivedMessage.Longitude = 12;
      receivedMessage.MilesTraveled = 10;
      receivedMessage.ServiceMeterHours = 15;
      receivedMessage.SpeedMPH = 10;
      receivedMessage.Track = 32;
      receivedMessage.UtcDateTime = DateTime.UtcNow;
      receivedMessage.LocationUncertaintyUnit = LocationUncertaintyUnitEnum.Meters;
      receivedMessage.LocationUncertainty = 25;
      receivedMessage.LocationAgeUnit = LocationAgeUnitEnum.Hours;
      receivedMessage.DevicePacketSequenceID = 15;
      receivedMessage.Blocks = new MachineEventBlock[1];

      TMSMessageBlock tmsMsgBlk = new TMSMessageBlock();
      MachineEventBlock block = new MachineEventBlock();
      block.Source = MachineEventSourceEnum.Gateway;
      block.DeltaTime = TimeSpan.FromMinutes(-2);
      tmsMsgBlk.SubType = 0x02;
      TMSInformationMessage tmsInfoMessage = new TMSInformationMessage();
      tmsInfoMessage.TransactionVersion = 0x00;
      tmsInfoMessage.MID = 1;
      tmsInfoMessage.InstallationStatus = 16;
      tmsInfoMessage.RecordsCount = 2;
      tmsInfoMessage.tmsInfo = new TMSInfo[2];
      tmsInfoMessage.tmsInfo[0] = new TMSInfo();
      tmsInfoMessage.tmsInfo[0].AxlePosition = 1;
      tmsInfoMessage.tmsInfo[0].TirePosition = 1;
      tmsInfoMessage.tmsInfo[0].SensorID = "sensorId1";
      tmsInfoMessage.tmsInfo[1] = new TMSInfo();
      tmsInfoMessage.tmsInfo[1].AxlePosition = 2;
      tmsInfoMessage.tmsInfo[1].TirePosition = 1;
      tmsInfoMessage.tmsInfo[1].SensorID = "sensorId2";
      tmsMsgBlk.Message = tmsInfoMessage;
      block.GatewayData = tmsMsgBlk;
      receivedMessage.Blocks[0] = block;
      uint bitPosition = 0;
      byte[] originalMsg = PlatformMessage.SerializePlatformMessage(receivedMessage);
    
      MachineEventMessage hydratedReceivedMessage = PlatformMessage.HydratePlatformMessage(originalMsg, ref bitPosition, true, true) as MachineEventMessage;

      TMSMessageBlock tmsReceivedMsgBlk = hydratedReceivedMessage.Blocks[0].GatewayData as TMSMessageBlock;
      TMSInformationMessage hydratedtmsInformationMessage = (TMSInformationMessage)tmsReceivedMsgBlk.Message;
      TMSInfo tmsInfoRecord1 = hydratedtmsInformationMessage.tmsInfo[0];
      TMSInfo tmsInfoRecord2 = hydratedtmsInformationMessage.tmsInfo[1];

      TMSInfo expectedTMSInfo1 = new TMSInfo
      {
        AxlePosition = 1,
        TirePosition = 1,
        SensorID = "sensorId1"
      };

      TMSInfo expectedTMSInfo2 = new TMSInfo
      {
        AxlePosition = 2,
        TirePosition = 1,
        SensorID = "sensorId2"
      };
   
      Assert.AreEqual(tmsInfoRecord1.AxlePosition, expectedTMSInfo1.AxlePosition, "Axle position should match");
      Assert.AreEqual(tmsInfoRecord1.TirePosition, expectedTMSInfo1.TirePosition, "Tire position should match");
      Assert.AreEqual(tmsInfoRecord1.SensorID, expectedTMSInfo1.SensorID, "SensorId should match");
      Assert.AreEqual(tmsInfoRecord2.AxlePosition, expectedTMSInfo2.AxlePosition, "Axle position should match");
      Assert.AreEqual(tmsInfoRecord2.TirePosition, expectedTMSInfo2.TirePosition, "Tire position should match");
      Assert.AreEqual(tmsInfoRecord2.SensorID, expectedTMSInfo2.SensorID, "SensorId should match");
          
    }


    #endregion
  }
}
