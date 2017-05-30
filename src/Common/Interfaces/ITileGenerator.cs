
using System.Threading.Tasks;
using VSS.Raptor.Service.Common.Models;

namespace WebApiModels.Interfaces
{
    public interface ITileGenerator
    {
      Task<bool> CreateDxfTiles(long projectId, FileDescriptor fileDescr, string suffix, bool regenerate);
      Task<bool> DeleteDxfTiles(long projectId, string generatedName, FileDescriptor fileDescr);
    }
}
