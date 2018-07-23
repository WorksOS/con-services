using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.ConfigurationStore;
using VSS.Log4Net.Extensions;
using VSS.MasterData.Repositories;

namespace RepositoryTests.Internal
{
  public class TestControllerBase
  {
    protected IServiceProvider ServiceProvider;

    protected string TargetJsonString = @"<ProjectSettings>  
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

    public void SetupLogging()
    {
      const string loggerRepoName = "UnitTestLogTest";
      Log4NetProvider.RepoName = loggerRepoName;
      Log4NetAspExtensions.ConfigureLog4Net(loggerRepoName, "log4nettest.xml");

      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(loggerRepoName);

      ServiceProvider = new ServiceCollection()
        .AddSingleton<ILoggerProvider, Log4NetProvider>()
        .AddSingleton(loggerFactory)
        .AddLogging()
        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        .BuildServiceProvider();

      Assert.IsNotNull(ServiceProvider.GetService<ILoggerFactory>());
    }
  }
}
