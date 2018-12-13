using System;
using VSS.TRex.Cells;
using VSS.TRex.Geometry;
using VSS.TRex.Common.Utilities.Interfaces;

namespace VSS.TRex.SiteModels.Interfaces
{
  public interface ISiteProofingRun : IBinaryReaderWriter
  {
    short MachineID { get; }
    string Name { get; }
    DateTime StartTime { get; set; }
    DateTime EndTime { get; set; }
    BoundingWorldExtent3D Extents { get; set; }
    BoundingWorldExtent3D WorkingExtents { get; set; }
    bool MatchesCellPass(CellPass cellPass);
  }
}
