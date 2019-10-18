using System.Threading.Tasks;
using TCCToDataOcean.DatabaseAgent;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;

namespace TCCToDataOcean.Interfaces
{
  public interface IWebApiUtils
  {
    Task<ProjectDataSingleResult> UpdateProjectCoordinateSystemFile(string uriRoot, MigrationJob job);
  }
}
