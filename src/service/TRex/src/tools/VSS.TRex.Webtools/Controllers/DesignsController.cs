using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Alignments.Interfaces;
using VSS.TRex.Common;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;

namespace VSS.TRex.Webtools.Controllers
{
  [Route("api/designs")]
  public class DesignController : Controller
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<DesignController>();

    /// <summary>
    /// Returns the list of designs registered for a site-model. If there are no designs the
    /// result will be an empty list.
    /// </summary>
    /// <param name="siteModelUid">Grid to return status for</param>
    /// <param name="importedFileType"></param>
    /// <returns></returns>
    [HttpGet("{siteModelUid}/{importedFileType}")]
    public JsonResult GetDesignsForSiteModel(string siteModelUid, string importedFileType)
    {
      var importedFileTypeEnum = ValidateImportedFileType(importedFileType);

      if (importedFileTypeEnum == ImportedFileType.DesignSurface)
      {
        return new JsonResult(DIContext.Obtain<IDesignManager>().List(Guid.Parse(siteModelUid)));
      }
      else if (importedFileTypeEnum == ImportedFileType.SurveyedSurface)
      {
        return new JsonResult(DIContext.Obtain<ISurveyedSurfaceManager>().List(Guid.Parse(siteModelUid)));
      }
      else if (importedFileTypeEnum == ImportedFileType.Alignment)
      {
        return new JsonResult(DIContext.Obtain<IAlignmentManager>().List(Guid.Parse(siteModelUid)));
      }
      throw new ArgumentException($"{nameof(GetDesignsForSiteModel)} Unsupported ImportedFileType: {importedFileType}");
    }


    /// <summary>
    /// Deletes a design from a site-model.
    /// </summary>
    /// <param name="siteModelUid">Grid to return status for</param>
    /// <param name="importedFileType"></param>
    /// <param name="designId"></param>
    /// <returns></returns>
    [HttpDelete("{siteModelUid}/{importedFileType}/{designId}")]
    public JsonResult DeleteDesignFromSiteModel(string siteModelUid, string importedFileType, string designId)
    {
      var importedFileTypeEnum = ValidateImportedFileType(importedFileType);

      if (importedFileTypeEnum == ImportedFileType.DesignSurface)
      {
        return new JsonResult(DIContext.Obtain<IDesignManager>().Remove(Guid.Parse(siteModelUid), Guid.Parse(designId)));
      }
      else if (importedFileTypeEnum == ImportedFileType.SurveyedSurface)
      {
        return new JsonResult(DIContext.Obtain<ISurveyedSurfaceManager>().Remove(Guid.Parse(siteModelUid), Guid.Parse(designId)));
      }
      else if (importedFileTypeEnum == ImportedFileType.Alignment)
      {
        return new JsonResult(DIContext.Obtain<IAlignmentManager>().Remove(Guid.Parse(siteModelUid), Guid.Parse(designId)));
      }

      throw new ArgumentException($"{nameof(DeleteDesignFromSiteModel)} Unsupported ImportedFileType: {importedFileType}");
    }

    /// <summary>
    /// Adds a new design to a site-model. 
    /// </summary>
    /// <param name="siteModelUid"></param>
    /// <param name="importedFileType"></param>
    /// <param name="fileNameAndLocalPath"></param>
    /// <returns></returns>
    [HttpPost("{siteModelUid}/{importedFileType}")]
    public JsonResult AddDesignToSiteModel(
      string siteModelUid,
      string importedFileType,
      [FromQuery] string fileNameAndLocalPath)
    {
      var importedFileTypeEnum = ValidateImportedFileType(importedFileType);

      if (string.IsNullOrEmpty(siteModelUid))
        throw new ArgumentException($"Invalid siteModelUid (you need to have selected one first): {siteModelUid}");

      if (string.IsNullOrEmpty(fileNameAndLocalPath) ||
          !Path.HasExtension(fileNameAndLocalPath) ||
          (importedFileTypeEnum != ImportedFileType.Alignment &&
           (string.Compare(Path.GetExtension(fileNameAndLocalPath), ".ttm", StringComparison.OrdinalIgnoreCase) != 0))
          ||
          (importedFileTypeEnum == ImportedFileType.Alignment &&
           (string.Compare(Path.GetExtension(fileNameAndLocalPath), ".svl", StringComparison.OrdinalIgnoreCase) != 0))
      )
        throw new ArgumentException($"Invalid [path]filename: {fileNameAndLocalPath}");

      if (!System.IO.File.Exists(fileNameAndLocalPath))
        throw new ArgumentException($"Unable to locate [path]fileNameAndLocalPath: {fileNameAndLocalPath}");

      var siteModelGuid = Guid.Parse(siteModelUid);
      var designUid = Guid.NewGuid();

      var fileNameOnly = Path.GetFileName(fileNameAndLocalPath);

      // copy local file to S3
      var designFileLoadedOk = S3FileTransfer.WriteFile(Path.GetDirectoryName(fileNameAndLocalPath),
        Guid.Parse(siteModelUid), fileNameOnly);
      if (!designFileLoadedOk)
        throw new ArgumentException($"Unable to copy design file to S3: {fileNameOnly}");

      var designRequest = new DesignRequest(siteModelGuid, importedFileTypeEnum, Path.GetFileName(fileNameAndLocalPath),
        designUid, null);

      if (designRequest.FileType == ImportedFileType.DesignSurface ||
          designRequest.FileType == ImportedFileType.SurveyedSurface)
      {
        if (designRequest.FileType == ImportedFileType.SurveyedSurface)
          designRequest.SurveyedUtc = DateTime.UtcNow; // unable to parse the date from UI DateTime.Parse(asAtDate);

        var executor = RequestExecutorContainer
          .Build<AddTTMDesignExecutor>(DIContext.Obtain<IConfigurationStore>(), DIContext.Obtain<ILoggerFactory>(),
            DIContext.Obtain<IServiceExceptionHandler>());
        var result = executor.Process(designRequest);
        if (result.Code != 0)
          throw new ArgumentException(
            $"Add TTM file: {fileNameAndLocalPath} failed. Code: {result.Code} Message: {result.Message}");

        return new JsonResult(DIContext.Obtain<IDesignManager>().List(siteModelGuid).Locate(designUid));
      }

      if (designRequest.FileType == ImportedFileType.Alignment)
      {
        var executor = RequestExecutorContainer
          .Build<AddSVLDesignExecutor>(DIContext.Obtain<IConfigurationStore>(), DIContext.Obtain<ILoggerFactory>(),
            DIContext.Obtain<IServiceExceptionHandler>());
        var result = executor.Process(designRequest);
        if (result.Code != 0)
          throw new ArgumentException(
            $"Add Alignment file: {fileNameAndLocalPath} failed. Code: {result.Code} Message: {result.Message}");

        return new JsonResult(DIContext.Obtain<IAlignmentManager>().List(siteModelGuid).Locate(designUid));
      }

      throw new ArgumentException($"{nameof(AddDesignToSiteModel)} Unsupported ImportedFileType: {importedFileType}");
    }

    private ImportedFileType ValidateImportedFileType(string importedFileType)
    {
      if (!Enum.IsDefined(typeof(ImportedFileType), (object) importedFileType))
      {
        throw new ArgumentException($"{nameof(ValidateImportedFileType)}. Invalid ImportedFileType: {importedFileType}");
      }

      return (ImportedFileType)Enum.Parse(typeof(ImportedFileType), importedFileType);
    }

  }
}
