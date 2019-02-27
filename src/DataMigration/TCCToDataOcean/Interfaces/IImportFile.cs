using TCCToDataOcean.Models;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Repositories.DBModels;

namespace TCCToDataOcean.Interfaces
{
  public interface IImportFile
  {
    FileDataResult GetImportedFilesFromWebApi(string uri, Project project);

    FileDataSingleResult SendRequestToFileImportV4(string uriRoot, FileData fileDescr,
      string fullFileName, ImportOptions importOptions = new ImportOptions());
  }
}
