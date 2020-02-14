using VSS.TRex.Rendering.Palettes.Interfaces;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;

namespace VSS.TRex.Rendering.Displayers
{
  public abstract class PVMDisplayerBase<TP, TS, TC> : ProductionPVMConsistentDisplayer<TP, TS, TC>
    where TP : class, IPlanViewPalette
    where TS : GenericClientLeafSubGrid<TC>, IClientLeafSubGrid
  {
  }
}
