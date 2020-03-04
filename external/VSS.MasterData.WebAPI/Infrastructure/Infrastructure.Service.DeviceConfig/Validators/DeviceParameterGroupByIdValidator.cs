using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities.Logging;
using Infrastructure.Common.DeviceSettings.Enums;
using Infrastructure.Common.DeviceSettings.Helpers;
using CommonModel.Error;
using ClientModel.DeviceConfig.Request.DeviceConfig.Parameter;
using Interfaces;
using DbModel.DeviceConfig;
using Infrastructure.Service.DeviceConfig.Interfaces;
using Infrastructure.Cache.Interfaces;

namespace Infrastructure.Service.DeviceConfig.Validators
{
    public class DeviceParameterGroupByIdValidator : RequestValidatorBase, IRequestValidator<DeviceConfigParameterRequest>
    {
        private readonly IDeviceParamGroupCache _deviceParamGroupCache;

        public DeviceParameterGroupByIdValidator(IDeviceParamGroupCache deviceParamGroupCache, ILoggingService loggingService) : base(loggingService)
        {
            this._deviceParamGroupCache = deviceParamGroupCache;
        }

        public async Task<IList<IErrorInfo>> Validate(DeviceConfigParameterRequest request)
        {
            IList<IErrorInfo> errorInfo = new List<IErrorInfo>();

            var result = await this._deviceParamGroupCache.Get(request.ParameterGroupID.ToString());
            if (result == null || !result.Any())
            {
                errorInfo.Add(base.GetValidationResult(ErrorCodes.InvalidDeviceParamGroup, Utils.GetEnumDescription(ErrorCodes.InvalidDeviceParamGroup), true, "DeviceParameterGroupByIdValidator.Validate"));
            }

            return errorInfo;
        }
    }
}
