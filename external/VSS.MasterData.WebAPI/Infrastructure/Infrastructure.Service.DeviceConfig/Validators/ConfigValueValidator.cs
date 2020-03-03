using ClientModel.DeviceConfig.Request.DeviceConfig;
using CommonModel.DeviceSettings.ConfigNameValues;
using CommonModel.Error;
using Infrastructure.Cache.Interfaces;
using Infrastructure.Common.DeviceSettings.Enums;
using Infrastructure.Common.DeviceSettings.Helpers;
using Infrastructure.Service.DeviceConfig.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities.IOC;
using Utilities.Logging;

namespace Infrastructure.Service.DeviceConfig.Validators
{
	public class AllAttributeAsMandatoryValidator : RequestValidatorBase, IRequestValidator<DeviceConfigRequestBase>
    {
        private readonly IParameterAttributeCache _parameterAttributeCache;
        private readonly ConfigListsCollection _parameterGroupExceptionLists;
        private readonly ConfigListsCollection _parameterExceptionLists;
        private readonly ConfigListsCollection _attributeExceptionLists;
        public AllAttributeAsMandatoryValidator(IInjectConfig injectConfig, IParameterAttributeCache parameterAttributeCache, ILoggingService loggingService): base(loggingService)
        {
            this._parameterGroupExceptionLists = injectConfig.ResolveKeyed<ConfigListsCollection>("ParameterGroupsNonMandatoryLists");
            this._parameterExceptionLists = injectConfig.ResolveKeyed<ConfigListsCollection>("ParametersNonMandatoryLists");
            this._attributeExceptionLists = injectConfig.ResolveKeyed<ConfigListsCollection>("AttributesNonMandatoryLists");
            this._parameterAttributeCache = parameterAttributeCache;
        }

        public async Task<IList<IErrorInfo>> Validate(DeviceConfigRequestBase request)
        {
            List<IErrorInfo> errorInfos = new List<IErrorInfo>();

            if (!string.IsNullOrEmpty(request.DeviceType) && !string.IsNullOrEmpty(request.ParameterGroupName))
            {
                var allAttributeIds = await this._parameterAttributeCache.Get(request.DeviceType, request.ParameterGroupName);

                if (allAttributeIds != null)
                {
                    if (_parameterGroupExceptionLists.Values.Contains(request.ParameterGroupName))
                    {
                        if (request.ConfigValues.Count > 0 && allAttributeIds.Any(x => request.ConfigValues.ContainsKey(x.ParameterName + "." + x.AttributeName)))
                        {
                            return errorInfos;
                        }
                        else
                        {
                            errorInfos.Add(base.GetValidationResult(ErrorCodes.NoDeviceAttributeMentioned, Utils.GetEnumDescription(ErrorCodes.NoDeviceAttributeMentioned), true, "AllAttributeAsMandatoryValidator.Validate"));
                            return errorInfos;
                        }
                    }

                    var validAttributeIds = allAttributeIds.Where(x => !this._attributeExceptionLists.Values.Contains(x.AttributeName));

                    foreach (var attribute in validAttributeIds)
                    {
                        if (!request.ConfigValues.ContainsKey(attribute.ParameterName + "." + attribute.AttributeName))
                        {
                            errorInfos.Add(base.GetValidationResult(ErrorCodes.DeviceAttributeMissing, string.Format(Utils.GetEnumDescription(ErrorCodes.DeviceAttributeMissing), attribute.AttributeName), true, "AllAttributeAsMandatoryValidator.Validate"));
                        }
                    }
                }
            }
            return errorInfos;
        }
    }
}
