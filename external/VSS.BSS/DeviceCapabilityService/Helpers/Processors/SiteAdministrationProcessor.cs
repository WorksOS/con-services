using log4net;
using System.Linq;
using System.Reflection;
using VSS.Hosted.VLCommon;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.DTOs;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Implementations.DTOs;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Query;
using ED = VSS.Nighthawk.ExternalDataTypes.Enumerations;

namespace VSS.Nighthawk.DeviceCapabilityService.Helpers.Processors
{
  public class SiteAdministrationProcessor : ISiteAdministrationProcessor
  {
    private readonly IStorage _storage;
    private readonly IDeviceHandlerParameters _deviceHandlerParameters;
    private readonly IDeviceQueryHelper _deviceQueryHelper;
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public SiteAdministrationProcessor(IStorage storage, IDeviceHandlerParameters deviceHandlerParameters, IDeviceQueryHelper deviceQueryHelper)
    {
      _storage = storage;
      _deviceHandlerParameters = deviceHandlerParameters;
      _deviceQueryHelper = deviceQueryHelper;
    }

    public IFactoryOutboundEventTypeDescriptor GetSiteDispatchedEvent(IDeviceQuery device)
    {
      Log.IfDebugFormat("{0} processing {1} request for {2}", GetType().Name, "GetSiteDispatchedEvent", _deviceQueryHelper.GetPrintableValues(device));

      ED.DeviceTypeEnum? deviceType = _deviceQueryHelper.GetDeviceType(device, _storage);
      return new FactoryOutboundEventTypeDescriptor
      {
        AssemblyQualifiedName =
          (deviceType.HasValue && _deviceHandlerParameters.DeviceHandlers.ContainsKey(deviceType.Value)) ?
          _deviceHandlerParameters.DeviceHandlers[deviceType.Value].SiteDispatchedEventType.AssemblyQualifiedName :
          _deviceHandlerParameters.UnknownDeviceHandler.SiteDispatchedEventType.AssemblyQualifiedName,
        Destinations = _storage.GetEndpointDescriptorsForNames(
        (deviceType.HasValue && _deviceHandlerParameters.DeviceHandlers.ContainsKey(deviceType.Value)) ?
        _deviceHandlerParameters.DeviceHandlers[deviceType.Value].OutboundEndpointNames :
        _deviceHandlerParameters.UnknownDeviceHandler.OutboundEndpointNames).ToArray()
      };
    }

    public IFactoryOutboundEventTypeDescriptor GetSiteRemovedEvent(IDeviceQuery device)
    {
      Log.IfDebugFormat("{0} processing {1} request for {2}", GetType().Name, "GetSiteRemovedEvent", _deviceQueryHelper.GetPrintableValues(device));

      ED.DeviceTypeEnum? deviceType = _deviceQueryHelper.GetDeviceType(device, _storage);
      return new FactoryOutboundEventTypeDescriptor
      {
        AssemblyQualifiedName =
          (deviceType.HasValue && _deviceHandlerParameters.DeviceHandlers.ContainsKey(deviceType.Value)) ?
          _deviceHandlerParameters.DeviceHandlers[deviceType.Value].SiteRemovedEventType.AssemblyQualifiedName :
          _deviceHandlerParameters.UnknownDeviceHandler.SiteRemovedEventType.AssemblyQualifiedName,
        Destinations = _storage.GetEndpointDescriptorsForNames(
        (deviceType.HasValue && _deviceHandlerParameters.DeviceHandlers.ContainsKey(deviceType.Value)) ?
        _deviceHandlerParameters.DeviceHandlers[deviceType.Value].OutboundEndpointNames :
        _deviceHandlerParameters.UnknownDeviceHandler.OutboundEndpointNames).ToArray()
      };
    }
  }
}