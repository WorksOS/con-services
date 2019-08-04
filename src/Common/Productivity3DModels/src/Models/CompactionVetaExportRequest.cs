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
    [JsonProperty(Required = Required.Default)]
    public CoordType CoordType { get; private set; }

    /// <summary>
    /// which type of passes
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public OutputTypes OutputType { get; private set; }

    /// <summary>
    /// Used for format export data
    ///   note that some of these items are defaulted in 3dp due to issues with comma-delimitation etc
    /// </summary>
    [JsonProperty(PropertyName = "userPreferences", Required = Required.Always)]
    public UserPreferences UserPreferences { get; set; }

    /// <summary>
    /// Include the names of these machines
    /// </summary>
    /// 
    [JsonProperty(Required = Required.Default)]
    public string[] MachineNames { get; private set; }


    protected CompactionVetaExportRequest()
    {
    }

    public CompactionVetaExportRequest(
      Guid projectUid,
      FilterResult filter,
      string fileName,
      CoordType coordType,
      OutputTypes coordinateOutputType,
      UserPreferences userPreferences,
      string[] machineNames,
      OverridingTargets overrides,
      LiftSettings liftSettings
    ) : base(projectUid, filter, fileName, overrides, liftSettings)
    {
      CoordType = coordType;
      OutputType = coordinateOutputType;
      UserPreferences = userPreferences;
      MachineNames = machineNames;
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public override void Validate()
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
            "Invalid output type for veta export"));
      }

      if (MachineNames != null)
      {
        foreach (var machineName in MachineNames)
        {
          if (string.IsNullOrEmpty(machineName))
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                "Invalid machineNames"));
          }
        }
      }
    }
  }
}
