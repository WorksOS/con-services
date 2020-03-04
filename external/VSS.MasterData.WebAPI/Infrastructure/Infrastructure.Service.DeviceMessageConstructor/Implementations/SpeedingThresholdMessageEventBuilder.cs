using ClientModel.DeviceConfig.Request.DeviceConfig;
using ClientModel.DeviceConfig.Request.DeviceConfig.SpeedingThresholds;
using Infrastructure.Common.DeviceMessageConstructor.Attributes;
using Infrastructure.Common.DeviceMessageConstructor.Interfaces;
using CommonModel.DeviceSettings;
using Infrastructure.Service.DeviceMessageConstructor.Helpers;
using Infrastructure.Service.DeviceMessageConstructor.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Utilities.Logging;
using VSS.VisionLink.Interfaces.Events.Commands.A5N2;
using VSS.VisionLink.Interfaces.Events.Commands.Interfaces;
using VSS.VisionLink.Interfaces.Events.Commands.MTS;

namespace Infrastructure.Service.DeviceMessageConstructor.Implementations
{
	[Group("SpeedingThresholds")]
    public class SpeedingThresholdMessageEventBuilder : IGroupMessageEventBuilder
    {
        private static ILoggingService _loggingService;
        private readonly IDataPopulator _dataPopulator;
        private readonly IDataValidator _validator;
        private const string RequestMessageType = "DeviceConfigSpeedingThresholdsRequest";

        public SpeedingThresholdMessageEventBuilder(IDataPopulator dataPopulator, IDataValidator validator, ILoggingService loggingService)
        {
            _dataPopulator = dataPopulator;
            _validator = validator;
			_loggingService = loggingService;
		}
        public IEnumerable<Tuple<IPLOutMessageEvent, DeviceDetails>> GetPlOutMessageEvent(DeviceConfigRequestBase requestBase, DeviceDetails deviceDetails)
        {
			_loggingService.Info("PL Device Type doesn't support SpeedingThresholds", "SpeedingThresholdMessageEventBuilder.GetPlOutMessageEvent");
            throw new NotImplementedException("PL Device Type doesn't support SpeedingThresholds");
        }

        public IEnumerable<Tuple<IOutMessageEvent, DeviceDetails>> GetDataOutMessageEvent(DeviceConfigRequestBase requestBase, DeviceDetails deviceDetails)
        {
            var dataOutMessages = new List<Tuple<IOutMessageEvent, DeviceDetails>>();
			_loggingService.Info("Recieved SpeedingThresholds Message for A5N2 Device " + deviceDetails.DeviceUid, "SpeedingThresholdMessageEventBuilder.GetDataOutMessageEvent");
            var requestMessage = _dataPopulator.GetRequestModel<DeviceConfigSpeedingThresholdsRequest>(requestBase);
            if (_validator.NullCheck(RequestMessageType, requestMessage.SpeedThresholdEnabled.Value))
            {
                var messageEvent = _dataPopulator.ConstructDataOutEvent<MovingCriteriaConfigurationChangedEvent>(deviceDetails);
                if (requestMessage.SpeedThresholdEnabled.Value)
                {
                    messageEvent.MovementDurationSeconds = requestMessage.SpeedThresholdDuration.HasValue ? requestMessage.SpeedThresholdDuration.Value : 3600;
                    messageEvent.MovementSpeedMPH = requestMessage.SpeedThreshold.HasValue ? NumericHelper.ConvertKilometersToMiles(requestMessage.SpeedThreshold.Value) : 150;
                }
                dataOutMessages.Add(new Tuple<IOutMessageEvent, DeviceDetails>(messageEvent, deviceDetails));
            }
			_loggingService.Info("Speeding Threshold Message Event Construction for Device Type " + deviceDetails.DeviceType + " completed !!" + JsonConvert.SerializeObject(requestMessage), "SpeedingThresholdMessageEventBuilder.GetDataOutMessageEvent");
            return dataOutMessages;
        }

        public IEnumerable<Tuple<IMTSOutMessageEvent, DeviceDetails>> GetMtsOutMessageEvent(DeviceConfigRequestBase requestBase, DeviceDetails deviceDetails)
        {
            var mtsOutMessages = new List<Tuple<IMTSOutMessageEvent, DeviceDetails>>();
			_loggingService.Info("Recieved SpeedingThresholds Message for MTS Device " + deviceDetails.DeviceUid, "SpeedingThresholdMessageEventBuilder.GetMtsOutMessageEvent");
            var requestMessage = _dataPopulator.GetRequestModel<DeviceConfigSpeedingThresholdsRequest>(requestBase);
            if (_validator.NullCheck(RequestMessageType, requestMessage.SpeedThresholdEnabled.Value))
            {
                var mtsMessageEvent = _dataPopulator.ConstructMtsEvent<SetSpeedingThresholdEvent>(deviceDetails);
                if (requestMessage.SpeedThresholdEnabled.Value)
                {
                    if (requestMessage.SpeedThresholdDuration.HasValue)
                        mtsMessageEvent.Duration = requestMessage.SpeedThresholdDuration.Value;
                    if (requestMessage.SpeedThreshold.HasValue)
                        mtsMessageEvent.Threshold = NumericHelper.ConvertKilometersToMiles(requestMessage.SpeedThreshold.Value);
                }
                mtsMessageEvent.Enabled = requestMessage.SpeedThresholdEnabled.Value;
                mtsOutMessages.Add(new Tuple<IMTSOutMessageEvent, DeviceDetails>(mtsMessageEvent, deviceDetails));
            }
			_loggingService.Info("Speeding Threshold Message Event Construction for Device Type " + deviceDetails.DeviceType + " completed !!" + JsonConvert.SerializeObject(requestMessage), "SpeedingThresholdMessageEventBuilder.GetMtsOutMessageEvent");
            return mtsOutMessages;
        }
    }
}
