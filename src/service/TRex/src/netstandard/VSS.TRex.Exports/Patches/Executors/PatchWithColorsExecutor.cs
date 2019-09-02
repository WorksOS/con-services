using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Common.RequestStatistics;
using VSS.TRex.Exports.Patches.GridFabric.PatchRequestWithColors;

namespace VSS.TRex.Exports.Patches.Executors
{
  /// <summary>
  /// Generates a patch of sub grids from a wider query
  /// </summary>
  public class PatchWithColorsExecutor
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

    /// <summary>
    /// The response object available for inspection once the Executor has completed processing
    /// </summary>
    public PatchRequestWithColorsResponse PatchSubGridsResponse { get; } = new PatchRequestWithColorsResponse();

    /// <summary>
    /// The TRex application service node performing the request
    /// </summary>
    private string RequestingTRexNodeID { get; }

    private Guid DataModelID;
    private DisplayMode Mode;

    /// <summary>
    /// Executor that implements requesting and rendering sub grid information to create the rendered tile
    /// </summary>
    /// <returns></returns>
    public async Task<bool> ExecuteAsync()
    {
      Log.LogInformation($"Performing Execute for DataModel:{DataModelID}, Mode={Mode}, RequestingNodeID={RequestingTRexNodeID}");

      ApplicationServiceRequestStatistics.Instance.NumSubgridPageRequests.Increment();

      // TODO: Some implementation here...

      return await Task.FromResult(true);
    }
  }
}
