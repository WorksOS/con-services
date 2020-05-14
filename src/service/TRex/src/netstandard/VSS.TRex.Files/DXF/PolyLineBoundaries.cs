using System.Collections.Generic;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.TRex.Files.DXF
{
  public class PolyLineBoundaries
  {
    public DxfUnitsType Units;
    public List<PolyLineBoundary> Boundaries;

    public PolyLineBoundaries(DxfUnitsType units, int maxBoundariesToProcess)
    {
      Units = units;
      Boundaries = new List<PolyLineBoundary>(maxBoundariesToProcess);
    }
  }
}
