using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Configuration;
using TechTalk.SpecFlow;
using VSS.MasterData.Device.AcceptanceTests.Utils.Config;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Device.AcceptanceTests.Scenarios.DeviceDetail
{
  [Binding]
  public class DeviceDetailSteps
  {
    public string Payload;
    public string DeviceSerialNumber;
    public string DeviceType;
    public string DatalinkType;
    public string ModuleType;
    public string GatewayFirmwarePartNumber;
    public string RadioFirmwarePartNumber;
    public string MainboardSoftwareVersion;
    // public static DeviceDetailsConfigInfoEvent DeviceDetailsConfigInfoEvent = new DeviceDetailsConfigInfoEvent();

    public static DeviceDetailsConfigInfoEvent DeviceDetailsConfigInfoEvent = new DeviceDetailsConfigInfoEvent();

    public DeviceDetailSteps()
    {
      DeviceServiceConfig.SetupEnvironment();
    }
    public void SetDefaultdeviceDetailvalues()
    {


      DeviceDetailsConfigInfoEvent.ModuleCode= ConfigurationManager.AppSettings["DeviceDetail_DeviceSerialNumber"];
      
      DeviceDetailsConfigInfoEvent.DeviceType = (VisionLink.Interfaces.Events.MasterData.Models.DeviceType)Enum.Parse(typeof(VisionLink.Interfaces.Events.MasterData.Models.DeviceType), ConfigurationManager.AppSettings["DeviceDetail_DeviceType"]);
      DeviceDetailsConfigInfoEvent.GeneralRegistries = new GeneralRegistry();
      DeviceDetailsConfigInfoEvent.GeneralRegistries.Software = new GeneralRegistry.SoftwareInfo();
      DeviceDetailsConfigInfoEvent.GeneralRegistries.ModuleType= "PL321R";
      DeviceDetailsConfigInfoEvent.GeneralRegistries.DataLinkType = "CDL";
      DeviceDetailsConfigInfoEvent.GeneralRegistries.Software.HC11SoftwarePartNumber = "3303409-00";
      //DeviceDetailsConfigInfoEvent.GeneralRegistries.Software.ModemSoftwarePartNumber = "123";


      DeviceDetailsConfigInfoEvent.IsGlobalGramSet = true;
      DeviceDetailsConfigInfoEvent.GlobalGramEnabled = false;
      DeviceDetailsConfigInfoEvent.Status = 0;
      DeviceDetailsConfigInfoEvent.ActionUTC = Convert.ToDateTime(DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.ffffff"));
      DeviceDetailsConfigInfoEvent.ReceivedUTC = Convert.ToDateTime(DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.ffffff"));

      DeviceSerialNumber = DeviceDetailsConfigInfoEvent.ModuleCode;
      DeviceType = Convert.ToString(DeviceDetailsConfigInfoEvent.DeviceType);
      DatalinkType = Convert.ToString(DeviceDetailsConfigInfoEvent.GeneralRegistries.DataLinkType);
      ModuleType = DeviceDetailsConfigInfoEvent.GeneralRegistries.ModuleType;
      GatewayFirmwarePartNumber = DeviceDetailsConfigInfoEvent.GeneralRegistries.Software.HC11SoftwarePartNumber;

    }

    [Given(@"There Is DeviceDetailConfigInfoEvent For existing PL(.*) Device")]
    public void GivenThereIsDeviceDetailConfigInfoEventForExistingPLDevice(int p0)
    {
      SetDefaultdeviceDetailvalues();
      Payload = JsonConvert.SerializeObject(new { DeviceDetailsConfigInfoEvent = DeviceDetailsConfigInfoEvent }, Formatting.Indented, new StringEnumConverter());
    }

    [When(@"I Publish To '(.*)' Kafka Topic")]
    public void WhenIPublishToKafkaTopic(string Topic)
    {
      Topic = DeviceServiceConfig.DeviceDetailKafkaTopic;
      KafkaServicesConfig.InitializeKafkaProducer(Topic);
      //LogResult.Report(Log, "log_ForInfo", "Posting the AssetStatusEvent request with Valid Values: " + payload);     
      try
      {
        KafkaServicesConfig.ProduceMessage(Payload, DeviceSerialNumber);
      }
      catch (Exception e)
      {
        throw new Exception("Unable To publish in kafka", e);
      }


    }



  }
}
