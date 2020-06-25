using System;
using Microsoft.Extensions.DependencyInjection;

namespace CoreX.Wrapper.UnitTests
{
  public class UnitTestBaseFixture : IDisposable
  {
    private readonly IServiceProvider _serviceProvider;

    public IConvertCoordinates ConvertCoordinates => _serviceProvider.GetRequiredService<IConvertCoordinates>();
    public CoreX CoreX => _serviceProvider.GetRequiredService<CoreX>();

    public UnitTestBaseFixture()
    {
      _serviceProvider = new ServiceCollection()
        .AddSingleton<CoreX>()
        .AddSingleton<IConvertCoordinates, ConvertCoordinates>()
        .BuildServiceProvider();
    }

    public void Dispose()
    { }
  }
}
