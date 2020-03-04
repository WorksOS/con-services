using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Logging;
using Infrastructure.Common.DeviceSettings.Enums;
using Infrastructure.Common.DeviceSettings.Helpers;
using CommonModel.Error;
using ClientModel.DeviceConfig.Request.DeviceConfig.MovingThresold;
using Infrastructure.Service.DeviceConfig.Interfaces;

namespace Infrastructure.Service.DeviceConfig.Validators
{
    public class MovingThresholdValidator : RequestValidatorBase, IRequestValidator<DeviceConfigMovingThresholdRequest>
    {
        public MovingThresholdValidator(ILoggingService loggingService) : base(loggingService)
        {

        }

        public async Task<IList<IErrorInfo>> Validate(DeviceConfigMovingThresholdRequest request)
        {
            List<IErrorInfo> result = new List<IErrorInfo>();

            if (request.MovingThresholdsDuration.HasValue)
            {
                if (request.MovingThresholdsDuration < 10 || request.MovingThresholdsDuration.Value > 65535)
                {
                  result.Add(base.GetValidationResult(ErrorCodes.MovingThresholdInvalidDuration, string.Format(Utils.GetEnumDescription(ErrorCodes.MovingThresholdInvalidDuration), 10, 65535), true, "MovingThresholdValidator.Validate"));
                }
            }

            if (request.Radius.HasValue)
            {
                if (request.Radius < 5 || request.Radius.Value > 19975)
                {
                    result.Add(base.GetValidationResult(ErrorCodes.MovingThresholdInvalidRadius, string.Format(Utils.GetEnumDescription(ErrorCodes.MovingThresholdInvalidRadius), 5, 19975), true, "MovingThresholdValidator.Validate"));
                }
            }

            if (request.MovingOrStoppedThreshold.HasValue)
            {
                if (request.MovingOrStoppedThreshold < 0 || request.MovingOrStoppedThreshold.Value > 42)
                {
                    result.Add(base.GetValidationResult(ErrorCodes.MovingThresholdInvalidStoppedThreshold, string.Format(Utils.GetEnumDescription(ErrorCodes.MovingThresholdInvalidStoppedThreshold), 0, 42), true, "MovingThresholdValidator.Validate"));
                }
            }

            return result;
        }
    }
}
