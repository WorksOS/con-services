using k8s;
using k8s.Exceptions;
using Microsoft.Extensions.Logging;
using VSS.Common.Kubernetes.Interfaces;

namespace VSS.Common.Kubernetes.Factories
{
  public class KubernetesClientFactory : IKubernetesClientFactory
  {
    private readonly ILogger<KubernetesClientFactory> logger;
    private KubernetesClientConfiguration kubernetesConfiguration;

    public KubernetesClientFactory(ILogger<KubernetesClientFactory> logger)
    {
      this.logger = logger;
    }

    public IKubernetes CreateClient(string kubernetesNamespace, string kubernetesContext = null)
    {
      if (string.IsNullOrEmpty(kubernetesNamespace))
      {
        logger.LogWarning($"{nameof(kubernetesNamespace)} is not defined, we cannot query the cluster to discover services");
        return null;
      }

      if (kubernetesConfiguration != null) 
        return new k8s.Kubernetes(kubernetesConfiguration);

      if (string.IsNullOrWhiteSpace(kubernetesContext))
      {
        try
        {
          kubernetesConfiguration = KubernetesClientConfiguration.InClusterConfig();
          logger.LogInformation("Using In Cluster Config");
        }
        catch (KubeConfigException)
        {
          try
          {
            logger.LogWarning("Cannot get InClusterConfig, using the config file");
            kubernetesConfiguration = KubernetesClientConfiguration.BuildConfigFromConfigFile();
          }
          catch (KubeConfigException)
          {
            logger.LogWarning("Cannot get ~/.kube/config file - giving up connecting to the cluster");
            return null;
          }
        }
      }
      else
      {
        logger.LogInformation($"Using Context {kubernetesContext}");
        try
        {
          kubernetesConfiguration =
            KubernetesClientConfiguration.BuildConfigFromConfigFile(currentContext: kubernetesContext);
        }
        catch (KubeConfigException)
        {
          logger.LogWarning("Cannot get ~/.kube/config file - giving up connecting to the cluster");
          return null;
        }
      }

      return new k8s.Kubernetes(kubernetesConfiguration);
    }
  }
}
