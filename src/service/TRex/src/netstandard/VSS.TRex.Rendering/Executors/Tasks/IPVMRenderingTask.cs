using VSS.TRex.Pipelines.Interfaces.Tasks;

namespace VSS.TRex.Rendering.Executors.Tasks
{
  public interface IPVMRenderingTask : ITRexTask
  {
    /// <summary>
    /// The renderer to be used to render the sub grids returned by the rendering query
    /// </summary>
    PlanViewTileRenderer TileRenderer { get; set; }

    /// <summary>
    /// The accumulator for the PVM rendering task to populate cell data from subgrids into
    /// </summary>
    IPVMTaskAccumulator Accumulator { get; set; }
  }
}
