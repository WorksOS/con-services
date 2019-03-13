using k8s;

namespace VSS.Common.Kubernetes.Interfaces
{
  public interface IKubernetesClientFactory
  {
    IKubernetes CreateClient(string kubernetesNamespace, string kubernetesContext = null);
  }
}