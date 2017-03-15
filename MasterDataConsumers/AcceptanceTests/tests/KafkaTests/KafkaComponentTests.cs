using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using System.Threading;
using System.Linq;
using KafkaConsumer;
using Microsoft.Extensions.Logging;
using log4netExtensions;
using VSS.GenericConfiguration;
using Repositories;
using Repositories.DBModels;
using KafkaConsumer.Kafka;
using KafkaConsumer.Interfaces;

namespace KafkaTests
{
  [TestClass]
  public class KafkaComponentTests
  {
    IServiceProvider serviceProvider = null;    

    [TestInitialize]
    public void InitTest()
    {
      // setup Ilogger
      string loggerRepoName = "UnitTestLogTest";
      var logPath = System.IO.Directory.GetCurrentDirectory();
      //var builder = new ConfigurationBuilder()
      //          .SetBasePath(logPath)
      //          .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
      //var Configuration = builder.Build();
      Log4NetAspExtensions.ConfigureLog4Net(logPath, "log4nettest.xml", loggerRepoName);

      ILoggerFactory loggerFactory = new LoggerFactory();
      //loggerFactory.AddConsole(Configuration.GetSection("Logging"));
      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(loggerRepoName);

      serviceProvider = new ServiceCollection()
          .AddTransient<IKafka, RdKafkaDriver>()          
          .AddTransient<IKafkaConsumer<IAssetEvent>, KafkaConsumer<IAssetEvent>>()
          .AddTransient<IKafkaConsumer<ICustomerEvent>, KafkaConsumer<ICustomerEvent>>()
          .AddTransient<IKafkaConsumer<IDeviceEvent>, KafkaConsumer<IDeviceEvent>>()
          .AddTransient<IKafkaConsumer<IGeofenceEvent>, KafkaConsumer<IGeofenceEvent>>()
          .AddTransient<IKafkaConsumer<IProjectEvent>, KafkaConsumer<IProjectEvent>>()
          .AddTransient<IKafkaConsumer<ISubscriptionEvent>, KafkaConsumer<ISubscriptionEvent>>()
          .AddTransient<IMessageTypeResolver, MessageResolver>()
          .AddTransient<IRepositoryFactory, RepositoryFactory>()
          
          .AddTransient<IRepository<IAssetEvent>, AssetRepository>()
          .AddTransient<IRepository<ICustomerEvent>, CustomerRepository>()
          .AddTransient<IRepository<IDeviceEvent>, DeviceRepository>()
          .AddTransient<IRepository<IGeofenceEvent>, GeofenceRepository>()
          .AddTransient<IRepository<IProjectEvent>, ProjectRepository>()
          .AddTransient<IRepository<ISubscriptionEvent>, SubscriptionRepository>()
          .AddSingleton<IConfigurationStore, GenericConfiguration>()
          .AddLogging()
          .AddSingleton<ILoggerFactory>(loggerFactory)
          .BuildServiceProvider();
    }

    /// <summary>
    /// This test attempted to:
    ///    write to the kafka que 
    ///    have the consumer to write to the DB
    ///    then we check that the object is in the database
    /// However it may not be possible to do this RELIABLY, serially 
    ///    so we may need to wait to do it using Daves pattern in end-end tests 
    ///    i.e. creating a Consumer container; polling DB waiting for the object to appear
    /// </summary>
    [TestMethod]
    public void AssetConsumerWritesToDB()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var createAssetEvent = new CreateAssetEvent
      {
        AssetUID = Guid.NewGuid(),
        AssetName = "The Asset Name",
        AssetType ="whatever",
        ActionUTC = actionUtc
      };

      var configurationStore = serviceProvider.GetService<IConfigurationStore>();
      var baseTopic = "VSS.Interfaces.Events.MasterData.IAssetEvent" + Guid.NewGuid().ToString();
      var suffix = configurationStore.GetValueString("KAFKA_TOPIC_NAME_SUFFIX");
      var topicName = baseTopic + configurationStore.GetValueString("KAFKA_TOPIC_NAME_SUFFIX");

      string messagePayload = JsonConvert.SerializeObject(new { CreateAssetEvent = createAssetEvent });

      var _producer = serviceProvider.GetService<IKafka>();
      _producer.InitProducer(serviceProvider.GetService<IConfigurationStore>());
      _producer.Send(topicName,
                new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>(createAssetEvent.AssetUID.ToString(), messagePayload)
                });

      var bar1 = serviceProvider.GetService<IKafkaConsumer<IAssetEvent>>();
      bar1.SetTopic(baseTopic);

      // don't appear to need to wait for writing to the kafka q
      //Thread.Sleep(1000);

      var assetContext = new AssetRepository(serviceProvider.GetService<IConfigurationStore>(), serviceProvider.GetService<ILoggerFactory>());
      Task<Asset> dbReturn = null;
      for (int i = 0; i < 10; i++)
      {
        bar1.StartProcessingSync();

        // wait for consumer, and anything to be written to the db;
        Thread.Sleep(5000);

        dbReturn = assetContext.GetAsset(createAssetEvent.AssetUID.ToString());
        dbReturn.Wait();
        if (dbReturn.Result != null)
          break;
      }

      Assert.IsNotNull(dbReturn.Result, "Unable to retrieve Asset from AssetRepo");
      Assert.AreEqual(createAssetEvent.AssetUID.ToString(), dbReturn.Result.AssetUID, "Asset details are incorrect from AssetRepo");
    }

    [TestMethod]
    public void CustomerConsumerWritesToDB()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var createCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = actionUtc
      };

      var configurationStore = serviceProvider.GetService<IConfigurationStore>();
      var baseTopic = "VSS.Interfaces.Events.MasterData.ICustomerEvent" + Guid.NewGuid().ToString();
      var suffix = configurationStore.GetValueString("KAFKA_TOPIC_NAME_SUFFIX");
      var topicName = baseTopic + configurationStore.GetValueString("KAFKA_TOPIC_NAME_SUFFIX");

      string messagePayload = JsonConvert.SerializeObject(new { CreateCustomerEvent = createCustomerEvent });

      var _producer = serviceProvider.GetService<IKafka>();
      _producer.InitProducer(serviceProvider.GetService<IConfigurationStore>());
      _producer.Send(topicName,
                new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>(createCustomerEvent.CustomerUID.ToString(), messagePayload)
                });

      var bar1 = serviceProvider.GetService<IKafkaConsumer<ICustomerEvent>>();
      bar1.SetTopic(baseTopic);

      // don't appear to need to wait for writing to the kafka q
      //Thread.Sleep(1000);

      var customerContext = new CustomerRepository(serviceProvider.GetService<IConfigurationStore>(), serviceProvider.GetService<ILoggerFactory>());
      Task<Customer> dbReturn = null;
      for (int i = 0; i < 10; i++)
      {
        bar1.StartProcessingSync();

        // wait for consumer, and anything to be written to the db;
        Thread.Sleep(5000);

        dbReturn = customerContext.GetCustomer(createCustomerEvent.CustomerUID);
        dbReturn.Wait();
        if (dbReturn.Result != null)
          break;
      }

      Assert.IsNotNull(dbReturn.Result, "Unable to retrieve Customer from CustomerRepo");
      Assert.AreEqual(createCustomerEvent.CustomerUID.ToString(), dbReturn.Result.CustomerUID, "Customer details are incorrect from CustomerRepo");
    }

    [TestMethod]
    public void DeviceConsumerWritesToDB()
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

      var configurationStore = serviceProvider.GetService<IConfigurationStore>();
      var baseTopic = "VSS.Interfaces.Events.MasterData.IDeviceEvent" + Guid.NewGuid().ToString();
      var suffix = configurationStore.GetValueString("KAFKA_TOPIC_NAME_SUFFIX");
      var topicName = baseTopic + configurationStore.GetValueString("KAFKA_TOPIC_NAME_SUFFIX");

      string messagePayload = JsonConvert.SerializeObject(new { CreateDeviceEvent = createDeviceEvent });

      var _producer = serviceProvider.GetService<IKafka>();
      _producer.InitProducer(serviceProvider.GetService<IConfigurationStore>());
      _producer.Send(topicName,
                new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>(createDeviceEvent.DeviceUID.ToString(), messagePayload)
                });
      Console.WriteLine("topic=" + topicName);
      Console.WriteLine("Message=" + messagePayload);
      var bar1 = serviceProvider.GetService<IKafkaConsumer<IDeviceEvent>>();
      bar1.SetTopic(baseTopic);

      // don't appear to need to wait for writing to the kafka q
      //Thread.Sleep(1000);

      var deviceContext = new DeviceRepository(serviceProvider.GetService<IConfigurationStore>(), serviceProvider.GetService<ILoggerFactory>());
      Task<Device> dbReturn = null;
      for (int i = 0; i < 10; i++)
      {
        bar1.StartProcessingSync();

        // wait for consumer, and anything to be written to the db;
        Thread.Sleep(5000);

        dbReturn = deviceContext.GetDevice(createDeviceEvent.DeviceUID.ToString());
        dbReturn.Wait();
        if (dbReturn.Result != null)
          break;
      }

      Assert.IsNotNull(dbReturn.Result, "Unable to retrieve Device from DeviceRepo");
      Assert.AreEqual(createDeviceEvent.DeviceUID.ToString(), dbReturn.Result.DeviceUID, "Device details are incorrect from DeviceRepo");
    }

    [TestMethod]
    public void ProjectConsumerWritesToDB()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var projectTimeZone = "New Zealand Standard Time";
      var createProjectEvent = new CreateProjectEvent()
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = 12343,
        ProjectName = "The Project Name",
        ProjectType = ProjectType.LandFill,
        ProjectTimezone = projectTimeZone,

        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ProjectBoundary = "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))",
        ActionUTC = actionUtc
      };

      var configurationStore = serviceProvider.GetService<IConfigurationStore>();
      var baseTopic = "VSS.Interfaces.Events.MasterData.CreateProjectEvent" + Guid.NewGuid().ToString();
      var suffix = configurationStore.GetValueString("KAFKA_TOPIC_NAME_SUFFIX");
      var topicName = baseTopic + configurationStore.GetValueString("KAFKA_TOPIC_NAME_SUFFIX");

      string messagePayload = JsonConvert.SerializeObject(new { CreateProjectEvent = createProjectEvent });

      var _producer = serviceProvider.GetService<IKafka>();
      _producer.InitProducer(serviceProvider.GetService<IConfigurationStore>());
      _producer.Send(topicName,
                new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>(createProjectEvent.ProjectUID.ToString(), messagePayload)
                });

      var bar1 = serviceProvider.GetService<IKafkaConsumer<IProjectEvent>>();
      bar1.SetTopic(baseTopic);

      // don't appear to need to wait for writing to the kafka q
      //Thread.Sleep(1000);

      var projectContext = new ProjectRepository(serviceProvider.GetService<IConfigurationStore>(), serviceProvider.GetService<ILoggerFactory>());
      Task<Project> dbReturn = null;
      for (int i = 0; i < 10; i++)
      {
        bar1.StartProcessingSync();

        // wait for consumer, and anything to be written to the db;
        Thread.Sleep(5000);

        dbReturn = projectContext.GetProject_UnitTest(createProjectEvent.ProjectUID.ToString());
        dbReturn.Wait();
        if (dbReturn.Result != null)
          break;
      }

      Assert.IsNotNull(dbReturn.Result, "Unable to retrieve Project from ProjectRepo");
      Assert.AreEqual(createProjectEvent.ProjectUID.ToString(), dbReturn.Result.ProjectUID, "Project details are incorrect from ProjectRepo");
    }

    [TestMethod]
    public void SubscriptionConsumerWritesToDB()
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

      var configurationStore = serviceProvider.GetService<IConfigurationStore>();
      var baseTopic = "VSS.Interfaces.Events.MasterData.CreateProjectSubscriptionEvent" + Guid.NewGuid().ToString();
      var suffix = configurationStore.GetValueString("KAFKA_TOPIC_NAME_SUFFIX");
      var topicName = baseTopic + configurationStore.GetValueString("KAFKA_TOPIC_NAME_SUFFIX");

      string messagePayload = JsonConvert.SerializeObject(new { CreateProjectSubscriptionEvent = createProjectSubscriptionEvent });

      var _producer = serviceProvider.GetService<IKafka>();
      _producer.InitProducer(serviceProvider.GetService<IConfigurationStore>());
      _producer.Send(topicName,
                new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>(createProjectSubscriptionEvent.SubscriptionUID.ToString(), messagePayload)
                });

      var bar1 = serviceProvider.GetService<IKafkaConsumer<ISubscriptionEvent>>();
      bar1.SetTopic(baseTopic);

      // don't appear to need to wait for writing to the kafka q
      //Thread.Sleep(1000);

      var subscriptionContext = new SubscriptionRepository(serviceProvider.GetService<IConfigurationStore>(), serviceProvider.GetService<ILoggerFactory>());
      Task<IEnumerable<Subscription>> dbReturn = null;
      for (int i = 0; i < 10; i++)
      {
        bar1.StartProcessingSync();

        // wait for consumer, and anything to be written to the db;
        Thread.Sleep(5000);

        dbReturn = subscriptionContext.GetSubscriptions_UnitTest(createProjectSubscriptionEvent.SubscriptionUID.ToString());
        dbReturn.Wait();
        if (dbReturn.Result != null && dbReturn.Result.Count() > 0)
          break;
      }

      Assert.IsNotNull(dbReturn.Result, "Unable to retrieve Subscription from SubscriptionRepo");
      Assert.AreEqual(1, dbReturn.Result.Count(), "Wrong Subscription count from SubscriptionRepo");
      var subs = dbReturn.Result.ToList();
      Assert.AreEqual(createProjectSubscriptionEvent.SubscriptionUID.ToString(), subs[0].SubscriptionUID, "Subscription details are incorrect from SubscriptionRepo");
    }

    [TestMethod]
    public void GeofenceConsumerWritesToDB()
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
        GeometryWKT = "POLYGON((172.68231141046 -43.6277661929154,172.692096108947 -43.6213045879588,172.701537484681 -43.6285117180247,172.698104257136 -43.6328604301996,172.689349526916 -43.6336058921214,172.682998055965 -43.6303754903428,172.68231141046 -43.6277661929154,172.68231141046 -43.6277661929154))",
        CustomerUID = customerUid,
        UserUID = Guid.NewGuid(),
        ActionUTC = actionUtc
      };

      var configurationStore = serviceProvider.GetService<IConfigurationStore>();
      var baseTopic = "VSS.Interfaces.Events.MasterData.CreateGeofenceEvent" + Guid.NewGuid().ToString();
      var suffix = configurationStore.GetValueString("KAFKA_TOPIC_NAME_SUFFIX");
      var topicName = baseTopic + configurationStore.GetValueString("KAFKA_TOPIC_NAME_SUFFIX");

      string messagePayload = JsonConvert.SerializeObject(new { CreateGeofenceEvent = createGeofenceEvent });

      var _producer = serviceProvider.GetService<IKafka>();
      _producer.InitProducer(serviceProvider.GetService<IConfigurationStore>());
      _producer.Send(topicName,
                new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>(createGeofenceEvent.GeofenceUID.ToString(), messagePayload)
                });

      var bar1 = serviceProvider.GetService<IKafkaConsumer<IGeofenceEvent>>();
      bar1.SetTopic(baseTopic);

      // don't appear to need to wait for writing to the kafka q
      //Thread.Sleep(1000);

      var geofenceContext = new GeofenceRepository(serviceProvider.GetService<IConfigurationStore>(), serviceProvider.GetService<ILoggerFactory>());
      Task<Geofence> dbReturn = null;
      for (int i = 0; i < 10; i++)
      {
        bar1.StartProcessingSync();

        // wait for consumer, and anything to be written to the db;
        Thread.Sleep(5000);

        dbReturn = geofenceContext.GetGeofence_UnitTest(createGeofenceEvent.GeofenceUID.ToString());
        dbReturn.Wait();
        if (dbReturn.Result != null)
          break;
      }

      Assert.IsNotNull(dbReturn.Result, "Unable to retrieve Geofence from GeofenceRepo");      
      Assert.AreEqual(createGeofenceEvent.GeofenceUID.ToString(), dbReturn.Result.GeofenceUID, "Geofence details are incorrect from GeofenceRepo");
    }
  }
}
