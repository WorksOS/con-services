using System.Collections.Generic;
using VSS.TRex.Geometry;

namespace VSS.TRex.Profiling.Interfaces
{
  public interface ICellProfileBuilder
  {
    bool Aborted { get; set; }

    double GridDistanceBetweenProfilePoints { get; set; }

    bool Build(XYZ[] nEECoords, List<IProfileCell> profileCells);
  }
}
