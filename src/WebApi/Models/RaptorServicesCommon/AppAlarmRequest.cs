using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace VSS.TagFileAuth.Service.Models.RaptorServicesCommon
{
  /// <summary>
  /// The request representation used to raise an application alarm.
  /// </summary>
  public class AppAlarmRequest //: IValidatable, IServiceDomainObject, IHelpSample
  {
    /// <summary>
    /// The type of message: Assert, Debug, Error, Exception, Message, Warning. 
    /// Only Error, Assert and Exception will raise an app alarm.
    /// </summary>
    [Required]
    [JsonProperty(PropertyName = "alarmType", Required = Required.Always)]
    public TSigLogMessageClass alarmType { get; private set; }

    /// <summary>
    /// The text of the message. 
    /// </summary>
    [Required]
    [JsonProperty(PropertyName = "message", Required = Required.Always)]
    public string message { get; private set; }

    /// <summary>
    /// The message from an exception. 
    /// </summary>
    [Required]
    [JsonProperty(PropertyName = "exceptionMessage", Required = Required.Always)]
    public string exceptionMessage { get; private set; }

    ///// <summary>
    ///// Private constructor
    ///// </summary>
    //private AppAlarmRequest()
    //{ }

    /// <summary>
    /// Create instance of AppAlarmRequest
    /// </summary>
    public static AppAlarmRequest CreateAppAlarmRequest(
      TSigLogMessageClass alarmType,
      string message,
      string exceptionMessage
      )
    {
      return new AppAlarmRequest
      {
        alarmType = alarmType,
        message = message,
        exceptionMessage = exceptionMessage
      };
    }

    /// <summary>
    /// Example for Help
    /// </summary>
    public static AppAlarmRequest HelpSample
    {
      get
      {
        return CreateAppAlarmRequest(TSigLogMessageClass.slmcError, "***ERROR*** Summary Volumes Profile BuildCellPassProfile. Less than 2 coordinates passed for profile.", string.Empty);
      }
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
    }
  }
}