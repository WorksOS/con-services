using VSS.TRex.Rendering.Executors.Tasks;

namespace VSS.TRex.Rendering.Displayers
{
  public interface IProductionPVMConsistentDisplayer
  {
    IPVMTaskAccumulator GetPVMTaskAccumulator(int cellsWidth, int cellsHeight,
      double worldX, double worldY,
      double originX, double originY);

    bool PerformConsistentRender();
  }
}
