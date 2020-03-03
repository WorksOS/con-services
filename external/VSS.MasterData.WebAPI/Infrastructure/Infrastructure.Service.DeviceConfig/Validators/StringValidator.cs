using DbModel.DeviceConfig;
using System;
using System.Linq;

namespace Infrastructure.Service.DeviceConfig.Validators
{
    public static class StringValidator
    {
        public static bool Validate(string attributeValue, AttributeValueValidationMap validationDetails)
        {
            var validationDataType = Type.GetType(validationDetails.Validation.Type);
            if (validationDataType == typeof(string))
            {
                if (!attributeValue.Equals(validationDetails.Validation.ValidationData.ToString()))
                {
                    return false;
                }
            }
            else if (validationDataType == typeof(string[]))
            {
                var validationData = (string[])validationDetails.Validation.ValidationData;
                if (!validationData.Contains(attributeValue))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
