using VSS.TRex.Pipelines.Interfaces.Tasks;

namespace VSS.TRex.Rendering.Executors.Tasks
{
  public interface IPVMRenderingTask : Pipelines.Interfaces.Tasks.ITask
  {
    PlanViewTileRenderer TileRenderer { get; set; }
  }
}
