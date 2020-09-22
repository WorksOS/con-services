using System;
using CCSS.CWS.Client;
using CCSS.WorksOS.Reports.Abstractions.Models.ResultsHandling;
using CCSS.WorksOS.Reports.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.WebApi.Common;

namespace CCSS.WorksOS.Reports
{
  /// <summary>
  /// WorksOS Reports application startup.
  /// </summary>
  public class Startup : BaseStartup
  {
    /// <inheritdoc />
    public override string ServiceName => "Reports API";

    /// <inheritdoc />
    public override string ServiceDescription => "A service to generate 3d XLSX reports for WorksOS";

    /// <inheritdoc />
    public override string ServiceVersion => "v1";

    private static IServiceProvider _serviceProvider;

    /// <inheritdoc />
    protected override void ConfigureAdditionalServices(IServiceCollection services)
    {
      // Required for authentication
      services.AddSingleton<IConfigurationStore, GenericConfiguration>();
      services.AddSingleton<IServiceExceptionHandler, ServiceExceptionHandler>();
      services.AddSingleton<IErrorCodesProvider, ReportsErrorCodesProvider>();
      services.AddTransient<IWebRequest, GracefulWebRequest>();

      services.AddTransient<ICwsAccountClient, CwsAccountClient>();
      services.AddSingleton<IPreferenceProxy, PreferenceProxy>();

      services.AddOpenTracing(builder => { builder.ConfigureAspNetCore(options => { options.Hosting.IgnorePatterns.Add(request => request.Request.Path.ToString() == "/ping"); }); });
    }

    /// <inheritdoc />
    protected override void ConfigureAdditionalAppSettings(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory factory)
    {
      app.UseFilterMiddleware<ReportsAuthentication>();
      _serviceProvider = ServiceProvider;
    }
  }
}
