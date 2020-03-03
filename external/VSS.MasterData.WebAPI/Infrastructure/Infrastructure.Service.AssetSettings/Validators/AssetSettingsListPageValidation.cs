using ClientModel.AssetSettings.Request;
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
	public class AssetSettingsListPageValidator : RequestValidatorBase, IRequestValidator<AssetSettingsListRequest>
    {
        public AssetSettingsListPageValidator(ILoggingService loggingService) : base(loggingService)
        {
        }

        public async Task<IList<IErrorInfo>> Validate(AssetSettingsListRequest request)
        {
            var result = new List<IErrorInfo>();
            if (request.PageNumber <= 0)
            {
                result.Add(base.GetValidationResult(ErrorCodes.PageNumberLessThanOne, UtilHelpers.GetEnumDescription(ErrorCodes.PageNumberLessThanOne), true, MethodInfo.GetCurrentMethod().Name));
            }
            if (request.PageSize <= 0)
            {
                result.Add(base.GetValidationResult(ErrorCodes.PageSizeLessThanOne, UtilHelpers.GetEnumDescription(ErrorCodes.PageSizeLessThanOne), true, MethodInfo.GetCurrentMethod().Name));
            }
            return result.Count > 0 ? result : null;
        }
    }
}
