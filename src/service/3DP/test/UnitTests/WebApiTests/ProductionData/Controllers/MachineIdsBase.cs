using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog;
using VSS.Common.Abstractions.Configuration;
using VSS.Productivity3D.Productivity3D.Models.ProductionData;
using VSS.Productivity3D.Productivity3D.Models.ProductionData.ResultHandling;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Serilog.Extensions;
using VSS.TRex.Gateway.Common.Abstractions;
#if RAPTOR
using VLPDDecls;
#endif

namespace VSS.Productivity3D.WebApiTests.ProductionData.Controllers
{
  public class MachineIdsBase
  {
    protected const int NULL_RAPTOR_MACHINE_DESIGN_ID = -1;
    protected const long NULL_ASSETID = -1;
    protected static IServiceProvider serviceProvider;
    protected static ILoggerFactory logger;
    protected static IHeaderDictionary _customHeaders;
    protected static Mock<ITRexCompactionDataProxy> tRexProxy;
#if RAPTOR
    protected static Mock<IASNodeClient> raptorClient;
#endif
    protected static Mock<IConfigurationStore> configStore;

    protected static void Init()
    {
      serviceProvider = new ServiceCollection()
        .AddLogging()
        .AddSingleton(new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.Productivity3D.WebApi.Tests.log")))
        .BuildServiceProvider();

      logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      _customHeaders = new HeaderDictionary();
      tRexProxy = new Mock<ITRexCompactionDataProxy>();
#if RAPTOR
      raptorClient = new Mock<IASNodeClient>();
#endif
      configStore = new Mock<IConfigurationStore>();
    }

    protected void GetTRexMachineIdsMock(List<MachineStatus> machineStatusList, Guid projectUid,
      Mock<IConfigurationStore> configStore, bool enableTRexGateway, bool isTRexAvailable,
      Mock<ITRexCompactionDataProxy> tRexProxy)
    {
      var expectedMachineExecutionResult = new MachineExecutionResult
      (
        new List<MachineStatus>(machineStatusList.Count)
      );
      expectedMachineExecutionResult.MachineStatuses.AddRange(machineStatusList);

      tRexProxy.Setup(x => x.SendDataGetRequest<MachineExecutionResult>(projectUid.ToString(),
          It.IsAny<string>(),
          It.IsAny<IHeaderDictionary>(),
          It.IsAny<IHeaderDictionary>()))
        .ReturnsAsync(expectedMachineExecutionResult);

      configStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_MACHINES")).Returns(enableTRexGateway);
      configStore.Setup(x => x.GetValueBool("TREX_IS_AVAILABLE")).Returns(isTRexAvailable);
    }

#if RAPTOR
    protected void GetRaptorMachineIdsMock(TMachineDetail[] machineDetailList, long projectId,
      Mock<IASNodeClient> raptorClient)
    {
      raptorClient
        .Setup(x => x.GetMachineIDs(projectId))
        .Returns(machineDetailList);
    }
#endif
  }
}
