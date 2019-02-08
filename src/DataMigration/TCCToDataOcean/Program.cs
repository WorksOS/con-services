using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TCCToDataOcean.Interfaces;
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
      Log.Logger = new LoggerConfiguration()
                   .Enrich.FromLogContext()
                   .MinimumLevel.Debug()
                   .WriteTo.Console()
                   .WriteTo.File("logs\\migrator.log", rollingInterval: RollingInterval.Day)
                   .CreateLogger();

      var serviceCollection = new ServiceCollection();
      ConfigureServices(serviceCollection);

      var serviceProvider = serviceCollection.BuildServiceProvider();
      var migrator = serviceProvider.GetRequiredService<IMigrator>();

      var migrationSettings = (MigrationSettings)serviceProvider.GetRequiredService<IMigrationSettings>();
      migrationSettings.IsDebug = true;

      var success = migrator.MigrateFilesForAllActiveProjects().Result;
    }

    private static void ConfigureServices(IServiceCollection services)
    {
      Log.Debug("Configuring services");

      Log4NetProvider.RepoName = LoggerRepoName;

      services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));
      
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
      services.AddSingleton<IMigrationSettings, MigrationSettings>();
    }
  }
}
