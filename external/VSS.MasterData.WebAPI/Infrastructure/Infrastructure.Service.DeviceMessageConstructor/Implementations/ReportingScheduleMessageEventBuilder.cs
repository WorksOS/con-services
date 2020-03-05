using ClientModel.DeviceConfig.Request.DeviceConfig;
using ClientModel.DeviceConfig.Request.DeviceConfig.ReportingSchedule;
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
using VSS.VisionLink.Interfaces.Events.Commands.Models;
using VSS.VisionLink.Interfaces.Events.Commands.MTS;
using VSS.VisionLink.Interfaces.Events.Commands.PL;

namespace Infrastructure.Service.DeviceMessageConstructor.Implementations
{
	[Group("ReportingSchedule")]
    public class ReportingScheduleMessageEventBuilder : IGroupMessageEventBuilder
    {
        private static ILoggingService _loggingService;
        private readonly IDataPopulator _dataPopulator;
        private readonly IDataValidator _validator;
        private const string RequestMessageType = "DeviceConfigReportingScheduleRequest";

        public ReportingScheduleMessageEventBuilder(IDataPopulator dataPopulator, IDataValidator validator, ILoggingService loggingService)
        {
            _dataPopulator = dataPopulator;
            _validator = validator;
			_loggingService = loggingService;
		}
        public IEnumerable<Tuple<IPLOutMessageEvent, DeviceDetails>> GetPlOutMessageEvent(DeviceConfigRequestBase requestBase, DeviceDetails deviceDetails)
        {
            var plOutMessages = new List<Tuple<IPLOutMessageEvent, DeviceDetails>>();
			_loggingService.Info("Recieved ReportingSchedule Message for PL Device " + deviceDetails.DeviceUid, "ReportingScheduleMessageEventBuilder.GetPlOutMessageEvent");
            var requestMessage = _dataPopulator.GetRequestModel<DeviceConfigReportingScheduleRequest>(requestBase);
            if (deviceDetails.DeviceType == "PL121")
            {
                if (requestMessage.DailyReportingTime != null || requestMessage.DailyLocationReportingFrequency != null || requestMessage.GlobalGram != null)
                {
                    var messageEvent = _dataPopulator.ConstructPlEvent<SendReportIntervalsConfig>(deviceDetails);
                    messageEvent.EventIntervals = (TimeSpan?)null;
                    messageEvent.Level1TransmissionFrequency = (EventFrequency?)null;
                    messageEvent.Level2TransmissionFrequency = (EventFrequency?)null;
                    messageEvent.Level3TransmissionFrequency = (EventFrequency?)null;
                    messageEvent.NextMessageInterval = (TimeSpan?)null;
                    messageEvent.GlobalGramEnable = requestMessage.GlobalGram.HasValue ? requestMessage.GlobalGram.Value : (bool?)null;
                    messageEvent.ReportStartTimeUTC = requestMessage.DailyReportingTime.HasValue ? new DateTime() + requestMessage.DailyReportingTime.Value : (DateTime?)null; //doubt
                    messageEvent.DiagnosticTransmissionFrequency = (EventFrequency?)null;
                    messageEvent.SmuFuelReporting = (SMUFuelReporting?)null;
                    messageEvent.StartStopConfigEnabled = (bool?)null;
                    // messageEvent.DiagnosticTransmissionFrequency = requestMessage.DailyLocationReportingFrequency.HasValue ? _dataPopulator.GetEventEnumValue<EventFrequency>(requestMessage.DailyLocationReportingFrequency.Value.ToString()) : (EventFrequency?)null; // doubt
                    plOutMessages.Add(new Tuple<IPLOutMessageEvent, DeviceDetails>(messageEvent, deviceDetails));
                }
            }
            else
            {
                {
                    var messageEvent = _dataPopulator.ConstructPlEvent<SendReportIntervalsConfig>(deviceDetails);
                    if (requestMessage.GlobalGram.HasValue)
                        messageEvent.GlobalGramEnable = requestMessage.GlobalGram.Value;
                    if (requestMessage.DailyReportingTime.HasValue)
                        messageEvent.ReportStartTimeUTC = new DateTime() + requestMessage.DailyReportingTime.Value;
                    if (requestMessage.ReportAssetStartStop.HasValue)
                        messageEvent.StartStopConfigEnabled = requestMessage.ReportAssetStartStop.Value;
                    //messageEvent.SmuFuelReporting = requestMessage.HourMeterFuelReport.Value;
                    //messageEvent.DiagnosticTransmissionFrequency = requestMessage.DailyLocationReportingFrequency.Value;
                    plOutMessages.Add(new Tuple<IPLOutMessageEvent, DeviceDetails>(messageEvent, deviceDetails));
                }
            }
			_loggingService.Info("Reporting Schedule Message Event Construction for Device Type " + deviceDetails.DeviceType + " completed !!" + JsonConvert.SerializeObject(requestMessage), "ReportingScheduleMessageEventBuilder.GetPlOutMessageEvent");
            return plOutMessages;
        }

        public IEnumerable<Tuple<IOutMessageEvent, DeviceDetails>> GetDataOutMessageEvent(DeviceConfigRequestBase requestBase, DeviceDetails deviceDetails)
        {
            var dataOutMessages = new List<Tuple<IOutMessageEvent, DeviceDetails>>();
			_loggingService.Info("Recieved ReportingSchedule Message for A5N2 Device " + deviceDetails.DeviceUid, "ReportingScheduleMessageEventBuilder.GetDataOutMessageEvent");
            var requestMessage = _dataPopulator.GetRequestModel<DeviceConfigReportingScheduleRequest>(requestBase);
            if (requestMessage.DailyReportingTime.HasValue)
            {
                var messageEvent = _dataPopulator.ConstructDataOutEvent<FirstDailyReportStartTimeUtcChangedEvent>(deviceDetails);
                messageEvent.DailyReportTimeUTC = new DateTime() + requestMessage.DailyReportingTime.Value;
                var dvcDetails = ConstructorHelpers.GetDeviceConfigMsg(deviceDetails, "DailyReportingTime");
                dataOutMessages.Add(new Tuple<IOutMessageEvent, DeviceDetails>(messageEvent, dvcDetails));
            }

            if (requestMessage.DailyLocationReportingFrequency.HasValue)
            {
                var messageEvent = _dataPopulator.ConstructDataOutEvent<SetDailyReportFrequencyEvent>(deviceDetails);
                messageEvent.Value = (int)requestMessage.DailyLocationReportingFrequency.Value;
                var dvcDetails = ConstructorHelpers.GetDeviceConfigMsg(deviceDetails, "DailyLocationReportingFrequency");
                dataOutMessages.Add(new Tuple<IOutMessageEvent, DeviceDetails>(messageEvent, dvcDetails));
            }

            //doubt
            if (requestMessage.DailyLocationReportingFrequency.HasValue)
            {
                var messageEvent = _dataPopulator.ConstructDataOutEvent<ReportingFrequencyChangedEvent>(deviceDetails);
                messageEvent.Frequency = 1;
                messageEvent.Interval = (int)requestMessage.DailyLocationReportingFrequency.Value; //doubt
                var dvcDetails = ConstructorHelpers.GetDeviceConfigMsg(deviceDetails, "DailyLocationReportingFrequency");
                dataOutMessages.Add(new Tuple<IOutMessageEvent, DeviceDetails>(messageEvent, dvcDetails));
            }
			_loggingService.Info("Reporting Schedule Message Event Construction for Device Type " + deviceDetails.DeviceType + " completed !!" + JsonConvert.SerializeObject(requestMessage), "ReportingScheduleMessageEventBuilder.GetDataOutMessageEvent");
            return dataOutMessages;
        }

        public IEnumerable<Tuple<IMTSOutMessageEvent, DeviceDetails>> GetMtsOutMessageEvent(DeviceConfigRequestBase requestBase, DeviceDetails deviceDetails)
        {
            var mtsOutMessages = new List<Tuple<IMTSOutMessageEvent, DeviceDetails>>();
			_loggingService.Info("Recieved ReportingSchedule Message for MTS Device " + deviceDetails.DeviceUid, "ReportingScheduleMessageEventBuilder.GetMtsOutMessageEvent");
            var requestMessage = _dataPopulator.GetRequestModel<DeviceConfigReportingScheduleRequest>(requestBase);
            if (_validator.NullCheck(RequestMessageType, requestMessage.DailyReportingTime.Value))
            {
                var mtsMessageEvent = _dataPopulator.ConstructMtsEvent<SendDailyReportConfigEvent>(deviceDetails);
                mtsMessageEvent.Enabled = true;
                mtsMessageEvent.DailyReportTimeHour = (byte)(new DateTime() + requestMessage.DailyReportingTime.Value).Hour;
                mtsMessageEvent.DailyReportTimeMinute = (byte)(new DateTime() + requestMessage.DailyReportingTime.Value).Minute;
                mtsMessageEvent.TimezoneName = "UTC";
                mtsOutMessages.Add(new Tuple<IMTSOutMessageEvent, DeviceDetails>(mtsMessageEvent, deviceDetails));
            }
			_loggingService.Info("Reporting Schedule Message Event Construction for Device Type " + deviceDetails.DeviceType + " completed !!" + JsonConvert.SerializeObject(requestMessage), "ReportingScheduleMessageEventBuilder.GetMtsOutMessageEvent");
            return mtsOutMessages;
        }
    }
}
