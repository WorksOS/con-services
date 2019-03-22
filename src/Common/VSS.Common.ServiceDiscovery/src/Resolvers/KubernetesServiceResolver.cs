using System;
using System.Linq;
using System.Threading.Tasks;
using k8s;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.Common.Kubernetes.Interfaces;
using VSS.ConfigurationStore;

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
    private const int PORT_NUMBER = 80;

    private const int DEFAULT_PRIORITY = 10;

    private readonly ILogger<KubernetesServiceResolver> logger;
    
    private readonly IKubernetes kubernetesClient;

    private readonly string kubernetesContext;
    private readonly string kubernetesNamespace;

    public KubernetesServiceResolver(ILogger<KubernetesServiceResolver> logger, IKubernetesClientFactory kubernetesClientFactory, IConfigurationStore configuration)
    {
      this.logger = logger;

      Priority =  configuration.GetValueInt("KubernetesServicePriority", DEFAULT_PRIORITY);

      kubernetesNamespace = configuration.GetValueString("KubernetesNamespace", null);
      if (string.IsNullOrEmpty(kubernetesNamespace))
        kubernetesNamespace = "default";

      logger.LogInformation($"Using the kubernetes namespace {kubernetesNamespace} with priority {Priority}");

      kubernetesContext = configuration.GetValueString("KubernetesContext", null);

      kubernetesClient = kubernetesClientFactory.CreateClient(kubernetesNamespace, kubernetesContext);
    }

    public ServiceResultType ServiceType => ServiceResultType.InternalKubernetes;

    public int Priority { get; }

    public Task<string> ResolveService(string serviceName)
    {
      var labelFilter = $"{LABEL_FILTER_NAME}={serviceName}";

      // Are we configured? if not we won't have setup the client 
      if (string.IsNullOrEmpty(kubernetesNamespace) || kubernetesClient == null)
        return Task.FromResult<string>(null);

      // Attempt to get the list of services in our namespace that match our label
      // We must use our Namespace as there could more than one of the service in our cluster, across namespaces
      // E.g Alpha and Dev pods are hosted in the same cluster
      var list = kubernetesClient.ListNamespacedService(kubernetesNamespace, labelSelector: labelFilter);
      if (list?.Items == null || list.Items.Count == 0)
        return Task.FromResult<string>(null);

      foreach (var item in list.Items)
      {
        // We require the kubernetes service to define an HTTP Port, and have a CLUSTER IP specified.
        // Any service without both of these will be ignored.
        var spec = item.Spec;
        var httpPort = item
          .Spec
          .Ports
          .FirstOrDefault(p => p.Port == PORT_NUMBER);

        if (httpPort == null)
        {
          logger.LogWarning($"Could not find Port {PORT_NUMBER} for the service `{item.Metadata?.Name}` - ignoring");
        }
        else if (string.IsNullOrEmpty(item.Spec.ClusterIP))
        {
          logger.LogWarning($"No clusterIP provided for service {item.Metadata.Name} - ignoring");
        }
        else
        {
          // First one found that matches
          var url = $"http://{spec.ClusterIP}:{httpPort.Port}";
          logger.LogInformation($"Found `{url}` for the service name `{serviceName}` from kubernetes");
          return Task.FromResult(url);
        }
      }

      // No results
      return Task.FromResult<string>(null);
    }
  }
}
