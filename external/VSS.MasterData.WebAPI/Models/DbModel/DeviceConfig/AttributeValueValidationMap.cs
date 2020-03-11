//using Infrastructure.Common.DeviceSettings.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DbModel.DeviceConfig
{
    public class AttributeValueValidationMap : ICommonValidatorBase
    {
        public string Group { get; set; }
        public string Parameter { get; set; }
        public string Attribute { get; set; }
        public string AttributeValue { get; set; }
        public ValidationDetails Validation { get; set; }
    }
    public class ValidationDetails
    {
        public string Type { get; set; }
        public object ValidationData { get; set; }
        public AttributeValidators Validator { get; set; }
        //TODO: Commented out, since others are waiting, Please add them later
        //public ErrorCodes ErrorCode { get; set; }
        public TimespanToken TimespanToken { get; set; }
    }
    public enum AttributeValidators
    {
        Range = 1,
        /// <summary>
        /// <para>For this validator timespan format should be like "hh\:mm\:ss" or "mm\:ss" or "ss"</para>
        /// </summary>
        TimespanRangeInSeconds,
        DateRange,
        Timespan
    }
    public enum TimespanToken
    {
        Days = 1,
        Hours,
        Minutes,
        Seconds
    }
}
