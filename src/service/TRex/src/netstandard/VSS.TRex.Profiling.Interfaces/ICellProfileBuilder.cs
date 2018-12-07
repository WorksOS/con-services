using System.Collections.Generic;
using VSS.TRex.Geometry;

namespace VSS.TRex.Profiling.Interfaces
{
  public interface ICellProfileBuilder<T>
  {
    bool Aborted { get; set; }

    double GridDistanceBetweenProfilePoints { get; set; }

    bool Build(XYZ[] nEECoords, List<T> profileCells);
  }
}
