using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.Log4Net.Extensions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Repositories;
using VSS.TCCFileAccess;
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
      services.AddScoped<IProjectRepository, ProjectRepository>();
      services.AddScoped<IErrorCodesProvider, MigrationErrorCodesProvider>();
      services.AddScoped<IServiceExceptionHandler, ServiceExceptionHandler>();
      services.AddTransient<IFileRepository, FileRepository>();
      services.AddTransient<IWebApiUtils, WebApiUtils>();
      services.AddTransient<IRestClient, RestClient>();
      services.AddTransient<IImportFile, ImportFile>();

      Log4NetAspExtensions.ConfigureLog4Net(LoggerRepoName, "log4nettest.xml");

      //services.AddLogging(builder => builder.AddLog4Net("log4nettest.xml");

    }
  }
}
