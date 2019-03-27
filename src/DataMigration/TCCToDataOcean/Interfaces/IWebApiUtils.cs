using System.Threading.Tasks;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;

namespace TCCToDataOcean.Interfaces
{
  public interface IWebApiUtils
  {
    Task<ProjectDataSingleResult> UpdateProjectCoordinateSystemFile(string uriRoot, Project project, byte[] coordSystemFileContent);
  }
}
