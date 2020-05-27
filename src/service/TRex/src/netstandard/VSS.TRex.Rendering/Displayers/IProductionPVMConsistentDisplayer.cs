using VSS.TRex.DataSmoothing;
using VSS.TRex.Rendering.Executors.Tasks;

namespace VSS.TRex.Rendering.Displayers
{
  public interface IProductionPVMConsistentDisplayer
  {
    IPVMTaskAccumulator GetPVMTaskAccumulator(double valueStoreCellSizeX, double valueStoreCellSizeY,
      int cellsWidth, int cellsHeight,
      double originX, double originY,
      double worldX, double worldY,
      double sourceCellSize);

    bool PerformConsistentRender();

    IDataSmoother DataSmoother { get; set; }
  }
}
