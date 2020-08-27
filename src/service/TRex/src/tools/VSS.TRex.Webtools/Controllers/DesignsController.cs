using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.AWS.TransferProxy;
using VSS.TRex.Alignments.Interfaces;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Common.Utilities;
using VSS.TRex.Designs;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.TRex.Webtools.Controllers
{
  [Route("api/designs")]
  public class DesignController : Controller
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<DesignController>();

    private string SubGridIndexFileName(string filename) => $"{filename}Designs.TTM.Optimised.Consts.DESIGN_SUB_GRID_INDEX_FILE_EXTENSION";
    private string SpatialIndexFileName(string filename) => $"{filename}Designs.TTM.Optimised.Consts.DESIGN_SPATIAL_INDEX_FILE_EXTENSION";
    private string BoundaryFileName(string filename) => $"{filename}Designs.TTM.Optimised.Consts.DESIGN_BOUNDARY_FILE_EXTENSION";

    /// <summary>
    /// Returns the list of designs registered for a sitemodel. If there are no designs the
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
    /// Deletes a design from a sitemodel.
    /// </summary>
    [HttpDelete("{siteModelUid}/{importedFileType}/{designUid}")]
    public JsonResult DeleteDesignFromSiteModel(string siteModelUid, string importedFileType, string designUid)
    {
      var importedFileTypeEnum = ValidateImportedFileType(importedFileType);

      var siteModelId = Guid.Parse(siteModelUid);
      var designId = Guid.Parse(designUid);

      if (importedFileTypeEnum == ImportedFileType.DesignSurface)
      {
        var designManager = DIContext.Obtain<IDesignManager>();
        var filename = designManager.List(siteModelId).Locate(designId)?.DesignDescriptor.FileName;
        DeleteLocalFiles(siteModelId, filename, importedFileTypeEnum);
        return new JsonResult(designManager.Remove(siteModelId, designId));
      }
      if (importedFileTypeEnum == ImportedFileType.SurveyedSurface)
      {
        var ssManager = DIContext.Obtain<ISurveyedSurfaceManager>();
        var filename = ssManager.List(siteModelId).Locate(designId)?.DesignDescriptor.FileName;
        DeleteLocalFiles(siteModelId, filename, importedFileTypeEnum);
        return new JsonResult(ssManager.Remove(siteModelId, designId));
      }
      if (importedFileTypeEnum == ImportedFileType.Alignment)
      {
        var alignManager = DIContext.Obtain<IAlignmentManager>();
        var filename = alignManager.List(siteModelId).Locate(designId)?.DesignDescriptor.FileName;
        DeleteLocalFiles(siteModelId, filename, importedFileTypeEnum);
        return new JsonResult(alignManager.Remove(siteModelId, designId));
      }
      throw new ArgumentException($"{nameof(DeleteDesignFromSiteModel)} Unsupported ImportedFileType: {importedFileType}");
    }

    private void DeleteLocalFiles(Guid siteModelUid, string filename, ImportedFileType importedFileTypeEnum)
    {
      var localPath = FilePathHelper.GetTempFolderForProject(siteModelUid);
      var localPathAndFileName = Path.Combine(new[] { localPath, filename });

      if (System.IO.File.Exists(localPathAndFileName))
      {
        System.IO.File.Delete(localPathAndFileName);

        if (importedFileTypeEnum != ImportedFileType.Alignment)
        {
          var indexFilename = SubGridIndexFileName(localPathAndFileName);
          if (System.IO.File.Exists(indexFilename))
            System.IO.File.Delete(indexFilename);
          indexFilename = SpatialIndexFileName(localPathAndFileName);
          if (System.IO.File.Exists(indexFilename))
            System.IO.File.Delete(indexFilename);
          indexFilename = BoundaryFileName(localPathAndFileName);
          if (System.IO.File.Exists(indexFilename))
            System.IO.File.Delete(indexFilename);
        }
      }
    }

    /// <summary>
    /// Upload a design to a site model
    /// </summary>
    [HttpPost("{siteModelUid}/{importedFileType}/upload")]
    public async Task<IActionResult> UploadDesignToSiteModel([FromRoute] string siteModelUid,
      [FromRoute] string importedFileType,
      [FromQuery] string asAtDate,
      [FromQuery] Guid designUid)
    {
      if (Request.Form.Files.Count != 1)
        return BadRequest("Upload a single file only");

      var uploadedFile = Request.Form.Files[0];
      var tempFile = Path.Combine(Path.GetTempPath(), uploadedFile.FileName);

      if (System.IO.File.Exists(tempFile))
        System.IO.File.Delete(tempFile);

      using (var tempFileStream = new FileStream(tempFile, FileMode.OpenOrCreate))
      {
        await uploadedFile.CopyToAsync(tempFileStream);
      }

      try
      {
        return await AddDesignToSiteModel(siteModelUid, importedFileType, tempFile, asAtDate, designUid);
      }
      finally
      {
          System.IO.File.Delete(tempFile);
      }
    }


    /// <summary>
    /// Adds a new design to a sitemodel. 
    /// </summary>
    [HttpPost("{siteModelUid}/{importedFileType}")]
    public async Task<JsonResult> AddDesignToSiteModel(
      string siteModelUid,
      string importedFileType,
      [FromQuery] string fileNameAndLocalPath,
      [FromQuery] string asAtDate,
      [FromQuery] Guid designUid)
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
        throw new ArgumentException($"Unable to locate [path]fileName: {fileNameAndLocalPath}");

      var siteModelGuid = Guid.Parse(siteModelUid);
      
      if (designUid == Guid.Empty)
        designUid = Guid.NewGuid();

      // copy local file to S3
      bool designFileLoadedOk;
      string s3FileName;
      var destinationFileName = string.Empty;
	    var tempFileNameOnly = Path.GetFileName(fileNameAndLocalPath);

      var s3FileTransfer = new S3FileTransfer(TransferProxyType.DesignImport);

      if (importedFileTypeEnum == ImportedFileType.SurveyedSurface)
      {
        var tempDate = asAtDate.Remove(asAtDate.IndexOf(".", 0, StringComparison.Ordinal)).Replace(":", "");
		    destinationFileName = Path.GetFileNameWithoutExtension(tempFileNameOnly) + $"_{tempDate}" + Path.GetExtension(tempFileNameOnly);

        designFileLoadedOk = s3FileTransfer.WriteFile(Path.GetDirectoryName(fileNameAndLocalPath), Guid.Parse(siteModelUid), tempFileNameOnly, destinationFileName);

        s3FileName = destinationFileName;
      }
      else
      {
        designFileLoadedOk = s3FileTransfer.WriteFile(Path.GetDirectoryName(fileNameAndLocalPath), Guid.Parse(siteModelUid), tempFileNameOnly);
        s3FileName = tempFileNameOnly;
      }

      if (!designFileLoadedOk)
        throw new ArgumentException($"Unable to copy design file to S3: {s3FileName}");

      // download to appropriate local location and add to site model
      var downloadLocalPath = FilePathHelper.GetTempFolderForProject(Guid.Parse(siteModelUid));
      var downloadedok = await s3FileTransfer.ReadFile(Guid.Parse(siteModelUid), s3FileName, downloadLocalPath).ConfigureAwait(false);
      if (!downloadedok)
        throw new ArgumentException($"Unable to restore same design file from S3: {s3FileName}");

      if (importedFileTypeEnum == ImportedFileType.DesignSurface)
      {
        AddTheDesignSurfaceToSiteModel(siteModelGuid, designUid, downloadLocalPath, s3FileName);

        // upload indices
        UploadIndices(s3FileTransfer, s3FileName, downloadLocalPath, siteModelUid);

        return new JsonResult(DIContext.Obtain<IDesignManager>().List(siteModelGuid).Locate(designUid));
      }

      if (importedFileTypeEnum == ImportedFileType.SurveyedSurface)
      {
        var surveyedUtc = DateTime.Parse(asAtDate).ToUniversalTime(); //DateTime.UtcNow; // unable to parse the date from UI DateTime.Parse(asAtDate);
        AddTheSurveyedSurfaceToSiteModel(siteModelGuid, designUid, downloadLocalPath, s3FileName, surveyedUtc);

        // upload indices
        UploadIndices(s3FileTransfer, s3FileName, downloadLocalPath, siteModelUid);

        return new JsonResult(DIContext.Obtain<ISurveyedSurfaceManager>().List(siteModelGuid).Locate(designUid));
      }

      if (importedFileTypeEnum == ImportedFileType.Alignment)
      {
        AddTheAlignmentToSiteModel(siteModelGuid, designUid, downloadLocalPath, s3FileName);
        return new JsonResult(DIContext.Obtain<IAlignmentManager>().List(siteModelGuid).Locate(designUid));
      }
      throw new ArgumentException($"{nameof(AddDesignToSiteModel)} Unsupported ImportedFileType: {importedFileType}");
    }

    private void UploadIndices(S3FileTransfer s3FileTransfer, string s3FileName, string downloadLocalPath, string siteModelUid)
    {
      var filename = SpatialIndexFileName(s3FileName);
      var spatialUploadedOk = s3FileTransfer.WriteFile(downloadLocalPath, Guid.Parse(siteModelUid), filename);
      if (!spatialUploadedOk)
        throw new ArgumentException($"Unable to copy spatial index file to S3: {filename}");
      filename = SubGridIndexFileName(s3FileName);
      var subgridUploadedOk = s3FileTransfer.WriteFile(downloadLocalPath, Guid.Parse(siteModelUid), filename);
      if (!subgridUploadedOk)
        throw new ArgumentException($"Unable to copy subgrid index file to S3: {filename}");
      // upload boundary...
      filename = BoundaryFileName(s3FileName);
      var boundaryUploadedOk = s3FileTransfer.WriteFile(downloadLocalPath, Guid.Parse(siteModelUid), filename);
      if (!boundaryUploadedOk)
        throw new ArgumentException($"Unable to copy boundary file to S3: {filename}");
    }

    private ImportedFileType ValidateImportedFileType(string importedFileType)
    {
      if (!Enum.IsDefined(typeof(ImportedFileType), (object) importedFileType))
      {
        throw new ArgumentException($"{nameof(ValidateImportedFileType)}. Invalid ImportedFileType: {importedFileType}");
      }

      return (ImportedFileType)Enum.Parse(typeof(ImportedFileType), importedFileType);
    }

    private void AddTheDesignSurfaceToSiteModel(Guid siteModelUid, Guid designUid, string localPath, string localFileName)
    {
      // Invoke the service to add the design surface
      try
      {
        // Load the file and extract its extents
        var TTM = new TTMDesign(SubGridTreeConsts.DefaultCellSize);
        TTM.LoadFromFile(Path.Combine(new[] { localPath, localFileName }));

        var extents = new BoundingWorldExtent3D();
        TTM.GetExtents(out extents.MinX, out extents.MinY, out extents.MaxX, out extents.MaxY);
        TTM.GetHeightRange(out extents.MinZ, out extents.MaxZ);

        // Create the new design for the site model
        //TODO: Use add design request
        var design = DIContext.Obtain<IDesignManager>()
          .Add(siteModelUid, new DesignDescriptor(designUid, string.Empty, localFileName), extents, TTM.SubGridOverlayIndex());
      }
      catch (Exception e)
      {
        throw new TRexException($"Exception writing design surface to siteModel:", e);
      }
    }

    private void AddTheSurveyedSurfaceToSiteModel(Guid siteModelUid, Guid designUid, string localPath, string localFileName, DateTime surveyedUtc)
    {
      // Invoke the service to add the surveyed surface
      try
      {
        // Load the file and extract its extents
        var TTM = new TTMDesign(SubGridTreeConsts.DefaultCellSize);
        TTM.LoadFromFile(Path.Combine(new[] { localPath, localFileName }));

        var extents = new BoundingWorldExtent3D();
        TTM.GetExtents(out extents.MinX, out extents.MinY, out extents.MaxX, out extents.MaxY);
        TTM.GetHeightRange(out extents.MinZ, out extents.MaxZ);

        // TODO: Convert to requet per designs
        // Create the new design for the site model (note that SS and design types are different)
        var design = DIContext.Obtain<ISurveyedSurfaceManager>()
          .Add(siteModelUid, new DesignDescriptor(designUid, string.Empty, localFileName), surveyedUtc, extents, TTM.SubGridOverlayIndex());
      }
      catch (Exception e)
      {
        throw new TRexException($"Exception writing surveyed surface to siteModel:", e);
      }
    }

    private void AddTheAlignmentToSiteModel(Guid siteModelUid, Guid designUid, string localPath, string localFileName)
    {
      // Invoke the service to add the alignment
      try
      {
        // Load the file and extract its extents?
        var alignmentDesign = new SVLAlignmentDesign();
        alignmentDesign.LoadFromFile(Path.Combine(new[] { localPath, localFileName }));

        var extents = new BoundingWorldExtent3D();
        alignmentDesign.GetExtents(out extents.MinX, out extents.MinY, out extents.MaxX, out extents.MaxY);
        alignmentDesign.GetHeightRange(out extents.MinZ, out extents.MaxZ);

        // Create the new design for the site model
        var design = DIContext.Obtain<IAlignmentManager>()
          .Add(siteModelUid, new VSS.TRex.Designs.Models.DesignDescriptor(designUid, string.Empty, localFileName), extents);
      }
      catch (Exception e)
      {
        throw new TRexException($"Exception writing alignment to siteModel:", e);
      }
    }

  }
}
