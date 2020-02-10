using System;
using DbModel.DeviceConfig;

namespace Infrastructure.Service.DeviceConfig.Validators
{
    public static class EnumValidator
    {
        public static bool Validate(string attributeValue, AttributeValueValidationMap validationDetails)
        {
            var validationDataType = Type.GetType(validationDetails.Validation.Type);
            bool isNumber = int.TryParse(attributeValue, out int input);
            if (!(isNumber ? Enum.IsDefined(validationDataType, input) : Enum.IsDefined(validationDataType, attributeValue)))
            {
                return false;
            }
            return true;
        }
    }
}
