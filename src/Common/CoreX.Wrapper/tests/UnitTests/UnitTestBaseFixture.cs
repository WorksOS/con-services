using System;
using CoreX.Interfaces;
using CoreX.Wrapper.UnitTests.Types;
using Microsoft.Extensions.DependencyInjection;
using VSS.Common.Abstractions.Configuration;
using VSS.ConfigurationStore;

namespace CoreX.Wrapper.UnitTests
{
  public class UnitTestBaseFixture : IDisposable
  {
    private readonly IServiceProvider _serviceProvider;

    public ICoreXWrapper CoreXWrapper => _serviceProvider.GetRequiredService<ICoreXWrapper>();

    private string _csib = null;
    public string CSIB => _csib ??= CoreXWrapper.DCFileToCSIB(DCFile.GetFilePath(DCFile.DIMENSIONS_2012_DC_FILE_WITH_VERT_ADJUST));

    public UnitTestBaseFixture()
    {
      _serviceProvider = new ServiceCollection()
        .AddLogging()
        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        .AddSingleton<ICoreXWrapper, CoreXWrapper>()
        .BuildServiceProvider();
    }

    public void Dispose()
    { }
  }
}
