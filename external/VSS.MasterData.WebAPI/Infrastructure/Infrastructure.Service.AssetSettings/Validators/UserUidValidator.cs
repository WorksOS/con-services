using CommonModel.Error;
using ClientModel.Interfaces;
using CommonModel.Enum;
using Infrastructure.Common.Helpers;
using Infrastructure.Service.AssetSettings.Interfaces;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Utilities.Logging;

namespace Infrastructure.Service.AssetSettings.Validators
{
	public class UserUidValidator : RequestValidatorBase, IRequestValidator<IServiceRequest>
    {
        public UserUidValidator(ILoggingService loggingService) : base(loggingService)
        {
        }
        public async Task<IList<IErrorInfo>> Validate(IServiceRequest request)
        {
            if (!request.UserUid.HasValue)
            {
                return new List<IErrorInfo> { base.GetValidationResult(ErrorCodes.UserUIDNull, UtilHelpers.GetEnumDescription(ErrorCodes.UserUIDNull), true, MethodInfo.GetCurrentMethod().Name) }; 
            }
            return null;
        }
    }
}
