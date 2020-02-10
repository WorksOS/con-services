using System.Collections.Generic;
using System.Linq;
using Utilities.Logging;
using Infrastructure.Common.DeviceSettings.Enums;
using CommonModel.Error;

namespace Infrastructure.Service.DeviceConfig.Validators
{
    public abstract class RequestValidatorBase
    {
        protected readonly ILoggingService _loggingService;

        protected RequestValidatorBase(ILoggingService loggingService)
        {
            this._loggingService = loggingService;
            this._loggingService.CreateLogger(this.GetType());
        }

        protected IErrorInfo GetValidationResult(ErrorCodes errorCode, string message, bool isInvalid, string methodName)
        {
            this._loggingService.Debug(message, methodName);

            return new ErrorInfo
            {
                ErrorCode = (int)errorCode,
                Message = message,
                IsInvalid = isInvalid
            };
        }

        protected IErrorInfo GetValidationResult(ErrorCodes errorCode, string assetUID, string message, bool isInvalid, string methodName)
        {
            AssetErrorInfo validationResult = new AssetErrorInfo
            {
                ErrorCode = (int)errorCode,
                Message = message,
                IsInvalid = isInvalid
            };

            if (!string.IsNullOrEmpty(assetUID))
            {
                validationResult.AssetUID = assetUID;
            }

            this._loggingService.Debug(message, methodName);

            return validationResult;
        }

        protected IList<IErrorInfo> GetValidationResults(ErrorCodes errorCode, IEnumerable<string> assetUIDs, string message, bool isInvalid, string methodName)
        {
            IList<IErrorInfo> validationResults = new List<IErrorInfo>();

            if (assetUIDs != null && assetUIDs.Any())
            {
                foreach (var assetUID in assetUIDs)
                {
                    validationResults.Add(new AssetErrorInfo
                    {
                        ErrorCode = (int)errorCode,
                        Message = message,
                        AssetUID = assetUID,
                        IsInvalid = isInvalid
                    });
                }
            }

            this._loggingService.Debug(message, methodName);

            return validationResults;
        }
    }
}
