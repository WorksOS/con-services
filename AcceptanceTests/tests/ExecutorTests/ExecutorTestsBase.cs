using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtility;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.Log4Net.Extensions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Project.WebAPI.Common.Internal;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.VisionLink.Interfaces.Events.Identity.User;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using RdKafkaDriver = VSS.KafkaConsumer.Kafka.RdKafkaDriver;

namespace ExecutorTests
{
  public class ExecutorTestsBase
  {
    protected IServiceProvider serviceProvider;
    protected IConfigurationStore configStore;
    protected ILoggerFactory logger;
    protected IServiceExceptionHandler serviceExceptionHandler;
    protected ProjectRepository projectRepo;
    protected CustomerRepository customerRepo;
    protected IRaptorProxy raptorProxy;
    protected IKafka producer;
    protected string kafkaTopicName;

    [TestInitialize]
    public virtual void InitTest()
    {
      var serviceCollection = new ServiceCollection();

      string loggerRepoName = "UnitTestLogTest";
      var logPath = System.IO.Directory.GetCurrentDirectory();

      Log4NetProvider.RepoName = loggerRepoName;
      Log4NetAspExtensions.ConfigureLog4Net(logPath, "log4nettest.xml", loggerRepoName);
      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(loggerRepoName);

      serviceCollection.AddLogging();
      serviceCollection.AddSingleton<ILoggerFactory>(loggerFactory)
        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        .AddTransient<IRepository<IProjectEvent>, ProjectRepository>()
        .AddTransient<IRepository<ICustomerEvent>, CustomerRepository>()
        .AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>()
        .AddTransient<IRaptorProxy, RaptorProxy>()
        .AddSingleton<IKafka, RdKafkaDriver>()
        .AddMemoryCache();  

      serviceProvider = serviceCollection.BuildServiceProvider();
      configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();
      projectRepo = serviceProvider.GetRequiredService<IRepository<IProjectEvent>>() as ProjectRepository;
      customerRepo = serviceProvider.GetRequiredService<IRepository<ICustomerEvent>>() as CustomerRepository;
      raptorProxy = serviceProvider.GetRequiredService<IRaptorProxy>();
      producer = serviceProvider.GetRequiredService<IKafka>();
      if (!producer.IsInitializedProducer)
        producer.InitProducer(configStore);

      kafkaTopicName = "VSS.Interfaces.Events.MasterData.IProjectEvent" +
                       configStore.GetValueString("KAFKA_TOPIC_NAME_SUFFIX");
    }

    protected IDictionary<string, string> CustomHeaders(string customerUid)
    {
      var headers = new Dictionary<string, string>();
      headers.Add("X-JWT-Assertion", RestClientUtil.DEFAULT_JWT);
      headers.Add("X-VisionLink-CustomerUid", customerUid);
      headers.Add("X-VisionLink-ClearCache", "true");
      return headers;
    }

    protected bool CreateCustomerProject(string customerUid, string projectUid)
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = Guid.Parse(customerUid),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = actionUtc
      };

      var createProjectEvent = new CreateProjectEvent()
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ProjectUID = Guid.Parse(projectUid),
        ProjectID = new Random().Next(1, 1999999),
        ProjectName = "The Project Name",
        Description = "the Description",
        ProjectType = ProjectType.LandFill,
        ProjectTimezone = "New Zealand Standard Time",
        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ActionUTC = actionUtc,
        ProjectBoundary =
          "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))",
        CoordinateSystemFileContent = new byte[] { 0, 1, 2, 3, 4 },
        CoordinateSystemFileName = "thisLocation\\this.cs"
      };

      var associateCustomerProjectEvent = new AssociateProjectCustomer
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ProjectUID = createProjectEvent.ProjectUID,
        LegacyCustomerID = 1234,
        RelationType = RelationType.Customer,
        ActionUTC = actionUtc
      };

      projectRepo.StoreEvent(createProjectEvent).Wait();
      customerRepo.StoreEvent(createCustomerEvent).Wait();
      projectRepo.StoreEvent(associateCustomerProjectEvent).Wait();
      var g = projectRepo.GetProject(projectUid); g.Wait();
      return (g.Result != null ? true : false);
    }

    protected bool CreateProjectSettings(string projectUid, string userId, string settings, ProjectSettingsType settingsType)
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);

      var createProjectSettingsEvent = new UpdateProjectSettingsEvent()
      {
        ProjectUID = Guid.Parse(projectUid),
        UserID = userId,
        Settings = settings,
        ProjectSettingsType = settingsType,
        ActionUTC = actionUtc
      };

      projectRepo.StoreEvent(createProjectSettingsEvent).Wait();
      var g = projectRepo.GetProjectSettings(projectUid, userId, settingsType); g.Wait();
      return (g.Result != null ? true : false);
    }
  }
}
