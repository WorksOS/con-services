using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utilities.Logging;
using Infrastructure.Common.DeviceSettings.Enums;
using Infrastructure.Common.DeviceSettings.Helpers;
using CommonModel.Error;
using ClientModel.DeviceConfig.Request.DeviceConfig.ReportingSchedule;
using Infrastructure.Service.DeviceConfig.Interfaces;

namespace Infrastructure.Service.DeviceConfig.Validators
{
    public class ReportingScheduleValidator : RequestValidatorBase, IRequestValidator<DeviceConfigReportingScheduleRequest>
    {
        public ReportingScheduleValidator(ILoggingService loggingService) : base(loggingService)
        {

        }

        public async Task<IList<IErrorInfo>> Validate(DeviceConfigReportingScheduleRequest request)
        {
            List<IErrorInfo> result = new List<IErrorInfo>();

            if (request.DailyReportingTime.HasValue)
            {
                if (request.DailyReportingTime.Value.Hours < 0 || request.DailyReportingTime.Value.Hours > 23)
                {
                    result.Add(base.GetValidationResult(ErrorCodes.ReportingScheduleInvalidReportingTime, string.Format(Utils.GetEnumDescription(ErrorCodes.ReportingScheduleInvalidReportingTime), 0, 23), true, "ReportingScheduleValidator.Validate"));
                }
            }

            if (request.DailyLocationReportingFrequency.HasValue)
            {
                if (!Enum.IsDefined(typeof(ReportingScheduleDailyLocationReportingFrequency), request.DailyLocationReportingFrequency.Value))
                {
                    result.Add(base.GetValidationResult(ErrorCodes.ReportingScheduleInvalidDailyLocationReportingFrequency, string.Format(Utils.GetEnumDescription(ErrorCodes.ReportingScheduleInvalidDailyLocationReportingFrequency), Utils.GetEnumValuesAsKeyValueString(typeof(ReportingScheduleDailyLocationReportingFrequency))), true, "ReportingScheduleValidator.Validate"));
                }
            }

            if (request.HourMeterFuelReport.HasValue)
            {
                if (!Enum.IsDefined(typeof(ReportingScheduleHourMeterFuelReport), request.HourMeterFuelReport.Value))
                {
                    result.Add(base.GetValidationResult(ErrorCodes.ReportingScheduleInvalidHourMeterFuelReport, string.Format(Utils.GetEnumDescription(ErrorCodes.ReportingScheduleInvalidHourMeterFuelReport), Utils.GetEnumValuesAsKeyValueString(typeof(ReportingScheduleHourMeterFuelReport))), true, "ReportingScheduleValidator.Validate"));
                }
            }

            return result;
        }
    }
}
