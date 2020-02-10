using ClientModel.DeviceConfig.Request.DeviceConfig;
using ClientModel.DeviceConfig.Request.DeviceConfig.Meters;
using CommonModel.DeviceSettings;
using Infrastructure.Common.DeviceMessageConstructor.Attributes;
using Infrastructure.Common.DeviceMessageConstructor.Interfaces;
using CommonModel.DeviceSettings;
using Infrastructure.Service.DeviceMessageConstructor.Helpers;
using Infrastructure.Service.DeviceMessageConstructor.Interfaces;
using Infrastructure.Service.DeviceMessagePublisher;
using Infrastructure.Service.DeviceMessagePublisher.Interfaces;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Utilities.Logging;
using VSS.MasterData.WebAPI.Transactions;
using VSS.VisionLink.Interfaces.Events.Commands.A5N2;
using VSS.VisionLink.Interfaces.Events.Commands.Interfaces;
using VSS.VisionLink.Interfaces.Events.Commands.Models;
using VSS.VisionLink.Interfaces.Events.Commands.MTS;
using VSS.VisionLink.Interfaces.Events.Commands.PL;
using Infrastructure.Common.DeviceMessageConstructor.Models;

namespace Infrastructure.Service.DeviceMessageConstructor.Implementations
{
	[Group("Meters")]
	public class MetersMessageEventBuilder : IGroupMessageEventBuilder
	{
		private readonly ILoggingService _loggingService;
		private readonly IDataPopulator _dataPopulator;
		private readonly IDataValidator _validator;
		private const string RequestMessageType = "DeviceConfigMetersRequest";
		private List<string> _serverSideRunTimeCalibrationDeviceTypes;
		private List<string> _newlySupportedServerSideRunTimeCalibrationDeviceTypes;

		private IDeviceMessageKafkaPublisher _kafkaPublisher;

		public MetersMessageEventBuilder(IDataPopulator dataPopulator, IDataValidator validator, ILoggingService loggingService)
		{
			_dataPopulator = dataPopulator;
			_validator = validator;
			//_serverSideRunTimeCalibrationDeviceTypes = String.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["ServerSideRunTimeCalibrationDeviceTypes"]) ? new List<string>() :
			//	ConfigurationManager.AppSettings["ServerSideRunTimeCalibrationDeviceTypes"].Split(',').ToList();
			//_newlySupportedServerSideRunTimeCalibrationDeviceTypes = String.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["NewlySupportedRunTimeCalibrationDeviceTypes"]) ? new List<string>() :
			//	ConfigurationManager.AppSettings["NewlySupportedRunTimeCalibrationDeviceTypes"].Split(',').ToList();
			_loggingService = loggingService;
			//_kafkaPublisher = new DeviceMessageKafkaPublisher(configuration.Value, transaction);
		}

		public IEnumerable<Tuple<IPLOutMessageEvent, DeviceDetails>> GetPlOutMessageEvent(DeviceConfigRequestBase requestBase, DeviceDetails deviceDetails)
		{
			var dataOutMessages = new List<Tuple<IPLOutMessageEvent, DeviceDetails>>();
			var requestMessage = _dataPopulator.GetRequestModel<DeviceConfigMetersRequest>(requestBase);
			_loggingService.Info("Recieved SendRuntimeAdjustmentConfig Message" + requestMessage + " for Device " + deviceDetails.DeviceUid + " and DeviceType" + deviceDetails.DeviceType, "MetersMessageEventBuilder.GetPlOutMessageEvent");
			if (_validator.NullCheck(RequestMessageType, requestMessage.HoursMeter.ProposedValue))
			{
				var plMessageEvent = _dataPopulator.ConstructPlEvent<SendRuntimeAdjustmentConfig>(deviceDetails);
				plMessageEvent.NewRuntimeValue = new TimeSpan((int)requestMessage.HoursMeter.ProposedValue, 0, 0);
				dataOutMessages.Add(new Tuple<IPLOutMessageEvent, DeviceDetails>(plMessageEvent, deviceDetails));
			}
			_loggingService.Info("SendRuntimeAdjustmentConfig message Event Construction for Device" + deviceDetails + " completed !!" + JsonConvert.SerializeObject(requestBase), "");
			return dataOutMessages;
		}

		public IEnumerable<Tuple<IOutMessageEvent, DeviceDetails>> GetDataOutMessageEvent(DeviceConfigRequestBase requestBase, DeviceDetails deviceDetails)
		{
			var dataOutMessages = new List<Tuple<IOutMessageEvent, DeviceDetails>>();
			_loggingService.Info("Recieved MovingThresholds Message for PL Device " + deviceDetails.DeviceUid, "MetersMessageEventBuilder.GetPlOutMessageEvent");
			var requestMessage = _dataPopulator.GetRequestModel<DeviceConfigMetersRequest>(requestBase);
			if (requestMessage.OdoMeter != null)
			{
				if (requestMessage.OdoMeter.ProposedValue.HasValue)
				{
					_loggingService.Info("Recieved OdometerModifiedEvent Message" + requestMessage + " for Device " + deviceDetails.DeviceUid + " and DeviceType" + deviceDetails.DeviceType, "MetersMessageEventBuilder.GetPlOutMessageEvent");
					var odometerModifiedEvent = _dataPopulator.ConstructDataOutEvent<OdometerModifiedEvent>(deviceDetails);
					if (requestMessage.OdoMeter.CurrentValue.HasValue)
						odometerModifiedEvent.MilesBefore = NumericHelper.ConvertKilometersToMiles(requestMessage.OdoMeter.CurrentValue.Value);
					odometerModifiedEvent.MilesAfter = NumericHelper.ConvertKilometersToMiles(requestMessage.OdoMeter.ProposedValue.Value);
					var dvcDetails = ConstructorHelpers.GetDeviceConfigMsg(deviceDetails, "Odometer");
					dataOutMessages.Add(new Tuple<IOutMessageEvent, DeviceDetails>(odometerModifiedEvent, dvcDetails));
					_loggingService.Info("OdometerModifiedEvent message Event Construction for Device" + deviceDetails.DeviceUid + " completed !!" + JsonConvert.SerializeObject(requestBase), "MetersMessageEventBuilder.GetPlOutMessageEvent");

				}
				if (requestMessage.HoursMeter != null)
				{
					if (requestMessage.HoursMeter.ProposedValue.HasValue)
					{
						_loggingService.Info("Recieved HourMeterModifiedEvent Message" + requestMessage + " for Device " + deviceDetails.DeviceUid + " and DeviceType" + deviceDetails.DeviceType, "MetersMessageEventBuilder.GetPlOutMessageEvent");
						var hourmeterModifiedEvent = _dataPopulator.ConstructDataOutEvent<HourMeterModifiedEvent>(deviceDetails);
						if (requestMessage.HoursMeter.CurrentValue.HasValue)
							hourmeterModifiedEvent.HoursBefore = requestMessage.HoursMeter.CurrentValue.Value;
						hourmeterModifiedEvent.HoursAfter = Convert.ToDouble(requestMessage.HoursMeter.ProposedValue);
						var dvcDetails = ConstructorHelpers.GetDeviceConfigMsg(deviceDetails, "HoursMeter");
						dataOutMessages.Add(new Tuple<IOutMessageEvent, DeviceDetails>(hourmeterModifiedEvent, dvcDetails));
						_loggingService.Info("HourMeterModifiedEvent message Event Construction for Device" + deviceDetails.DeviceUid + " completed !!" + JsonConvert.SerializeObject(requestBase), "MetersMessageEventBuilder.GetPlOutMessageEvent");
					}
				}
			}
			return dataOutMessages;
		}

		public IEnumerable<Tuple<IMTSOutMessageEvent, DeviceDetails>> GetMtsOutMessageEvent(DeviceConfigRequestBase requestBase, DeviceDetails deviceDetails)
		{
			var mtsOutMessages = new List<Tuple<IMTSOutMessageEvent, DeviceDetails>>();
			var requestMessage = _dataPopulator.GetRequestModel<DeviceConfigMetersRequest>(requestBase);

			//if (_serverSideRunTimeCalibrationDeviceTypes.Contains(deviceDetails.DeviceType) && requestMessage.HoursMeter != null && requestMessage.HoursMeter.ProposedValue.HasValue)
			//{
			//	_kafkaPublisher.PublishMessage(deviceDetails.SerialNumber, new List<MTSServerSideRunTimeCalibration> { new MTSServerSideRunTimeCalibration { DeviceSerialNumber = deviceDetails.SerialNumber, DeviceType = deviceDetails.DeviceType, ProposedRunTimeHours = requestMessage.HoursMeter.ProposedValue.Value, IsDeleted = false, ActionUtc = DateTime.UtcNow } }, String.Empty);
			//	_loggingService.Info($"Published MTSServerSideRunTimeCalibration Event for Device SerialNUmber : {deviceDetails.SerialNumber} with RunTimeHours {requestMessage.HoursMeter.ProposedValue.Value}", "MetersMessageEventBuilder.GetMtsOutMessageEvent");
			//}
			//else if (_newlySupportedServerSideRunTimeCalibrationDeviceTypes.Contains(deviceDetails.DeviceType) && requestMessage.HoursMeter != null && requestMessage.HoursMeter.ProposedValue.HasValue)
			//{
			//	_kafkaPublisher.PublishMessage(deviceDetails.SerialNumber, new List<MTSServerSideRunTimeCalibration> { new MTSServerSideRunTimeCalibration { DeviceSerialNumber = deviceDetails.SerialNumber, DeviceType = deviceDetails.DeviceType, IsDeleted = true, ActionUtc = DateTime.UtcNow } }, String.Empty);
			//	_loggingService.Info($"Published Delete MTSServerSideRunTimeCalibration Event for Device SerialNUmber : {deviceDetails.SerialNumber} with RunTimeHours {requestMessage.HoursMeter.ProposedValue.Value}", "MetersMessageEventBuilder.GetMtsOutMessageEvent");
			//}

			if ((requestMessage.OdoMeter != null) || (requestMessage.HoursMeter != null))
			{
				_loggingService.Info("Recieved SetRuntimeMileageEvent Message" + requestMessage + " for Device " + deviceDetails.DeviceUid + " and DeviceType" + deviceDetails.DeviceType, "MetersMessageEventBuilder.GetMtsOutMessageEvent");
				var mtsMessageEvent = _dataPopulator.ConstructMtsEvent<SetRuntimeMileageEvent>(deviceDetails);
				if (requestMessage.OdoMeter != null)
				{
					if (requestMessage.OdoMeter.ProposedValue.HasValue || requestMessage.OdoMeter.CurrentValue.HasValue)
						mtsMessageEvent.Mileage = requestMessage.OdoMeter.ProposedValue.HasValue ? NumericHelper.ConvertKilometersToMiles(requestMessage.OdoMeter.ProposedValue.Value) : NumericHelper.ConvertKilometersToMiles(requestMessage.OdoMeter.CurrentValue.Value);
				}
				if (requestMessage.HoursMeter != null)
				{
					if (requestMessage.HoursMeter.ProposedValue.HasValue || requestMessage.HoursMeter.CurrentValue.HasValue)
						mtsMessageEvent.Runtime = requestMessage.HoursMeter.ProposedValue.HasValue ? Convert.ToInt64(requestMessage.HoursMeter.ProposedValue) : Convert.ToInt64(requestMessage.HoursMeter.CurrentValue);
				}
				var dvcDetails = ConstructorHelpers.GetDeviceConfigMsg(deviceDetails, "Odometer", "HoursMeter");
				if (mtsMessageEvent != null)
					mtsOutMessages.Add(new Tuple<IMTSOutMessageEvent, DeviceDetails>(mtsMessageEvent, dvcDetails));
				_loggingService.Info("SetRuntimeMileageEvent message Event Construction for Device" + deviceDetails.DeviceUid + " completed !!" + JsonConvert.SerializeObject(requestBase), "MetersMessageEventBuilder.GetMtsOutMessageEvent");
			}

			if (requestMessage.HoursMeter != null)
			{
				if (requestMessage.HoursMeter.ProposedValue.HasValue || requestMessage.HoursMeter.CurrentValue.HasValue)
				{
					_loggingService.Info("Recieved SendOtaConfigurationEvent Message" + requestMessage + " for Device " + deviceDetails.DeviceUid + " and DeviceType" + deviceDetails.DeviceType, "MetersMessageEventBuilder.GetMtsOutMessageEvent");
					var mtsOtaMessageEvent = _dataPopulator.ConstructMtsEvent<SendOtaConfigurationEvent>(deviceDetails);
					mtsOtaMessageEvent.Smu = TimeSpan.FromHours((double)(requestMessage.HoursMeter.ProposedValue.HasValue ? requestMessage.HoursMeter.ProposedValue : requestMessage.HoursMeter.CurrentValue));
					mtsOtaMessageEvent.Input1 = new OtaConfigDetail();
					mtsOtaMessageEvent.Input2 = new OtaConfigDetail();
					mtsOtaMessageEvent.Input3 = new OtaConfigDetail();
					mtsOtaMessageEvent.Input4 = new OtaConfigDetail();
					var dvcDetails = ConstructorHelpers.GetDeviceConfigMsg(deviceDetails, "HoursMeter");
					mtsOutMessages.Add(new Tuple<IMTSOutMessageEvent, DeviceDetails>(mtsOtaMessageEvent, dvcDetails));
					_loggingService.Info("SendOtaConfigurationEvent message Event Construction for Device" + deviceDetails.DeviceUid + " completed !!" + JsonConvert.SerializeObject(requestBase), "MetersMessageEventBuilder.GetMtsOutMessageEvent");
				}
			}

			if (requestMessage.SmhOdometerConfig != null)
			{
				_loggingService.Info("Recieved SetMachineEventHeaderConfiguration Message" + requestMessage + " for Device " + deviceDetails.DeviceUid + " and DeviceType" + deviceDetails.DeviceType, "MetersMessageEventBuilder.GetMtsOutMessageEvent");
				var mtsMessageEvent = _dataPopulator.ConstructMtsEvent<SetMachineEventHeaderConfiguration>(deviceDetails);
				mtsMessageEvent.PrimaryDataSource = _dataPopulator.GetEventEnumValue<PrimaryDataSourceEnum>(requestMessage.SmhOdometerConfig.Value.ToString());
				var dvcDetails = ConstructorHelpers.GetDeviceConfigMsg(deviceDetails, "SMHOdometerConfig");
				mtsOutMessages.Add(new Tuple<IMTSOutMessageEvent, DeviceDetails>(mtsMessageEvent, dvcDetails));
				_loggingService.Info("SetMachineEventHeaderConfiguration message Event Construction for Device" + deviceDetails.DeviceUid + " completed !!" + JsonConvert.SerializeObject(requestBase), "MetersMessageEventBuilder.GetMtsOutMessageEvent");
			}
			return mtsOutMessages;
		}
	}
}
