using System.Collections.Generic;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.Productivity3D.TagFileAuth.Models.ResultsHandling;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Enums;

namespace VSS.Productivity3D.TagFileAuth.Models
{
  /// <summary>
  /// The request representation used to request the shortRaptorAssetID and serviceType 
  /// for a given machine whose tag-files are to be processed.
  /// The id of the project into which the tag-file data should be processed:
  ///             value of -1 indicates 'unknown' i.e. when the tag-files are being automatically processed.
  ///             value greater than zero is when the project is known i.e manual Import 
  /// </summary>
  public class GetAssetIdRequest 
  {
    /// <summary>
    /// The shortRaptorProjectId into which the tag-file data should be processed.
    /// </summary>
    [JsonProperty(PropertyName = "projectId", Required = Required.Always)]
    public long shortRaptorProjectId { get; set; }

    /// <summary>
    /// The device type of the machine.
    ///           Valid values are 0=Manual Device (John Doe machines)
    ///           and SNM940/SNM941/EC520. 
    /// </summary>
    [JsonProperty(PropertyName = "deviceType", Required = Required.Always)]
    public int deviceType { get; set; }

    /// <summary>
    /// The serial number (could be of a radio or ec device) of the machine from the tag-file.
    /// </summary>
    [JsonProperty(PropertyName = "serialNumber", Required = Required.Default)]
    public string serialNumber { get; set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private GetAssetIdRequest()
    {
    }

    /// <summary>
    /// Create instance of GetAssetIdRequest
    /// </summary>
    public static GetAssetIdRequest CreateGetAssetIdRequest(long shortRaptorProjectId, int deviceType, string serialNumber)
    {
      return new GetAssetIdRequest
      {
        shortRaptorProjectId = shortRaptorProjectId,
        deviceType = deviceType,
        serialNumber = serialNumber
      };
    }


    /// <summary>
    /// Validates shortRaptorAssetId and/or shortRaptorProjectId are provided
    /// </summary>
    public void Validate()
    {
      if (string.IsNullOrEmpty(serialNumber) && shortRaptorProjectId <= 0)
      {
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
          GetAssetIdResult.CreateGetAssetIdResult(false, -1, 0, 
            ContractExecutionStatesEnum.ValidationError, 24));
      }

      var allowedDeviceTypes = new List<int>() { (int) TagFileDeviceTypeEnum.ManualImport, (int) TagFileDeviceTypeEnum.SNM940, (int) TagFileDeviceTypeEnum.SNM941, (int) TagFileDeviceTypeEnum.EC520 };
      var isDeviceTypeValid = allowedDeviceTypes.Contains(deviceType);

      // rule changed to match cgen --> if a manualDeviceType, allow a serialNumber, EVEN  though the serialNumber is NEVER used
      if (!string.IsNullOrEmpty(serialNumber) && (!isDeviceTypeValid))
      {
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
          GetAssetIdResult.CreateGetAssetIdResult(false, -1, 0, 
            ContractExecutionStatesEnum.ValidationError, 25));
      }

      if (deviceType == 0 && shortRaptorProjectId <= 0)
      {
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
          GetAssetIdResult.CreateGetAssetIdResult(false, -1, 0, 
            ContractExecutionStatesEnum.ValidationError, 26));
      }

    }
  }
}
