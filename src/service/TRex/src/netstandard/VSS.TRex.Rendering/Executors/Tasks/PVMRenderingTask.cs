using System.Linq;
using Microsoft.Extensions.Logging;
using System.Reflection;
using VSS.TRex.Pipelines.Tasks;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Core;
using VSS.TRex.SubGridTrees.Factories;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Types;

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
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

    public ISubGridTree SubGridTree { get; private set; }
    public PVMTaskAccumulator Accumulator { get; set; }

    /// <summary>
    /// The tile renderer responsible for processing sub grid information into tile based thematic rendering
    /// </summary>
    public PlanViewTileRenderer TileRenderer { get; set; }

    public PVMRenderingTask()
    {
    }


    private bool AddSubGridToTree(IClientLeafSubGrid leaf)
    {
      if (SubGridTree == null)
        SubGridTree = new SubGridTree(leaf.Level, leaf.CellSize, new SubGridFactory<NodeSubGrid, LeafSubGrid>());

      leaf.Owner = SubGridTree;

      var node = SubGridTree.ConstructPathToCell(leaf.OriginX, leaf.OriginY, SubGridPathConstructionType.CreatePathToLeaf);
      node.GetSubGridCellIndex(leaf.OriginX, leaf.OriginY, out var subGridX, out var subGridY);
      node.SetSubGrid(subGridX, subGridY, leaf);

      return true;
    }

    public override bool TransferResponse(object response)
    {
      // Log.InfoFormat("Received a SubGrid to be processed: {0}", (response as IClientLeafSubGrid).Moniker());
      var result = false;

      if (base.TransferResponse(response))
      {
        if (!(response is IClientLeafSubGrid[] subGridResponses) || subGridResponses.Length == 0)
        {
          Log.LogWarning("No sub grid responses returned");
        }
        else
        {
          result = Accumulator?.Transcribe(subGridResponses) ?? subGridResponses.Where(x => x != null).All(TileRenderer.Displayer.RenderSubGrid);
        }
      }

      return result;
    }

    // This code added to correctly implement the disposable pattern.
    public override void Dispose()
    {
      base.Dispose();

      SubGridTree = null;
      TileRenderer = null;
    }
  }
}
