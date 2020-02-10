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
	public class AssetSettingsSortColumnValidator : RequestValidatorBase, IRequestValidator<AssetSettingsListRequest>
    {
        public AssetSettingsSortColumnValidator(ILoggingService loggingService): base(loggingService)
        {
        }

        public async Task<IList<IErrorInfo>> Validate(AssetSettingsListRequest request)
        {
            var result = new List<IErrorInfo>();
            if (!string.IsNullOrEmpty(request.SortColumn))
            {
                var sortColumn = request.SortColumn.Replace("-", string.Empty);
                if (!Enum.GetNames(typeof(AssetSettingsSortColumns)).Any(x => x.ToLower() == sortColumn.ToLower()))
                {
                    result.Add(base.GetValidationResult(ErrorCodes.InvalidSortColumn, string.Format(UtilHelpers.GetEnumDescription(ErrorCodes.InvalidSortColumn), sortColumn), true, MethodInfo.GetCurrentMethod().Name));
                }
            }
            return result.Count > 0 ? result : null;
        }
    }
}
