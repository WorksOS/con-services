using System.Threading.Tasks;

namespace VSS.Tpaas.Client.Abstractions
{
  public interface ITPaaSClient
  {
    Task<string> GetBearerTokenAsync();
  }
}
