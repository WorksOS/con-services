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
    public class UserUIDValidator : RequestValidatorBase, IRequestValidator<IServiceRequest>
    {
        public UserUIDValidator(ILoggingService loggingService) : base(loggingService)
        {
        }
        
        public async Task<IList<IErrorInfo>> Validate(IServiceRequest request)
        {
            if (!request.UserUID.HasValue)
            {
                return await Task.FromResult(new List<IErrorInfo> { base.GetValidationResult(ErrorCodes.UserUIDNull, Utils.GetEnumDescription(ErrorCodes.UserUIDNull), true, MethodInfo.GetCurrentMethod().Name) });
            }
			return null;
        }
    }
}
