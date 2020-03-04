using System;
using System.Linq;
using DbModel.DeviceConfig;

namespace Infrastructure.Service.DeviceConfig.Validators
{
    public static class IntegerValidator
    {
        public static bool Validate(string attributeValue, AttributeValueValidationMap validationDetails)
        {
            var validationDataType = Type.GetType(validationDetails.Validation.Type);
            if (validationDataType == typeof(int))
            {
                var validationData = Convert.ToInt32(validationDetails.Validation.ValidationData);
                var inputValue = Convert.ToInt32(attributeValue);
                if (inputValue != validationData)
                {
                    return false;
                }
            }
            else if (validationDataType == typeof(int[]))
            {
                var validationData = (int[])validationDetails.Validation.ValidationData;
                if (validationDetails.Validation.Validator == 0)
                {
                    var input = Convert.ToInt32(attributeValue);
                    if (!validationData.Contains(input))
                    {
                        return false;
                    }
                }
                else if (validationDetails.Validation.Validator == AttributeValidators.TimespanRangeInSeconds)
                {
                    bool isNumber = int.TryParse(attributeValue, out int timespanvalue);
                    TimeSpan input = default(TimeSpan);
                    if (isNumber)
                    {
                        switch (validationDetails.Validation.TimespanToken)
                        {
                            case TimespanToken.Days:
                                input = TimeSpan.FromDays(timespanvalue);
                                break;
                            case TimespanToken.Hours:
                                input = TimeSpan.FromHours(timespanvalue);
                                break;
                            case TimespanToken.Minutes:
                                input = TimeSpan.FromMinutes(timespanvalue);
                                break;
                            case TimespanToken.Seconds:
                                input = TimeSpan.FromSeconds(timespanvalue);
                                break;
                        }
                        if (!(validationData.Length == 2 && validationData[0] < validationData[1] && input.TotalSeconds >= validationData[0] && input.TotalSeconds <= validationData[1]))
                        {
                            return false;
                        }
                    }
                    else
                        return false;
                }
                else if (validationDetails.Validation.Validator == AttributeValidators.Range)
                {
                    var input = Convert.ToInt32(attributeValue);
                    if (!(validationData.Length == 2 && validationData[0] < validationData[1] && input >= validationData[0] && input <= validationData[1]))
                    {
                        return false;
                    }
                }
                else if (validationDetails.Validation.Validator == AttributeValidators.Timespan)
                {
                    var isTimespan = TimeSpan.TryParse(attributeValue, out TimeSpan input);
                    //Hours validation
                    if (validationData.Length == 2)
                        if (!(
                        isTimespan &&
                        validationData[0] < validationData[1] &&
                        input.Hours >= validationData[0] &&
                        input.Hours <= validationData[1]))
                        {
                            return false;
                        }
                    //Hours & Minutes validation
                    if (validationData.Length == 4)
                        if (!(
                        isTimespan &&
                        validationData[0] < validationData[1] &&
                        validationData[2] < validationData[3] &&
                        input.Hours >= validationData[0] &&
                        input.Hours <= validationData[1] &&
                        input.Minutes >= validationData[2] &&
                        input.Minutes <= validationData[3]))
                        {
                            return false;
                        }
                    //Hours, Minutes & Seconds validation
                    if (validationData.Length == 6)
                        if (!(
                        isTimespan &&
                        validationData[0] < validationData[1] &&
                        validationData[2] < validationData[3] &&
                        validationData[4] < validationData[5] &&
                        input.Hours >= validationData[0] &&
                        input.Hours <= validationData[1] &&
                        input.Minutes >= validationData[2] &&
                        input.Minutes <= validationData[3] &&
                        input.Seconds >= validationData[4] &&
                        input.Seconds <= validationData[5]))
                        {
                            return false;
                        }
                }
            }
            return true;
        }
    }
}
