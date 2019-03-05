using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TCCToDataOcean.DatabaseAgent;
using TCCToDataOcean.Interfaces;
using TCCToDataOcean.Types;
using TCCToDataOcean.Utils;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.Log4Net.Extensions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.TCCFileAccess;
using VSS.WebApi.Common;

namespace TCCToDataOcean
{
  class Program
  {
    public const string LoggerRepoName = "TCCToDataOcean";

    static void Main()
    {
      var serviceCollection = new ServiceCollection();
      ConfigureServices(serviceCollection);

      var serviceProvider = serviceCollection.BuildServiceProvider();
      var migrator = serviceProvider.GetRequiredService<IMigrator>();

      _ = migrator.MigrateFilesForAllActiveProjects().ConfigureAwait(true);

      serviceProvider.GetRequiredService<ILiteDbAgent>()
                     .SetMigationInfo_EndTime();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
      Log4NetProvider.RepoName = LoggerRepoName;
      services.AddSingleton<ILoggerProvider, Log4NetProvider>();
      services.AddLogging(configure => configure.AddConsole()
                                                .AddDebug())
              .Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Trace);
      services.AddSingleton<ITPaaSApplicationAuthentication, TPaaSApplicationAuthentication>();
      services.AddSingleton<IConfigurationStore, GenericConfiguration>();
      services.AddScoped<IProjectRepository, ProjectRepository>();
      services.AddScoped<IErrorCodesProvider, MigrationErrorCodesProvider>();
      services.AddScoped<IServiceExceptionHandler, ServiceExceptionHandler>();
      services.AddTransient<IFileRepository, FileRepository>();
      services.AddTransient<IWebApiUtils, WebApiUtils>();
      services.AddSingleton<IRestClient, RestClient>();
      services.AddTransient<IImportFile, ImportFile>();
      services.AddTransient<IMigrator, Migrator>();
      services.AddTransient<ITPaasProxy, TPaasProxy>();
      services.AddSingleton<ILiteDbAgent, LiteDbAgent>();

      Log4NetAspExtensions.ConfigureLog4Net(LoggerRepoName, "log4net.xml");
    }
  }
}
