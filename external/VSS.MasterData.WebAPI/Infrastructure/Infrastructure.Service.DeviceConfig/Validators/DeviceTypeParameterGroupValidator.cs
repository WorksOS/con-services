using ClientModel.DeviceConfig.Request.DeviceConfig;
using CommonModel.Error;
using Infrastructure.Cache.Interfaces;
using Infrastructure.Common.DeviceSettings.Enums;
using Infrastructure.Common.DeviceSettings.Helpers;
using Infrastructure.Service.DeviceConfig.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities.Logging;

namespace Infrastructure.Service.DeviceConfig.Validators
{
	public class DeviceTypeParameterGroupValidator : RequestValidatorBase, IRequestValidator<DeviceConfigRequestBase>
    {
        private readonly IParameterAttributeCache _parameterAttributeCache;

        public DeviceTypeParameterGroupValidator(IParameterAttributeCache parameterAttributeCache, ILoggingService loggingService) : base(loggingService)
        {
            this._parameterAttributeCache = parameterAttributeCache;
        }

        public async Task<IList<IErrorInfo>> Validate(DeviceConfigRequestBase request)
        {
            IList<IErrorInfo> errorInfo = new List<IErrorInfo>();
            if (!string.IsNullOrEmpty(request.DeviceType) && !string.IsNullOrEmpty(request.ParameterGroupName))
            {
                var cachedParameters = await this._parameterAttributeCache.Get(request.DeviceType, request.ParameterGroupName);

                if (cachedParameters == null || !cachedParameters.Any())
                {
                    errorInfo.Add(base.GetValidationResult(ErrorCodes.InvalidParameterGroupForDeviceType, Utils.GetEnumDescription(ErrorCodes.InvalidParameterGroupForDeviceType), true, "DeviceTypeParameterGroupValidator.Validate"));
                }
            }
            return errorInfo;
        }
    }
}
