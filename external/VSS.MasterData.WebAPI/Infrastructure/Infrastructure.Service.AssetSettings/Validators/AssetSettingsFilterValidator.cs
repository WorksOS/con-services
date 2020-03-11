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
	public class AssetSettingsFilterValidator : RequestValidatorBase, IRequestValidator<AssetSettingsListRequest>
    {
        public AssetSettingsFilterValidator(ILoggingService loggingService) : base(loggingService)
        {
        }

        public async Task<IList<IErrorInfo>> Validate(AssetSettingsListRequest request)
        {
            var result = new List<IErrorInfo>();
            if (!string.IsNullOrEmpty(request.FilterName))
            {
                if (string.IsNullOrEmpty(request.FilterValue))
                {
                    result.Add(base.GetValidationResult(ErrorCodes.FilterValueNull, string.Format(UtilHelpers.GetEnumDescription(ErrorCodes.FilterValueNull)), true, MethodInfo.GetCurrentMethod().Name));
                }
                if (!Enum.GetNames(typeof(AssetSettingsFilters)).Any(x => x.ToLower() == request.FilterName.ToLower()))
                {
                    result.Add(base.GetValidationResult(ErrorCodes.InvalidFilterName, string.Format(UtilHelpers.GetEnumDescription(ErrorCodes.InvalidFilterName), request.FilterName), true, MethodInfo.GetCurrentMethod().Name));
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(request.FilterValue))
                {
                    result.Add(base.GetValidationResult(ErrorCodes.FilterNameNull, UtilHelpers.GetEnumDescription(ErrorCodes.FilterNameNull), true, MethodInfo.GetCurrentMethod().Name));
                }
            }
            return result.Count > 0 ? result : null;
        }
    }
}
