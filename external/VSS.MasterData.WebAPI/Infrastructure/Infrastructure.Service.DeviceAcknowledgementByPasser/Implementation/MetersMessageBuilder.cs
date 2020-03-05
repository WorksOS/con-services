using ClientModel.DeviceConfig.Request.DeviceConfig;
using ClientModel.DeviceConfig.Request.DeviceConfig.Meters;
using CommonModel.DeviceSettings;
using Infrastructure.Common.DeviceMessageConstructor.Attributes;
using Infrastructure.Common.DeviceMessageConstructor.Interfaces;
using Infrastructure.Common.DeviceMessageConstructor.Models;
using Infrastructure.Service.DeviceAcknowledgementByPasser.Interfaces;
using System;
using System.Collections.Generic;
using Utilities.Logging;

namespace Infrastructure.Service.DeviceAcknowledgementByPasser.Implementation
{
	[Group("Meters")]
    public class MetersMessageBuilder : IGroupMessageBuilder
    {
        private static ILoggingService _loggingService;
        private readonly IDataPopulator _dataPopulator;
        private const string RequestMessageType = "DeviceConfigMetersRequest";

        public MetersMessageBuilder(IDataPopulator dataPopulator)
        {
            _dataPopulator = dataPopulator;
        }

        public IEnumerable<object> ProcessGroupMessages(string assetuid, DeviceConfigRequestBase requestBase, ParamGroup group)
        {
            //Get device type parameter names as collection.
            _loggingService.Info("Started processing group messages", "MetersMessageBuilder.ProcessGroupMessages");
            var metersMessages = new List<object>();
            foreach (var param in group.Parameters)
            {
                if (param.ParameterName == "HoursMeter")
                {                    
                    var hourMtroffsetDataObj = GetHourMeterOffsetMessageEvent(requestBase);
                    hourMtroffsetDataObj.AssetId = new Guid(assetuid);
                    metersMessages.Add(hourMtroffsetDataObj);
                }
                if (param.ParameterName == "Odometer")
                {
                    var odoMtroffsetDataObj = GetOdometerOffsetMessageEvent(requestBase);
                    odoMtroffsetDataObj.AssetId = new Guid(assetuid);
                    metersMessages.Add(odoMtroffsetDataObj);
                }
            }
			_loggingService.Info("Ended processing group messages", "MetersMessageBuilder.ProcessGroupMessages");
            return metersMessages;
        }

        private RuntimeHoursOffset GetHourMeterOffsetMessageEvent(DeviceConfigRequestBase requestBase)
        {
			_loggingService.Info("Started processing hour meter offset values","MetersMessageBuilder.GetHourMeterOffsetMessageEvent");
            var hourmeterOffsetEvent = new RuntimeHoursOffset();
            var requestMessage = _dataPopulator.GetRequestModel<DeviceConfigMetersRequest>(requestBase);
            if (requestMessage.HoursMeter != null)
            {
                if (requestMessage.HoursMeter.ProposedValue.HasValue)
                {
                    hourmeterOffsetEvent.Offset = Math.Round(Convert.ToDouble(requestMessage.HoursMeter.ProposedValue - Convert.ToDouble(requestMessage.HoursMeter.CurrentValue)), 1);
                }
            }
			_loggingService.Info("Ended processing hour meter offset values", "MetersMessageBuilder.GetHourMeterOffsetMessageEvent");
            return hourmeterOffsetEvent;
        }

        private OdometerOffset GetOdometerOffsetMessageEvent(DeviceConfigRequestBase requestBase)
        {
			_loggingService.Info("Started processing odometer offset values", "MetersMessageBuilder.GetOdometerOffsetMessageEvent");
            var odometerOffsetEvent = new OdometerOffset();
            var requestMessage = _dataPopulator.GetRequestModel<DeviceConfigMetersRequest>(requestBase);
            if (requestMessage.OdoMeter != null)
            {
                if (requestMessage.OdoMeter.ProposedValue.HasValue)
                {
                    odometerOffsetEvent.Offset = Math.Round(Convert.ToDouble(requestMessage.OdoMeter.ProposedValue - Convert.ToDouble(requestMessage.OdoMeter.CurrentValue)), 10);
                }
            }
			_loggingService.Info("Ended processing odometer offset values", "MetersMessageBuilder.GetOdometerOffsetMessageEvent");
            return odometerOffsetEvent;
        }
    }
}
