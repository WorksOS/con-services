using System.Configuration;
using EasyHttp.Http;
using VSS.BaseEvents;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Factories;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Implementations.Helpers;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Query;

namespace VSS.Nighthawk.DeviceCapabilityService.Interfaces.Implementations.Factories
{
  public class SiteAdministrationEventFactory : ISiteAdministrationEventFactory
  {
    private readonly string _deviceCapabilitySvcUri;
    private readonly IEventTypeHelper _eventTypeHelper;

    static SiteAdministrationEventFactory()
    {
      FixReferencedTypesAtCompileTime();
    }

    private static void FixReferencedTypesAtCompileTime()
    {
      // Need to load all the referenced types for compiler to force copy
      // dependent type assemblies to clients of this assembly;
      // otherwise, clients of this assembly must reference assemblies with these types explicitly
      var referencedTypes = new[]
                              {
                                typeof(DataOut.Interfaces.Events.ISiteDispatchedEvent).AssemblyQualifiedName,
                                typeof(MTSGateway.Interfaces.Events.ISiteDispatchedEvent).AssemblyQualifiedName,
                                typeof(DataOut.Interfaces.Events.ISiteRemovedEvent).AssemblyQualifiedName,
                                typeof(MTSGateway.Interfaces.Events.ISiteRemovedEvent).AssemblyQualifiedName
                              };
    }

    public SiteAdministrationEventFactory()
    {
      _deviceCapabilitySvcUri = ConfigurationManager.AppSettings["DeviceCapabilityServiceBaseUri"] + "/" + ControllerConstants.SiteAdministrationControllerRouteName;
      _eventTypeHelper = new EventTypeHelper(new HttpClientWrapper(new HttpClient() { Request = { Accept = HttpContentTypes.ApplicationJson } }));
    }

    public ISiteDispatchedEvent BuildSiteDispatchedEventForDevice(IDeviceQuery deviceQuery)
    {
      return _eventTypeHelper.QueryServiceForTypeAndBuildInstance<ISiteDispatchedEvent>(ActionConstants.GetSiteDispatchedEvent, _deviceCapabilitySvcUri, deviceQuery);
    }

    public ISiteRemovedEvent BuildSiteRemovedEventForDevice(IDeviceQuery deviceQuery)
    {
      return _eventTypeHelper.QueryServiceForTypeAndBuildInstance<ISiteRemovedEvent>(ActionConstants.GetSiteRemovedEvent, _deviceCapabilitySvcUri, deviceQuery);
    }
  }
}
