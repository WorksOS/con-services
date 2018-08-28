using VSS.TRex.Pipelines.Tasks.Interfaces;

namespace VSS.TRex.Rendering.Executors.Tasks
{
  public interface IPVMRenderingTask : ITask
  {
    PlanViewTileRenderer TileRenderer { get; set; }
  }
}
