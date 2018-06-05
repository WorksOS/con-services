using System.Collections.Generic;
using VSS.TRex.Geometry;

namespace VSS.TRex.Profiling.Interfaces
{
  public interface ICellProfileBuilder
  {
    bool Build(XYZ[] nEECoords, List<ProfileCell> profileCells);
  }
}
