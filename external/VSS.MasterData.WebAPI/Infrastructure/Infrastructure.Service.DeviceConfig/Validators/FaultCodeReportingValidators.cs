using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Logging;
using Infrastructure.Common.DeviceSettings.Enums;
using Infrastructure.Common.DeviceSettings.Helpers;
using CommonModel.Error;
using ClientModel.DeviceConfig.Request.DeviceConfig.FaultCodeReporting;
using Infrastructure.Service.DeviceConfig.Interfaces;

namespace Infrastructure.Service.DeviceConfig.Validators
{
    public class FaultCodeReportingValidator : RequestValidatorBase, IRequestValidator<DeviceConfigFaultCodeReportingRequest>
    {
        public FaultCodeReportingValidator(ILoggingService loggingService) : base(loggingService)
        {
        }

        public async Task<IList<IErrorInfo>> Validate(DeviceConfigFaultCodeReportingRequest request)
        {
            List<IErrorInfo> result = new List<IErrorInfo>();

            if (request.LowSeverityEvents.HasValue)
            {
                if (!Enum.IsDefined(typeof(FaultCodeReportingEventSeverity), request.LowSeverityEvents.Value))
                {
                    result.Add(base.GetValidationResult(ErrorCodes.FaultCodeReportingInvalidLowSeverityEvents, string.Format(Utils.GetEnumDescription(ErrorCodes.FaultCodeReportingInvalidLowSeverityEvents), Utils.GetEnumValuesAsKeyValueString(typeof(FaultCodeReportingEventSeverity))), true, "FaultCodeReportingValidators.Validate"));
                }
            }

            if (request.MediumSeverityEvents.HasValue)
            {
                if (!Enum.IsDefined(typeof(FaultCodeReportingEventSeverity), request.MediumSeverityEvents.Value))
                {
                    result.Add(base.GetValidationResult(ErrorCodes.FaultCodeReportingInvalidMediumSeverityEvents, string.Format(Utils.GetEnumDescription(ErrorCodes.FaultCodeReportingInvalidMediumSeverityEvents), Utils.GetEnumValuesAsKeyValueString(typeof(FaultCodeReportingEventSeverity))), true, "FaultCodeReportingValidators.Validate"));
                }
            }

            if (request.HighSeverityEvents.HasValue)
            {
                if (!Enum.IsDefined(typeof(FaultCodeReportingEventSeverity), request.HighSeverityEvents.Value))
                {
                    result.Add(base.GetValidationResult(ErrorCodes.FaultCodeReportingInvalidHighSeverityEvents, string.Format(Utils.GetEnumDescription(ErrorCodes.FaultCodeReportingInvalidHighSeverityEvents), Utils.GetEnumValuesAsKeyValueString(typeof(FaultCodeReportingEventSeverity))), true, "FaultCodeReportingValidators.Validate"));
                }
            }

            if (request.DiagnosticReportFrequency.HasValue)
            {
                if (!Enum.IsDefined(typeof(FaultCodeReportingEventSeverity), request.DiagnosticReportFrequency.Value))
                {
                    result.Add(base.GetValidationResult(ErrorCodes.FaultCodeReportingInvalidDiagnosticReportFrequency, string.Format(Utils.GetEnumDescription(ErrorCodes.FaultCodeReportingInvalidDiagnosticReportFrequency), Utils.GetEnumValuesAsKeyValueString(typeof(FaultCodeReportingEventSeverity))), true, "FaultCodeReportingValidators.Validate"));
                }
            }

            if (request.EventDiagnosticFilterInterval.HasValue && request.EventDiagnosticFilterInterval.Value != -9999999)    
            {
                if (request.EventDiagnosticFilterInterval < 4 || request.EventDiagnosticFilterInterval > 65535)
                {
                    result.Add(base.GetValidationResult(ErrorCodes.FaultCodeReportingInvalidDiagnosticFilterInterval, string.Format(Utils.GetEnumDescription(ErrorCodes.FaultCodeReportingInvalidDiagnosticFilterInterval), 4, 65535), true, "FaultCodeReportingValidators.Validate"));
                }
            }

            if (request.NextSentEventInHours.HasValue && request.NextSentEventInHours.Value != -9999999)
            {
                if (request.NextSentEventInHours < 4 || request.NextSentEventInHours > 65535)
                {
                    result.Add(base.GetValidationResult(ErrorCodes.FaultCodeReportingInvalidNextSentEvent, string.Format(Utils.GetEnumDescription(ErrorCodes.FaultCodeReportingInvalidNextSentEvent), 4, 65535), true, "FaultCodeReportingValidators.Validate"));
                }
            }

            return result;
        }
    }
}
