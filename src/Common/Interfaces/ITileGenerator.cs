using System.Threading.Tasks;
using VSS.Productivity3D.Common.Models;

namespace VSS.Productivity3D.Common.Interfaces
{
    public interface ITileGenerator
    {
      Task<bool> CreateDxfTiles(long projectId, FileDescriptor fileDescr, string suffix, bool regenerate);
      Task<bool> DeleteDxfTiles(long projectId, string generatedName, FileDescriptor fileDescr);
    }
}
