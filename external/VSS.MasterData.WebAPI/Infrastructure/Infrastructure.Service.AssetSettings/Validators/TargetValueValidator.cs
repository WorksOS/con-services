using ClientModel.AssetSettings.Request;
using CommonModel.Error;
using CommonModel.Enum;
using Infrastructure.Common.Helpers;
using Infrastructure.Service.AssetSettings.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Utilities.Logging;

namespace Infrastructure.Service.AssetSettings.Validators
{
    public class TargetValueValidator : RequestValidatorBase, IRequestValidator<AssetSettingsRequestBase>
    {

        public TargetValueValidator(ILoggingService loggingService) : base(loggingService)
        {
        }

        public async Task<IList<IErrorInfo>> Validate(AssetSettingsRequestBase request)
        {
            if (request.TargetValues != null && request.TargetValues.Keys.Count == 1)
            {
                foreach(var targetValue in request.TargetValues)
                {
                    if (targetValue.Value < 0)
                    {
                        return new List<IErrorInfo> { base.GetValidationResult(ErrorCodes.TargetValueIsNegative, UtilHelpers.GetEnumDescription(ErrorCodes.TargetValueIsNegative), true, MethodInfo.GetCurrentMethod().Name) };
                    }
                }
            }
            return null;
        }
    }
}
