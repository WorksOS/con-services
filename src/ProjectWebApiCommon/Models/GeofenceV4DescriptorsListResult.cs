using System.Collections.Immutable;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.MasterData.Project.WebAPI.Common.Models
{

  /// <summary>
  /// List of geofences descriptors
  /// </summary>
  /// <seealso cref="ContractExecutionResult" />
  public class GeofenceV4DescriptorsListResult : ContractExecutionResult
  {
    /// <summary>
    /// Gets or sets the project descriptors.
    /// </summary>
    /// <value>
    /// The project descriptors.
    /// </value>
    public ImmutableList<GeofenceV4Descriptor> GeofenceDescriptors { get; set; }
  }
}
