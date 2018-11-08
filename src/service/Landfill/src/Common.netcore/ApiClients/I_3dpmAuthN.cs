using System.Threading.Tasks;

namespace Common.netstandard.ApiClients
{
  public interface I_3dpmAuthN
  {
    Task<string> Get3DPmSchedulerBearerToken();
  }
}