using System.ServiceModel;
using System.ServiceModel.Channels;

namespace VSS.Hosted.VLCommon
{
  public interface IProxyFactory<T>
  {
    ChannelFactory<T> Create(Binding binding, string address);
  }
}