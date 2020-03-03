using ClientModel.DeviceConfig.Request.DeviceConfig;
using ClientModel.DeviceConfig.Request.DeviceConfig.AssetSecurity;
using CommonModel.DeviceSettings;
using Infrastructure.Common.DeviceMessageConstructor.Attributes;
using Infrastructure.Common.DeviceMessageConstructor.Interfaces;
using Infrastructure.Common.DeviceSettings.Enums;
using Infrastructure.Service.DeviceMessageConstructor.Helpers;
using Infrastructure.Service.DeviceMessageConstructor.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Utilities.Logging;
using VSS.VisionLink.Interfaces.Events.Commands.A5N2;
using VSS.VisionLink.Interfaces.Events.Commands.Interfaces;
using VSS.VisionLink.Interfaces.Events.Commands.Models;

namespace Infrastructure.Service.DeviceMessageConstructor.Implementations
{
	[Group("AssetSecurity")]
    public class AssetSecurityMessageEventBuilder : IGroupMessageEventBuilder
    {
        private readonly ILoggingService _loggingService;
        private readonly IDataPopulator _dataPopulator;
        private readonly IDataValidator _validator;
        private readonly string _requestMessageType = "DeviceConfigAssetSecurityRequest";

        public AssetSecurityMessageEventBuilder(IDataPopulator dataPopulator, IDataValidator validator, ILoggingService loggingService)
        {
            _dataPopulator = dataPopulator;
            _validator = validator;
			_loggingService = loggingService;
		}

        public IEnumerable<Tuple<IOutMessageEvent, DeviceDetails>> GetDataOutMessageEvent(DeviceConfigRequestBase requestBase, DeviceDetails deviceDetails)
        {
            var dataOutMessages = new List<Tuple<IOutMessageEvent, DeviceDetails>>();
			_loggingService.Info("Received Asset Security Message for Device " + deviceDetails.DeviceType, "AssetSecurityMessageEventBuilder.GetDataOutMessageEvent");
            var requestMessage = _dataPopulator.GetRequestModel<DeviceConfigAssetSecurityRequest>(requestBase);
            if (requestMessage.SecurityMode.HasValue)
            {
                var setTamperLevelEvent = _dataPopulator.ConstructDataOutEvent<SetTamperLevelEvent>(deviceDetails);
                setTamperLevelEvent.TamperLevel = requestMessage.SecurityMode.Value == true ? TamperResistanceStatus.TamperResistanceLevel1 : TamperResistanceStatus.Off;
                var securityModeDeviceDetails = ConstructorHelpers.GetDeviceConfigMsg(deviceDetails, "SecurityMode");
                dataOutMessages.Add(new Tuple<IOutMessageEvent, DeviceDetails>(setTamperLevelEvent, securityModeDeviceDetails));
            }
            if (requestMessage.SecurityStatus.HasValue)
            {
                var setStartModeEvent = _dataPopulator.ConstructDataOutEvent<SetStartModeEvent>(deviceDetails);
                switch (requestMessage.SecurityStatus.Value)
                {
                    case AssetSecurityStatus.NormalOperation:
                        setStartModeEvent.StartMode = MachineStartStatus.NormalOperation;
                        break;
                    case AssetSecurityStatus.Derated:
                        setStartModeEvent.StartMode = MachineStartStatus.Derate;
                        break;
                    case AssetSecurityStatus.Disable:
                        setStartModeEvent.StartMode = MachineStartStatus.Disable;
                        break;
                }

                var securityStatusDeviceDetails = ConstructorHelpers.GetDeviceConfigMsg(deviceDetails, "SecurityStatus");
                dataOutMessages.Add(new Tuple<IOutMessageEvent, DeviceDetails>(setStartModeEvent, securityStatusDeviceDetails));
            }
			_loggingService.Info("Asset Security Message Event Construction for Device Type " + deviceDetails.DeviceType + " completed !!" + JsonConvert.SerializeObject(requestMessage), "AssetSecurityMessageEventBuilder.GetDataOutMessageEvent");
            return dataOutMessages;
        }

        public IEnumerable<Tuple<IMTSOutMessageEvent, DeviceDetails>> GetMtsOutMessageEvent(DeviceConfigRequestBase requestBase, DeviceDetails deviceDetails)
        {
            _loggingService.Info("Device Type doesn't support this Group", "AssetSecurityMessageEventBuilder.GetMtsOutMessageEvent");
            throw new NotImplementedException("Device Type doesn't support this Group");
        }

        public IEnumerable<Tuple<IPLOutMessageEvent, DeviceDetails>> GetPlOutMessageEvent(DeviceConfigRequestBase requestBase, DeviceDetails deviceDetails)
        {
			_loggingService.Info("Device Type doesn't support this Group", "AssetSecurityMessageEventBuilder.GetPlOutMessageEvent");
            throw new NotImplementedException("Device Type doesn't support this Group");
        }     
    }
}
