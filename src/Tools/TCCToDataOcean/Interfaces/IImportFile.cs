using System.Threading.Tasks;
using TCCToDataOcean.Models;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;

namespace TCCToDataOcean.Interfaces
{
  public interface IImportFile
  {
    Task<ImportedFileDescriptorListResult> GetImportedFilesFromWebApi(string uri, Project project);

    FileDataSingleResult SendRequestToFileImportV4(string uriRoot,
                                                   ImportedFileDescriptor fileDescr,
                                                   string fullFileName,
                                                   ImportOptions importOptions = new ImportOptions(),
                                                   bool uploadToTCC = false);
  }
}
