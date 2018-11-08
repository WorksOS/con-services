using System;
using Newtonsoft.Json;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models
{
    /// <summary>
    /// The request representation used to raise an alert for a tag file processing error if required.
    /// </summary>
    public class AppAlarmMessage
    {
        /// <summary>
        /// The code for app alarm
        /// </summary>
        [JsonProperty(PropertyName = "alarmType", Required = Required.Always)]
        public long alarmType { get; set; }

        /// <summary>
        /// The name of the error.
        /// </summary>
        [JsonProperty(PropertyName = "message", Required = Required.Always)]
        public string message { get; set; } = String.Empty;

        /// <summary>
        /// The exception message related to the error
        /// </summary>
        [JsonProperty(PropertyName = "exceptionMessage", Required = Required.Always)]
        public string exceptionMessage { get; set; } = String.Empty;

        /// <summary>
        /// Private constructor
        /// </summary>
        private AppAlarmMessage()
        {
        }

        /// <summary>
        /// Create instance of TagFileProcessingErrorRequest
        /// </summary>
        public static AppAlarmMessage CreateTagFileProcessingErrorRequest(long alarmType, string message,
            string exceptionMessage)
        {
            return new AppAlarmMessage
            {
                alarmType = alarmType,
                message = message,
                exceptionMessage = exceptionMessage
            };
        }


        /// <summary>
        /// Validates all properties
        /// </summary>
        public void Validate()
        {
        }
    }
}