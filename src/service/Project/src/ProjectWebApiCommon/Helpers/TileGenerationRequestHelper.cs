using System;
using VSS.Common.Abstractions.Extensions;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.Productivity3D.Scheduler.Jobs.DxfTileJob;
using VSS.Productivity3D.Scheduler.Jobs.DxfTileJob.Models;
using VSS.Productivity3D.Scheduler.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Helpers
{
  /// <summary>
  /// Helper for creating raster tile generation job requests
  /// </summary>
  public static class TileGenerationRequestHelper
  {
    /// <summary>
    /// Creates a job request for generating raster tiles
    /// </summary>
    public static JobRequest CreateRequest(ImportedFileType importedFileType, string customerUid, string projectUid,
      string importedFileUid, string dataOceanRootFolder, string fileName, string dcFileName = null, DxfUnitsType 
        dxfUnitsType = DxfUnitsType.Meters, DateTime? surveyedUtc = null)
    {
      TileGenerationRequest runParams;
      Guid jobUid;
      switch (importedFileType)
      {
        case ImportedFileType.GeoTiff:
          runParams = new TileGenerationRequest();
          jobUid = GeoTiffTileGenerationJob.VSSJOB_UID;
          break;
        case ImportedFileType.Alignment:
        case ImportedFileType.Linework:
          runParams = new DxfTileGenerationRequest
          {
            DcFileName = dcFileName,
            DxfUnitsType = dxfUnitsType
          };
          jobUid = DxfTileGenerationJob.VSSJOB_UID;
          break;
        default:
          throw new NotImplementedException();
      }

      runParams.CustomerUid = Guid.Parse(customerUid);
      runParams.ProjectUid = Guid.Parse(projectUid);
      runParams.ImportedFileUid = Guid.Parse(importedFileUid);
      runParams.DataOceanRootFolder = dataOceanRootFolder;
      runParams.FileName = fileName;

      return new JobRequest
      {
        JobUid = jobUid,
        RunParameters = runParams
      };    
    }
  }
}
