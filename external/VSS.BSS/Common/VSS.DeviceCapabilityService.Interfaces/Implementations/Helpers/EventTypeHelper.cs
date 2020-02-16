using Magnum.Reflection;
using System;
using System.Linq;
using System.Net;
using VSS.BaseEvents;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.DTOs;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Implementations.DTOs;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Query;

namespace VSS.Nighthawk.DeviceCapabilityService.Interfaces.Implementations.Helpers
{
  public class EventTypeHelper : IEventTypeHelper
  {
    private readonly IHttpClientWrapper _httpClientWrapper;
    private readonly object _syncRoot = new object(); // synchronize access to HttpClient

    public EventTypeHelper(IHttpClientWrapper httpClientWrapper)
    {
      _httpClientWrapper = httpClientWrapper;
    }

    public T QueryServiceForTypeAndBuildInstance<T>(string serverAction, string deviceCapabilitySvcUri, IDeviceQuery deviceQuery)
      where T : IEndpointDestinedEvent
    {
      T constructedInstance = default(T);

      lock (_syncRoot)
      {
        var response = _httpClientWrapper.Get(String.Format("{0}/{1}?{2}", deviceCapabilitySvcUri,
          serverAction,
          GetQueryString(deviceQuery)));

        if (response.StatusCode == HttpStatusCode.OK)
        {
          IFactoryOutboundEventTypeDescriptor typeDescriptor = response.StaticBody<FactoryOutboundEventTypeDescriptor>();
          Type typeToConstruct = Type.GetType(typeDescriptor.AssemblyQualifiedName);

          // Build a dynamic implementation for the specified interface type to construct
          object target =
            FastActivator.Create(InterfaceImplementationBuilder.GetProxyFor(typeToConstruct));

          constructedInstance = (T)target;

          constructedInstance.Destinations =
            typeDescriptor.Destinations.Select(e =>
              new EndpointDefinition
              {
                ContentType = e.ContentType,
                EncryptedPwd = Convert.FromBase64String(e.EncryptedPwd),
                EndpointDefinitionId = e.Id,
                Name = e.Name,
                Url = e.Url,
                UserName = e.Username
              }).ToArray();
        }
        else
        {
          throw new Exception(response.RawText);
        }
      }

      return constructedInstance;
    }

    public static string GetQueryString(IDeviceQuery deviceQuery)
    {
      if (deviceQuery.DeviceType.HasValue)
      {
        return string.Format("gpsdeviceid={0}&devicetype={1}", deviceQuery.GPSDeviceID, deviceQuery.DeviceType);
      }
      if (deviceQuery.ID.HasValue)
      {
        return string.Format("id={0}", deviceQuery.ID);
      }
      if (deviceQuery.AssetID.HasValue)
      {
        return string.Format("assetid={0}", deviceQuery.AssetID);
      }
      return string.Empty;
    }
  }
}
