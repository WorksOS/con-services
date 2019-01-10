using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Log4Net.Extensions;
using VSS.WebApi.Common;

namespace TCCToDataOcean
{
  class Program
  {
    public const string LoggerRepoName = "TCCToDataOcean";

    static void Main(string[] args)
    {
      var serviceCollection = new ServiceCollection();
      ConfigureServices(serviceCollection);

      var serviceProvider = serviceCollection.BuildServiceProvider();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
      Log4NetProvider.RepoName = LoggerRepoName;
      services.AddSingleton<ILoggerProvider, Log4NetProvider>();
      services.AddLogging(configure => configure.AddConsole()
        .AddDebug())
        .Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Debug);
      services.AddSingleton<ITPaaSApplicationAuthentication, TPaaSApplicationAuthentication>();

      Log4NetAspExtensions.ConfigureLog4Net(LoggerRepoName, "log4nettest.xml");

      //services.AddLogging(builder => builder.AddLog4Net("log4nettest.xml");

    }
  }
}
