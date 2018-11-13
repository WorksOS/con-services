using System.Net.Http;
using System.Threading.Tasks;

namespace VSS.TRex.ConnectedSite.Gateway.WebApi.Abstractions
{
  public interface IConnectedSiteClient
  {
    Task<HttpResponseMessage> PostMessage<T>(T message) where T : IConnectedSiteMessage;
  }
}
