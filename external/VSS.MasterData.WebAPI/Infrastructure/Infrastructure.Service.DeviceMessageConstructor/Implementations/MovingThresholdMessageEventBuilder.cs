using ClientModel.DeviceConfig.Request.DeviceConfig;
using ClientModel.DeviceConfig.Request.DeviceConfig.MovingThresold;
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
	[Group("MovingThresholds")]
    public class MovingThresholdMessageEventBuilder : IGroupMessageEventBuilder
    {
        private readonly ILoggingService _loggingService;
        private readonly IDataPopulator _dataPopulator;
        private readonly IDataValidator _validator;
        private const string RequestMessageType = "DeviceConfigMovingThresholdRequest";

        public MovingThresholdMessageEventBuilder(IDataPopulator dataPopulator, IDataValidator validator, ILoggingService loggingService)
        {
            _dataPopulator = dataPopulator;
            _validator = validator;
			_loggingService = loggingService;
		}
        public IEnumerable<Tuple<IPLOutMessageEvent, DeviceDetails>> GetPlOutMessageEvent(DeviceConfigRequestBase requestBase, DeviceDetails deviceDetails)
        {
			_loggingService.Info("PL Device Type doesn't support MovingThresholds", "MovingThresholdMessageEventBuilder.GetDataOutMessageEvent");
            throw new NotImplementedException("PL Device Type doesn't support MovingThresholds");
        }

        public IEnumerable<Tuple<IOutMessageEvent, DeviceDetails>> GetDataOutMessageEvent(DeviceConfigRequestBase requestBase, DeviceDetails deviceDetails)
        {
            var dataOutMessages = new List<Tuple<IOutMessageEvent, DeviceDetails>>();
			_loggingService.Info("Recieved MovingThresholds Message for A5N2 Device " + deviceDetails.DeviceUid, "MovingThresholdMessageEventBuilder.GetDataOutMessageEvent");
            var requestMessage = _dataPopulator.GetRequestModel<DeviceConfigMovingThresholdRequest>(requestBase);
            var messageEvent = _dataPopulator.ConstructDataOutEvent<MovingCriteriaConfigurationChangedEvent>(deviceDetails);
            messageEvent.MovementDurationSeconds = requestMessage.MovingThresholdsDuration.HasValue ? requestMessage.MovingThresholdsDuration.Value : 30;
            messageEvent.MovementRadiusInFeet = requestMessage.Radius.HasValue ? NumericHelper.ConvertMetersToFeet(requestMessage.Radius.Value) : 30;
            messageEvent.MovementSpeedMPH = requestMessage.MovingOrStoppedThreshold.HasValue ? NumericHelper.ConvertKilometersToMiles((double)requestMessage.MovingOrStoppedThreshold.Value) : 0.2;
            dataOutMessages.Add(new Tuple<IOutMessageEvent, DeviceDetails>(messageEvent, deviceDetails));
			_loggingService.Info("Moving Thresholds Message Event Construction for Device Type " + deviceDetails.DeviceType + " completed !!" + JsonConvert.SerializeObject(requestMessage), "MovingThresholdMessageEventBuilder.GetDataOutMessageEvent");
            return dataOutMessages;
        }

        public IEnumerable<Tuple<IMTSOutMessageEvent, DeviceDetails>> GetMtsOutMessageEvent(DeviceConfigRequestBase requestBase, DeviceDetails deviceDetails)
        {
            var mtsOutMessages = new List<Tuple<IMTSOutMessageEvent, DeviceDetails>>();
			_loggingService.Info("Recieved MovingThresholds Message for MTS Device " + deviceDetails.DeviceUid, "MovingThresholdMessageEventBuilder.GetMtsOutMessageEvent");
            var requestMessage = _dataPopulator.GetRequestModel<DeviceConfigMovingThresholdRequest>(requestBase);

            if (requestMessage.Radius.HasValue)
            {
                var mtsMessageEvent = _dataPopulator.ConstructMtsEvent<SetMovingConfigurationEvent>(deviceDetails);
                mtsMessageEvent.Radius = (ushort)NumericHelper.ConvertMetersToFeet(requestMessage.Radius.Value);
                var dvcDetails = ConstructorHelpers.GetDeviceConfigMsg(deviceDetails, "MovingThresholdsRadius");
                mtsOutMessages.Add(new Tuple<IMTSOutMessageEvent, DeviceDetails>(mtsMessageEvent, dvcDetails));
            }

            if (requestMessage.MovingOrStoppedThreshold.HasValue || requestMessage.MovingThresholdsDuration.HasValue)
            {
                var mtsMessageEvent = _dataPopulator.ConstructMtsEvent<SetStoppedThresholdEvent>(deviceDetails);
                var parameterNames = new List<string>();
                if (requestMessage.MovingOrStoppedThreshold.HasValue)
                {
                    mtsMessageEvent.Threshold = NumericHelper.ConvertKilometersToMiles((double)requestMessage.MovingOrStoppedThreshold.Value);
                    parameterNames.Add("MovingOrStoppedThreshold");
                }
                if (requestMessage.MovingThresholdsDuration.HasValue)
                {
                    mtsMessageEvent.Duration = requestMessage.MovingThresholdsDuration.Value;
                    parameterNames.Add("MovingThresholdsDuration");
                }
                mtsMessageEvent.Enabled = true; //doubt
                var dvcDetails = ConstructorHelpers.GetDeviceConfigMsg(deviceDetails, parameterNames.ToArray());
                mtsOutMessages.Add(new Tuple<IMTSOutMessageEvent, DeviceDetails>(mtsMessageEvent, dvcDetails));
            }
			_loggingService.Info("Moving Thresholds Message Event Construction for Device Type " + deviceDetails.DeviceType + " completed !!" + JsonConvert.SerializeObject(requestMessage), "MovingThresholdMessageEventBuilder.GetMtsOutMessageEvent");

            return mtsOutMessages;
        }
    }
}
