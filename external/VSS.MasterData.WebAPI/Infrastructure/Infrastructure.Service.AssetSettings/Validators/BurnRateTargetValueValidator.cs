using ClientModel.AssetSettings.Request.AssetSettings;
using CommonModel.Error;
using CommonModel.Enum;
using Infrastructure.Common.Helpers;
using Infrastructure.Service.AssetSettings.Interfaces;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Utilities.Logging;

namespace Infrastructure.Service.AssetSettings.Validators
{
	public class BurnRateTargetValueValidator : RequestValidatorBase, IRequestValidator<AssetFuelBurnRateSettingRequest>
    {
        public BurnRateTargetValueValidator(ILoggingService loggingService) : base(loggingService)
        {
        }

        public async Task<IList<IErrorInfo>> Validate(AssetFuelBurnRateSettingRequest request)
        {
            if (request.WorkTargetValue >= 0 &&  request.IdleTargetValue > 0)
            {
                if (request.IdleTargetValue > request.WorkTargetValue)
                {
                    return new List<IErrorInfo> { base.GetValidationResult(ErrorCodes.WorkValueShouldBeLessThanIdleValue, UtilHelpers.GetEnumDescription(ErrorCodes.WorkValueShouldBeLessThanIdleValue), true, MethodInfo.GetCurrentMethod().Name) };
                }
            }
            else if (request.WorkTargetValue < 0 && request.IdleTargetValue < 0)
            {
                return new List<IErrorInfo> { base.GetValidationResult(ErrorCodes.WorkAndIdleValueShouldNotBeNegative, UtilHelpers.GetEnumDescription(ErrorCodes.WorkAndIdleValueShouldNotBeNegative), true, MethodInfo.GetCurrentMethod().Name) };
            }
            else if (request.WorkTargetValue < 0)
            {
                return new List<IErrorInfo> { base.GetValidationResult(ErrorCodes.WorkValueShouldNotBeNegative, UtilHelpers.GetEnumDescription(ErrorCodes.WorkValueShouldNotBeNegative), true, MethodInfo.GetCurrentMethod().Name) };
            }
            else if (request.IdleTargetValue < 0)
            {
                return new List<IErrorInfo> { base.GetValidationResult(ErrorCodes.IdleValueShouldNotBeNegative, UtilHelpers.GetEnumDescription(ErrorCodes.IdleValueShouldNotBeNegative), true, MethodInfo.GetCurrentMethod().Name) };
            }
            return null;
        }
    }
}
