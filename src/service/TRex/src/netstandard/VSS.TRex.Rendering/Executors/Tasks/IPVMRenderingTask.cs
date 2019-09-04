using VSS.TRex.Pipelines.Interfaces.Tasks;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Rendering.Executors.Tasks
{
  public interface IPVMRenderingTask : ITRexTask
  {
    /// <summary>
    /// The renderer to be used to render the sub grids returned by the rendering query
    /// </summary>
    PlanViewTileRenderer TileRenderer { get; set; }

    /// <summary>
    /// The collection of sub grids returned by the rendering query
    /// </summary>
    ISubGridTree SubGridTree { get; }
  }
}
