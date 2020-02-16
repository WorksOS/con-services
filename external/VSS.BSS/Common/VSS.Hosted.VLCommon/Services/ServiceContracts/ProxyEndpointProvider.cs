using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace VSS.Hosted.VLCommon
{
  public interface IProxyEndpointProvider<T>
  {
    IList<ChannelFactory<T>> GetEndpoints();
    IList<string> GetOrderedAddresses();
  }
}