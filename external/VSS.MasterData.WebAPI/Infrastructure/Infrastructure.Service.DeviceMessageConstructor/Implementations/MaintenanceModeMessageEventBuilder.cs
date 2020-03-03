using ClientModel.DeviceConfig.Request.DeviceConfig;
using ClientModel.DeviceConfig.Request.DeviceConfig.MaintenanceMode;
using Infrastructure.Common.DeviceMessageConstructor.Attributes;
using Infrastructure.Common.DeviceMessageConstructor.Interfaces;
using CommonModel.DeviceSettings;
using Infrastructure.Service.DeviceMessageConstructor.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Utilities.Logging;
using VSS.VisionLink.Interfaces.Events.Commands.A5N2;
using VSS.VisionLink.Interfaces.Events.Commands.Interfaces;
using VSS.VisionLink.Interfaces.Events.Commands.Models;
using VSS.VisionLink.Interfaces.Events.Commands.MTS;

namespace Infrastructure.Service.DeviceMessageConstructor.Implementations
{
	[Group("MaintenanceMode")]
    public class MaintenanceModeMessageEventBuilder : IGroupMessageEventBuilder
    {
		private readonly ILoggingService _loggingService;
        private readonly IDataPopulator _dataPopulator;
        private readonly IDataValidator _validator;
        private const string RequestMessageType = "DeviceConfigMaintenanceModeRequest";

        public MaintenanceModeMessageEventBuilder(IDataPopulator dataPopulator, IDataValidator validator, ILoggingService loggingService)
        {
            _dataPopulator = dataPopulator;
            _validator = validator;
			_loggingService = loggingService;
		}
        public IEnumerable<Tuple<IPLOutMessageEvent, DeviceDetails>> GetPlOutMessageEvent(DeviceConfigRequestBase requestBase, DeviceDetails deviceDetails)
        {
			_loggingService.Info("Device Type doesn't support this Group", "MaintenanceModeMessageEventBuilder.GetPlOutMessageEvent");
            throw new NotImplementedException("Device Type doesn't support this Group");
        }

        public IEnumerable<Tuple<IOutMessageEvent, DeviceDetails>> GetDataOutMessageEvent(DeviceConfigRequestBase requestBase, DeviceDetails deviceDetails)
        {
            var dataOutMessages = new List<Tuple<IOutMessageEvent, DeviceDetails>>();
			_loggingService.Info("Recieved Maintenance Mode Message for Device " + deviceDetails.DeviceType, "MaintenanceModeMessageEventBuilder.GetDataOutMessageEvent");
            var requestMessage = _dataPopulator.GetRequestModel<DeviceConfigMaintenanceModeRequest>(requestBase);
            if (_validator.NullCheck(RequestMessageType, requestMessage.Status))
            {
                if (requestMessage.Status.Value)
                {
                    var enableMessageEvent = _dataPopulator.ConstructDataOutEvent<EnableMaintenanceModeEvent>(deviceDetails);
                    enableMessageEvent.Duration = _validator.NullCheck(RequestMessageType, requestMessage.MaintenanceModeDuration) ? new TimeSpan(requestMessage.MaintenanceModeDuration.Value, 0, 0) : new TimeSpan();
                    enableMessageEvent.StartUtc = requestMessage.StartTime;
                    dataOutMessages.Add(new Tuple<IOutMessageEvent, DeviceDetails>(enableMessageEvent, deviceDetails));
                }
                else
                {
                    var disableMessageEvent = _dataPopulator.ConstructDataOutEvent<DisableMaintenanceModeEvent>(deviceDetails);
                    dataOutMessages.Add(new Tuple<IOutMessageEvent, DeviceDetails>(disableMessageEvent, deviceDetails));
                }
            }
			_loggingService.Info("Maintenance Mode Message Event Construction for Device Type " + deviceDetails.DeviceType + " completed !!" + JsonConvert.SerializeObject(requestMessage), "MaintenanceModeMessageEventBuilder.GetDataOutMessageEvent");
            return dataOutMessages;
        }

        public IEnumerable<Tuple<IMTSOutMessageEvent, DeviceDetails>> GetMtsOutMessageEvent(DeviceConfigRequestBase requestBase, DeviceDetails deviceDetails)
        {
            var mtsOutMessages = new List<Tuple<IMTSOutMessageEvent, DeviceDetails>>();
			_loggingService.Info("Recieved Maintenance Mode Message for Device " + deviceDetails.DeviceType, "MaintenanceModeMessageEventBuilder.GetMtsOutMessageEvent");
            var requestMessage = _dataPopulator.GetRequestModel<DeviceConfigMaintenanceModeRequest>(requestBase);
            if (_validator.NullCheck(RequestMessageType, requestMessage.Status))
            {
                var mtsMessageEvent = _dataPopulator.ConstructMtsEvent<SendOtaConfigurationEvent>(deviceDetails);
                mtsMessageEvent.Input1 = mtsMessageEvent.Input2 = mtsMessageEvent.Input3 = mtsMessageEvent.Input4 = new OtaConfigDetail();
                if (requestMessage.Status.Value && _validator.NullCheck(RequestMessageType, requestMessage.MaintenanceModeDuration))
                    mtsMessageEvent.MaintenanceModeDuration = new TimeSpan(requestMessage.MaintenanceModeDuration.Value, 0, 0);
                mtsMessageEvent.MaintenanceModeEnabled = requestMessage.Status.Value;
                mtsOutMessages.Add(new Tuple<IMTSOutMessageEvent, DeviceDetails>(mtsMessageEvent, deviceDetails));
            }
			_loggingService.Info("Maintenance Mode Message Event Construction for Device Type " + deviceDetails.DeviceType + " completed !!" + JsonConvert.SerializeObject(requestMessage), "MaintenanceModeMessageEventBuilder.GetMtsOutMessageEvent");
            return mtsOutMessages;
        }        
    }
}
