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
  public class CompactionVetaExportRequest : CompactionExportRequest
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
    /// Include data gathered from these machine names
    /// </summary>
    /// 
    [JsonProperty(PropertyName = "machineNames", Required = Required.Default)]
    public string[] MachineNames { get; private set; }


    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    /// <param name="projectUid"></param>
    /// <param name="filter"></param>
    /// <param name="fileName"></param>
    /// <param name="coordType"></param>
    /// <param name="machineNames"></param>
    public CompactionVetaExportRequest(
      Guid projectUid,
      FilterResult filter,
      string fileName,
      CoordType coordType,
      OutputTypes outputType,
      string[] machineNames
    )
    {
      ProjectUid = projectUid;
      Filter = filter;
      FileName = fileName;
      CoordType = coordType;
      OutputType = outputType;
      MachineNames = machineNames;
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

      if (OutputType != OutputTypes.VedaFinalPass &&
          OutputType != OutputTypes.VedaAllPasses)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Invalid output type for machine passes export report for VETA"));
      }

      // todo UserPreferences
    }
  }
}
