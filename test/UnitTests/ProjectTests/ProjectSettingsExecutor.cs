using System;
using System.Collections.Generic;
using KafkaConsumer.Kafka;
using MasterDataProxies.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Repositories;
using VSS.Productivity3D.ProjectWebApiCommon.Executors;
using Microsoft.Extensions.Logging;
using VSS.GenericConfiguration;
using VSS.Productivity3D.ProjectWebApiCommon.Internal;
using VSS.Productivity3D.ProjectWebApiCommon.ResultsHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace MasterDataConsumerTests
{
  [TestClass]
  public class ProjectSettingsRequestValidation : ExecutorBaseTests
  {

    [TestMethod]
    public async System.Threading.Tasks.Task CanCallGetProjectSettingsExecutorNoValidInputAsync()
    {
      string projectUid = Guid.NewGuid().ToString();
      var projectRepo = serviceProvider.GetRequiredService<IRepository<IProjectEvent>>() as ProjectRepository;
      var raptorProxy = serviceProvider.GetRequiredService<IRaptorProxy>();
      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();
      var customHeaders = new Dictionary<string, string>();
      var producer = serviceProvider.GetRequiredService<IKafka>();

      var executor = RequestExecutorContainer.Build<GetProjectSettingsExecutor>(projectRepo, raptorProxy, configStore, logger, serviceExceptionHandler, customHeaders, producer );
      var result = await executor.ProcessAsync(projectUid) as ProjectSettingsResult;

      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(projectUid, result.ProjectUid, "executor returned incorrect ProjectUid");
      Assert.IsNotNull(result.Settings, "executor returned incorrect Settings");
    }

  }
}
