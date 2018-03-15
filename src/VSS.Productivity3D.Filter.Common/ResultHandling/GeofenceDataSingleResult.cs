using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Filter.Common.ResultHandling
{
  /// <summary>
  /// Single <see cref="MasterData.Models.Models.GeofenceData"/> descriptor.
  /// </summary>
  public class GeofenceDataSingleResult : ContractExecutionResult
  {
    public GeofenceDataSingleResult(GeofenceData geofenceData)
    {
      GeofenceData = geofenceData;
    }

    /// <summary>
    /// Gets or sets the <see cref="MasterData.Models.Models.GeofenceData"/> descriptor.
    /// </summary>
    public GeofenceData GeofenceData { get; set; }
  }
}