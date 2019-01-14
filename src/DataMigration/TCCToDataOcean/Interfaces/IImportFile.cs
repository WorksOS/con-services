using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;

namespace TCCToDataOcean.Interfaces
{
  public interface IImportFile
  {
    FileDataResult GetImportedFilesFromWebApi(string uri, string customerUid);

    FileDataSingleResult SendRequestToFileImportV4(string uriRoot, FileData fileDescr,
      string fullFileName, ImportOptions importOptions = new ImportOptions());
  }
}
