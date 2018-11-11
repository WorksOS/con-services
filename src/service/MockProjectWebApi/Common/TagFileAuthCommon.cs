using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Globalization;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using ContractExecutionResult = VSS.MasterData.Models.ResultHandling.Abstractions.ContractExecutionResult;
using ContractExecutionStatesEnum = VSS.MasterData.Models.ResultHandling.Abstractions.ContractExecutionStatesEnum;


namespace MockProjectWebApi.Common
{


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
    public static AppAlarmMessage CreateTagFileProcessingErrorRequest(long alarmType, string message, string exceptionMessage)
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

  //////////////////////////////////////////////////////////////////////////////////////
  /// 
  /// 
  /// 


  public class TagFileProcessingErrorResult : ContractExecutionResultWithResult
  {
    /// <summary>
    /// Create instance of TagFileProcessingErrorResult
    /// </summary>
    public static TagFileProcessingErrorResult CreateTagFileProcessingErrorResult(bool result,
      int code = 0,
      int customCode = 0, string errorMessage1 = null, string errorMessage2 = null)
    {
      return new TagFileProcessingErrorResult
             {
               Result = result,
               Code = code,
               Message = code == 0 ? DefaultMessage : string.Format(_contractExecutionStatesEnum.FirstNameWithOffset(customCode), errorMessage1 ?? "null", errorMessage2 ?? "null")
             };
    }
  }

  public enum TagFileErrorsEnum
  {
    [Description("Unknown Project")]
    UnknownProject = -2,   // UnableToDetermineProjectID
    [Description("Unknown Cell")]
    UnknownCell = -1,      // NoValidCellPassesInTagfile
    // invalid None = 0, // used in raptor for indicating not set, but never send to logger as not valid enum
    [Description("Project: No matching date/time")]
    ProjectID_NoMatchingDateTime = 1,  // UnableToDetermineProjectID
    [Description("Project: No matching area")]
    ProjectID_NoMatchingArea = 2,    // UnableToDetermineProjectID
    [Description("Project: Multiple projects")]
    ProjectID_MultipleProjects = 3,   // UnableToDetermineProjectID
    [Description("Project: Invalid LLH/NEE position")]
    ProjectID_InvalidLLHNEPosition = 4,   // UnableToDetermineProjectID
    [Description("No valid cells: OnGround flag not set")]
    NoValidCells_OnGroundFlagNotSet = 5, // NoValidCellPassesInTagfile
    [Description("No valid cells: Invalid position")]
    NoValidCells_InValidPosition = 6, // NoValidCellPassesInTagfile
    [Description("Coordinate conversion failure")]
    CoordConversion_Failure = 7 // NoValidCellPassesInTagfile
  }


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
      var isDeviceTypeValid = ((DeviceTypeEnum)deviceType).ToString() != deviceType.ToString();

      deviceTypeString = isDeviceTypeValid ? ((DeviceTypeEnum)deviceType).ToString() : string.Format($"Invalid {deviceType}");
      return deviceTypeString;
    }

    public string DisplaySerialNumber()
    {
      var startPos = 0;
      var endPos = tagFileName.IndexOf("--", StringComparison.Ordinal);
      return tagFileName.Substring(0, endPos - startPos).Trim();
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
      /*
      Guid tccOrgUID = Guid.Empty;
      if (!string.IsNullOrEmpty(tccOrgId) && !Guid.TryParseExact(tccOrgId, "D", out tccOrgUID))
      {
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.ValidationError, 1));
      }
      */
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





  //////////////////////////////////////////////////////////////////////////////////////
  //GetBoundaries


  public class ProjectBoundaryPackage
  {
    public long ProjectID;
    public TWGS84FenceContainer Boundary;
  }

  public class GetProjectBoundariesAtDateResult : ContractExecutionResultWithResult
  {
    /// <summary>
    /// The boundaries of the projects. Empty if none.
    /// </summary>
    public ProjectBoundaryPackage[] projectBoundaries { get; set; }

    /// <summary>
    /// Create instance of GetProjectBoundariesAtDateResult
    /// </summary>
    public static GetProjectBoundariesAtDateResult CreateGetProjectBoundariesAtDateResult(bool result,
      ProjectBoundaryPackage[] projectBoundaries,
      int code = 0,
      int customCode = 0, string errorMessage1 = null, string errorMessage2 = null)
    {
      return new GetProjectBoundariesAtDateResult
             {
               Result = result,
               projectBoundaries = projectBoundaries,
               Code = code,
               Message = code == 0 ? DefaultMessage : string.Format(_contractExecutionStatesEnum.FirstNameWithOffset(customCode), errorMessage1 ?? "null", errorMessage2 ?? "null")
             };
    }

  }



  /// </summary>
  public class GetProjectBoundariesAtDateRequest : ContractRequest
  {
    /// <summary>
    /// The id of the asset owned by the customer whose active project boundaries are returned. 
    /// </summary>
    [JsonProperty(PropertyName = "assetId", Required = Required.Always)]
    public long assetId { get; set; }

    /// <summary>
    /// The date time from the tag file which must be within the active project date range. 
    /// </summary>
    [JsonProperty(PropertyName = "tagFileUTC", Required = Required.Always)]
    public DateTime tagFileUTC { get; set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private GetProjectBoundariesAtDateRequest()
    {
    }

    /// <summary>
    /// Create instance of GetProjectBoundariesAtDateRequest
    /// </summary>
    public static GetProjectBoundariesAtDateRequest CreateGetProjectBoundariesAtDateRequest(long assetId,
      DateTime tagFileUTC)
    {
      return new GetProjectBoundariesAtDateRequest
             {
               assetId = assetId,
               tagFileUTC = tagFileUTC
             };
    }


    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      if (assetId <= 0)
      {
        //   throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
        //   GetProjectBoundariesAtDateResult.CreateGetProjectBoundariesAtDateResult(false, new ProjectBoundaryPackage[0],
        //   ResultHandling.ContractExecutionStatesEnum.ValidationError, 9));
      }

      if (!(tagFileUTC > DateTime.UtcNow.AddYears(-50) && tagFileUTC <= DateTime.UtcNow.AddDays(30)))
      {
        //throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
        //GetProjectBoundariesAtDateResult.CreateGetProjectBoundariesAtDateResult(false, new ProjectBoundaryPackage[0],
        //ResultHandling.ContractExecutionStatesEnum.ValidationError, 17));
      }
    }
  }

  //////////////////////////////////////////////////////////////////////////////////////

  public class TWGS84Point
  {
    // Note: Lat and Lon expressed as radians
    public double Lat;
    public double Lon;

    public TWGS84Point(double ALon, double ALat) { Lat = ALat; Lon = ALon; }

    public override bool Equals(object obj)
    {
      var otherPoint = obj as TWGS84Point;
      if (otherPoint == null) return false;
      return otherPoint.Lat == this.Lat
             && otherPoint.Lon == this.Lon
          ;
    }
    public override int GetHashCode() { return 0; }

  }

  public class TWGS84FenceContainer
  {
    public TWGS84Point[] FencePoints = null;
  }

  public class GetProjectBoundaryAtDateResult : ContractExecutionResultWithResult
  {
    /// <summary>
    /// The boundary of the project. Empty if none.
    /// </summary>
    public TWGS84FenceContainer projectBoundary { get; set; }

    /// <summary>
    /// Create instance of GetProjectBoundaryAtDateResult
    /// </summary>
    public static GetProjectBoundaryAtDateResult CreateGetProjectBoundaryAtDateResult(bool result,
      TWGS84FenceContainer projectBoundary,
      int code = 0,
      int customCode = 0, string errorMessage1 = null, string errorMessage2 = null)
    {
      return new GetProjectBoundaryAtDateResult
             {
               Result = result,
               projectBoundary = projectBoundary,
               Code = code,
               Message = code == 0 ? DefaultMessage : string.Format(_contractExecutionStatesEnum.FirstNameWithOffset(customCode), errorMessage1 ?? "null", errorMessage2 ?? "null")
             };
    }
  }



  public class GetProjectBoundaryAtDateRequest : ContractRequest
  {
    /// <summary>
    /// The id of the project to get the boundary of. 
    /// </summary>
    [JsonProperty(PropertyName = "projectId", Required = Required.Always)]
    public long projectId { get; set; }

    /// <summary>
    /// The date time from the tag file which must be within the active project date range. 
    /// </summary>
    [JsonProperty(PropertyName = "tagFileUTC", Required = Required.Always)]
    public DateTime tagFileUTC { get; set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private GetProjectBoundaryAtDateRequest()
    {
    }

    /// <summary>
    /// Create instance of GetProjectBoundaryAtDateRequest
    /// </summary>
    public static GetProjectBoundaryAtDateRequest CreateGetProjectBoundaryAtDateRequest(long projectId,
      DateTime tagFileUTC)
    {
      return new GetProjectBoundaryAtDateRequest
             {
               projectId = projectId,
               tagFileUTC = tagFileUTC
             };
    }


    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      if (projectId <= 0)
      {
        //   throw new ServiceException(HttpStatusCode.BadRequest,
        //   GetProjectBoundaryAtDateResult.CreateGetProjectBoundaryAtDateResult(false, new TWGS84FenceContainer(),
        //   ResultHandling.ContractExecutionStatesEnum.ValidationError, 18));
      }

      if (!(tagFileUTC > DateTime.UtcNow.AddYears(-50) && tagFileUTC <= DateTime.UtcNow.AddDays(30)))
      {
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
          GetProjectBoundaryAtDateResult.CreateGetProjectBoundaryAtDateResult(false, new TWGS84FenceContainer(),
            ContractExecutionStatesEnum.ValidationError, 17));
      }
    }
  }



  //////////////////////////////////////////////////////////////////////////////////////



  public class ContractExecutionResultWithResult : ContractExecutionResult
  {
    protected static ContractExecutionStatesEnum _contractExecutionStatesEnum = new ContractExecutionStatesEnum();

    public bool Result { get; set; } = false;
  }


  public class GetProjectIdResult : ContractExecutionResultWithResult
  {
    /// <summary>
    /// The id of the project. -1 if none.
    /// </summary>
    public long projectId { get; set; }

    /// <summary>
    /// Create instance of GetProjectIdResult
    /// </summary>
    public static GetProjectIdResult CreateGetProjectIdResult(bool result, long projectId,
      int code = 0,
      int customCode = 0, string errorMessage1 = null, string errorMessage2 = null)
    {
      return new GetProjectIdResult
             {
               Result = result,
               projectId = projectId,
               Code = code,
               Message = code == 0
                 ? DefaultMessage
                 : string.Format(_contractExecutionStatesEnum.FirstNameWithOffset(customCode), errorMessage1 ?? "null",
                   errorMessage2 ?? "null")
             };
    }

  }

  public class ContractRequest
  {
  }

  public class GetProjectIdRequest : ContractRequest
  {
    /// <summary>
    /// The id of the asset whose tagfile is to be processed. A value of -1 indicates 'none' so all assets are considered (depending on tccOrgId). 
    /// </summary>
    [JsonProperty(PropertyName = "assetId", Required = Required.Always)]
    public long assetId { get; set; }

    /// <summary>
    /// WGS84 latitude in decimal degrees. 
    /// </summary>
    [JsonProperty(PropertyName = "latitude", Required = Required.Always)]
    public double latitude { get; set; }

    /// <summary>
    /// WGS84 longitude in decimal degrees. 
    /// </summary>    
    [JsonProperty(PropertyName = "longitude", Required = Required.Always)]
    public double longitude { get; set; }

    /// <summary>
    /// Elevation in meters. 
    /// </summary>
    [JsonProperty(PropertyName = "height", Required = Required.Always)]
    public double height { get; set; }

    /// <summary>
    /// Date and time the asset was at the given location. 
    /// </summary>
    [JsonProperty(PropertyName = "timeOfPosition", Required = Required.Always)]
    public DateTime timeOfPosition { get; set; }

    /// <summary>
    /// Date and time the asset was at the given location. 
    /// </summary>
    [JsonProperty(PropertyName = "tccOrgUid", Required = Required.Default)]
    public string tccOrgUid { get; set; }


    /// <summary>
    /// Private constructor
    /// </summary>
    private GetProjectIdRequest()
    {
    }

    /// <summary>
    /// Create instance of GetProjectIdRequest
    /// </summary>
    public static GetProjectIdRequest CreateGetProjectIdRequest(long assetId, double latitude, double longitude,
      double height, DateTime timeOfPosition, string tccOrgUid)
    {
      return new GetProjectIdRequest
             {
               assetId = assetId,
               latitude = latitude,
               longitude = longitude,
               height = height,
               timeOfPosition = timeOfPosition,
               tccOrgUid = tccOrgUid
             };
    }


    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      if (assetId <= 0 && string.IsNullOrEmpty(tccOrgUid))
      {
        //  throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
        //  GetProjectIdResult.CreateGetProjectIdResult(false, -1,
        //  ResultHandling.ContractExecutionStatesEnum.ValidationError, 20));
      }

      if (latitude < -90 || latitude > 90)
      {
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
          GetProjectIdResult.CreateGetProjectIdResult(false, -1,
            ContractExecutionStatesEnum.ValidationError, 21));
      }

      if (longitude < -180 || longitude > 180)
      {
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
          GetProjectIdResult.CreateGetProjectIdResult(false, -1,
            ContractExecutionStatesEnum.ValidationError, 22));
      }

      if (!(timeOfPosition > DateTime.UtcNow.AddYears(-50) && timeOfPosition <= DateTime.UtcNow.AddDays(30)))
      {
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
          GetProjectIdResult.CreateGetProjectIdResult(false, -1,
            ContractExecutionStatesEnum.ValidationError, 23));
      }

    }





  }


  /// <summary>
  /// The result of a request to get the legacyAssetID for a radioSerial
  /// </summary>
  public class GetAssetIdResult : ContractExecutionResultWithResult
  {
    /// <summary>
    /// The id of the asset. -1 if unknown. 
    /// </summary>
    public long assetId { get; set; }

    /// <summary>
    /// The subscription level of the asset. 
    /// Valid values are 0=Unknown, 15=2D Project Monitoring, 16=3D Project Monitoring, 18=Manual 3D Project Monitoring
    /// </summary>
    public int machineLevel { get; set; }

    /// <summary>
    /// Create instance of GetAssetIdResult
    /// </summary>
    public static GetAssetIdResult CreateGetAssetIdResult(bool result, long assetId, int machineLevel,
      int code = 0,
      int customCode = 0, string errorMessage1 = null, string errorMessage2 = null)
    {
      return new GetAssetIdResult
             {
               Result = result,
               assetId = assetId,
               machineLevel = machineLevel,
               Code = code,
               Message = code == 0 ? DefaultMessage : string.Format(_contractExecutionStatesEnum.FirstNameWithOffset(customCode), errorMessage1 ?? "null", errorMessage2 ?? "null")
             };
    }
  }



  public class GetAssetIdRequest : ContractRequest
  {
    /// <summary>
    /// The id of the project into which the tagfile data should be processed. A value of -1 indicates 'unknown' 
    /// which is when the tagfiles are being automatically processed. A value greater than zero is when the project 
    /// is known which is when a tagfile is being manually imported by a user.
    /// </summary>
    [JsonProperty(PropertyName = "projectId", Required = Required.Always)]
    public long projectId { get; set; }

    /// <summary>
    /// The device type of the machine. Valid values are 0=Manual Device (John Doe machines) and 6=SNM940 (torch machines).
    /// </summary>
    [JsonProperty(PropertyName = "deviceType", Required = Required.Always)]
    public int deviceType { get; set; }

    /// <summary>
    /// The radio serial number of the machine from the tagfile.
    /// </summary>
    [JsonProperty(PropertyName = "radioSerial", Required = Required.Default)]
    public string radioSerial { get; set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private GetAssetIdRequest()
    {
    }

    /// <summary>
    /// Create instance of GetAssetIdRequest
    /// </summary>
    public static GetAssetIdRequest CreateGetAssetIdRequest(long projectId, int deviceType, string radioSerial)
    {
      return new GetAssetIdRequest
             {
               projectId = projectId,
               deviceType = deviceType,
               radioSerial = radioSerial
             };
    }

   
    /// <summary>
    /// Validates assetID And/or projectID is provided
    /// </summary>
    public void Validate()
    {
      if (string.IsNullOrEmpty(radioSerial) && projectId <= 0)
      {
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
          GetAssetIdResult.CreateGetAssetIdResult(false, -1, 0,
            ContractExecutionStatesEnum.ValidationError, 24));
      }

      // if the number is not in enum then it returns the number
      var isDeviceTypeValid = (((DeviceTypeEnum)deviceType).ToString() != deviceType.ToString());

      if (!string.IsNullOrEmpty(radioSerial) && (deviceType < 1 || !isDeviceTypeValid))
      {
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
          GetAssetIdResult.CreateGetAssetIdResult(false, -1, 0,
            ContractExecutionStatesEnum.ValidationError, 25));
      }

      if (deviceType == 0 && projectId <= 0)
      {
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
          GetAssetIdResult.CreateGetAssetIdResult(false, -1, 0,
            ContractExecutionStatesEnum.ValidationError, 26));
      }

    }
  }

  //////////////////////////////////////////////////////////////////////////////////////







}
