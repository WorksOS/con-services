using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Description;

namespace VSS.Nighthawk.NHBssSvc
{
  public class CustomHttpEndpointBehavior : IEndpointBehavior
  {
    #region Constructor

    CustomHttpMessageInspector _inspector;
    public CustomHttpEndpointBehavior(CustomHttpMessageInspector inspector)
    {
      _inspector = inspector;
    }

    #endregion

    #region IEndpointBehavior Implementation Methods

    /// <summary>
    /// Apply any custom behaviours to the endpoint that needs to be 
    /// applied before dispatching the message to the server
    /// </summary>
    /// <param name="endpoint"></param>
    /// <param name="clientRuntime"></param>
    public void ApplyClientBehavior(ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.ClientRuntime clientRuntime)
    {
      clientRuntime.MessageInspectors.Add(_inspector);
    }

    //wantedly removed "throw new NotImplementedException()" as we don't have any custom 
    //implementation for the below methods and don't want to fail custom behaviour for that reason.
    public void AddBindingParameters(ServiceEndpoint endpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters) { }

    public void ApplyDispatchBehavior(ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.EndpointDispatcher endpointDispatcher) { }

    public void Validate(ServiceEndpoint endpoint) { }

    #endregion
  }
}
