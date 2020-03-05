//TODO: Uncomment only if needed
//using CommonModel.Error;
//using DbModel.DeviceConfig;
//using Infrastructure.Common.DeviceSettings.Helpers;
//using Infrastructure.Service.DeviceConfig.Interfaces;
//using System;
//using System.Collections.Generic;
//using System.Reflection;
//using System.Threading.Tasks;
//using Utilities.Logging;

//namespace Infrastructure.Service.DeviceConfig.Validators
//{
//	public class AttributeValueValidator : RequestValidatorBase, IAttributeValueValidator
//    {
//        IParameterAttributeValidationCache _parameterAttributeValidationCache;
//        public AttributeValueValidator(ILoggingService loggingService, IParameterAttributeValidationCache parameterAttributeValidationCache) : base(loggingService)
//        {
//            _parameterAttributeValidationCache = parameterAttributeValidationCache;
//        }

//        public async Task<IList<IErrorInfo>> Validate(AttributeValueValidationMap request)
//        {
//            IList<IErrorInfo> errors = new List<IErrorInfo>();
//            var validationDetails = _parameterAttributeValidationCache.GetParameterAttributeValidation(request.Group, request.Parameter, request.Attribute);
//            if (validationDetails != null && !string.IsNullOrEmpty(validationDetails?.Validation?.Type))
//            {
//                var validationDataType = Type.GetType(validationDetails.Validation.Type);
//                if (validationDataType.BaseType == typeof(Enum))
//                {
//                    if (!EnumValidator.Validate(request.AttributeValue, validationDetails))
//                        errors.Add(GetValidationResult(validationDetails.Validation.ErrorCode, Utils.GetEnumDescription(validationDetails.Validation.ErrorCode), true, MethodBase.GetCurrentMethod().Name));
//                }
//                else if (validationDataType == typeof(string) || validationDataType == typeof(string[]))
//                {
//                    if (!StringValidator.Validate(request.AttributeValue, validationDetails))
//                        errors.Add(GetValidationResult(validationDetails.Validation.ErrorCode, Utils.GetEnumDescription(validationDetails.Validation.ErrorCode), true, MethodBase.GetCurrentMethod().Name));
//                }
//                else if (validationDataType == typeof(int) || validationDataType == typeof(int[]))
//                {
//                    if (!IntegerValidator.Validate(request.AttributeValue, validationDetails))
//                        errors.Add(GetValidationResult(validationDetails.Validation.ErrorCode, Utils.GetEnumDescription(validationDetails.Validation.ErrorCode), true, MethodBase.GetCurrentMethod().Name));
//                }
//                else if (validationDataType == typeof(DateTime) || validationDataType == typeof(DateTime[]))
//                {
//                    if (!DateTimeValidator.Validate(request.AttributeValue, validationDetails))
//                        errors.Add(GetValidationResult(validationDetails.Validation.ErrorCode, Utils.GetEnumDescription(validationDetails.Validation.ErrorCode), true, MethodBase.GetCurrentMethod().Name));
//                }
//                else if (validationDataType == typeof(bool))
//                {
//                    if (!bool.TryParse(request.AttributeValue, out bool input))
//                        errors.Add(GetValidationResult(validationDetails.Validation.ErrorCode, string.Format(Utils.GetEnumDescription(validationDetails.Validation.ErrorCode), request.Parameter), true, MethodBase.GetCurrentMethod().Name));
//                }
//            }
//            return errors;
//        }
//    }
//}
