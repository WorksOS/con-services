using Microsoft.Extensions.Logging;
using VSS.TRex.Pipelines.Tasks;
using VSS.TRex.SubGridTrees.Client.Interfaces;

// Notes for consistent value accumulation and smoothing
// - Construct a rectangular array of values to contain the overall bounds of cells in the default north orientation of the cell selection
//    [as opposed to the project orienttion which is taken into account when the values are rendered]
// - Copy the values from the sub grids as they arrive into the task
// - Apply any required smoothing, gap filling etc, activities to the resulting grid of values once all subgrids have arrived
// - render the arrya of values into the tile as if it were a single large subgrid

namespace VSS.TRex.Rendering.Executors.Tasks
{
  /// <summary>
  /// A Task specialized towards rendering sub grid based information onto Plan View Map tiles
  /// </summary>
  public class PVMRenderingTask : PipelinedSubGridTask, IPVMRenderingTask
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<PVMRenderingTask>();

    public IPVMTaskAccumulator Accumulator { get; set; }

    /// <summary>
    /// The tile renderer responsible for processing sub grid information into tile based thematic rendering
    /// </summary>
    public PlanViewTileRenderer TileRenderer { get; set; }

    public PVMRenderingTask()
    {
    }

    public override bool TransferResponse(object response)
    {
      // Log.InfoFormat("Received a SubGrid to be processed: {0}", (response as IClientLeafSubGrid).Moniker());

      if (base.TransferResponse(response))
      {
        if (!(response is IClientLeafSubGrid[] subGridResponses) || subGridResponses.Length == 0)
        {
          Log.LogWarning("No sub grid responses returned");
        }
        else
        {
          return Accumulator?.Transcribe(subGridResponses) ?? false;
        }
      }

      return false;
    }

    // This code added to correctly implement the disposable pattern.
    public override void Dispose()
    {
      base.Dispose();

      TileRenderer = null;
    }
  }
}
