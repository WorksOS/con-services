using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer;
using VSS.KafkaConsumer.Interfaces;
using VSS.KafkaConsumer.Kafka;
using VSS.Log4Net.Extensions;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;


/****
 * upgrading to confluent 0.11 results in delay when opening subsequent consumers on the same group:
 *            'preparing to restabilize group .... with old generation....
 *            
 * Tried:
 *     defining KafkaNames in appsettings rather than creating each test
 *     Disposing or not, the consumers. Ditto with producer. 
 *          Seems that in local debug you MUST dispose the providers else get an exception to do with libre on rubbish collection on exiting the test.
 *     using kafka 0.10.2.1 rather than latest 0.11.0
 *     make wait longer (seems to always require 7 consumer hits??? batching?)
 *     
 *     whether you have the producer/consumer local to test or global made no difference. This should be an issue I would have thought as not disposed between usage types.          
 *     creating a KafkaComponentTests IAbstractKafkaConsumer _consumer and assigning each new topic (was internal to test)
 *     creating a KafkaComponentTests IKafka producer and writing to each new topic (was internal to test)
 *     
 * Going back to "VSS.KafkaConsumer.netcore" Version="2.3.*" and "RdKafka" Version="0.9.2-ci-170" in KafkaTests and TestRun 
 *     reverts back to good behaviour of KafkaTests. Note that leaving EventTests and TestUtil as Confluent didn't seem to hurt anything.
 *     
 */

namespace KafkaTests
{
  [TestClass]
  public class KafkaComponentTests
  {
    IServiceProvider _serviceProvider = null;
    private int _consumerWaitMs = 1000;
    private ILogger _log;
    private IConfigurationStore _configurationStore;

    [TestInitialize]
    public void InitTest()
    {
      // setup Ilogger
      string loggerRepoName = "UnitTestLogTest";
      var logPath = System.IO.Directory.GetCurrentDirectory();
      Log4NetAspExtensions.ConfigureLog4Net(logPath, "log4nettest.xml", loggerRepoName);

      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(loggerRepoName);

      _serviceProvider = new ServiceCollection()
        .AddTransient<IKafka, RdKafkaDriver>()
        .AddTransient<IKafkaConsumer<IAssetEvent>, KafkaConsumer<IAssetEvent>>()
        .AddTransient<IKafkaConsumer<ICustomerEvent>, KafkaConsumer<ICustomerEvent>>()
        .AddTransient<IKafkaConsumer<IDeviceEvent>, KafkaConsumer<IDeviceEvent>>()
        .AddTransient<IKafkaConsumer<IGeofenceEvent>, KafkaConsumer<IGeofenceEvent>>()
        .AddTransient<IKafkaConsumer<IProjectEvent>, KafkaConsumer<IProjectEvent>>()
        .AddTransient<IKafkaConsumer<ISubscriptionEvent>, KafkaConsumer<ISubscriptionEvent>>()
        .AddTransient<IKafkaConsumer<IFilterEvent>, KafkaConsumer<IFilterEvent>>()
        .AddTransient<IMessageTypeResolver, MessageResolver>()
        .AddTransient<IRepositoryFactory, RepositoryFactory>()

        .AddTransient<IRepository<IAssetEvent>, AssetRepository>()
        .AddTransient<IRepository<ICustomerEvent>, CustomerRepository>()
        .AddTransient<IRepository<IDeviceEvent>, DeviceRepository>()
        .AddTransient<IRepository<IGeofenceEvent>, GeofenceRepository>()
        .AddTransient<IRepository<IProjectEvent>, ProjectRepository>()
        .AddTransient<IRepository<ISubscriptionEvent>, SubscriptionRepository>()
        .AddTransient<IRepository<IFilterEvent>, FilterRepository>()
        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        .AddLogging()
        .AddSingleton<ILoggerFactory>(loggerFactory)
        .BuildServiceProvider();

      _log = _serviceProvider.GetService<ILoggerFactory>().CreateLogger("KafkaComponentTests");
      _configurationStore = _serviceProvider.GetService<IConfigurationStore>();
    }

    /// <summary>
    /// This test attempted to:
    ///    write to the kafka que 
    ///    have the consumer to write to the DB
    ///    then we check that the object is in the database
    /// This works (i.e. not a timing issue) because of how the kafka que is configured i.e.
    ///    Generate New que
    ///    read earliest 
    /// </summary>
    [TestMethod]
    [Ignore]
    public void AssetConsumerWritesToDb()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var createAssetEvent = new CreateAssetEvent
      {
        AssetUID = Guid.NewGuid(),
        AssetName = "The Asset Name",
        AssetType = "whatever",
        ActionUTC = actionUtc
      };

      var baseTopic = "VSS.Interfaces.Events.MasterData.IAssetEvent" + Guid.NewGuid();
      var topicName = baseTopic + _configurationStore.GetValueString("KAFKA_TOPIC_NAME_SUFFIX");
      _log.LogDebug($"BaseTopic: {baseTopic} topicName: {topicName}");

      string messagePayload = JsonConvert.SerializeObject(new {CreateAssetEvent = createAssetEvent});

      var producer = _serviceProvider.GetService<IKafka>();
      producer.InitProducer(_configurationStore);
      producer.Send(topicName,
        new List<KeyValuePair<string, string>>()
        {
          new KeyValuePair<string, string>(createAssetEvent.AssetUID.ToString(), messagePayload)
        });
      Thread.Sleep(_consumerWaitMs);

      var consumer = _serviceProvider.GetService<IKafkaConsumer<IAssetEvent>>();
      consumer.SetTopic(baseTopic);

      var assetContext = new AssetRepository(_configurationStore, _serviceProvider.GetService<ILoggerFactory>());
      Task<Asset> dbReturn = null;
      Thread.Sleep(_consumerWaitMs);

      for (int i = 0; i < 10; i++)
      {
        consumer.StartProcessingSync();
        Thread.Sleep(_consumerWaitMs);

        dbReturn = assetContext.GetAsset(createAssetEvent.AssetUID.ToString());
        dbReturn.Wait();
        if (dbReturn.Result != null)
          break;
      }

      Assert.IsNotNull(dbReturn, "Invalid result from AssetRepo");
      Assert.IsNotNull(dbReturn?.Result, "Unable to retrieve Asset from AssetRepo");
      Assert.AreEqual(createAssetEvent.AssetUID.ToString(), dbReturn?.Result?.AssetUID,
        "Asset details are incorrect from AssetRepo");
    }

    [TestMethod]
    [Ignore]
    public void CustomerConsumerWritesToDb()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var createCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = actionUtc
      };

      var baseTopic = "VSS.Interfaces.Events.MasterData.ICustomerEvent" + Guid.NewGuid();
      var topicName = baseTopic + _configurationStore.GetValueString("KAFKA_TOPIC_NAME_SUFFIX");
      _log.LogDebug($"BaseTopic: {baseTopic} topicName: {topicName}");

      string messagePayload = JsonConvert.SerializeObject(new {CreateCustomerEvent = createCustomerEvent});

      var producer = _serviceProvider.GetService<IKafka>();
      producer.InitProducer(_configurationStore);
      producer.Send(topicName,
        new List<KeyValuePair<string, string>>()
        {
          new KeyValuePair<string, string>(createCustomerEvent.CustomerUID.ToString(), messagePayload)
        });
      Thread.Sleep(_consumerWaitMs);

      var consumer = _serviceProvider.GetService<IKafkaConsumer<ICustomerEvent>>();
      consumer.SetTopic(baseTopic);

      var customerContext = new CustomerRepository(_configurationStore, _serviceProvider.GetService<ILoggerFactory>());
      Task<Customer> dbReturn = null;
      Thread.Sleep(_consumerWaitMs);

      for (int i = 0; i < 10; i++)
      {
        consumer.StartProcessingSync();
        Thread.Sleep(_consumerWaitMs);

        dbReturn = customerContext.GetCustomer(createCustomerEvent.CustomerUID);
        dbReturn.Wait();
        if (dbReturn.Result != null)
          break;
      }

      Assert.IsNotNull(dbReturn, "Invalid result from CustomerRepo");
      Assert.IsNotNull(dbReturn?.Result, "Unable to retrieve Customer from CustomerRepo");
      Assert.AreEqual(createCustomerEvent.CustomerUID.ToString(), dbReturn?.Result?.CustomerUID,
        "Customer details are incorrect from CustomerRepo");
    }

    [TestMethod]
    [Ignore]
    public void DeviceConsumerWritesToDb()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var createDeviceEvent = new CreateDeviceEvent
      {
        DeviceUID = Guid.NewGuid(),
        DeviceSerialNumber = "The radio serial",
        DeviceType = "SNM940",
        DeviceState = "active",
        ActionUTC = actionUtc
      };

      var baseTopic = "VSS.Interfaces.Events.MasterData.IDeviceEvent" + Guid.NewGuid();
      var topicName = baseTopic + _configurationStore.GetValueString("KAFKA_TOPIC_NAME_SUFFIX");
      _log.LogDebug($"BaseTopic: {baseTopic} topicName: {topicName}");

      string messagePayload = JsonConvert.SerializeObject(new {CreateDeviceEvent = createDeviceEvent});

      var producer = _serviceProvider.GetService<IKafka>();
      producer.InitProducer(_configurationStore);
      producer.Send(topicName,
        new List<KeyValuePair<string, string>>()
        {
          new KeyValuePair<string, string>(createDeviceEvent.DeviceUID.ToString(), messagePayload)
        });
      Thread.Sleep(_consumerWaitMs);

      var consumer = _serviceProvider.GetService<IKafkaConsumer<IDeviceEvent>>();
      consumer.SetTopic(baseTopic);

      var deviceContext = new DeviceRepository(_configurationStore, _serviceProvider.GetService<ILoggerFactory>());
      Task<Device> dbReturn = null;
      Thread.Sleep(_consumerWaitMs);

      for (int i = 0; i < 10; i++)
      {
        consumer.StartProcessingSync();
        Thread.Sleep(_consumerWaitMs);

        dbReturn = deviceContext.GetDevice(createDeviceEvent.DeviceUID.ToString());
        dbReturn.Wait();
        if (dbReturn.Result != null)
          break;
      }

      Assert.IsNotNull(dbReturn, "Invalid result from DeviceRepo");
      Assert.IsNotNull(dbReturn?.Result, "Unable to retrieve Device from DeviceRepo");
      Assert.AreEqual(createDeviceEvent.DeviceUID.ToString(), dbReturn?.Result?.DeviceUID,
        "Device details are incorrect from DeviceRepo");
    }

    [TestMethod]
    [Ignore]
    public void ProjectConsumerWritesToDb()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var projectTimeZone = "New Zealand Standard Time";
      var createProjectEvent = new CreateProjectEvent()
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = new Random().Next(1, 1999999),
        ProjectName = "The Project Name",
        ProjectType = ProjectType.LandFill,
        ProjectTimezone = projectTimeZone,

        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ProjectBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))",
        ActionUTC = actionUtc
      };

      var baseTopic = "VSS.Interfaces.Events.MasterData.CreateProjectEvent" + Guid.NewGuid();
      var topicName = baseTopic + _configurationStore.GetValueString("KAFKA_TOPIC_NAME_SUFFIX");
      _log.LogDebug($"BaseTopic: {baseTopic} topicName: {topicName}");

      string messagePayload = JsonConvert.SerializeObject(new {CreateProjectEvent = createProjectEvent});

      var producer = _serviceProvider.GetService<IKafka>();
      producer.InitProducer(_configurationStore);
      producer.Send(topicName,
        new List<KeyValuePair<string, string>>()
        {
          new KeyValuePair<string, string>(createProjectEvent.ProjectUID.ToString(), messagePayload)
        });
      Thread.Sleep(_consumerWaitMs);

      var consumer = _serviceProvider.GetService<IKafkaConsumer<IProjectEvent>>();
      consumer.SetTopic(baseTopic);

      var projectContext = new ProjectRepository(_configurationStore, _serviceProvider.GetService<ILoggerFactory>());
      Task<Project> dbReturn = null;
      Thread.Sleep(_consumerWaitMs);

      for (int i = 0; i < 10; i++)
      {
        consumer.StartProcessingSync();
        Thread.Sleep(_consumerWaitMs);

        dbReturn = projectContext.GetProject_UnitTest(createProjectEvent.ProjectUID.ToString());
        dbReturn.Wait();
        if (dbReturn.Result != null)
          break;
      }

      Assert.IsNotNull(dbReturn, "Invalid result from ProjectRepo");
      Assert.IsNotNull(dbReturn?.Result, "Unable to retrieve Project from ProjectRepo");
      Assert.AreEqual(createProjectEvent.ProjectUID.ToString(), dbReturn?.Result?.ProjectUID,
        "Project details are incorrect from ProjectRepo");
    }

    [TestMethod]
    [Ignore]
    public void ProjectConsumerWritesToDb_CreateProjectSettings()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var projectUid = Guid.NewGuid();
      string settings = @"<ProjectSettings>  
        < CompactionSettings >
        < OverrideTargetCMV > false </ OverrideTargetCMV >
        < OverrideTargetCMVValue > 50 </ OverrideTargetCMVValue >
        < MinTargetCMVPercent > 80 </ MinTargetCMVPercent >
        < MaxTargetCMVPercent > 130 </ MaxTargetCMVPercent >
        < OverrideTargetPassCount > false </ OverrideTargetPassCount >
        < OverrideTargetPassCountValue > 5 </ OverrideTargetPassCountValue >
        < OverrideTargetLiftThickness > false </ OverrideTargetLiftThickness >
        < OverrideTargetLiftThicknessMeters > 0.5 </ OverrideTargetLiftThicknessMeters >
        < CompactedLiftThickness > true </ CompactedLiftThickness >
        < ShowCCVSummaryTopLayerOnly > true </ ShowCCVSummaryTopLayerOnly >
        < FirstPassThickness > 0 </ FirstPassThickness >
        < OverrideTemperatureRange > false </ OverrideTemperatureRange >
        < MinTemperatureRange > 65 </ MinTemperatureRange >
        < MaxTemperatureRange > 175 </ MaxTemperatureRange >
        < OverrideTargetMDP > false </ OverrideTargetMDP >
        < OverrideTargetMDPValue > 50 </ OverrideTargetMDPValue >
        < MinTargetMDPPercent > 80 </ MinTargetMDPPercent >
        < MaxTargetMDPPercent > 130 </ MaxTargetMDPPercent >
        < ShowMDPSummaryTopLayerOnly > true </ ShowMDPSummaryTopLayerOnly >
        </ CompactionSettings >
        < VolumeSettings >
        < ApplyShrinkageAndBulking > false </ ApplyShrinkageAndBulking >
        < PercentShrinkage > 0 </ PercentShrinkage >
        < PercentBulking > 0 </ PercentBulking >
        < NoChangeTolerance > 0.02 </ NoChangeTolerance >
        </ VolumeSettings >
        < ExpiryPromptDismissed > false </ ExpiryPromptDismissed >
        </ ProjectSettings > ";

      var updateProjectSettingsEvent = new UpdateProjectSettingsEvent()
      {
        ProjectUID = projectUid,
        Settings = settings,
        ActionUTC = actionUtc
      };

      var baseTopic = "VSS.Interfaces.Events.MasterData.UpdateProjectSettingsEvent" + Guid.NewGuid();
      var topicName = baseTopic + _configurationStore.GetValueString("KAFKA_TOPIC_NAME_SUFFIX");
      _log.LogDebug($"BaseTopic: {baseTopic} topicName: {topicName}");

      string messagePayload =
        JsonConvert.SerializeObject(new {UpdateProjectSettingsEvent = updateProjectSettingsEvent});

      var producer = _serviceProvider.GetService<IKafka>();
      producer.InitProducer(_configurationStore);
      producer.Send(topicName,
        new List<KeyValuePair<string, string>>()
        {
          new KeyValuePair<string, string>(updateProjectSettingsEvent.ProjectUID.ToString(), messagePayload)
        });
      Thread.Sleep(_consumerWaitMs);

      var consumer = _serviceProvider.GetService<IKafkaConsumer<IProjectEvent>>();
      consumer.SetTopic(baseTopic);

      var projectContext = new ProjectRepository(_configurationStore, _serviceProvider.GetService<ILoggerFactory>());
      Task<ProjectSettings> dbReturn = null;
      Thread.Sleep(_consumerWaitMs);

      for (int i = 0; i < 10; i++)
      {
        consumer.StartProcessingSync();
        Thread.Sleep(_consumerWaitMs);

        dbReturn = projectContext.GetProjectSettings(updateProjectSettingsEvent.ProjectUID.ToString());
        dbReturn.Wait();
        if (dbReturn.Result != null)
          break;
      }

      Assert.IsNotNull(dbReturn, "Invalid result from ProjectRepo");
      Assert.IsNotNull(dbReturn?.Result, "Unable to retrieve ProjectSettings from ProjectRepo");
      Assert.AreEqual(updateProjectSettingsEvent.ProjectUID.ToString(), dbReturn?.Result?.ProjectUid,
        "ProjectSettings are incorrect from ProjectRepo");
    }

    [TestMethod]
    public void ProjectConsumerWritesToDb_CreateImportedFile()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var createImportedFileEvent = new CreateImportedFileEvent()
      {
        ProjectUID = Guid.NewGuid(),
        ImportedFileUID = Guid.NewGuid(),
        ImportedFileID = new Random().Next(1, 1999999),
        CustomerUID = Guid.NewGuid(),
        ImportedFileType = ImportedFileType.SurveyedSurface,
        Name = "Test SS type.ttm",
        FileDescriptor = "fd",
        FileCreatedUtc = actionUtc,
        FileUpdatedUtc = actionUtc,
        ImportedBy = "JoeSmoe",
        SurveyedUTC = actionUtc.AddDays(-1),
        ActionUTC = actionUtc
      };

      var baseTopic = "VSS.Interfaces.Events.MasterData.CreateImportedFileEvent" + Guid.NewGuid();
      var topicName = baseTopic + _configurationStore.GetValueString("KAFKA_TOPIC_NAME_SUFFIX");
      _log.LogDebug($"BaseTopic: {baseTopic} topicName: {topicName}");

      string messagePayload = JsonConvert.SerializeObject(new {CreateImportedFileEvent = createImportedFileEvent});

      var producer = _serviceProvider.GetService<IKafka>();
      producer.InitProducer(_configurationStore);
      producer.Send(topicName,
        new List<KeyValuePair<string, string>>()
        {
          new KeyValuePair<string, string>(createImportedFileEvent.ProjectUID.ToString(), messagePayload)
        });
      Thread.Sleep(_consumerWaitMs);

      var consumer = _serviceProvider.GetService<IKafkaConsumer<IProjectEvent>>();
      consumer.SetTopic(baseTopic);

      var projectContext = new ProjectRepository(_configurationStore, _serviceProvider.GetService<ILoggerFactory>());
      Task<ImportedFile> dbReturn = null;
      Thread.Sleep(_consumerWaitMs);

      for (int i = 0; i < 10; i++)
      {
        consumer.StartProcessingSync();
        Thread.Sleep(_consumerWaitMs);

        dbReturn = projectContext.GetImportedFile(createImportedFileEvent.ImportedFileUID.ToString());
        dbReturn.Wait();
        if (dbReturn.Result != null)
          break;
      }

      Assert.IsNotNull(dbReturn, "Invalid result from ProjectRepo");
      Assert.IsNotNull(dbReturn?.Result, "Unable to retrieve importedFile from ProjectRepo");
      Assert.AreEqual(createImportedFileEvent.ProjectUID.ToString(), dbReturn?.Result?.ProjectUid,
        "ProjectUID from importedFile is incorrect from ProjectRepo");
      Assert.AreEqual(createImportedFileEvent.ImportedFileUID.ToString(), dbReturn?.Result?.ImportedFileUid,
        "importedFile is incorrect from ProjectRepo");
    }

    [TestMethod]
    [Ignore]
    public void SubscriptionConsumerWritesToDb()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var customerUid = Guid.NewGuid();

      var createProjectSubscriptionEvent = new CreateProjectSubscriptionEvent()
      {
        CustomerUID = customerUid,
        SubscriptionUID = Guid.NewGuid(),
        SubscriptionType = "Project Monitoring",
        StartDate = new DateTime(2016, 02, 01),
        EndDate = new DateTime(9999, 12, 31),
        ActionUTC = actionUtc
      };

      var baseTopic = "VSS.Interfaces.Events.MasterData.CreateProjectSubscriptionEvent" + Guid.NewGuid();
      var topicName = baseTopic + _configurationStore.GetValueString("KAFKA_TOPIC_NAME_SUFFIX");
      _log.LogDebug($"BaseTopic: {baseTopic} topicName: {topicName}");

      string messagePayload =
        JsonConvert.SerializeObject(new {CreateProjectSubscriptionEvent = createProjectSubscriptionEvent});

      var producer = _serviceProvider.GetService<IKafka>();
      producer.InitProducer(_configurationStore);
      producer.Send(topicName,
        new List<KeyValuePair<string, string>>()
        {
          new KeyValuePair<string, string>(createProjectSubscriptionEvent.SubscriptionUID.ToString(), messagePayload)
        });
      Thread.Sleep(_consumerWaitMs);

      var consumer = _serviceProvider.GetService<IKafkaConsumer<ISubscriptionEvent>>();
      consumer.SetTopic(baseTopic);

      var subscriptionContext =
        new SubscriptionRepository(_configurationStore, _serviceProvider.GetService<ILoggerFactory>());
      Task<IEnumerable<Subscription>> dbReturn = null;
      Thread.Sleep(_consumerWaitMs);

      for (int i = 0; i < 10; i++)
      {
        consumer.StartProcessingSync();
        Thread.Sleep(_consumerWaitMs);

        dbReturn =
          subscriptionContext.GetSubscriptions_UnitTest(createProjectSubscriptionEvent.SubscriptionUID.ToString());
        dbReturn.Wait();
        if (dbReturn.Result != null && dbReturn?.Result?.Count() > 0)
          break;
      }

      Assert.IsNotNull(dbReturn, "Invalid result from SubscriptionRepo");
      Assert.IsNotNull(dbReturn?.Result, "Unable to retrieve Subscription from SubscriptionRepo");
      Assert.AreEqual(1, dbReturn?.Result?.Count(), "Wrong Subscription count from SubscriptionRepo");
      var subs = dbReturn?.Result?.ToList();
      Assert.AreEqual(createProjectSubscriptionEvent.SubscriptionUID.ToString(), subs?[0].SubscriptionUID,
        "Subscription details are incorrect from SubscriptionRepo");
    }

    [TestMethod]
    [Ignore]
    public void GeofenceConsumerWritesToDb()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var customerUid = Guid.NewGuid();
      var createGeofenceEvent = new CreateGeofenceEvent()
      {
        GeofenceUID = Guid.NewGuid(),
        GeofenceName = "Test Geofence",
        Description = "Testing 123",
        GeofenceType = GeofenceType.Borrow.ToString(),
        FillColor = 16744448,
        IsTransparent = true,
        GeometryWKT =
          "POLYGON((172.68231141046 -43.6277661929154,172.692096108947 -43.6213045879588,172.701537484681 -43.6285117180247,172.698104257136 -43.6328604301996,172.689349526916 -43.6336058921214,172.682998055965 -43.6303754903428,172.68231141046 -43.6277661929154,172.68231141046 -43.6277661929154))",
        CustomerUID = customerUid,
        UserUID = Guid.NewGuid(),
        ActionUTC = actionUtc
      };

      var baseTopic = "VSS.Interfaces.Events.MasterData.CreateGeofenceEvent" + Guid.NewGuid();
      var topicName = baseTopic + _configurationStore.GetValueString("KAFKA_TOPIC_NAME_SUFFIX");
      _log.LogDebug($"BaseTopic: {baseTopic} topicName: {topicName}");

      string messagePayload = JsonConvert.SerializeObject(new {CreateGeofenceEvent = createGeofenceEvent});

      var producer = _serviceProvider.GetService<IKafka>();
      producer.InitProducer(_configurationStore);
      producer.Send(topicName,
        new List<KeyValuePair<string, string>>()
        {
          new KeyValuePair<string, string>(createGeofenceEvent.GeofenceUID.ToString(), messagePayload)
        });
      Thread.Sleep(_consumerWaitMs);

      var consumer = _serviceProvider.GetService<IKafkaConsumer<IGeofenceEvent>>();
      consumer.SetTopic(baseTopic);

      var geofenceContext = new GeofenceRepository(_configurationStore, _serviceProvider.GetService<ILoggerFactory>());
      Task<Geofence> dbReturn = null;
      Thread.Sleep(_consumerWaitMs);

      for (int i = 0; i < 10; i++)
      {
        consumer.StartProcessingSync();
        Thread.Sleep(_consumerWaitMs);

        dbReturn = geofenceContext.GetGeofence_UnitTest(createGeofenceEvent.GeofenceUID.ToString());
        dbReturn.Wait();
        if (dbReturn.Result != null)
          break;
      }

      Assert.IsNotNull(dbReturn, "Invalid result from GeofenceRepo");
      Assert.IsNotNull(dbReturn?.Result, "Unable to retrieve Geofence from GeofenceRepo");
      Assert.AreEqual(createGeofenceEvent.GeofenceUID.ToString(), dbReturn?.Result?.GeofenceUID,
        "Geofence details are incorrect from GeofenceRepo");
    }

    [TestMethod]
    public void FilterConsumerWritesToDb()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var createFilterEvent = new CreateFilterEvent()
      {
        CustomerUID = Guid.NewGuid(),
        UserID = Guid.NewGuid().ToString(),
        ProjectUID = Guid.NewGuid(),
        FilterUID = Guid.NewGuid(),
        Name = "Persistant filter Name",
        FilterJson = "{\"startUTC\":\"2012-11-05\",\"endUTC\":\"2012-11-06\"}",
        ActionUTC = actionUtc,
        ReceivedUTC = actionUtc
      };

      var baseTopic = "VSS.Interfaces.Events.MasterData.IFilterEvent" + Guid.NewGuid();
      var topicName = baseTopic + _configurationStore.GetValueString("KAFKA_TOPIC_NAME_SUFFIX");
      _log.LogDebug($"BaseTopic: {baseTopic} topicName: {topicName}");

      string messagePayload = JsonConvert.SerializeObject(new {CreateFilterEvent = createFilterEvent});

      var producer = _serviceProvider.GetService<IKafka>();
      producer.InitProducer(_configurationStore);
      producer.Send(topicName,
        new List<KeyValuePair<string, string>>()
        {
          new KeyValuePair<string, string>(createFilterEvent.FilterUID.ToString(), messagePayload)
        });
      Thread.Sleep(_consumerWaitMs);

      var consumer = _serviceProvider.GetService<IKafkaConsumer<IFilterEvent>>();
      consumer.SetTopic(baseTopic);

      var filterContext = new FilterRepository(_configurationStore, _serviceProvider.GetService<ILoggerFactory>());
      Task<Filter> dbReturn = null;
      Thread.Sleep(_consumerWaitMs);

      for (int i = 0; i < 10; i++)
      {
        consumer.StartProcessingSync();
        Thread.Sleep(_consumerWaitMs);

        dbReturn = filterContext.GetFilter(createFilterEvent.FilterUID.ToString());
        dbReturn.Wait();
        if (dbReturn.Result != null)
          break;
      }

      Assert.IsNotNull(dbReturn, "Invalid result from FilterRepo");
      Assert.IsNotNull(dbReturn?.Result, "Unable to retrieve Filter from FilterRepo");
      Assert.AreEqual(createFilterEvent.FilterUID.ToString(), dbReturn?.Result?.FilterUid,
        "Filter details are incorrect from FilterRepo");
    }
  }
}
