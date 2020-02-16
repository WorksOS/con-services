using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Microsoft.CSharp.RuntimeBinder;

namespace VSS.Hosted.VLCommon
{
	public class ContextMessageInspector<T> : IDispatchMessageInspector where T : IExtension<OperationContext>, new()
  {
    public object AfterReceiveRequest(ref System.ServiceModel.Channels.Message request,
                                      IClientChannel channel,
                                      InstanceContext instanceContext)
    {
      var t = OperationContext.Current.Extensions.Find<T>();

      if (t != null)
        throw new InvalidOperationException(string.Format("Cannot have more than once custom context of the same type '{0}'", typeof(T)));

      OperationContext.Current.Extensions.Add(new T());

      return request.Headers.MessageId;

    }

    public void BeforeSendReply(ref System.ServiceModel.Channels.Message reply, object correlationState)
    {
      var t = OperationContext.Current.Extensions.Find<T>();

      OperationContext.Current.Extensions.Remove(t);

      dynamic dt = t;

      try
      {
        dt.Dispose();
      }
      catch (RuntimeBinderException)
      {
        // Dispose not implemented. Ignore
      }
    }
  }

  public class CustomOperationContextBehaviorAttribute : Attribute, IServiceBehavior
  {
    private Type CustomContextType { get; set; }

    public CustomOperationContextBehaviorAttribute(Type customContextType)
    {
      CustomContextType = customContextType;
    }

    #region IServiceBehavior Members

    public void AddBindingParameters(ServiceDescription serviceDescription,
                                     ServiceHostBase serviceHostBase,
                                     System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints,
                                     BindingParameterCollection bindingParameters)
    {
    }

    public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
    {
      foreach (ChannelDispatcher cd in serviceHostBase.ChannelDispatchers)
      {
        foreach (EndpointDispatcher ed in cd.Endpoints)
        {
          var openType = typeof(ContextMessageInspector<>);
          var closedType = openType.MakeGenericType(CustomContextType);

          ed.DispatchRuntime.MessageInspectors.Add(Activator.CreateInstance(closedType) as IDispatchMessageInspector);
        }
      }
    }

    public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase) { }

    #endregion
  }
}
