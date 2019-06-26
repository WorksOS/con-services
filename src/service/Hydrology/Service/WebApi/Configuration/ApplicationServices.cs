﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.Hydrology.WebApi.Configuration;
using VSS.MasterData.Models.Handlers;

// ReSharper disable once CheckNamespace
namespace VSS.Hydrology.WebApi
{
  /// <summary>
  /// Partial implementation of startup configuration for service descriptor contracts.
  /// </summary>
  public partial class Startup
  {
    /// <summary>
    /// Add required service descriptors to support the DI contract.
    /// </summary>
    /// <param name="services">Collection of service descriptors provided by ASP.NET on configuration startup</param>
    /// <returns>IServiceCollection collection of services for controller DI.</returns>
    public void ConfigureApplicationServices(IServiceCollection services)
    {
      //TODO We may switch over to IOptions as it is safer - proactive config validation vs lazy and strongly typed config values
      services.AddSingleton<IConfigureOptions<MvcOptions>, ConfigureMvcOptions>();
      services.AddSingleton<IConfigurationStore, GenericConfiguration>();
      services.AddScoped<IServiceExceptionHandler, ServiceExceptionHandler>();
    }
  }
}