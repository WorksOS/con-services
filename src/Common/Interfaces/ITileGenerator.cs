using System;
using System.Threading.Tasks;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Common.Models;

namespace VSS.Productivity3D.Common.Interfaces
{
    public interface ITileGenerator
    {
      Task<bool> CreateDxfTiles(long projectId, FileDescriptor fileDescr, string suffix, ZoomRangeResult zoomResult, bool regenerate);
      Task<bool> DeleteDxfTiles(long projectId, string generatedName, FileDescriptor fileDescr);
      Task<ZoomRangeResult> CalculateTileZoomRange(string filespaceId, string fullGeneratedName);
    }
}
