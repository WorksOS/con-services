using ClientModel.DeviceConfig.Request.DeviceConfig;
using ClientModel.DeviceConfig.Request.DeviceConfig.Switches;
using Infrastructure.Common.DeviceMessageConstructor.Attributes;
using Infrastructure.Common.DeviceMessageConstructor.Interfaces;
using CommonModel.DeviceSettings;
using Infrastructure.Service.DeviceMessageConstructor.Helpers;
using Infrastructure.Service.DeviceMessageConstructor.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities.Logging;
using VSS.VisionLink.Interfaces.Events.Commands.A5N2;
using VSS.VisionLink.Interfaces.Events.Commands.Interfaces;
using VSS.VisionLink.Interfaces.Events.Commands.Models;
using VSS.VisionLink.Interfaces.Events.Commands.MTS;
using VSS.VisionLink.Interfaces.Events.Commands.PL;

namespace Infrastructure.Service.DeviceMessageConstructor.Implementations
{
	[Group("Switches")]
    public class SwitchesMessageEventBuilder : IGroupMessageEventBuilder
    {
        private static ILoggingService _loggingService;
        private readonly IDataPopulator _dataPopulator;
        private readonly IDataValidator _validator;
        private const string RequestMessageType = "DeviceConfigSwitchesRequest";

        public SwitchesMessageEventBuilder(IDataPopulator dataPopulator, IDataValidator validator, ILoggingService loggingService)
        {
            _dataPopulator = dataPopulator;
            _validator = validator;
			_loggingService = loggingService;
		}

        public IEnumerable<Tuple<IPLOutMessageEvent, DeviceDetails>> GetPlOutMessageEvent(DeviceConfigRequestBase requestBase, DeviceDetails deviceDetails)
        {
            var plOutMessages = new List<Tuple<IPLOutMessageEvent, DeviceDetails>>();
            var digitalConfigDetails = new List<DigitalInputConfigDetails>();
			_loggingService.Info("Recieved Switches Message for Device " + deviceDetails.DeviceType, "SwitchesMessageEventBuilder.GetPlOutMessageEvent");
            var requestMessage = _dataPopulator.GetRequestModel<DeviceConfigSwitchesRequest>(requestBase);
            if (requestMessage.SingleStateSwitches != null && requestMessage.SingleStateSwitches.Any())
            {
                foreach (var singleSwitch in requestMessage.SingleStateSwitches)
                {
                    var digitalInputConfigDetail = new DigitalInputConfigDetails();
                    digitalInputConfigDetail.InputConfig = string.IsNullOrEmpty(singleSwitch.SwitchActiveState) ? (InputConfig?)null : _dataPopulator.GetEventEnumValue<InputConfig>(singleSwitch.SwitchActiveState);
                    digitalInputConfigDetail.DigitalInputMonitoringCondition = string.IsNullOrEmpty(singleSwitch.SwitchMonitoringStatus) ? (DigitalInputMonitoringConditions?)null : _dataPopulator.GetEventEnumValue<DigitalInputMonitoringConditions>(singleSwitch.SwitchMonitoringStatus);
                    digitalInputConfigDetail.Description = singleSwitch.SwitchName;
                    digitalInputConfigDetail.InputDelayTime = new TimeSpan(0, 0, 0, 0, (int)Math.Round(singleSwitch.SwitchSensitivity * 1000));
                    digitalConfigDetails.Add(digitalInputConfigDetail);
                }
                var plMessageEvent = _dataPopulator.ConstructPlEvent<SendDigitalInputConfig>(deviceDetails);
                plMessageEvent.Config1 = digitalConfigDetails[0];
                plMessageEvent.Config2 = digitalConfigDetails.Count >= 2 ? digitalConfigDetails[1] : new DigitalInputConfigDetails();
                plMessageEvent.Config3 = digitalConfigDetails.Count >= 3 ? digitalConfigDetails[2] : new DigitalInputConfigDetails();
                plMessageEvent.Config4 = digitalConfigDetails.Count >= 4 ? digitalConfigDetails[3] : new DigitalInputConfigDetails();
                plOutMessages.Add(new Tuple<IPLOutMessageEvent, DeviceDetails>(plMessageEvent, deviceDetails));
            }
			_loggingService.Info("Switches Message Event Construction for Device Type " + deviceDetails.DeviceType + " completed !!" + JsonConvert.SerializeObject(requestMessage), "SwitchesMessageEventBuilder.GetPlOutMessageEvent");
            return plOutMessages;
        }

        public IEnumerable<Tuple<IOutMessageEvent, DeviceDetails>> GetDataOutMessageEvent(DeviceConfigRequestBase requestBase, DeviceDetails deviceDetails)
        {
            var dataOutMessages = new List<Tuple<IOutMessageEvent, DeviceDetails>>();
			_loggingService.Info("Recieved Switches Message for Device " + deviceDetails.DeviceType, "SwitchesMessageEventBuilder.GetDataOutMessageEvent");
            var requestMessage = _dataPopulator.GetRequestModel<DeviceConfigSwitchesRequest>(requestBase);
            if (requestMessage.SingleStateSwitches != null)
            {
                foreach (var singleSwitch in requestMessage.SingleStateSwitches)
                {
                    if (_validator.NullCheck(RequestMessageType, singleSwitch.SwitchActiveState, singleSwitch.SwitchMonitoringStatus))
                    {
                        var dataOutMessageEvent = _dataPopulator.ConstructDataOutEvent<DigitalSwitchConfigurationEvent>(deviceDetails);
                        dataOutMessageEvent.DefaultState = _dataPopulator.GetEventEnumValue<SwitchState>(singleSwitch.SwitchActiveState);
                        dataOutMessageEvent.MonitoredWhen = _dataPopulator.GetEventEnumValue<DigitalInputMonitoringConditions>(singleSwitch.SwitchMonitoringStatus);
                        dataOutMessageEvent.Sensitivity = singleSwitch.SwitchSensitivity;
                        dataOutMessageEvent.SwitchNumber = singleSwitch.SwitchNumber;
                        dataOutMessageEvent.SwitchOnDescription = singleSwitch.SwitchName;
                        var dvcDetails = ConstructorHelpers.GetDeviceConfigMsg(deviceDetails, singleSwitch.SwitchParameterName);
                        dataOutMessages.Add(new Tuple<IOutMessageEvent, DeviceDetails>(dataOutMessageEvent, dvcDetails));
                    }
                }
            }
            if (requestMessage.DualStateSwitches != null)
            {
                foreach (var dualSwitch in requestMessage.DualStateSwitches)
                {
                    var dataOutMessageEvent = _dataPopulator.ConstructDataOutEvent<DiscreteInputConfigurationEvent>(deviceDetails);
                    dataOutMessageEvent.Name = dualSwitch.SwitchName;
                    dataOutMessageEvent.MonitoredWhen = string.IsNullOrEmpty(dualSwitch.SwitchMonitoringStatus) ? (DigitalInputMonitoringConditions?)null : _dataPopulator.GetEventEnumValue<DigitalInputMonitoringConditions>(dualSwitch.SwitchMonitoringStatus);
                    dataOutMessageEvent.Sensitivity = dualSwitch.SwitchSensitivity;
                    dataOutMessageEvent.SwitchNumber = dualSwitch.SwitchNumber;
                    dataOutMessageEvent.OpenDescription = dualSwitch.SwitchOpen;
                    dataOutMessageEvent.ClosedDescription = dualSwitch.SwitchClosed;
                    dataOutMessageEvent.Enabled = dualSwitch.SwitchEnabled;
                    var dvcDetails = ConstructorHelpers.GetDeviceConfigMsg(deviceDetails, dualSwitch.SwitchParameterName);
                    dataOutMessages.Add(new Tuple<IOutMessageEvent, DeviceDetails>(dataOutMessageEvent, dvcDetails));
                }
            }
			_loggingService.Info("Switches Message Event Construction for Device Type " + deviceDetails.DeviceType + " completed !!" + JsonConvert.SerializeObject(requestMessage), "SwitchesMessageEventBuilder.GetDataOutMessageEvent");
            return dataOutMessages;
        }

        public IEnumerable<Tuple<IMTSOutMessageEvent, DeviceDetails>> GetMtsOutMessageEvent(DeviceConfigRequestBase requestBase, DeviceDetails deviceDetails)
        {
            var mtsOutMessages = new List<Tuple<IMTSOutMessageEvent, DeviceDetails>>();
            var otaConfigDetails = new Dictionary<int,OtaConfigDetail>(4);
			_loggingService.Info("Recieved Switches Message for Device " + deviceDetails.DeviceType, "SwitchesMessageEventBuilder.GetMtsOutMessageEvent");
            var requestMessage = _dataPopulator.GetRequestModel<DeviceConfigSwitchesRequest>(requestBase);
            if (requestMessage.SingleStateSwitches != null && requestMessage.SingleStateSwitches.Any())
            {
                foreach (var singleSwitch in requestMessage.SingleStateSwitches)
                {
                    var otaInputConfigDetail = new OtaConfigDetail();
					otaInputConfigDetail.InputConfig = string.IsNullOrEmpty(singleSwitch.SwitchActiveState) ? (InputConfigType?)null : _dataPopulator.GetEventEnumValue<InputConfigType>(singleSwitch.SwitchActiveState);
					otaInputConfigDetail.MonitoringCondition = string.IsNullOrEmpty(singleSwitch.SwitchMonitoringStatus) ? (DigitalInputMonitoringConditions?)null : _dataPopulator.GetEventEnumValue<DigitalInputMonitoringConditions>(singleSwitch.SwitchMonitoringStatus);
                    otaInputConfigDetail.InputDesc = singleSwitch.SwitchName;
                    otaInputConfigDetail.InputDelay = new TimeSpan(0, 0, 0, 0, (int)Math.Round(singleSwitch.SwitchSensitivity * 1000));
                    otaConfigDetails.Add(singleSwitch.SwitchNumber, otaInputConfigDetail);
                }
                var mtsMessageEvent = _dataPopulator.ConstructMtsEvent<SendOtaConfigurationEvent>(deviceDetails);
				mtsMessageEvent.Input1 = otaConfigDetails.ContainsKey(1) ? otaConfigDetails[1] : new OtaConfigDetail();
                mtsMessageEvent.Input2 = otaConfigDetails.ContainsKey(2) ? otaConfigDetails[2] : new OtaConfigDetail();
				mtsMessageEvent.Input3 = otaConfigDetails.ContainsKey(3) ? otaConfigDetails[3] : new OtaConfigDetail();
				mtsMessageEvent.Input4 = otaConfigDetails.ContainsKey(4) ? otaConfigDetails[4] : new OtaConfigDetail();
				var dvcDetails = ConstructorHelpers.GetDeviceConfigMsg(deviceDetails, "SingleStateSwitch1", "SingleStateSwitch2", "SingleStateSwitch3", "SingleStateSwitch4");
                mtsOutMessages.Add(new Tuple<IMTSOutMessageEvent, DeviceDetails>(mtsMessageEvent, dvcDetails));
            }
            var sensorDetails = new Dictionary<int,SensorDetail>(3);
            if (requestMessage.DualStateSwitches != null && requestMessage.DualStateSwitches.Any())
            {
				var mtsMessageEvent = _dataPopulator.ConstructMtsEvent<ConfigureSensorsEvent>(deviceDetails);

				foreach (var dualSwitch in requestMessage.DualStateSwitches)
                {
                    var sensorDetail = new SensorDetail();
                    sensorDetail.HasPosPolarity = false;
                    sensorDetail.IgnReqired = false;
                    sensorDetail.HystHalfSec = dualSwitch.SwitchSensitivity * 2;
                    sensorDetail.Enabled = dualSwitch.SwitchEnabled;
                    sensorDetails.Add(dualSwitch.SwitchNumber, sensorDetail);
                }
				mtsMessageEvent.Sensor1 = sensorDetails.ContainsKey(5) ? sensorDetails[5] : null;
				mtsMessageEvent.Sensor2 = sensorDetails.ContainsKey(6) ? sensorDetails[6] : null;
				mtsMessageEvent.Sensor3 = sensorDetails.ContainsKey(7) ? sensorDetails[7] : null;
				var dvcDetails = ConstructorHelpers.GetDeviceConfigMsg(deviceDetails, "MTSDualStateSwitch5", "MTSDualStateSwitch6", "MTSDualStateSwitch7");
                mtsOutMessages.Add(new Tuple<IMTSOutMessageEvent, DeviceDetails>(mtsMessageEvent, dvcDetails));
            }
			_loggingService.Info("Switches Message Event Construction for Device Type " + deviceDetails.DeviceType + " completed !!" + JsonConvert.SerializeObject(requestMessage), "SwitchesMessageEventBuilder.GetMtsOutMessageEvent");

			return mtsOutMessages;
        }
    }
}
