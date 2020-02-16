using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using VSS.Hosted.VLCommon;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces;
using VSS.Nighthawk.ExternalDataTypes.Enumerations;

namespace VSS.Nighthawk.DeviceCapabilityService.Helpers.DeviceHandlers
{
    public class PL161DeviceHandler : IDeviceHandlerStrategy
    {
        public PL161DeviceHandler(IEnumerable<string> outboundEndpointNames)
        {
            OutboundEndpointNames = outboundEndpointNames;
        }

        public Type AssetIdConfigurationChangedEventType
        {
            get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotSupported(DeviceTypeEnum.PL161, "AssetIdConfigurationChangedEventType")); }
        }

        public Type DigitalSwitchConfigurationEventType
        {
            get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotSupported(DeviceTypeEnum.PL161, "DigitalSwitchConfigurationEventType")); }
        }

        public Type DisableMaintenanceModeEventType
        {
            get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotSupported(DeviceTypeEnum.PL161, "DisableMaintenanceModeEventType")); }
        }

        public Type DiscreteInputConfigurationEventType
        {
            get { return typeof(DataOut.Interfaces.Events.IDiscreteInputConfigurationEvent); }
        }

        public Type EnableMaintenanceModeEventType
        {
            get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotSupported(DeviceTypeEnum.PL161, "EnableMaintenanceModeEventType")); }
        }

        public Type FirstDailyReportStartTimeUtcChangedEventType
        {
            get { return typeof(DataOut.Interfaces.Events.IFirstDailyReportStartTimeUtcChangedEvent); }
        }

        public Type HourMeterModifiedEventType
        {
            get { return typeof(DataOut.Interfaces.Events.IHourMeterModifiedEvent); }
        }

        public Type LocationUpdateRequestedEventType
        {
            get { return typeof(DataOut.Interfaces.Events.ILocationStatusUpdateRequestedEvent); }
        }

        public Type MovingCriteriaConfigurationChangedEventType
        {
            get { return typeof(DataOut.Interfaces.Events.IMovingCriteriaConfigurationChangedEvent); }
        }

        public Type OdometerModifiedEventType
        {
            get { return typeof(DataOut.Interfaces.Events.IOdometerModifiedEvent); }
        }

        public Type SiteDispatchedEventType
        {
            get { return typeof(DataOut.Interfaces.Events.ISiteDispatchedEvent); }
        }

        public Type SiteRemovedEventType
        {
            get { return typeof(DataOut.Interfaces.Events.ISiteRemovedEvent); }
        }

        public Type SetStartModeEventType
        {
            get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotCurrentlyImplemented(DeviceTypeEnum.PL161, "SetStartModeEventType")); }
        }

        public Type GetStartModeEventType
        {
            get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotCurrentlyImplemented(DeviceTypeEnum.PL161, "GetStartModeEventType")); }
        }

        public Type SetTamperLevelEventType
        {
            get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotCurrentlyImplemented(DeviceTypeEnum.PL161, "SetTamperLevelEventType")); }
        }

        public Type GetTamperLevelEventType
        {
            get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotCurrentlyImplemented(DeviceTypeEnum.PL161, "GetTamperLevelEventType")); }
        }

        public Type SetDailyReportFrequencyEventType
        {
            get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotCurrentlyImplemented(DeviceTypeEnum.PL161, "SetDailyReportFrequencyEventType")); }
        }

        public Type SetReportFrequencyEventType
        {
            get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotCurrentlyImplemented(DeviceTypeEnum.PL161, "SetReportFrequencyEventType")); }
        }

        public Type DisableRapidReportingEventType
        {
            get { return typeof(DataOut.Interfaces.Events.IDisableRapidReportingEvent); }
        }

        public Type EnableRapidReportingEventType
        {
            get { return typeof(DataOut.Interfaces.Events.IEnableRapidReportingEvent); }
        }

        public IEnumerable<string> OutboundEndpointNames { get; private set; }
    }
}
