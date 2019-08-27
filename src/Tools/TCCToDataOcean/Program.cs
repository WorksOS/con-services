﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using TCCToDataOcean.DatabaseAgent;
using TCCToDataOcean.Interfaces;
using TCCToDataOcean.Types;
using TCCToDataOcean.Utils;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Interfaces.Repository;
using VSS.Productivity3D.Project.Repository;
using VSS.Serilog.Extensions;
using VSS.TCCFileAccess;
using VSS.WebApi.Common;

namespace TCCToDataOcean
{
  class Program
  {
    private static void Main()
    {
      MainAsync().GetAwaiter().GetResult();
    }

    private static async Task MainAsync()
    {
      var serviceCollection = new ServiceCollection();
      ConfigureServices(serviceCollection);

      var serviceProvider = serviceCollection.BuildServiceProvider();
      var migrator = serviceProvider.GetRequiredService<IMigrator>();

      await migrator.MigrateFilesForAllActiveProjects().ConfigureAwait(false);

      serviceProvider.GetRequiredService<ILiteDbAgent>()
                     .SetMigationInfo_EndTime();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
      var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                                             .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ENVIRONMENT_NAME")}.json", optional: true, reloadOnChange: false)
                                             .Build();

      var loggerFactory = new LoggerFactory().AddSerilog(SerilogExtensions.Configure("TCCToDataOcean.log"));

      services.AddLogging()
              .AddSingleton(loggerFactory);

      services.AddSingleton<ITPaaSApplicationAuthentication, TPaaSApplicationAuthentication>();
      services.AddSingleton<IConfigurationStore, GenericConfiguration>(_ => new GenericConfiguration(loggerFactory, config));
      services.AddScoped<IProjectRepository, ProjectRepository>();
      services.AddScoped<IErrorCodesProvider, MigrationErrorCodesProvider>();
      services.AddScoped<IServiceExceptionHandler, ServiceExceptionHandler>();
      services.AddSingleton<IEnvironmentHelper, EnvironmentHelper>();
      services.AddTransient<IFileRepository, FileRepository>();
      services.AddTransient<IWebApiUtils, WebApiUtils>();
      services.AddSingleton<IRestClient, RestClient>();
      services.AddTransient<IImportFile, ImportFile>();
      services.AddTransient<IMigrator, Migrator>();
      services.AddTransient<ITPaasProxy, TPaasProxy>();
      services.AddSingleton<ILiteDbAgent, LiteDbAgent>();
      services.AddSingleton<ICSIBAgent, CSIBAgent>();
    }
  }
}