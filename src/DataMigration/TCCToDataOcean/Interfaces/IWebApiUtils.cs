using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Repositories.DBModels;

namespace TCCToDataOcean.Interfaces
{
  public interface IWebApiUtils
  {
    ProjectDataSingleResult UpdateProjectViaWebApi(string uriRoot, Project project, byte[] coordSystemFileContent);
  }
}
