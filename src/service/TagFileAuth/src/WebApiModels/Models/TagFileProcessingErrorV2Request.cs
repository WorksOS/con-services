using System;
using System.Globalization;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Enums;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;
using ContractExecutionStatesEnum = VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling.ContractExecutionStatesEnum;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models
{
  /// <summary>
  /// The request representation used to raise an alert for a tag file processing error if required.
  /// </summary>
  public class TagFileProcessingErrorV2Request
  {
    /// <summary>
    /// The id reflecting our customer in TCC whose tag file has the error. 
    /// </summary>
    [JsonProperty(PropertyName = "tccOrgId", Required = Required.Default)]
    public string tccOrgId { get; set; }
    
    /// <summary>
                                            /// The id of the asset whose tag file has the error. 
                                            /// </summary>
    [JsonProperty(PropertyName = "assetId", Required = Required.Default)]
    public long? assetId { get; set; }

    /// <summary>
    /// The id of the project into which the tagfile data was identified, but unable to be processed into. 
    /// Are there other possible values e.g. -1, -2, -3?
    /// </summary>
    [JsonProperty(PropertyName = "projectId", Required = Required.Default)]
    public int? projectId { get; set; } = -1;

    
    [JsonProperty(PropertyName = "error", Required = Required.Always)]
    public TagFileErrorsEnum error { get; set; }

    /// <summary>
    /// The name of the tag file with the error.
    /// '0346J084SW--ALJON 600 0102--160819203837.tag' (Format: Display Serial--Machine Name--DateTime.tag yymmddhhmmss)
    /// </summary>
    [JsonProperty(PropertyName = "tagFileName", Required = Required.Always)]
    public string tagFileName { get; set; } = String.Empty;


    /// <summary>
    /// The radioSerial whose tag file has the error. 
    /// </summary>
    [JsonProperty(PropertyName = "deviceSerialNumber", Required = Required.Default)]
    public string deviceSerialNumber { get; set; }

    /// <summary>
    /// The device type of the machine. Valid values are 0=Manual Device (John Doe machines) and 6=SNM940 (torch machines).
    /// </summary>
    [JsonProperty(PropertyName = "deviceType", Required = Required.Default)]
    public int? deviceType { get; set; }

    public string DeviceTypeString()
    {
      string deviceTypeString = string.Empty;
      var isDeviceTypeValid = ((DeviceTypeEnum) deviceType).ToString() != deviceType.ToString();

      deviceTypeString = isDeviceTypeValid ? ((DeviceTypeEnum)deviceType).ToString() : string.Format($"Invalid {deviceType}");
      return deviceTypeString;
    }
    
    public string DisplaySerialNumber()
    {
      var startPos = 0;
      var endPos = tagFileName.IndexOf("--", StringComparison.Ordinal);
      return tagFileName.Substring(0, endPos-startPos).Trim(); 
    }

    public string MachineName()
    {
      var startPos = tagFileName.IndexOf("--", StringComparison.Ordinal) + 2;
      var endPos = tagFileName.LastIndexOf("--", StringComparison.Ordinal);
      return tagFileName.Substring(startPos, endPos - startPos).Trim();
    }
    
    public DateTime TagFileDateTimeUtc()
    {
      var startPos = tagFileName.LastIndexOf("--", StringComparison.Ordinal) + 2;
      var endPos = tagFileName.LastIndexOf(".tag", StringComparison.Ordinal);
      endPos = endPos > -1 ? endPos : tagFileName.Length;

      CultureInfo enUs = new CultureInfo("en-US");
      DateTime dtUtc = DateTime.MinValue;
      DateTime.TryParseExact(tagFileName.Substring(startPos, endPos - startPos).Trim(), "yyMMddHHmmss", enUs, DateTimeStyles.None, out dtUtc);
      return dtUtc;
    }


    /// <summary>
    /// Private constructor
    /// </summary>
    private TagFileProcessingErrorV2Request()
    {
    }

    /// <summary>
    /// Create instance of TagFileProcessingErrorRequest
    /// </summary>
    public static TagFileProcessingErrorV2Request CreateTagFileProcessingErrorRequest
      (string tccOrgId, long? assetId, int? projectId, 
       int error, string tagFileName,
       string deviceSerialNumber,
       int deviceType = 0)
    {
      return new TagFileProcessingErrorV2Request
      {
        tccOrgId = string.IsNullOrEmpty(tccOrgId) ? "" : tccOrgId.Trim(),
        assetId = assetId,
        projectId = projectId,
        error = (TagFileErrorsEnum)Enum.ToObject(typeof(TagFileErrorsEnum), error),
        tagFileName = string.IsNullOrEmpty(tagFileName) ? "" : tagFileName.Trim(),
        deviceSerialNumber = string.IsNullOrEmpty(deviceSerialNumber) ? "" : deviceSerialNumber.Trim(),
        deviceType = deviceType
      };
    }


    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      Guid tccOrgUID = Guid.Empty;
      if (!string.IsNullOrEmpty(tccOrgId) && !Guid.TryParseExact(tccOrgId, "D", out tccOrgUID))
      {
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.ValidationError, 1));
      }

      if (assetId.HasValue && assetId < -1)
      {
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.ValidationError, 2));
      }

      if (projectId.HasValue && projectId < -3 || projectId == 0)
      {
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.ValidationError, 3));
      }


      if (Enum.IsDefined(typeof(TagFileErrorsEnum), error) == false)
      {
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.ValidationError, 4));
      }
      
      if (string.IsNullOrEmpty(tagFileName))
      {
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.ValidationError, 5));
      }

      //  no validation required on deviceSerialNumber,


      // if the number is not in enum then it returns the number
      var isDeviceTypeValid = (((DeviceTypeEnum)deviceType).ToString() != deviceType.ToString());

      if (deviceType > 0 && !isDeviceTypeValid)
      {
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.ValidationError, 30));
      }

      if (string.IsNullOrEmpty(DisplaySerialNumber()))
      {
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.ValidationError, 6));
      }

      if (string.IsNullOrEmpty(MachineName()))
      {
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.ValidationError, 7));
      }

      if (TagFileDateTimeUtc() == DateTime.MinValue)
      {
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.ValidationError, 8));
      }

    }
  }
}