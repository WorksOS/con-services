using ClientModel.DeviceConfig.Request.DeviceConfig;
using ClientModel.DeviceConfig.Request.DeviceConfig.FaultCodeReporting;
using Infrastructure.Common.DeviceMessageConstructor.Attributes;
using Infrastructure.Common.DeviceMessageConstructor.Interfaces;
using CommonModel.DeviceSettings;
using Infrastructure.Service.DeviceMessageConstructor.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Utilities.Logging;
using VSS.VisionLink.Interfaces.Events.Commands.Interfaces;
using VSS.VisionLink.Interfaces.Events.Commands.Models;
using VSS.VisionLink.Interfaces.Events.Commands.PL;

namespace Infrastructure.Service.DeviceMessageConstructor.Implementations
{
	[Group("FaultCodeReporting")]
    public class FaultCodeReportingEventBuilder : IGroupMessageEventBuilder
    {
        //private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IDataPopulator _dataPopulator;
        private readonly IDataValidator _validator;
		private readonly ILoggingService _loggingService;

        public FaultCodeReportingEventBuilder(IDataPopulator dataPopulator, IDataValidator validator, ILoggingService loggingService)
        {
            _dataPopulator = dataPopulator;
            _validator = validator;
			_loggingService = loggingService;
		}
        public IEnumerable<Tuple<IOutMessageEvent, DeviceDetails>> GetDataOutMessageEvent(DeviceConfigRequestBase requestBase, DeviceDetails deviceDetails)
        {
			_loggingService.Info("Device Type doesn't support this Group", "FaultCodeReportingEventBuilder.GetDataOutMessageEvent");
            throw new NotImplementedException("Device Type doesn't support this Group");
        }

        public IEnumerable<Tuple<IMTSOutMessageEvent, DeviceDetails>> GetMtsOutMessageEvent(DeviceConfigRequestBase requestBase, DeviceDetails deviceDetails)
        {
			_loggingService.Info("Device Type doesn't support this Group", "FaultCodeReportingEventBuilder.GetDataOutMessageEvent");
            throw new NotImplementedException("Device Type doesn't support this Group");
        }

        public IEnumerable<Tuple<IPLOutMessageEvent, DeviceDetails>> GetPlOutMessageEvent(DeviceConfigRequestBase requestBase, DeviceDetails deviceDetails)
        {
            var plOutMessages = new List<Tuple<IPLOutMessageEvent, DeviceDetails>>();
			_loggingService.Info(string.Format("Received Fault code Reporting message for devicetype {0} ", deviceDetails.DeviceType), "FaultCodeReportingEventBuilder.GetPlOutMessageEvent");
            var requestMessage = _dataPopulator.GetRequestModel<DeviceConfigFaultCodeReportingRequest>(requestBase);
            var plMessageEvent = _dataPopulator.ConstructPlEvent<SendReportIntervalsConfig>(deviceDetails);
            if (requestMessage.EventDiagnosticFilterInterval.HasValue)
                plMessageEvent.EventIntervals = new TimeSpan(days: 0, hours: requestMessage.EventDiagnosticFilterInterval.Value, minutes: 0, seconds: 0);
            if (requestMessage.LowSeverityEvents.HasValue)
                plMessageEvent.Level1TransmissionFrequency = (EventFrequency)requestMessage.LowSeverityEvents;
            if (requestMessage.MediumSeverityEvents.HasValue)
                plMessageEvent.Level2TransmissionFrequency = (EventFrequency)requestMessage.MediumSeverityEvents;
            if (requestMessage.HighSeverityEvents.HasValue)
                plMessageEvent.Level3TransmissionFrequency = (EventFrequency)requestMessage.HighSeverityEvents;
            if (requestMessage.NextSentEventInHours.HasValue)
                plMessageEvent.NextMessageInterval = new TimeSpan(days: 0, hours: requestMessage.NextSentEventInHours.Value, minutes: 0, seconds: 0);
            if (requestMessage.DiagnosticReportFrequency.HasValue)
                plMessageEvent.DiagnosticTransmissionFrequency = (EventFrequency)requestMessage.DiagnosticReportFrequency;
            plOutMessages.Add(new Tuple<IPLOutMessageEvent, DeviceDetails>(plMessageEvent, deviceDetails));
			_loggingService.Info(string.Format("Fault code Reporting message event construction for devicetype {0} completed", deviceDetails.DeviceType), "FaultCodeReportingEventBuilder.GetPlOutMessageEvent");
			_loggingService.Debug(string.Format("Json payload {0}", JsonConvert.SerializeObject(requestBase)), "FaultCodeReportingEventBuilder.GetPlOutMessageEvent");
            return  plOutMessages;
        }        
    }
}
