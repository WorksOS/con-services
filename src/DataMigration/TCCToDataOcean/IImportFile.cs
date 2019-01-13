using System;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;

namespace TCCToDataOcean
{
  public interface IImportFile
  {
    FileDataResult GetImportedFilesFromWebApi(string uri, string customerUid);

    FileDataSingleResult SendRequestToFileImportV4(string uriRoot, FileData fileDescr,
      string fullFileName, ImportOptions importOptions = new ImportOptions());
  }
}
