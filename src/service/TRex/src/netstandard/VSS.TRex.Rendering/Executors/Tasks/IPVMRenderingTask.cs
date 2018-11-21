using VSS.TRex.Pipelines.Interfaces.Tasks;

namespace VSS.TRex.Rendering.Executors.Tasks
{
  public interface IPVMRenderingTask : ITRexTask
  {
    PlanViewTileRenderer TileRenderer { get; set; }
  }
}
