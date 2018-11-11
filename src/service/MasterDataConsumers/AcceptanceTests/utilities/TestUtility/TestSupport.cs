using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using Newtonsoft.Json;
using System.Globalization;
using System.Threading;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using System.Text.RegularExpressions;

namespace TestUtility
{
  public class TestSupport
  {
    #region Public Properties
    public string AssetUid { get; set; }
    public DateTime FirstEventDate { get; set; }
    public DateTime LastEventDate { get; set; }
    public bool IsPublishToKafka { get; set; }
    public readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified, NullValueHandling = NullValueHandling.Ignore };
    public readonly TestConfig tsCfg = new TestConfig();
    #endregion

    #region Private Properties

    private readonly Random rndNumber = new Random();
    private readonly object syncLock = new object();
    private const char SEPARATOR = '|';

    private readonly Msg msg = new Msg();

    #endregion

    #region Public Methods

    public TestSupport()
    {
      SetFirstEventDate();
      SetAssetUid();
      IsPublishToKafka = true;
    }

    /// <summary>
    /// Set up the first event date for the events to go in. Also used as project start date for project tests.
    /// </summary>
    public void SetFirstEventDate()
    {
      FirstEventDate = DateTime.SpecifyKind(DateTime.Today.AddDays(-RandomNumber(10, 360)), DateTimeKind.Unspecified);
    }

    /// <summary>
    /// Set the asset UID to a random GUID
    /// </summary>
    public void SetAssetUid()
    {
      AssetUid = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Set the legacy asset id
    /// </summary>
    /// <returns>get the maximum legacy asset id plus 1</returns>
    public int SetLegacyAssetId()
    {
      var mysql = new MySqlHelper();
      var query = "SELECT max(LegacyAssetID) FROM Asset;";
      var result = mysql.ExecuteMySqlQueryAndReturnRecordCountResult(tsCfg.DbConnectionString, query);
      if (string.IsNullOrEmpty(result))
      { return 1000; }
      var legacyAssetId = Convert.ToInt32(result);
      return legacyAssetId + 1001;
    }


    public int SetLegacyProjectId()
    {
      var mysql = new MySqlHelper();
      var query = "SELECT max(LegacyProjectID) FROM Project;";
      var result = mysql.ExecuteMySqlQueryAndReturnRecordCountResult(tsCfg.DbConnectionString, query);
      if (string.IsNullOrEmpty(result))
      { return 1000; }
      var legacyAssetId = Convert.ToInt32(result);
      return legacyAssetId + 1001;
    }
    /// <summary>
    /// Set to true if writing to kafka instead of the database
    /// </summary>
    /// <param name="iAmPublishingToKafka">true or false</param>
    public void SetPublishToKafka(bool iAmPublishingToKafka)
    {
      IsPublishToKafka = iAmPublishingToKafka;
    }

    /// <summary>
    /// Publish events to kafka from string array
    /// </summary>
    /// <param name="eventArray">string array with all the events we are going to publish</param>
    public void PublishEventCollection(string[] eventArray)
    {
      msg.DisplayEventsToConsole(eventArray);
      var allColumnNames = eventArray.ElementAt(0).Split(SEPARATOR);
      var kafkaDriver = new RdKafkaDriver();
      for (var rowCnt = 1; rowCnt <= eventArray.Length - 1; rowCnt++)
      {
        var eventRow = eventArray.ElementAt(rowCnt).Split(SEPARATOR);
        dynamic eventObject = ConvertToExpando(allColumnNames, eventRow);
        var eventDate = eventObject.EventDate;
        LastEventDate = eventDate;
        if (IsPublishToKafka)
        {
          BuildEventAndPublishToKafka(eventObject, kafkaDriver);
          WaitForTimeBasedOnNumberOfRecords(eventArray.Length);
        }
        else
        {
          IsNotSameAsset(true);  // This can be added directly to tests
          BuildMySqlInsertStringAndWriteToDatabase(eventObject);
        }
      }
    }

    public string GetBaseUri()
    {
      var baseUri = tsCfg.webApiUri;
      if (Debugger.IsAttached)
      {
        baseUri = tsCfg.debugWebApiUri;
      }
      return baseUri;
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Set the full kafka topic name
    /// </summary>
    /// <param name="masterDataEvent"></param>
    /// <returns></returns>
    private string SetKafkaTopicName(string masterDataEvent)
    {
      var topicName = tsCfg.masterDataTopic + masterDataEvent + tsCfg.kafkaTopicSuffix;
      return topicName;
    }

    /// <summary>
    /// Wait for time to let the data feed process
    /// </summary>
    /// <param name="count"></param>
    private static void WaitForTimeBasedOnNumberOfRecords(int count)
    {
      if (count < 7)
      { Thread.Sleep(1000); }
      if (count > 6)
      { Thread.Sleep(1000); }
      if (count > 13)
      { Thread.Sleep(1000); }
    }
    /// <summary>
    /// Create an instance of the master data events. Convert to JSON and send to kafka 
    /// </summary>
    /// <param name="eventObject">event to be published</param>
    /// <param name="kafkaDriver">kafkadriver</param>
    public void BuildEventAndPublishToKafka(dynamic eventObject, RdKafkaDriver kafkaDriver)
    {
      var topicName = string.Empty;
      var jsonString = string.Empty;
      string eventType = eventObject.EventType;
      #region publish kafka events
      switch (eventType)
      {
        case "CreateAssetEvent":
          topicName = SetKafkaTopicName("IAssetEvent");
          var createAssetEvent = new CreateAssetEvent()
          {
            ActionUTC = eventObject.EventDate,
            AssetUID = new Guid(AssetUid),
            AssetName = eventObject.AssetName,
            AssetType = eventObject.AssetType,
            SerialNumber = eventObject.SerialNumber,
            MakeCode = eventObject.Make,
            Model = eventObject.Model,
            IconKey = Convert.ToInt32(eventObject.IconKey)
          };
          if (HasProperty(eventObject, "OwningCustomerUID"))
          {
            createAssetEvent.OwningCustomerUID = Guid.Parse(eventObject.OwningCustomerUID);
          }
          if (HasProperty(eventObject, "LegacyAssetId"))
          {
            createAssetEvent.LegacyAssetId = Convert.ToInt64(eventObject.LegacyAssetId);
          }
          if (HasProperty(eventObject, "EquipmentVIN"))
          {
            createAssetEvent.EquipmentVIN = eventObject.EquipmentVIN;
          }

          jsonString = JsonConvert.SerializeObject(new { CreateAssetEvent = createAssetEvent }, jsonSettings);

          break;
        case "UpdateAssetEvent":
          topicName = SetKafkaTopicName("IAssetEvent");
          var updateAssetEvent = new UpdateAssetEvent()
          {
            ActionUTC = eventObject.EventDate,
            AssetUID = new Guid(AssetUid)
          };
          if (HasProperty(eventObject, "AssetName"))
          {
            updateAssetEvent.AssetName = eventObject.AssetName;
          }
          if (HasProperty(eventObject, "AssetType"))
          {
            updateAssetEvent.AssetType = eventObject.AssetType;
          }
          if (HasProperty(eventObject, "Model"))
          {
            updateAssetEvent.Model = eventObject.Model;
          }
          if (HasProperty(eventObject, "IconKey"))
          {
            updateAssetEvent.IconKey = Convert.ToInt32(eventObject.IconKey);
          }
          if (HasProperty(eventObject, "LegacyAssetId"))
          {
            updateAssetEvent.LegacyAssetId = Convert.ToInt32(eventObject.LegacyAssetId);
          }
          if (HasProperty(eventObject, "OwningCustomerUID"))
          {
            updateAssetEvent.OwningCustomerUID = Guid.Parse(eventObject.OwningCustomerUID);
          }
          if (HasProperty(eventObject, "EquipmentVIN"))
          {
            updateAssetEvent.EquipmentVIN = eventObject.EquipmentVIN;
          }

          jsonString = JsonConvert.SerializeObject(new { UpdateAssetEvent = updateAssetEvent }, jsonSettings);
          break;
        case "DeleteAssetEvent":
          topicName = SetKafkaTopicName("IAssetEvent");
          var deleteAssetEvent = new DeleteAssetEvent()
          {
            ActionUTC = eventObject.EventDate,
            AssetUID = new Guid(AssetUid)
          };
          jsonString = JsonConvert.SerializeObject(new { DeleteAssetEvent = deleteAssetEvent }, jsonSettings);
          break;
        case "CreateDeviceEvent":
          topicName = SetKafkaTopicName("IDeviceEvent");
          var createDeviceEvent = new CreateDeviceEvent()
          {
            ActionUTC = eventObject.EventDate,
            ReceivedUTC = eventObject.EventDate, //TODO is this required?
            DeviceSerialNumber = eventObject.DeviceSerialNumber,
            DeviceState = eventObject.DeviceState,
            DeviceType = eventObject.DeviceType,
            DeviceUID = new Guid(eventObject.DeviceUID)
          };
          if (HasProperty(eventObject, "DeregisteredUTC"))
          {
            createDeviceEvent.DeregisteredUTC = DateTime.Parse(eventObject.DeregisteredUTC);
          }
          if (HasProperty(eventObject, "DataLinkType"))
          {
            createDeviceEvent.DataLinkType = eventObject.DataLinkType;
          }
          if (HasProperty(eventObject, "GatewayFirmwarePartNumber"))
          {
            createDeviceEvent.GatewayFirmwarePartNumber = eventObject.GatewayFirmwarePartNumber;
          }
          if (HasProperty(eventObject, "MainboardSoftwareVersion"))
          {
            createDeviceEvent.MainboardSoftwareVersion = eventObject.MainboardSoftwareVersion;
          }
          if (HasProperty(eventObject, "ModuleType"))
          {
            createDeviceEvent.ModuleType = eventObject.ModuleType;
          }
          if (HasProperty(eventObject, "RadioFirmwarePartNumber"))
          {
            createDeviceEvent.RadioFirmwarePartNumber = eventObject.RadioFirmwarePartNumber;
          }
          jsonString = JsonConvert.SerializeObject(new { CreateDeviceEvent = createDeviceEvent }, jsonSettings);
          break;
        case "UpdateDeviceEvent":
          topicName = SetKafkaTopicName("IDeviceEvent");
          var updateDeviceEvent = new UpdateDeviceEvent()
          {
            ActionUTC = eventObject.EventDate,
            ReceivedUTC = eventObject.EventDate,
            DeviceSerialNumber = eventObject.DeviceSerialNumber,
            DeviceState = eventObject.DeviceState,
            DeviceType = eventObject.DeviceType,
            DeviceUID = new Guid(eventObject.DeviceUID)
          };
          if (HasProperty(eventObject, "DataLinkType"))
          {
            updateDeviceEvent.DataLinkType = eventObject.DataLinkType;
          }
          if (HasProperty(eventObject, "GatewayFirmwarePartNumber"))
          {
            updateDeviceEvent.GatewayFirmwarePartNumber = eventObject.GatewayFirmwarePartNumber;
          }
          if (HasProperty(eventObject, "MainboardSoftwareVersion"))
          {
            updateDeviceEvent.MainboardSoftwareVersion = eventObject.MainboardSoftwareVersion;
          }
          if (HasProperty(eventObject, "ModuleType"))
          {
            updateDeviceEvent.ModuleType = eventObject.ModuleType; //missing from Update repo
          }
          if (HasProperty(eventObject, "DataLinkType"))
          {
            updateDeviceEvent.RadioFirmwarePartNumber = eventObject.RadioFirmwarePartNumber;
          }
          if (HasProperty(eventObject, "DeregisteredUTC"))
          {
            updateDeviceEvent.DeregisteredUTC = DateTime.Parse(eventObject.DeregisteredUTC);
          }
          jsonString = JsonConvert.SerializeObject(new { UpdateDeviceEvent = updateDeviceEvent }, jsonSettings);
          break;
        case "AssociateDeviceAssetEvent":
          topicName = SetKafkaTopicName("IDeviceEvent");
          var associateDeviceEvent = new AssociateDeviceAssetEvent()
          {
            ActionUTC = eventObject.EventDate,
            ReceivedUTC = eventObject.EventDate,
            AssetUID = new Guid(eventObject.AssetUID),
            DeviceUID = new Guid(eventObject.DeviceUID),
          };
          jsonString = JsonConvert.SerializeObject(new { AssociateDeviceAssetEvent = associateDeviceEvent }, jsonSettings);
          break;
        case "DissociateDeviceAssetEvent":
          topicName = SetKafkaTopicName("IDeviceEvent");
          var dissociateDeviceEvent = new DissociateDeviceAssetEvent()
          {
            ActionUTC = eventObject.EventDate,
            ReceivedUTC = eventObject.EventDate,
            AssetUID = new Guid(eventObject.AssetUID),
            DeviceUID = new Guid(eventObject.DeviceUID),
          };
          jsonString = JsonConvert.SerializeObject(new { DissociateDeviceAssetEvent = dissociateDeviceEvent }, jsonSettings);
          break;

        case "CreateCustomerEvent":
          topicName = SetKafkaTopicName("ICustomerEvent");
          var createCustomerEvent = new CreateCustomerEvent()
          {
            ActionUTC = eventObject.EventDate,
            ReceivedUTC = eventObject.EventDate,
            CustomerName = eventObject.CustomerName,
            CustomerType = eventObject.CustomerType,
            CustomerUID = new Guid(eventObject.CustomerUID)
          };
          jsonString = JsonConvert.SerializeObject(new { CreateCustomerEvent = createCustomerEvent }, jsonSettings);
          break;
        case "UpdateCustomerEvent":
          topicName = SetKafkaTopicName("ICustomerEvent");
          var updateCustomerEvent = new UpdateCustomerEvent()
          {
            ActionUTC = eventObject.EventDate,
            ReceivedUTC = eventObject.EventDate,
            CustomerName = eventObject.CustomerName,
            CustomerUID = new Guid(eventObject.CustomerUID)
          };
          jsonString = JsonConvert.SerializeObject(new { UpdateCustomerEvent = updateCustomerEvent }, jsonSettings);
          break;
        case "DeleteCustomerEvent":
          topicName = SetKafkaTopicName("ICustomerEvent");
          var deleteCustomerEvent = new DeleteCustomerEvent()
          {
            ActionUTC = eventObject.EventDate,
            ReceivedUTC = eventObject.EventDate,
            CustomerUID = new Guid(eventObject.CustomerUID)
          };
          jsonString = JsonConvert.SerializeObject(new { DeleteCustomerEvent = deleteCustomerEvent }, jsonSettings);
          break;
        case "AssociateCustomerUserEvent":
          topicName = SetKafkaTopicName("ICustomerEvent");
          var associateCustomerUserEvent = new AssociateCustomerUserEvent()
          {
            ActionUTC = eventObject.EventDate,
            ReceivedUTC = eventObject.EventDate,
            CustomerUID = new Guid(eventObject.CustomerUID),
            UserUID = new Guid(eventObject.UserUID)
          };
          jsonString = JsonConvert.SerializeObject(new { AssociateCustomerUserEvent = associateCustomerUserEvent }, jsonSettings);
          break;
        case "DissociateCustomerUserEvent":
          topicName = SetKafkaTopicName("ICustomerEvent");
          var dissociateCustomerUserEvent = new DissociateCustomerUserEvent()
          {
            ActionUTC = eventObject.EventDate,
            ReceivedUTC = eventObject.EventDate,
            CustomerUID = new Guid(eventObject.CustomerUID),
            UserUID = new Guid(eventObject.UserUID)
          };
          jsonString = JsonConvert.SerializeObject(new { DissociateCustomerUserEvent = dissociateCustomerUserEvent }, jsonSettings);
          break;
        case "CreateProjectSubscriptionEvent":
          topicName = SetKafkaTopicName("ISubscriptionEvent");
          var createProjectSubscriptionEvent = new CreateProjectSubscriptionEvent()
          {
            ActionUTC = eventObject.EventDate,
            ReceivedUTC = eventObject.EventDate,
            StartDate = DateTime.Parse(eventObject.StartDate),
            EndDate = DateTime.Parse(eventObject.EndDate),
            SubscriptionType = eventObject.SubscriptionType,
            SubscriptionUID = new Guid(eventObject.SubscriptionUID)
          };
          jsonString = JsonConvert.SerializeObject(new { CreateProjectSubscriptionEvent = createProjectSubscriptionEvent }, jsonSettings);
          break;
        case "UpdateProjectSubscriptionEvent":
          topicName = SetKafkaTopicName("ISubscriptionEvent");
          var updateProjectSubscriptionEvent = new UpdateProjectSubscriptionEvent()
          {
            ActionUTC = eventObject.EventDate,
            ReceivedUTC = eventObject.EventDate,
            StartDate = DateTime.Parse(eventObject.StartDate),
            EndDate = DateTime.Parse(eventObject.EndDate),
            SubscriptionType = eventObject.SubscriptionType,
            SubscriptionUID = new Guid(eventObject.SubscriptionUID)
          };
          jsonString = JsonConvert.SerializeObject(new { UpdateProjectSubscriptionEvent = updateProjectSubscriptionEvent }, jsonSettings);
          break;
        case "AssociateProjectSubscriptionEvent":
          topicName = SetKafkaTopicName("ISubscriptionEvent");
          var associateProjectSubscriptionEvent = new AssociateProjectSubscriptionEvent()
          {
            ActionUTC = eventObject.EventDate,
            ReceivedUTC = eventObject.EventDate,
            EffectiveDate = DateTime.Parse(eventObject.EffectiveDate, CultureInfo.InvariantCulture),
            ProjectUID = new Guid(eventObject.ProjectUID),
            SubscriptionUID = new Guid(eventObject.SubscriptionUID)
          };
          jsonString = JsonConvert.SerializeObject(new { AssociateProjectSubscriptionEvent = associateProjectSubscriptionEvent }, jsonSettings);
          break;
        case "CreateProjectEvent":
          topicName = SetKafkaTopicName("IProjectEvent");
          var createProjectEvent = new CreateProjectEvent()
          {
            ActionUTC = eventObject.EventDate,
            ReceivedUTC = eventObject.EventDate,
            ProjectEndDate = DateTime.Parse(eventObject.ProjectEndDate),
            ProjectID = Int32.Parse(eventObject.ProjectID),
            ProjectName = eventObject.ProjectName,
            ProjectStartDate = DateTime.Parse(eventObject.ProjectStartDate),
            ProjectTimezone = eventObject.ProjectTimezone,
            ProjectType = (ProjectType)Enum.Parse(typeof(ProjectType), eventObject.ProjectType),
            ProjectUID = new Guid(eventObject.ProjectUID),
            ProjectBoundary = eventObject.GeometryWKT
          };
          jsonString = JsonConvert.SerializeObject(new { CreateProjectEvent = createProjectEvent }, jsonSettings);
          break;
        case "UpdateProjectEvent":
          topicName = SetKafkaTopicName("IProjectEvent");
          var updateProjectEvent = new UpdateProjectEvent()
          {
            ActionUTC = eventObject.EventDate,
            ReceivedUTC = eventObject.EventDate,
            ProjectEndDate = DateTime.Parse(eventObject.ProjectEndDate),
            ProjectName = eventObject.ProjectName,
            ProjectTimezone = eventObject.ProjectTimezone,
            ProjectType = (ProjectType)Enum.Parse(typeof(ProjectType), eventObject.ProjectType),
            ProjectUID = new Guid(eventObject.ProjectUID)
          };
          jsonString = JsonConvert.SerializeObject(new { UpdateProjectEvent = updateProjectEvent }, jsonSettings);
          break;
        case "DeleteProjectEvent":
          topicName = SetKafkaTopicName("IProjectEvent");
          var deleteProjectEvent = new DeleteProjectEvent()
          {
            ActionUTC = eventObject.EventDate,
            ReceivedUTC = eventObject.EventDate,
            ProjectUID = new Guid(eventObject.ProjectUID)
          };
          jsonString = JsonConvert.SerializeObject(new { DeleteProjectEvent = deleteProjectEvent }, jsonSettings);
          break;
        case "AssociateProjectCustomer":
          topicName = SetKafkaTopicName("IProjectEvent");
          var associateCustomerProject = new AssociateProjectCustomer()
          {
            ActionUTC = eventObject.EventDate,
            ReceivedUTC = eventObject.EventDate,
            ProjectUID = new Guid(eventObject.ProjectUID),
            CustomerUID = new Guid(eventObject.CustomerUID)
          };
          jsonString = JsonConvert.SerializeObject(new { AssociateProjectCustomer = associateCustomerProject }, jsonSettings);
          break;
        case "AssociateProjectGeofence":
          topicName = SetKafkaTopicName("IProjectEvent");
          var associateProjectGeofence = new AssociateProjectGeofence()
          {
            ActionUTC = eventObject.EventDate,
            ReceivedUTC = eventObject.EventDate,
            ProjectUID = new Guid(eventObject.ProjectUID),
            GeofenceUID = new Guid(eventObject.GeofenceUID)
          };
          jsonString = JsonConvert.SerializeObject(new { AssociateProjectGeofence = associateProjectGeofence }, jsonSettings);
          break;
        case "CreateGeofenceEvent":
          topicName = SetKafkaTopicName("IGeofenceEvent");
          var createGeofenceEvent = new CreateGeofenceEvent()
          {
            ActionUTC = eventObject.EventDate,
            ReceivedUTC = eventObject.EventDate,
            GeofenceUID = new Guid(eventObject.GeofenceUID),
            CustomerUID = new Guid(eventObject.CustomerUID),
            Description = eventObject.Description,
            FillColor = Int32.Parse(eventObject.FillColor),
            GeofenceName = eventObject.GeofenceName,
            GeofenceType = eventObject.GeofenceType,
            GeometryWKT = eventObject.GeometryWKT,
            IsTransparent = Boolean.Parse(eventObject.IsTransparent),
            UserUID = new Guid(eventObject.UserUID),
            AreaSqMeters = double.Parse(eventObject.AreaSqMeters)
          };
          jsonString = JsonConvert.SerializeObject(new { CreateGeofenceEvent = createGeofenceEvent }, jsonSettings);
          break;
        case "UpdateProjectSettingsEvent":
          topicName = SetKafkaTopicName("IProjectEvent");
          var updateProjectSettingsEvent = new UpdateProjectSettingsEvent()
          {
            ActionUTC = eventObject.EventDate,
            ReceivedUTC = eventObject.EventDate,
            Settings = eventObject.Settings,
            ProjectSettingsType = (ProjectSettingsType)Enum.Parse(typeof(ProjectSettingsType), eventObject.ProjectSettingsType),
            UserID = eventObject.UserID,
            ProjectUID = new Guid(eventObject.ProjectUID)
          };
          jsonString = JsonConvert.SerializeObject(new { UpdateProjectSettingsEvent = updateProjectSettingsEvent }, jsonSettings);
          break;
        case "CreateImportedFileEvent":
          topicName = SetKafkaTopicName("IProjectEvent");
          var createImportedFileEvent = new CreateImportedFileEvent()
          {
            ActionUTC = eventObject.EventDate,
            ReceivedUTC = eventObject.EventDate,
            ProjectUID = new Guid(eventObject.ProjectUID),
            ImportedFileUID = new Guid(eventObject.ImportedFileUID),
            ImportedFileID = long.Parse(eventObject.ImportedFileID),
            CustomerUID = new Guid(eventObject.CustomerUID),
            ImportedFileType = (ImportedFileType)Enum.Parse(typeof(ImportedFileType), eventObject.ImportedFileType),
            Name = eventObject.Name,
            FileDescriptor = eventObject.FileDescriptor,
            FileCreatedUtc = DateTime.Parse(eventObject.FileCreatedUTC),
            FileUpdatedUtc = DateTime.Parse(eventObject.FileUpdatedUTC),
            ImportedBy = eventObject.ImportedBy,
            SurveyedUTC = DateTime.Parse(eventObject.SurveyedUTC),
          };
          jsonString = JsonConvert.SerializeObject(new { CreateImportedFileEvent = createImportedFileEvent }, jsonSettings);
          break;
        case "CreateFilterEvent":
          topicName = SetKafkaTopicName("IFilterEvent");
          var createFilterEvent = new CreateFilterEvent()
          {
            ActionUTC = eventObject.EventDate,
            ReceivedUTC = eventObject.EventDate,
            ProjectUID = new Guid(eventObject.ProjectUID),
            CustomerUID = new Guid(eventObject.CustomerUID),
            UserID = eventObject.UserID,
            FilterUID = new Guid(eventObject.FilterUID),
            Name = eventObject.Name,
            FilterJson = eventObject.FilterJson,
            FilterType = (FilterType)Enum.Parse(typeof(FilterType), eventObject.FilterType),
          };
          jsonString = JsonConvert.SerializeObject(new { CreateFilterEvent = createFilterEvent }, jsonSettings);
          break;
      }
      #endregion
      kafkaDriver.SendKafkaMessage(topicName, jsonString);
    }

    /// <summary>
    /// Inserts the events into the database
    /// </summary>
    /// <param name="eventObject"></param>
    private void BuildMySqlInsertStringAndWriteToDatabase(dynamic eventObject)
    {
      string dbTable = eventObject.TableName;
      var mysqlHelper = new MySqlHelper();
      var sqlCmd = $@"INSERT INTO `{tsCfg.dbSchema}`.{dbTable} ";
      switch (dbTable)
      {
        case "Asset":
          sqlCmd += $@"(AssetUID,LegacyAssetID,Name,MakeCode,SerialNumber,Model,IconKey,AssetType,OwningCustomerUID,LastActionedUTC) VALUES 
                ('{AssetUid}',{eventObject.LegacyAssetID},'{eventObject.Name}','{eventObject.MakeCode}','{eventObject.SerialNumber}','{eventObject.Model}',
                {eventObject.IconKey},'{eventObject.AssetType}','{eventObject.OwningCustomerUID}','{eventObject.EventDate:yyyy-MM-dd HH\:mm\:ss.fffffff}');";
          break;
        case "AssetDevice":
          sqlCmd += $@"(fk_DeviceUID,fk_AssetUID,LastActionedUTC) VALUES 
                ('{eventObject.fk_DeviceUID}','{eventObject.fk_AssetUID}','{eventObject.EventDate:yyyy-MM-dd HH\:mm\:ss.fffffff}');";
          break;
        case "AssetSubscription":
          sqlCmd += $@"(fk_AssetUID,fk_SubscriptionUID,EffectiveDate,LastActionedUTC) VALUES
                     ('{eventObject.fk_AssetUID}','{eventObject.fk_SubscriptionUID}','{eventObject.EffectiveDate}','{eventObject.EventDate:yyyy-MM-dd HH\:mm\:ss.fffffff}');";
          break;
        case "Customer":
          sqlCmd += $@"(CustomerUID,Name,fk_CustomerTypeID,IsDeleted,LastActionedUTC) VALUES
                     ('{eventObject.CustomerUID}','{eventObject.Name}',{eventObject.fk_CustomerTypeID},0,'{eventObject.EventDate:yyyy-MM-dd HH\:mm\:ss.fffffff}');";
          break;
        case "CustomerProject":
          sqlCmd += $@"(fk_CustomerUID,fk_ProjectUID,LastActionedUTC) VALUES
                     ('{eventObject.fk_CustomerUID}','{eventObject.fk_ProjectUID}','{eventObject.EventDate:yyyy-MM-dd HH\:mm\:ss.fffffff}');";
          break;
        case "Device":
          sqlCmd += $@"(DeviceUID,DeviceSerialNumber,DeviceType,DeviceState,DataLinkType,LastActionedUTC) VALUES 
                ('{eventObject.DeviceUID}','{eventObject.DeviceSerialNumber}','{eventObject.DeviceType}','{eventObject.DeviceState}','{eventObject.DataLinkType}','{eventObject.EventDate:yyyy-MM-dd HH\:mm\:ss.fffffff}');";
          break;
        case "Geofence":
          sqlCmd += $@"(GeofenceUID,Name,fk_GeofenceTypeID,GeometryWKT,FillColor,IsTransparent,IsDeleted,Description,AreaSqMeters,fk_CustomerUID,UserUID,LastActionedUTC) VALUES
                     ('{eventObject.GeofenceUID}','{eventObject.Name}',{eventObject.fk_GeofenceTypeID},'{eventObject.GeometryWKT}',
                       {eventObject.FillColor},{eventObject.IsTransparent},{eventObject.IsDeleted},'{eventObject.Description}','{eventObject.AreaSqMeters}',
                      '{eventObject.fk_CustomerUID}',{eventObject.UserUID},{eventObject.LastActionedUTC:yyyy-MM-dd HH\:mm\:ss.fffffff}');";
          break;
        case "Project":
          sqlCmd += $@"(ProjectUID,LegacyProjectID,Name,fk_ProjectTypeID,ProjectTimeZone,LandfillTimeZone,StartDate,EndDate,GeometryWKT,LastActionedUTC) VALUES
                     ('{eventObject.ProjectUID}',{eventObject.LegacyProjectID},'{eventObject.Name}',{eventObject.fk_ProjectTypeID},
                      '{eventObject.ProjectTimeZone}','{eventObject.LandfillTimeZone}','{eventObject.StartDate:yyyy-MM-dd}','{eventObject.EndDate:yyyy-MM-dd}','{eventObject.GeometryWKT}','{eventObject.EventDate:yyyy-MM-dd HH\:mm\:ss.fffffff}');";
          break;
        case "ProjectGeofence":
          sqlCmd += $@"(fk_ProjectUID,fk_GeofenceUID,LastActionedUTC) VALUES
                     ('{eventObject.fk_ProjectUID}','{eventObject.fk_GeofenceUID}','{eventObject.LastActionedUTC:yyyy-MM-dd HH\:mm\:ss.fffffff}');";
          break;
        case "ProjectSubscription":
          sqlCmd += $@"(fk_SubscriptionUID,fk_ProjectUID,EffectiveDate,LastActionedUTC) VALUES
                     ('{eventObject.fk_SubscriptionUID}','{eventObject.fk_ProjectUID}','{eventObject.EffectiveDate:yyyy-MM-dd}','{eventObject.LastActionedUTC:yyyy-MM-dd HH\:mm\:ss.fffffff}');";
          break;
        case "Subscription":
          sqlCmd += $@"(SubscriptionUID,fk_CustomerUID,fk_ServiceTypeID,StartDate,EndDate,LastActionedUTC) VALUES
                     ('{eventObject.SubscriptionUID}','{eventObject.fk_CustomerUID}','{eventObject.fk_ServiceTypeID}','{eventObject.StartDate}','{eventObject.EndDate}','{eventObject.EventDate:yyyy-MM-dd HH\:mm\:ss.fffffff}');";
          break;
      }
      mysqlHelper.ExecuteMySqlInsert(tsCfg.DbConnectionString, sqlCmd);
    }

    /// <summary>
    /// This is used to set multiple asset events in the database
    /// </summary>
    /// <param name="isSameAsset">True if it's the same asset id we are dealing with</param>
    private void IsNotSameAsset(bool isSameAsset)
    {
      if (isSameAsset) return;
      SetAssetUid();
      SetFirstEventDate();
    }

    /// <summary>
    /// Check that a property exists in the dynamic object
    /// </summary>
    /// <param name="obj">dynamic object</param>
    /// <param name="propertyName">Property name as string</param>
    /// <returns>true or false</returns>
    private static bool HasProperty(dynamic obj, string propertyName)
    {
      return ((IDictionary<string, object>)obj).ContainsKey(propertyName);
    }

    /// <summary>
    /// Create an ExpandoObject of all the fields from the event array
    /// </summary>
    /// <param name="allColumnNames">All the column names from the array</param>
    /// <param name="singleEventRow">A single row of event data</param>
    /// <returns>Object with all properties from array</returns>
    private ExpandoObject ConvertToExpando(string[] allColumnNames, string[] singleEventRow)
    {
      var expObj = new ExpandoObject() as IDictionary<string, Object>;
      var colIdx = -1;
      foreach (var colName in allColumnNames)
      {
        colIdx++;
        if (colName.Trim() == string.Empty)
        { continue; }

        dynamic obj = TransformObject(singleEventRow[colIdx].Trim());
        expObj.Add(colName.Trim(), obj);
      }
      return (ExpandoObject)expObj;
    }

    /// <summary>
    /// For some of the events in the event array they need some transforming 
    /// </summary>
    /// <param name="propertyValue">returns a converted/transformed single property. Mainly used for string null or special dates</param>
    /// <returns></returns>
    private dynamic TransformObject(string propertyValue)
    {
      dynamic obj;
      if (propertyValue == "null" || propertyValue == string.Empty)
      {
        return null;
      }
      if (Regex.IsMatch(propertyValue, @"^\s*\d+d\+\d+"))
      {
        obj = ConvertTimeStampAndDayOffSetToDateTime(propertyValue, FirstEventDate);
        return obj;
      }
      obj = propertyValue;
      return obj;
    }

    /// <summary>
    /// Generate a random number. This is use for the number of days in the past to get a start date from.
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    private int RandomNumber(int min, int max)
    {
      lock (syncLock)
      {
        return rndNumber.Next(min, max);
      }
    }

    /// <summary>
    /// Converts a special date string eg 2d+12:00:00 which signifies a two date and 12 hour offset
    /// to a normal date time based on the first event date.
    /// </summary>
    /// <param name="timeStampAndDayOffSet">Date day off set and timestamp from first event date</param>
    /// <param name="startEventDateTime"></param>
    /// <returns>Datetime</returns>
    public DateTime ConvertTimeStampAndDayOffSetToDateTime(string timeStampAndDayOffSet, DateTime startEventDateTime)
    {
      var components = Regex.Split(timeStampAndDayOffSet, @"d+\+");
      var offset = double.Parse(components[0].Trim());
      return DateTime.Parse(startEventDateTime.AddDays(offset).ToString("yyyy-MM-dd") + " " + components[1].Trim());
    }
    #endregion
  }
}
