using AutomationCore.Shared.Library;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TechTalk.SpecFlow;
using VSS.MasterData.Device.AcceptanceTests.Utils.Config;
using VSS.MasterData.Device.AcceptanceTests.Utils.Features.Classes.DeviceService;

namespace VSS.MasterData.Device.AcceptanceTests.Scenarios.DeviceService
{
  [Binding]
  public class DeviceServiceSteps
  {

    #region Variables
    public string TestName;

    //DB Configuration
    public static string MySqlConnectionString;

    private static Log4Net Log = new Log4Net(typeof(DeviceServiceSteps));
    public static DeviceServiceSupport deviceServiceSupport = new DeviceServiceSupport(Log);

    public static CreateDeviceEvent defaultValidDeviceServiceCreateEvent = new CreateDeviceEvent();
    public static UpdateDeviceEvent defaultValidDeviceServiceUpdateEvent = new UpdateDeviceEvent();

    #endregion

    public DeviceServiceSteps()
    {
      MySqlConnectionString = DeviceServiceConfig.MySqlConnection;
    }

    [BeforeFeature()]
    public static void InitializeKafka()
    {
      if (FeatureContext.Current.FeatureInfo.Title.Equals("DeviceService"))
      {
        KafkaServicesConfig.InitializeKafkaConsumer(deviceServiceSupport);
      }
    }

    [Given(@"DeviceService Is Ready To Verify '(.*)'")]
    public void GivenDeviceServiceIsReadyToVerify(string testDescription)
    {
      //log the scenario info
      TestName = (ScenarioContext.Current.ScenarioInfo).Title + "_" + testDescription;
      //TestName = TestDescription;
      LogResult.Report(Log, "log_ForInfo", "Execution started for Test Scenario" + TestName);
    }

    [Given(@"DeviceServiceCreate Request Is Setup With Default Values")]
    public void GivenDeviceServiceCreateRequestIsSetupWithDefaultValues()
    {
      deviceServiceSupport.CreateDeviceEvent = GetDefaultValidDeviceServiceCreateRequest();
    }

    [Then(@"I Update '(.*)' And '(.*)'")]
    public void ThenIUpdateAnd(string radiofirmwarepartnumber, string firmwarepartnumber)
    {
      if (radiofirmwarepartnumber == "NULL")
      {
        deviceServiceSupport.CreateDeviceEvent.RadioFirmwarePartNumber = null;
      }

      if (firmwarepartnumber == "NULL")
      {
        deviceServiceSupport.CreateDeviceEvent.FirmwarePartNumber = null;
      }
    }



    [Then(@"I Set '(.*)' And '(.*)' And '(.*)' And '(.*)' And '(.*)'")]
    public void ThenISetAndAndAndAnd(string CellModemIMEI, string DevicePartNumber, string CellularFirmwarePartnumber, string NetworkFirmwarePartnumber, string SatelliteFirmwarePartnumber)
    {
      if (CellModemIMEI!="NULL")
        deviceServiceSupport.CreateDeviceEvent.CellModemIMEI = CellModemIMEI;
      if (DevicePartNumber != "NULL")
        deviceServiceSupport.CreateDeviceEvent.DevicePartNumber = DevicePartNumber;
      if (CellularFirmwarePartnumber != "NULL")
        deviceServiceSupport.CreateDeviceEvent.CellularFirmwarePartnumber = CellularFirmwarePartnumber;
      if (NetworkFirmwarePartnumber != "NULL")
        deviceServiceSupport.CreateDeviceEvent.NetworkFirmwarePartnumber = NetworkFirmwarePartnumber;
      if (SatelliteFirmwarePartnumber != "NULL")
        deviceServiceSupport.CreateDeviceEvent.SatelliteFirmwarePartnumber = SatelliteFirmwarePartnumber;
    }



    [Then(@"DeviceServiceUpdate Request Is Setup With Default Values")]
    public void ThenDeviceServiceUpdateRequestIsSetupWithDefaultValues()
    {
      deviceServiceSupport.UpdateDeviceEvent = GetDefaultValidDeviceServiceUpdateRequest();
    }

    [Then(@"I Update CreateEventRequest With '(.*)'")]
    public void ThenIUpdateCreateEventRequestWith(string element)
    {
      if (element == "Radio")
      {
        deviceServiceSupport.CreateDeviceEvent.FirmwarePartNumber = null;
      }
      else if (element == "Firmware")
      {
        deviceServiceSupport.CreateDeviceEvent.RadioFirmwarePartNumber = null;
      }
    }

    [Then(@"I Update UpdateEventRequest With '(.*)'")]
    public void ThenIUpdateUpdateEventRequestWith(string element)
    {
      if (element == "Radio")
      {
        deviceServiceSupport.UpdateDeviceEvent.FirmwarePartNumber = null;
      }
      else if (element == "Firmware")
      {
        deviceServiceSupport.UpdateDeviceEvent.RadioFirmwarePartNumber = "3303408-00";
      }
    }

    [When(@"I Post Valid DeviceServiceCreate Request")]
    public void WhenIPostValidDeviceServiceCreateRequest()
    {
      deviceServiceSupport.PostValidCreateRequestToService();
    }

    [When(@"I Post Valid DeviceServiceupdate Request")]
    public void WhenIPostValidDeviceServiceupdateRequest()
    {
      deviceServiceSupport.PostValidUpdateRequestToService();
    }

    [Then(@"The DeviceCreated Details must be stored in MySql DB")]
    public void ThenTheDeviceCreatedDetailsMustBeStoredInMySqlDB()
    {
      try
      {
        Assert.IsTrue(deviceServiceSupport.ValidateDB("CreateEvent"), "DB Verification failed");
        LogResult.Report(Log, "log_ForInfo", "DB Validation Successful\n");
      }
      catch (Exception e)
      {
        LogResult.Report(Log, "log_ForError", "Got Error While Validating DB", e);
        throw new Exception(e + "Got Error While Validating DB\n");
      }
    }

    [Then(@"The DeviceUpdated Details must be stored in MySql DB")]
    public void ThenTheDeviceUpdatedDetailsMustBeStoredInMySqlDB()
    {
      try
      {
        Assert.IsTrue(deviceServiceSupport.ValidateDB("UpdateEvent"), "DB Verification failed");
        LogResult.Report(Log, "log_ForInfo", "DB Validation Successful\n");
      }
      catch (Exception e)
      {
        LogResult.Report(Log, "log_ForError", "Got Error While Validating DB", e);
        throw new Exception(e + "Got Error While Validating DB\n");
      }
    }


    [Then(@"The Processed DeviceServiceCreate Message must be available in Kafka topic")]
    public void ThenTheProcessedDeviceServiceCreateMessageMustBeAvailableInKafkaTopic()
    {
      deviceServiceSupport.VerifyDeviceServiceCreateResponse();
    }

    #region Helpers

    public static CreateDeviceEvent GetDefaultValidDeviceServiceCreateRequest()
    {
      defaultValidDeviceServiceCreateEvent.DeviceUID = Guid.NewGuid();
      defaultValidDeviceServiceCreateEvent.DeviceSerialNumber = "AutoTestAPICreateDeviceSerial" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultValidDeviceServiceCreateEvent.DeviceType = "PLE641";
      defaultValidDeviceServiceCreateEvent.DeviceState = "Subscribed";
      defaultValidDeviceServiceCreateEvent.DeregisteredUTC = DateTime.UtcNow;
      defaultValidDeviceServiceCreateEvent.ModuleType = null;
      defaultValidDeviceServiceCreateEvent.MainboardSoftwareVersion = "534";
      defaultValidDeviceServiceCreateEvent.RadioFirmwarePartNumber = "3303408-00";
      defaultValidDeviceServiceCreateEvent.GatewayFirmwarePartNumber = "3303409-00";
      defaultValidDeviceServiceCreateEvent.DataLinkType = "CDL";
      defaultValidDeviceServiceCreateEvent.FirmwarePartNumber = "5468";
      defaultValidDeviceServiceCreateEvent.CellModemIMEI = null;
      defaultValidDeviceServiceCreateEvent.DevicePartNumber = null;
      defaultValidDeviceServiceCreateEvent.CellularFirmwarePartnumber = null;
      defaultValidDeviceServiceCreateEvent.NetworkFirmwarePartnumber = null;
      defaultValidDeviceServiceCreateEvent.SatelliteFirmwarePartnumber = null;
      defaultValidDeviceServiceCreateEvent.ActionUTC = DateTime.UtcNow;
      defaultValidDeviceServiceCreateEvent.ReceivedUTC = DateTime.UtcNow;
      return defaultValidDeviceServiceCreateEvent;
    }

    public static UpdateDeviceEvent GetDefaultValidDeviceServiceUpdateRequest()
    {
      defaultValidDeviceServiceUpdateEvent.DeviceUID = defaultValidDeviceServiceCreateEvent.DeviceUID;
      defaultValidDeviceServiceUpdateEvent.OwningCustomerUID = Guid.NewGuid();
      defaultValidDeviceServiceUpdateEvent.DeviceSerialNumber = "AutoTestAPICreateDeviceSerial" + DateTime.UtcNow.ToString("yyyyMMddhhmmss");
      defaultValidDeviceServiceUpdateEvent.DeviceType = "PL121";
      defaultValidDeviceServiceUpdateEvent.DeviceState = "Provisioned";
      defaultValidDeviceServiceUpdateEvent.DeregisteredUTC = DateTime.UtcNow;
      defaultValidDeviceServiceUpdateEvent.ModuleType = "PL121SR";
      defaultValidDeviceServiceUpdateEvent.MainboardSoftwareVersion = "634";
      defaultValidDeviceServiceUpdateEvent.RadioFirmwarePartNumber = "3305154-08";
      defaultValidDeviceServiceUpdateEvent.GatewayFirmwarePartNumber = "356123-61";
      defaultValidDeviceServiceUpdateEvent.DataLinkType = "J1939";
      defaultValidDeviceServiceUpdateEvent.FirmwarePartNumber = "56159";
      defaultValidDeviceServiceUpdateEvent.CellModemIMEI = null;
      defaultValidDeviceServiceUpdateEvent.DevicePartNumber = null;
      defaultValidDeviceServiceUpdateEvent.CellularFirmwarePartnumber = null;
      defaultValidDeviceServiceUpdateEvent.NetworkFirmwarePartnumber = null;
      defaultValidDeviceServiceUpdateEvent.SatelliteFirmwarePartnumber = null;
      defaultValidDeviceServiceUpdateEvent.ActionUTC = DateTime.UtcNow;
      defaultValidDeviceServiceUpdateEvent.ReceivedUTC = DateTime.UtcNow;
      return defaultValidDeviceServiceUpdateEvent;
    }
    #endregion

  }
}
