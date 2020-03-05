using ClientModel.DeviceConfig.Request.DeviceConfig.SpeedingThresholds;
using CommonModel.Error;
using Infrastructure.Common.DeviceSettings.Enums;
using Infrastructure.Common.DeviceSettings.Helpers;
using Infrastructure.Service.DeviceConfig.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utilities.Logging;

namespace Infrastructure.Service.DeviceConfig.Validators
{
	public class SpeedingThresholdsValidator : RequestValidatorBase, IRequestValidator<DeviceConfigSpeedingThresholdsRequest>
    {
        public SpeedingThresholdsValidator(ILoggingService loggingService) : base(loggingService)
        {

        }

        public async Task<IList<IErrorInfo>> Validate(DeviceConfigSpeedingThresholdsRequest request)
        {
            List<IErrorInfo> result = new List<IErrorInfo>();

            if (request.SpeedThresholdEnabled.HasValue && request.SpeedThresholdEnabled.Value)
            {
                if (!request.SpeedThresholdDuration.HasValue || request.SpeedThresholdDuration < 1 || request.SpeedThresholdDuration.Value > 65535)
                {
                    result.Add(base.GetValidationResult(ErrorCodes.SpeedingThresholdsInvalidDuration, string.Format(Utils.GetEnumDescription(ErrorCodes.SpeedingThresholdsInvalidDuration), 1, 65535), true, "SpeedingThresholdsValidator.Validate"));
                }

                if (!request.SpeedThreshold.HasValue || request.SpeedThreshold < 1 || request.SpeedThreshold.Value > 410)
                {
                    result.Add(base.GetValidationResult(ErrorCodes.SpeedingThresholdsInvalidValue, string.Format(Utils.GetEnumDescription(ErrorCodes.SpeedingThresholdsInvalidValue), 1, 410), true, "SpeedingThresholdsValidator.Validate"));
                }
            }
            return result;
        }
    }
}
