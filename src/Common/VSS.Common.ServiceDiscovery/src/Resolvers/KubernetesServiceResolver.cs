using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using k8s;
using k8s.Exceptions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Cache.Models;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.Common.Abstractions.ServiceDiscovery.Models;

namespace VSS.Common.ServiceDiscovery.Resolvers
{
  public class KubernetesServiceResolver : IServiceResolver
  {
    /// <summary>
    /// We will search for this label on the kubernetes service, and use the value as the return
    /// </summary>
    private const string LABEL_FILTER_NAME = "service-name";

    /// <summary>
    /// We are specifically looking for HTTP only services, we currently do not (or need) support other types (e.g https, binary)
    /// </summary>
    private const string PORT_NAME = "http";

    private const int DEFAULT_PRIORITY = 100;

    private readonly ILogger<KubernetesServiceResolver> logger;
    private readonly IDataCache cache;
    
    private readonly Kubernetes kubernetesClient;

    private readonly string kubernetesContext;
    private readonly string kubernetesNamespace;

    public KubernetesServiceResolver(ILogger<KubernetesServiceResolver> logger, IConfiguration configuration, IDataCache cache)
    {
      this.logger = logger;
      this.cache = cache;

      if (int.TryParse(configuration["KubernetesServicePriority"], out var priority))
        Priority = priority;
      else
      {
        logger.LogWarning($"Cannot find priority, defaulting to {DEFAULT_PRIORITY}");
        Priority = DEFAULT_PRIORITY;
      }

      kubernetesNamespace = configuration["KubernetesNamespace"];
      if (string.IsNullOrEmpty(kubernetesNamespace))
        kubernetesNamespace = "default";

      logger.LogInformation($"Using the kubernetes namespace {kubernetesNamespace} with priority {Priority}");

      if (string.IsNullOrEmpty(kubernetesContext))
        kubernetesContext = configuration["KubernetesContext"];

      if (string.IsNullOrEmpty(kubernetesNamespace))
      {
        logger.LogWarning(
          $"{nameof(kubernetesNamespace)} is not defined, we cannot query the cluster to discover services");
      }
      else
      {
        KubernetesClientConfiguration config;
        if (string.IsNullOrWhiteSpace(kubernetesContext))
        {
          try
          {
            config = KubernetesClientConfiguration.InClusterConfig();
            logger.LogInformation("Using In Cluster Config");
          }
          catch (KubeConfigException e)
          {
            logger.LogWarning("Cannot get InClusterConfig, using the config file");
            config = KubernetesClientConfiguration.BuildConfigFromConfigFile();
          }
        }
        else
        {
          logger.LogInformation($"Using Context {kubernetesContext}");
          config = KubernetesClientConfiguration.BuildConfigFromConfigFile(currentContext: kubernetesContext);
        }

        kubernetesClient = new Kubernetes(config);
      }
    }

    public ServiceResultType ServiceType => ServiceResultType.InternalKubernetes;

    public int Priority { get; }

    public Task<string> ResolveService(string serviceName)
    {
      var cacheKey = $"{nameof(KubernetesServiceResolver)}-{serviceName}";
      var item = cache.GetOrCreate(cacheKey, entry =>
      {
        entry.SetOptions(new MemoryCacheEntryOptions()
        {
          AbsoluteExpirationRelativeToNow = new TimeSpan(0, 0, 0, 30)
        });

        var result = GetService(serviceName);
        var cacheItem = new CacheItem<string>(result, new List<string>
        {
          serviceName
        });

        return Task.FromResult(cacheItem);

      });

      return item;
    }

    private string GetService(string serviceName)
    {
      var labelFilter = $"{LABEL_FILTER_NAME}={serviceName}";

      // Are we configured? if not we won't have setup the client 
      if (string.IsNullOrEmpty(kubernetesNamespace) || kubernetesClient == null)
        return null;

      // Attempt to get the list of services in our namespace that match our label
      // We must use our Namespace as there could more than one of the service in our cluster, across namespaces
      // E.g Alpha and Dev pods are hosted in the same cluster
      var list = kubernetesClient.ListNamespacedService(kubernetesNamespace, labelSelector: labelFilter);
      if (list?.Items == null || list.Items.Count == 0)
        return null;

      foreach (var item in list.Items)
      {
        // We require the kubernetes service to define an HTTP Port, and have a CLUSTER IP specified.
        // Any service without both of these will be ignored.
        var spec = item.Spec;
        var httpPort = item
          .Spec
          .Ports
          .FirstOrDefault(p => string.Compare(PORT_NAME, p.Name, StringComparison.OrdinalIgnoreCase) == 0);

        if (httpPort == null)
        {
          logger.LogWarning($"Could not find an {PORT_NAME} Port for the service {item.Metadata.Name} - ignoring");
        }
        else if (string.IsNullOrEmpty(item.Spec.ClusterIP))
        {
          logger.LogWarning($"No clusterIP provided for service {item.Metadata.Name} - ignoring");
        }
        else
        {
          // First one found that matches
          var url = $"{httpPort.Name}://{spec.ClusterIP}:{httpPort.Port}";
          logger.LogInformation($"Found `{url}` for the service name `{serviceName}` from kubernetes");
          return url;
        }
      }

      // No results
      return null;
    }
  }
}
