
using VSS.Raptor.Service.Common.Models;

namespace WebApiModels.Interfaces
{
    public interface ITileGenerator
    {
      void CreateDxfTiles(long projectId, FileDescriptor fileDescr, bool regenerate);
      void DeleteDxfTiles(long projectId, FileDescriptor fileDescr, string suffix);
    }
}
