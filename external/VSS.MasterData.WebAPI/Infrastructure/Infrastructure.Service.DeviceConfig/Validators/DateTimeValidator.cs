using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbModel.DeviceConfig;

namespace Infrastructure.Service.DeviceConfig.Validators
{
    public static class DateTimeValidator
    {
        public static bool Validate(string attributeValue, AttributeValueValidationMap validationDetails)
        {
            var validationDataType = Type.GetType(validationDetails.Validation.Type);
            if (validationDataType == typeof(DateTime))
            {
                bool isDate = DateTime.TryParse(attributeValue, out DateTime input);
                var validationData = Convert.ToDateTime(validationDetails.Validation.ValidationData);
                if (!(isDate && input == validationData))
                {
                    return false;
                }
            }
            else if (validationDataType == typeof(DateTime[]))
            {
                var validationData = (DateTime[])validationDetails.Validation.ValidationData;
                if (validationDetails.Validation.Validator == 0)
                {
                    bool isDate = DateTime.TryParse(attributeValue, out DateTime input);
                    if (!(isDate && validationData.Contains(input)))
                    {
                        return false;
                    }
                }
                else if (validationDetails.Validation.Validator == AttributeValidators.DateRange)
                {
                    var isTimespan = DateTime.TryParse(attributeValue, out DateTime input);
                    if (!(isTimespan && validationData.Length == 2 && validationData[0] < validationData[1] && input > validationData[0] && input < validationData[1]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
