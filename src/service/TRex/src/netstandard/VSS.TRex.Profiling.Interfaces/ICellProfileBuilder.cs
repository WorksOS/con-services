using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.TRex.Geometry;

namespace VSS.TRex.Profiling.Interfaces
{
  public interface ICellProfileBuilder<T> where T : class, IProfileCellBase, new()
  {
    bool Aborted { get; set; }

    double GridDistanceBetweenProfilePoints { get; set; }

    Task<bool> Build(XYZ[] nEECoords, List<T> profileCells);
  }
}
