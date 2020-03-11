using ClientModel.DeviceConfig.Request;
using CommonModel.Error;
using Infrastructure.Common.DeviceSettings.Enums;
using Infrastructure.Common.DeviceSettings.Helpers;
using Infrastructure.Service.DeviceConfig.Interfaces;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Utilities.Logging;

namespace Infrastructure.Service.DeviceConfig.Validators
{
	public class CustomerUIDValidator : RequestValidatorBase, IRequestValidator<IServiceRequest>
    {
        public CustomerUIDValidator(ILoggingService loggingService) : base(loggingService)
        {
        }
        public async Task<IList<IErrorInfo>> Validate(IServiceRequest request)
        {
            if (!request.CustomerUID.HasValue)
            {
                return new List<IErrorInfo> { base.GetValidationResult(ErrorCodes.CustomerUIDNull, Utils.GetEnumDescription(ErrorCodes.CustomerUIDNull), true, MethodInfo.GetCurrentMethod().Name) };
            }
            
            return null;
        }
    }
}
