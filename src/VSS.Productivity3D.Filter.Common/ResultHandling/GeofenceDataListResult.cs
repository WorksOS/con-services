using System.Collections.Immutable;
using VSS.Common.ResultsHandling;
using VSS.MasterData.Models.Models;

namespace VSS.Productivity3D.Filter.Common.ResultHandling
{
  /// <summary>
  /// Collection of <see cref="MasterData.Models.Models.GeofenceData"/> descriptor objects.
  /// </summary>
  /// <seealso cref="ContractExecutionResult" />
  public class GeofenceDataListResult : ContractExecutionResult
  {
    /// <summary>
    /// Gets or sets a collection of <see cref="MasterData.Models.Models.GeofenceData"/> descriptors.
    /// </summary>
    public ImmutableList<GeofenceData> GeofenceData { get; set; }
  }
}