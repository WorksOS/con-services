using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using VSS.MasterData.Device.AcceptanceTests.Utils.Config;
using VSS.MasterData.Device.AcceptanceTests.Utils.DeviceFirmwarePartNumberModel;
using System.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AutomationCore.Shared.Library;

namespace VSS.MasterData.Device.AcceptanceTests.Scenarios.DeviceFirmwarePartnumber
{
  [Binding]
  public class DeviceFirmwarePartNumberSteps
  {

    public static Log4Net Log = new Log4Net(typeof(DeviceFirmwarePartNumberSteps));

    public static string DeviceFirmwarePartNumberKafkaTopic;

    public static DeviceFirmwarePartNumberModel DeviceFirmwarePartnumberModel = new DeviceFirmwarePartNumberModel();
    public static string Payload;
    public static string DeviceUID;

    public static bool IsCellular = false;
    public static bool IsNetwork = false;
    public static bool IsSatellite = false;
    public const string MINLENGTH = "A";
    public const string MAXLENGTH = "123456789012345678901234567890123456789-0123456789";

    public DeviceFirmwarePartNumberSteps()
    {
      DeviceServiceConfig.SetupEnvironment();
    }

    public void SetDefaultCellularRadioFirmwarePartNumber()
    {
      DeviceFirmwarePartnumberModel.Description = "AUTO_CELLULARFIRMWAREPN";
      DeviceFirmwarePartnumberModel.Value = "5269979-00";

      SetDefaultPartNumberValues();

    }
    public void SetDefaultNetworkManagerfirmwarePartNumber()
    {
      
      DeviceFirmwarePartnumberModel.Description = "AUTO_NETWORKFIRMWAREPN";
      DeviceFirmwarePartnumberModel.Value = DateTime.Now.ToString("yyyyMMddTHHmmssffffff"); ;
      SetDefaultPartNumberValues();

    }

    public void SetDefaultSatelliteRadiofirmwarePartNumber()
    {
      
      DeviceFirmwarePartnumberModel.Description = "AUTO_SatelliteRadioFIRMWAREPN";
      DeviceFirmwarePartnumberModel.Value = DateTime.Now.ToString("yyyyMMddTHHmmssffffff"); ;
      SetDefaultPartNumberValues();

    }

    public void SetDefaultPartNumberValues()
    {

      DeviceFirmwarePartnumberModel.MessageHash = "D5D682CEF695547BB34A37EEBAF1DF6F";

      DeviceFirmwarePartnumberModel.Asset = new Utils.DeviceFirmwarePartNumberModel.Asset();

      DeviceFirmwarePartnumberModel.Asset.AssetUid = ConfigurationManager.AppSettings["A5N2AssetUID"];
      DeviceFirmwarePartnumberModel.Asset.MakeCode = ConfigurationManager.AppSettings["A5N2Make"];
      DeviceFirmwarePartnumberModel.Asset.SerialNumberVin = ConfigurationManager.AppSettings["A5N2SerialNUmberVin"];

      DeviceFirmwarePartnumberModel.Device = new Utils.DeviceFirmwarePartNumberModel.Device();
      DeviceFirmwarePartnumberModel.Device.DeviceUID = ConfigurationManager.AppSettings["A5N2Deviceuid"];
      DeviceFirmwarePartnumberModel.Device.DeviceId = ConfigurationManager.AppSettings["A5N2DeviceID"];
      DeviceFirmwarePartnumberModel.Device.DeviceType = ConfigurationManager.AppSettings["A5N2DeviceType"];

      DeviceFirmwarePartnumberModel.Timestamp = new Timestamp();
      DeviceFirmwarePartnumberModel.Timestamp.EventUtc = Convert.ToDateTime(DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.ffffff"));
      DeviceFirmwarePartnumberModel.Timestamp.ReceivedUtc = Convert.ToDateTime(DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.ffffff"));

    }

    [Given(@"There Is DeviceCellularRadioFirmwarePartNumber_HappyPath Request")]
    public void GivenThereIsDeviceCellularRadioFirmwarePartNumber_HappyPathRequest()
    {
      IsCellular = true;
      SetDefaultCellularRadioFirmwarePartNumber();
      DeviceUID = DeviceFirmwarePartnumberModel.Device.DeviceUID;
      Payload = JsonConvert.SerializeObject( DeviceFirmwarePartnumberModel, Formatting.Indented, new StringEnumConverter());
    }


    [Given(@"There Is DeviceNetworkManagerFirmwarePartNumber_HappyPath Request")]
    public void GivenThereIsDeviceNetworkManagerFirmwarePartNumber_HappyPathRequest()
    {
      IsNetwork = true;
      SetDefaultNetworkManagerfirmwarePartNumber();
      DeviceUID = DeviceFirmwarePartnumberModel.Device.DeviceUID;
      Payload = JsonConvert.SerializeObject(DeviceFirmwarePartnumberModel, Formatting.Indented, new StringEnumConverter());
    }

    [Given(@"There Is DeviceSatelliteRadioFirmwarePartNumber_HappyPath Request")]
    public void GivenThereIsDeviceSatelliteRadioFirmwarePartNumber_HappyPathRequest()
    {
      IsSatellite = true;
      SetDefaultSatelliteRadiofirmwarePartNumber();
      DeviceUID = DeviceFirmwarePartnumberModel.Device.DeviceUID;
      Payload = JsonConvert.SerializeObject(DeviceFirmwarePartnumberModel, Formatting.Indented, new StringEnumConverter());
    }

    [Given(@"There Is DeviceCellularRadioFirmwarePartNumber_HappyPath Request With '(.*)'")]
    public void GivenThereIsDeviceCellularRadioFirmwarePartNumber_HappyPathRequestWith(string cellularRadioLength)
    {

      IsCellular = true;
      SetDefaultCellularRadioFirmwarePartNumber();

      
      if (cellularRadioLength == "MINLENGTH")
        DeviceFirmwarePartnumberModel.Value = MINLENGTH;
      if (cellularRadioLength == "MAXLENGTH")
        DeviceFirmwarePartnumberModel.Value = MAXLENGTH;
 

      DeviceUID = DeviceFirmwarePartnumberModel.Device.DeviceUID;
      Payload = JsonConvert.SerializeObject(DeviceFirmwarePartnumberModel, Formatting.Indented, new StringEnumConverter());
    }

    [Given(@"There Is DeviceNetworkManagerFirmwarePartNumber_HappyPath Request With '(.*)'")]
    public void GivenThereIsDeviceNetworkManagerFirmwarePartNumber_HappyPathRequestWith(string networkFirmwareLength)
    {
      IsNetwork = true;

      SetDefaultNetworkManagerfirmwarePartNumber();


      if (networkFirmwareLength == "MINLENGTH")
        DeviceFirmwarePartnumberModel.Value = MINLENGTH;
      if (networkFirmwareLength == "MAXLENGTH")
        DeviceFirmwarePartnumberModel.Value = MAXLENGTH;


      DeviceUID = DeviceFirmwarePartnumberModel.Device.DeviceUID;
      Payload = JsonConvert.SerializeObject(DeviceFirmwarePartnumberModel, Formatting.Indented, new StringEnumConverter());
    }




    [When(@"I Publish Valid Device Firmware Info to Kafka Topic")]
    public void WhenIPublishValidDeviceFirmwareInfoToKafkaTopic()
    {

      if(IsCellular)
        DeviceServiceConfig.DeviceFirmwareKafkaTopic = ConfigurationManager.AppSettings["CellularFirmwareKafkaTopic"];

      if(IsNetwork)
      DeviceServiceConfig.DeviceFirmwareKafkaTopic = ConfigurationManager.AppSettings["NetworkManagerFirmwareKafkaTopic"];

      if(IsSatellite)
        DeviceServiceConfig.DeviceFirmwareKafkaTopic = ConfigurationManager.AppSettings["SatelliteRadioFirmwarePartNUmber"];

      KafkaServicesConfig.InitializeKafkaProducer(DeviceServiceConfig.DeviceFirmwareKafkaTopic);
      try
      {
        KafkaServicesConfig.ProduceMessage(Payload, DeviceUID);
      }
      catch (Exception e)
      {
        throw new Exception("Unable To publish in kafka", e);
      }

    }

    [Then(@"The Device Firmware Info Value Should Be Available In VSSDB")]
    public void ThenTheDeviceFirmwareInfoValueShouldBeAvailableInVSSDB()
    {
      Assert.IsTrue(DeviceFirmwarePartNumberSupport.VerifyDeviceFirmware(DeviceUID),"DB Validation Fail");
    }



  }
}
