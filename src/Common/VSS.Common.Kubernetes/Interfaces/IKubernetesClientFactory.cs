using k8s;

namespace VSS.Common.Kubernetes.Interfaces
{
  public interface IKubernetesClientFactory
  {
    (IKubernetes client, string currentNamespace) CreateClient(string kubernetesContext = null);
  }
}