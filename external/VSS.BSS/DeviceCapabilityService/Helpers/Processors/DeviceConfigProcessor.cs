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
  public class DeviceConfigProcessor : IDeviceConfigProcessor
  {
    private readonly IStorage _storage;
    private readonly IDeviceHandlerParameters _deviceHandlerParameters;
    private readonly IDeviceQueryHelper _deviceQueryHelper;
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public DeviceConfigProcessor(IStorage storage, IDeviceHandlerParameters deviceHandlerParameters, IDeviceQueryHelper deviceQueryHelper)
    {
      _storage = storage;
      _deviceHandlerParameters = deviceHandlerParameters;
      _deviceQueryHelper = deviceQueryHelper;
    }

    public IFactoryOutboundEventTypeDescriptor GetDigitalSwitchConfigurationEvent(IDeviceQuery device)
    {
      Log.IfDebugFormat("{0} processing {1} request for {2}", GetType().Name, "GetDigitalSwitchConfigurationEvent", _deviceQueryHelper.GetPrintableValues(device));

      ED.DeviceTypeEnum? deviceType = _deviceQueryHelper.GetDeviceType(device, _storage);

      return new FactoryOutboundEventTypeDescriptor
      {
        AssemblyQualifiedName =
          (deviceType.HasValue && _deviceHandlerParameters.DeviceHandlers.ContainsKey(deviceType.Value)) ?
          _deviceHandlerParameters.DeviceHandlers[deviceType.Value].DigitalSwitchConfigurationEventType.AssemblyQualifiedName :
          _deviceHandlerParameters.UnknownDeviceHandler.DigitalSwitchConfigurationEventType.AssemblyQualifiedName,
        Destinations = _storage.GetEndpointDescriptorsForNames(
        (deviceType.HasValue && _deviceHandlerParameters.DeviceHandlers.ContainsKey(deviceType.Value)) ?
        _deviceHandlerParameters.DeviceHandlers[deviceType.Value].OutboundEndpointNames :
        _deviceHandlerParameters.UnknownDeviceHandler.OutboundEndpointNames).ToArray()
      };
    }

    public IFactoryOutboundEventTypeDescriptor GetDisableMaintenanceModeEvent(IDeviceQuery device)
    {
      Log.IfDebugFormat("{0} processing {1} request for {2}", GetType().Name, "GetDisableMaintenanceModeEvent", _deviceQueryHelper.GetPrintableValues(device));

      ED.DeviceTypeEnum? deviceType = _deviceQueryHelper.GetDeviceType(device, _storage);

      return new FactoryOutboundEventTypeDescriptor
      {
        AssemblyQualifiedName =
          (deviceType.HasValue && _deviceHandlerParameters.DeviceHandlers.ContainsKey(deviceType.Value)) ?
          _deviceHandlerParameters.DeviceHandlers[deviceType.Value].DisableMaintenanceModeEventType.AssemblyQualifiedName :
          _deviceHandlerParameters.UnknownDeviceHandler.DisableMaintenanceModeEventType.AssemblyQualifiedName,
        Destinations = _storage.GetEndpointDescriptorsForNames(
        (deviceType.HasValue && _deviceHandlerParameters.DeviceHandlers.ContainsKey(deviceType.Value)) ?
        _deviceHandlerParameters.DeviceHandlers[deviceType.Value].OutboundEndpointNames :
        _deviceHandlerParameters.UnknownDeviceHandler.OutboundEndpointNames).ToArray()
      };
    }

    public IFactoryOutboundEventTypeDescriptor GetDiscreteInputConfigurationEvent(IDeviceQuery device)
    {
      Log.IfDebugFormat("{0} processing {1} request for {2}", GetType().Name, "GetDiscreteInputConfigurationEvent", _deviceQueryHelper.GetPrintableValues(device));

      ED.DeviceTypeEnum? deviceType = _deviceQueryHelper.GetDeviceType(device, _storage);

      return new FactoryOutboundEventTypeDescriptor
      {
        AssemblyQualifiedName =
          (deviceType.HasValue && _deviceHandlerParameters.DeviceHandlers.ContainsKey(deviceType.Value)) ?
          _deviceHandlerParameters.DeviceHandlers[deviceType.Value].DiscreteInputConfigurationEventType.AssemblyQualifiedName :
          _deviceHandlerParameters.UnknownDeviceHandler.DiscreteInputConfigurationEventType.AssemblyQualifiedName,
        Destinations = _storage.GetEndpointDescriptorsForNames(
        (deviceType.HasValue && _deviceHandlerParameters.DeviceHandlers.ContainsKey(deviceType.Value)) ?
        _deviceHandlerParameters.DeviceHandlers[deviceType.Value].OutboundEndpointNames :
        _deviceHandlerParameters.UnknownDeviceHandler.OutboundEndpointNames).ToArray()
      };
    }

    public IFactoryOutboundEventTypeDescriptor GetEnableMaintenanceModeEvent(IDeviceQuery device)
    {
      Log.IfDebugFormat("{0} processing {1} request for {2}", GetType().Name, "GetEnableMaintenanceModeEvent", _deviceQueryHelper.GetPrintableValues(device));

      ED.DeviceTypeEnum? deviceType = _deviceQueryHelper.GetDeviceType(device, _storage);

      return new FactoryOutboundEventTypeDescriptor
      {
        AssemblyQualifiedName =
          (deviceType.HasValue && _deviceHandlerParameters.DeviceHandlers.ContainsKey(deviceType.Value)) ?
          _deviceHandlerParameters.DeviceHandlers[deviceType.Value].EnableMaintenanceModeEventType.AssemblyQualifiedName :
          _deviceHandlerParameters.UnknownDeviceHandler.EnableMaintenanceModeEventType.AssemblyQualifiedName,
        Destinations = _storage.GetEndpointDescriptorsForNames(
        (deviceType.HasValue && _deviceHandlerParameters.DeviceHandlers.ContainsKey(deviceType.Value)) ?
        _deviceHandlerParameters.DeviceHandlers[deviceType.Value].OutboundEndpointNames :
        _deviceHandlerParameters.UnknownDeviceHandler.OutboundEndpointNames).ToArray()
      };
    }

    public IFactoryOutboundEventTypeDescriptor GetFirstDailyReportStartTimeUtcChangedEvent(IDeviceQuery device)
    {
      Log.IfDebugFormat("{0} processing {1} request for {2}", GetType().Name, "GetFirstDailyReportStartTimeUtcChangedEvent", _deviceQueryHelper.GetPrintableValues(device));

      ED.DeviceTypeEnum? deviceType = _deviceQueryHelper.GetDeviceType(device, _storage);

      return new FactoryOutboundEventTypeDescriptor
      {
        AssemblyQualifiedName =
          (deviceType.HasValue && _deviceHandlerParameters.DeviceHandlers.ContainsKey(deviceType.Value)) ?
          _deviceHandlerParameters.DeviceHandlers[deviceType.Value].FirstDailyReportStartTimeUtcChangedEventType.AssemblyQualifiedName :
          _deviceHandlerParameters.UnknownDeviceHandler.FirstDailyReportStartTimeUtcChangedEventType.AssemblyQualifiedName,
        Destinations = _storage.GetEndpointDescriptorsForNames(
        (deviceType.HasValue && _deviceHandlerParameters.DeviceHandlers.ContainsKey(deviceType.Value)) ?
        _deviceHandlerParameters.DeviceHandlers[deviceType.Value].OutboundEndpointNames :
        _deviceHandlerParameters.UnknownDeviceHandler.OutboundEndpointNames).ToArray()
      };
    }

    public IFactoryOutboundEventTypeDescriptor GetHourMeterModifiedEvent(IDeviceQuery device)
    {
      Log.IfDebugFormat("{0} processing {1} request for {2}", GetType().Name, "GetHourMeterModifiedEvent", _deviceQueryHelper.GetPrintableValues(device));

      ED.DeviceTypeEnum? deviceType = _deviceQueryHelper.GetDeviceType(device, _storage);

      return new FactoryOutboundEventTypeDescriptor
      {
        AssemblyQualifiedName =
          (deviceType.HasValue && _deviceHandlerParameters.DeviceHandlers.ContainsKey(deviceType.Value)) ?
          _deviceHandlerParameters.DeviceHandlers[deviceType.Value].HourMeterModifiedEventType.AssemblyQualifiedName :
          _deviceHandlerParameters.UnknownDeviceHandler.HourMeterModifiedEventType.AssemblyQualifiedName,
        Destinations = _storage.GetEndpointDescriptorsForNames(
        (deviceType.HasValue && _deviceHandlerParameters.DeviceHandlers.ContainsKey(deviceType.Value)) ?
        _deviceHandlerParameters.DeviceHandlers[deviceType.Value].OutboundEndpointNames :
        _deviceHandlerParameters.UnknownDeviceHandler.OutboundEndpointNames).ToArray()
      };
    }

    public IFactoryOutboundEventTypeDescriptor GetMovingCriteriaConfigurationChangedEvent(IDeviceQuery device)
    {
      Log.IfDebugFormat("{0} processing {1} request for {2}", GetType().Name, "GetMovingCriteriaConfigurationChangedEvent", _deviceQueryHelper.GetPrintableValues(device));

      ED.DeviceTypeEnum? deviceType = _deviceQueryHelper.GetDeviceType(device, _storage);

      return new FactoryOutboundEventTypeDescriptor
      {
        AssemblyQualifiedName =
          (deviceType.HasValue && _deviceHandlerParameters.DeviceHandlers.ContainsKey(deviceType.Value)) ?
          _deviceHandlerParameters.DeviceHandlers[deviceType.Value].MovingCriteriaConfigurationChangedEventType.AssemblyQualifiedName :
          _deviceHandlerParameters.UnknownDeviceHandler.MovingCriteriaConfigurationChangedEventType.AssemblyQualifiedName,
        Destinations = _storage.GetEndpointDescriptorsForNames(
        (deviceType.HasValue && _deviceHandlerParameters.DeviceHandlers.ContainsKey(deviceType.Value)) ?
        _deviceHandlerParameters.DeviceHandlers[deviceType.Value].OutboundEndpointNames :
        _deviceHandlerParameters.UnknownDeviceHandler.OutboundEndpointNames).ToArray()
      };
    }

    public IFactoryOutboundEventTypeDescriptor GetOdometerModifiedEvent(IDeviceQuery device)
    {
      Log.IfDebugFormat("{0} processing {1} request for {2}", GetType().Name, "GetOdometerModifiedEvent", _deviceQueryHelper.GetPrintableValues(device));

      ED.DeviceTypeEnum? deviceType = _deviceQueryHelper.GetDeviceType(device, _storage);

      return new FactoryOutboundEventTypeDescriptor
      {
        AssemblyQualifiedName =
          (deviceType.HasValue && _deviceHandlerParameters.DeviceHandlers.ContainsKey(deviceType.Value)) ?
          _deviceHandlerParameters.DeviceHandlers[deviceType.Value].OdometerModifiedEventType.AssemblyQualifiedName :
          _deviceHandlerParameters.UnknownDeviceHandler.OdometerModifiedEventType.AssemblyQualifiedName,
        Destinations = _storage.GetEndpointDescriptorsForNames(
        (deviceType.HasValue && _deviceHandlerParameters.DeviceHandlers.ContainsKey(deviceType.Value)) ?
        _deviceHandlerParameters.DeviceHandlers[deviceType.Value].OutboundEndpointNames :
        _deviceHandlerParameters.UnknownDeviceHandler.OutboundEndpointNames).ToArray()
      };
    }

    public IFactoryOutboundEventTypeDescriptor SetStartModeEvent(IDeviceQuery device)
    {
      Log.IfDebugFormat("{0} processing {1} request for {2}", GetType().Name, "SetStartModeEvent", _deviceQueryHelper.GetPrintableValues(device));

      ED.DeviceTypeEnum? deviceType = _deviceQueryHelper.GetDeviceType(device, _storage);

      return new FactoryOutboundEventTypeDescriptor
      {
        AssemblyQualifiedName =
          (deviceType.HasValue && _deviceHandlerParameters.DeviceHandlers.ContainsKey(deviceType.Value)) ?
          _deviceHandlerParameters.DeviceHandlers[deviceType.Value].SetStartModeEventType.AssemblyQualifiedName :
          _deviceHandlerParameters.UnknownDeviceHandler.SetStartModeEventType.AssemblyQualifiedName,
        Destinations = _storage.GetEndpointDescriptorsForNames(
        (deviceType.HasValue && _deviceHandlerParameters.DeviceHandlers.ContainsKey(deviceType.Value)) ?
        _deviceHandlerParameters.DeviceHandlers[deviceType.Value].OutboundEndpointNames :
        _deviceHandlerParameters.UnknownDeviceHandler.OutboundEndpointNames).ToArray()
      };
    }

    public IFactoryOutboundEventTypeDescriptor GetStartModeEvent(IDeviceQuery device)
    {
      Log.IfDebugFormat("{0} processing {1} request for {2}", GetType().Name, "GetStartModeEvent", _deviceQueryHelper.GetPrintableValues(device));

      ED.DeviceTypeEnum? deviceType = _deviceQueryHelper.GetDeviceType(device, _storage);

      return new FactoryOutboundEventTypeDescriptor
      {
        AssemblyQualifiedName =
          (deviceType.HasValue && _deviceHandlerParameters.DeviceHandlers.ContainsKey(deviceType.Value)) ?
          _deviceHandlerParameters.DeviceHandlers[deviceType.Value].GetStartModeEventType.AssemblyQualifiedName :
          _deviceHandlerParameters.UnknownDeviceHandler.GetStartModeEventType.AssemblyQualifiedName,
        Destinations = _storage.GetEndpointDescriptorsForNames(
        (deviceType.HasValue && _deviceHandlerParameters.DeviceHandlers.ContainsKey(deviceType.Value)) ?
        _deviceHandlerParameters.DeviceHandlers[deviceType.Value].OutboundEndpointNames :
        _deviceHandlerParameters.UnknownDeviceHandler.OutboundEndpointNames).ToArray()
      };
    }

    public IFactoryOutboundEventTypeDescriptor SetTamperLevelEvent(IDeviceQuery device)
    {
      Log.IfDebugFormat("{0} processing {1} request for {2}", GetType().Name, "SetTamperLevelEvent", _deviceQueryHelper.GetPrintableValues(device));

      ED.DeviceTypeEnum? deviceType = _deviceQueryHelper.GetDeviceType(device, _storage);

      return new FactoryOutboundEventTypeDescriptor
      {
        AssemblyQualifiedName =
          (deviceType.HasValue && _deviceHandlerParameters.DeviceHandlers.ContainsKey(deviceType.Value)) ?
          _deviceHandlerParameters.DeviceHandlers[deviceType.Value].SetTamperLevelEventType.AssemblyQualifiedName :
          _deviceHandlerParameters.UnknownDeviceHandler.SetTamperLevelEventType.AssemblyQualifiedName,
        Destinations = _storage.GetEndpointDescriptorsForNames(
        (deviceType.HasValue && _deviceHandlerParameters.DeviceHandlers.ContainsKey(deviceType.Value)) ?
        _deviceHandlerParameters.DeviceHandlers[deviceType.Value].OutboundEndpointNames :
        _deviceHandlerParameters.UnknownDeviceHandler.OutboundEndpointNames).ToArray()
      };
    }

    public IFactoryOutboundEventTypeDescriptor GetTamperLevelEvent(IDeviceQuery device)
    {
      Log.IfDebugFormat("{0} processing {1} request for {2}", GetType().Name, "GetTamperLevelEvent", _deviceQueryHelper.GetPrintableValues(device));

      ED.DeviceTypeEnum? deviceType = _deviceQueryHelper.GetDeviceType(device, _storage);

      return new FactoryOutboundEventTypeDescriptor
      {
        AssemblyQualifiedName =
          (deviceType.HasValue && _deviceHandlerParameters.DeviceHandlers.ContainsKey(deviceType.Value)) ?
          _deviceHandlerParameters.DeviceHandlers[deviceType.Value].GetTamperLevelEventType.AssemblyQualifiedName :
          _deviceHandlerParameters.UnknownDeviceHandler.GetTamperLevelEventType.AssemblyQualifiedName,
        Destinations = _storage.GetEndpointDescriptorsForNames(
        (deviceType.HasValue && _deviceHandlerParameters.DeviceHandlers.ContainsKey(deviceType.Value)) ?
        _deviceHandlerParameters.DeviceHandlers[deviceType.Value].OutboundEndpointNames :
        _deviceHandlerParameters.UnknownDeviceHandler.OutboundEndpointNames).ToArray()
      };
    }

    public IFactoryOutboundEventTypeDescriptor SetDailyReportFrequencyEvent(IDeviceQuery device)
    {
      Log.IfDebugFormat("{0} processing {1} request for {2}", GetType().Name, "SetDailyReportFrequencyEvent", _deviceQueryHelper.GetPrintableValues(device));

      ED.DeviceTypeEnum? deviceType = _deviceQueryHelper.GetDeviceType(device, _storage);

      return new FactoryOutboundEventTypeDescriptor
      {
        AssemblyQualifiedName =
          (deviceType.HasValue && _deviceHandlerParameters.DeviceHandlers.ContainsKey(deviceType.Value)) ?
          _deviceHandlerParameters.DeviceHandlers[deviceType.Value].SetDailyReportFrequencyEventType.AssemblyQualifiedName :
          _deviceHandlerParameters.UnknownDeviceHandler.SetDailyReportFrequencyEventType.AssemblyQualifiedName,
        Destinations = _storage.GetEndpointDescriptorsForNames(
        (deviceType.HasValue && _deviceHandlerParameters.DeviceHandlers.ContainsKey(deviceType.Value)) ?
        _deviceHandlerParameters.DeviceHandlers[deviceType.Value].OutboundEndpointNames :
        _deviceHandlerParameters.UnknownDeviceHandler.OutboundEndpointNames).ToArray()
      };
    }
    
    public IFactoryOutboundEventTypeDescriptor DisableRapidReportingEvent(IDeviceQuery device)
    {
      Log.IfDebugFormat("{0} processing {1} request for {2}", GetType().Name, "DisableRapidReportingEvent", _deviceQueryHelper.GetPrintableValues(device));

      ED.DeviceTypeEnum? deviceType = _deviceQueryHelper.GetDeviceType(device, _storage);

      return new FactoryOutboundEventTypeDescriptor
      {
        AssemblyQualifiedName =
          (deviceType.HasValue && _deviceHandlerParameters.DeviceHandlers.ContainsKey(deviceType.Value)) ?
          _deviceHandlerParameters.DeviceHandlers[deviceType.Value].DisableRapidReportingEventType.AssemblyQualifiedName :
          _deviceHandlerParameters.UnknownDeviceHandler.DisableRapidReportingEventType.AssemblyQualifiedName,
        Destinations = _storage.GetEndpointDescriptorsForNames(
        (deviceType.HasValue && _deviceHandlerParameters.DeviceHandlers.ContainsKey(deviceType.Value)) ?
        _deviceHandlerParameters.DeviceHandlers[deviceType.Value].OutboundEndpointNames :
        _deviceHandlerParameters.UnknownDeviceHandler.OutboundEndpointNames).ToArray()
      };
    }

    public IFactoryOutboundEventTypeDescriptor EnableRapidReportingEvent(IDeviceQuery device)
    {
      Log.IfDebugFormat("{0} processing {1} request for {2}", GetType().Name, "EnableRapidReportingEvent", _deviceQueryHelper.GetPrintableValues(device));

      ED.DeviceTypeEnum? deviceType = _deviceQueryHelper.GetDeviceType(device, _storage);

      return new FactoryOutboundEventTypeDescriptor
      {
        AssemblyQualifiedName =
          (deviceType.HasValue && _deviceHandlerParameters.DeviceHandlers.ContainsKey(deviceType.Value)) ?
          _deviceHandlerParameters.DeviceHandlers[deviceType.Value].EnableRapidReportingEventType.AssemblyQualifiedName :
          _deviceHandlerParameters.UnknownDeviceHandler.EnableRapidReportingEventType.AssemblyQualifiedName,
        Destinations = _storage.GetEndpointDescriptorsForNames(
        (deviceType.HasValue && _deviceHandlerParameters.DeviceHandlers.ContainsKey(deviceType.Value)) ?
        _deviceHandlerParameters.DeviceHandlers[deviceType.Value].OutboundEndpointNames :
        _deviceHandlerParameters.UnknownDeviceHandler.OutboundEndpointNames).ToArray()
      };
    }

    public IFactoryOutboundEventTypeDescriptor GetReportingFrequencyChangedEvent(IDeviceQuery device)
    {
      Log.IfDebugFormat("{0} processing {1} request for {2}", GetType().Name, "GetReportingFrequencyChangedEvent", _deviceQueryHelper.GetPrintableValues(device));

      ED.DeviceTypeEnum? deviceType = _deviceQueryHelper.GetDeviceType(device, _storage);

      return new FactoryOutboundEventTypeDescriptor
      {
        AssemblyQualifiedName =
          (deviceType.HasValue && _deviceHandlerParameters.DeviceHandlers.ContainsKey(deviceType.Value)) ?
          _deviceHandlerParameters.DeviceHandlers[deviceType.Value].SetReportFrequencyEventType.AssemblyQualifiedName :
          _deviceHandlerParameters.UnknownDeviceHandler.SetReportFrequencyEventType.AssemblyQualifiedName,
        Destinations = _storage.GetEndpointDescriptorsForNames(
        (deviceType.HasValue && _deviceHandlerParameters.DeviceHandlers.ContainsKey(deviceType.Value)) ?
        _deviceHandlerParameters.DeviceHandlers[deviceType.Value].OutboundEndpointNames :
        _deviceHandlerParameters.UnknownDeviceHandler.OutboundEndpointNames).ToArray()
      };
    }

  }
}
