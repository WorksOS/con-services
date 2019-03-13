using System;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Enums;

namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// The request representation used to request export data.
  /// </summary>
  public class CompactionPassCountExportRequest : CompactionExportRequest
  {
    /// <summary>
    /// Type of Coordinates required in result e.g. NE
    /// </summary>
    [JsonProperty(PropertyName = "coordType", Required = Required.Default)]
    public CoordType CoordType { get; private set; }

    /// <summary>
    /// which type of passes
    /// </summary>
    [JsonProperty(PropertyName = "outputType", Required = Required.Default)]
    public OutputTypes OutputType { get; private set; }

    /// <summary>
    /// Used for format export data
    ///   note that some of these items are defaulted in 3dp due to issues with comma-delimitation etc
    /// </summary>
    [JsonProperty(PropertyName = "userPreferences", Required = Required.Always)]
    public UserPreferences UserPreferences { get; set; }

    /// <summary>
    /// Output .CSV file is restricted to 65535 rows if it is true.
    /// </summary>
    [JsonProperty(PropertyName = "restrictOutputSize", Required = Required.Default)]
    public bool RestrictOutputSize { get; protected set; }

    /// <summary>
    /// Column headers in an output .CSV file's are in the dBase format.
    /// </summary>
    [JsonProperty(PropertyName = "rawDataAsDBase", Required = Required.Default)]
    public bool RawDataAsDBase { get; protected set; }

    protected CompactionPassCountExportRequest()
    {
    }

    public CompactionPassCountExportRequest(
      Guid projectUid,
      FilterResult filter,
      string fileName,
      CoordType coordType,
      OutputTypes outputType,
      UserPreferences userPreferences,
      bool restrictOutputSize,
      bool rawDataAsDBase
    )
    {
      ProjectUid = projectUid;
      Filter = filter;
      FileName = fileName;
      CoordType = coordType;
      OutputType = outputType;
      UserPreferences = userPreferences;
      RestrictOutputSize = restrictOutputSize;
      RawDataAsDBase = rawDataAsDBase;
    }


    /// <summary>
    /// Validates all properties
    /// </summary>
    public new void Validate()
    {
      base.Validate();

      if (CoordType != CoordType.Northeast && CoordType != CoordType.LatLon)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Invalid coordinates type for export report"));
      }

      if (OutputType != OutputTypes.PassCountLastPass &&
          OutputType != OutputTypes.PassCountAllPasses)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Invalid output type for pass count export"));
      }

    }
  }
}
