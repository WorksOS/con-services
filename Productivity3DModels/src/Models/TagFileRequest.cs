

using System;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.FIlters;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// TAG file domain object. Model represents TAG file submitted to Raptor.
  /// The project, identified by ID/UID, to process the TAG file into. These project identifires are optional. 
  /// If not set, Raptor will determine automatically which project the TAG file should be processed into. 
  /// When provided it acts as an override value. 
  /// </summary>
  public class TagFileRequest // : IValidatable
  {
    /// <summary>
    /// Project UID 
    /// </summary>
    [JsonProperty]
    public Guid? ProjectUID { get; private set; }


    /// <summary>
    /// Asset UID 
    /// </summary>
    [JsonProperty]
    public Guid? AssetUID { get; private set; }


    /// <summary>
    /// The name of the TAG file.
    /// </summary>
    /// <value>Required. Shall contain only ASCII characters. Maximum length is 256 characters.</value>
    [JsonProperty(Required = Required.Always)]
    [ValidFilename(256)]
    public string FileName { get; private set; }

    /// <summary>
    /// The content of the TAG file as an array of bytes.
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public byte[] Data { get; private set; }

    /// <summary>
    /// The boundary of the project to process the TAG file into. If the location of the data in the TAG file is outside of this boundary it will not be processed into the project.
    /// May be null.
    /// </summary>
    [JsonProperty]
    public WGS84Fence Boundary { get; private set; }

    /// <summary>
    /// The machine (asset) ID to process the TAG file as. When not provided the TagProc service will use the project listener to determine the machine/asset ID. When provided it acts as an override value.
    /// May be null.
    /// </summary>
    [JsonProperty]
    public long? MachineId { get; private set; }

    [JsonProperty]
    public string TccOrgId { get; private set; }

    /// <summary>
    /// Default private constructor.
    /// </summary>
    private TagFileRequest()
    {
    }

    /// <summary>
    /// Static constructor.
    /// </summary>
    public static TagFileRequest CreateTagFile(string fileName,
        byte[] data,
        Guid? ProjectUID,
        WGS84Fence boundary,
        long machineId,
        bool convertToCsv,
        bool convertToDxf,
        string tccOrgId = null)
    {
      return new TagFileRequest
             {
                 FileName = fileName,
                 Data = data,
                 ProjectUID = ProjectUID,
                 Boundary = boundary,
                 MachineId = machineId,
                 TccOrgId = tccOrgId
             };
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      if (Data == null || !Data.Any())
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                "Data cannot be null"));
      }
    }

  }
}
