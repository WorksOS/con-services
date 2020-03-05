using CommonModel.Error;
using ClientModel.Interfaces;
using CommonModel.Enum;
using Infrastructure.Common.Helpers;
using Infrastructure.Service.AssetSettings.Interfaces;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Utilities.Logging;
using CustomerRepository;
using System;
using System.Linq;

namespace Infrastructure.Service.AssetSettings.Validators
{
	public class CustomerUidValidator : RequestValidatorBase, IRequestValidator<IServiceRequest>
    {
		private readonly ICustomerRepository _customerRepository;

		public CustomerUidValidator(ICustomerRepository customerRepository, ILoggingService loggingService) : base(loggingService)
        {
			_customerRepository = customerRepository;
		}

        public async Task<IList<IErrorInfo>> Validate(IServiceRequest request)
        {
            if (!request.CustomerUid.HasValue)
            {
                return new List<IErrorInfo> { base.GetValidationResult(ErrorCodes.CustomerUIDNull, UtilHelpers.GetEnumDescription(ErrorCodes.CustomerUIDNull), true, MethodInfo.GetCurrentMethod().Name) };
            }
			else 
			{
				var result = await _customerRepository.GetCustomerInfo(new List<Guid> { request.CustomerUid.Value });
				if (result.Count() <= 0)
				{
					return new List<IErrorInfo> { base.GetValidationResult(ErrorCodes.InvalidCustomerUID, UtilHelpers.GetEnumDescription(ErrorCodes.InvalidCustomerUID), true, MethodInfo.GetCurrentMethod().Name) };
				}
			}
			return null;
        }
    }
}
