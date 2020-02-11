using System.Threading.Tasks;
using TCCToDataOcean.DatabaseAgent;

namespace TCCToDataOcean.Interfaces
{
  public interface ICalibrationFileAgent
  {
    Task<bool> ResolveProjectCoordinateSystemFile(MigrationJob job);
  }
}
