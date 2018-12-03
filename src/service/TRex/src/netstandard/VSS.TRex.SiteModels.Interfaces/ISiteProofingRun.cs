using System;
using VSS.TRex.Cells;
using VSS.TRex.Geometry;
using VSS.TRex.Utilities.Interfaces;

namespace VSS.TRex.SiteModels.Interfaces
{
  public interface ISiteProofingRun : IBinaryReaderWriter
  {
    long MachineID { get; }
    string Name { get; }
    DateTime StartTime { get; set; }
    DateTime EndTime { get; set; }
    BoundingWorldExtent3D Extents { get; set; }
    BoundingWorldExtent3D WorkingExtents { get; set; }
    bool MatchesCellPass(CellPass cellPass);
    bool Equals(string other);
  }
}
