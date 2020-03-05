using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Logging;
using Infrastructure.Common.DeviceSettings.Enums;
using Infrastructure.Common.DeviceSettings.Helpers;
using CommonModel.Error;
using ClientModel.DeviceConfig.Request.DeviceConfig.MaintenanceMode;
using Infrastructure.Service.DeviceConfig.Interfaces;

namespace Infrastructure.Service.DeviceConfig.Validators
{
    public class MaintenanceModeValidator : RequestValidatorBase, IRequestValidator<DeviceConfigMaintenanceModeRequest>
    {
        public MaintenanceModeValidator(ILoggingService loggingService) : base(loggingService)
        {

        }

        public async Task<IList<IErrorInfo>> Validate(DeviceConfigMaintenanceModeRequest request)
        {
            List<IErrorInfo> result = new List<IErrorInfo>();

            if (request.Status.HasValue && request.Status.Value)
            {
                if (!request.MaintenanceModeDuration.HasValue || request.MaintenanceModeDuration.Value <= 0 || Utils.GetDigitsCount(request.MaintenanceModeDuration.Value) > 2)
                {
                    result.Add(base.GetValidationResult(ErrorCodes.MaintenanceModeInvalidDuration, string.Format(Utils.GetEnumDescription(ErrorCodes.MaintenanceModeInvalidDuration), 1, 99), true, "MaintenanceModeValidator.Validate"));
                }
            }


            //if (request.StartTime.HasValue)
            //{
            //    if (request.StartTime.Value.Hours < 0 || request.StartTime.Value.Hours > 23)
            //    {
            //        result.Add(base.GetValidationResult(ErrorCodes.MaintenanceModeInvalidStartTime, string.Format(Utils.GetEnumDescription(ErrorCodes.MaintenanceModeInvalidStartTime), 0, 23), true, "MaintenanceModeValidator.Validate"));
            //    }
            //}

            return result;
        }
    }
}
