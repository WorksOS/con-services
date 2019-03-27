using System.Threading.Tasks;
using TCCToDataOcean.Models;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;

namespace TCCToDataOcean.Interfaces
{
  public interface IImportFile
  {
    Task<FileDataResult> GetImportedFilesFromWebApi(string uri, Project project);
    FileDataSingleResult SendRequestToFileImportV4(string uriRoot, FileData fileDescr, string fullFileName, ImportOptions importOptions = new ImportOptions());
  }
}
