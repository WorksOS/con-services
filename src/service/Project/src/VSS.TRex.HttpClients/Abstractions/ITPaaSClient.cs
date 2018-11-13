using System.Threading.Tasks;

namespace VSS.TRrex.HttpClients.Abstractions
{
  public interface ITPaaSClient
  {
    Task<string> GetBearerTokenAsync();
  }
}
