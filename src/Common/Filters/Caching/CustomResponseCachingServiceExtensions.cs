using System;
using Microsoft.AspNetCore.ResponseCaching;
using Microsoft.AspNetCore.ResponseCaching.Internal;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VSS.Productivity3D.Common.Filters.Caching
{
  public static class CustomResponseCachingServiceExtensions
  {

    /// <summary>
    /// Add response caching services.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> for adding services.</param>
    public static IServiceCollection AddCustomResponseCaching(this IServiceCollection services)
    {
      if (services == null)
      {
        throw new ArgumentNullException(nameof(services));
      }

      services.TryAdd(ServiceDescriptor.Singleton<IResponseCachingPolicyProvider, CustomCachingPolicyProvider>());
      services.TryAdd(ServiceDescriptor.Singleton<IResponseCachingKeyProvider, CustomResponseCachingKeyProvider>());
      services.AddSingleton<IResponseCache>(new MemoryResponseCache(new MemoryCache(new MemoryCacheOptions())));

     return services;
    }

    /// <summary>
    /// Add response caching services and configure the related options.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> for adding services.</param>
    /// <param name="configureOptions">A delegate to configure the <see cref="ResponseCachingOptions"/>.</param>
    public static IServiceCollection AddCustomResponseCaching(this IServiceCollection services,
      Action<ResponseCachingOptions> configureOptions)
    {
      if (services == null)
      {
        throw new ArgumentNullException(nameof(services));
      }
      if (configureOptions == null)
      {
        throw new ArgumentNullException(nameof(configureOptions));
      }

      services.Configure(configureOptions);
      services.AddCustomResponseCaching();

      return services;
    }
  }
}
