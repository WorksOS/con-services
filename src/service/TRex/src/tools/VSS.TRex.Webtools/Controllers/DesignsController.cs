using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Alignments.Interfaces;
using VSS.TRex.Common;
using VSS.TRex.Common.Utilities;
using VSS.TRex.Designs;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.ExistenceMaps.Interfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using Consts = VSS.TRex.ExistenceMaps.Interfaces.Consts;

namespace VSS.TRex.Webtools.Controllers
{
  [Route("api/designs")]
  public class DesignController : Controller
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<DesignController>();

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
    /// Adds a new design to a sitemodel. 
    /// </summary>
    /// <param name="siteModelUid"></param>
    /// <param name="importedFileType"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    [HttpPost("{siteModelUid}/{importedFileType}")]
    public async Task<JsonResult> AddDesignToSiteModel(
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
        throw new ArgumentException($"Unable to locate [path]fileName: {fileNameAndLocalPath}");
      
      var siteModelGuid = Guid.Parse(siteModelUid);
      var designUid = Guid.NewGuid();

      var fileNameOnly = Path.GetFileName(fileNameAndLocalPath);

      // copy local file to S3
      var designFileLoadedOk = S3FileTransfer.WriteFile(Path.GetDirectoryName(fileNameAndLocalPath), Guid.Parse(siteModelUid), fileNameOnly);
      if (!designFileLoadedOk)
        throw new ArgumentException($"Unable to copy design file to S3: {fileNameOnly}");

      // download to appropriate local location and add to site model
      string downloadLocalPath = FilePathHelper.GetTempFolderForProject(Guid.Parse(siteModelUid));
      var downloadedok = await S3FileTransfer.ReadFile(Guid.Parse(siteModelUid), fileNameOnly, downloadLocalPath).ConfigureAwait(false);
      if (!downloadedok)
        throw new ArgumentException($"Unable to restore same design file from S3: {fileNameOnly}");

      if (importedFileTypeEnum == ImportedFileType.DesignSurface)
      {
        AddTheDesignSurfaceToSiteModel(siteModelGuid, designUid, downloadLocalPath, fileNameOnly);

        // upload indices
        var spatialUploadedOk = S3FileTransfer.WriteFile(downloadLocalPath, Guid.Parse(siteModelUid), fileNameOnly + ".$DesignSpatialIndex$");
        if (!spatialUploadedOk)
          throw new ArgumentException($"Unable to copy spatial index file to S3: {fileNameOnly + ".$DesignSpatialIndex$"}");
        var subgridUploadedOk = S3FileTransfer.WriteFile(downloadLocalPath, Guid.Parse(siteModelUid), fileNameOnly + ".$DesignSubgridIndex$");
        if (!subgridUploadedOk)
          throw new ArgumentException($"Unable to copy subgrid index file to S3: {fileNameOnly + ".$DesignSubgridIndex$"}");
        
        return new JsonResult(DIContext.Obtain<IDesignManager>().List(siteModelGuid).Locate(designUid));
      }
      else if (importedFileTypeEnum == ImportedFileType.SurveyedSurface)
      {
        var surveyedUtc = DateTime.UtcNow; // unable to parse the date from UI DateTime.Parse(asAtDate);
        AddTheSurveyedSurfaceToSiteModel(siteModelGuid, designUid, downloadLocalPath, fileNameOnly, surveyedUtc);

        // upload indices
        var spatialUploadedOk = S3FileTransfer.WriteFile(downloadLocalPath, Guid.Parse(siteModelUid), fileNameOnly + ".$DesignSpatialIndex$");
        if (!spatialUploadedOk)
          throw new ArgumentException($"Unable to copy spatial index file to S3: {fileNameOnly + ".$DesignSpatialIndex$"}");
        var subgridUploadedOk = S3FileTransfer.WriteFile(downloadLocalPath, Guid.Parse(siteModelUid), fileNameOnly + ".$DesignSubgridIndex$");
        if (!subgridUploadedOk)
          throw new ArgumentException($"Unable to copy subgrid index file to S3: {fileNameOnly + ".$DesignSubgridIndex$"}");

        return new JsonResult(DIContext.Obtain<IDesignManager>().List(siteModelGuid).Locate(designUid));
      }
      else if (importedFileTypeEnum == ImportedFileType.Alignment)
      {
        AddTheAlignmentToSiteModel(siteModelGuid, designUid, downloadLocalPath, fileNameOnly);
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

    private void AddTheDesignSurfaceToSiteModel(Guid siteModelUid, Guid designUid, string localPath, string localFileName)
    {
      // Invoke the service to add the design surface
      try
      {
        // Load the file and extract its extents
        TTMDesign TTM = new TTMDesign(SubGridTreeConsts.DefaultCellSize);
        TTM.LoadFromFile(Path.Combine(new[] { localPath, localFileName }));

        BoundingWorldExtent3D extents = new BoundingWorldExtent3D();
        TTM.GetExtents(out extents.MinX, out extents.MinY, out extents.MaxX, out extents.MaxY);
        TTM.GetHeightRange(out extents.MinZ, out extents.MaxZ);

        // Create the new design for the site model
        var design = DIContext.Obtain<IDesignManager>()
          .Add(siteModelUid, new VSS.TRex.Designs.Models.DesignDescriptor(designUid, string.Empty, localFileName), extents);

        DIContext.Obtain<IExistenceMaps>().SetExistenceMap(siteModelUid, Consts.EXISTENCE_MAP_DESIGN_DESCRIPTOR, design.ID, TTM.SubGridOverlayIndex());
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
        TTMDesign TTM = new TTMDesign(SubGridTreeConsts.DefaultCellSize);
        TTM.LoadFromFile(Path.Combine(new[] { localPath, localFileName }));

        BoundingWorldExtent3D extents = new BoundingWorldExtent3D();
        TTM.GetExtents(out extents.MinX, out extents.MinY, out extents.MaxX, out extents.MaxY);
        TTM.GetHeightRange(out extents.MinZ, out extents.MaxZ);

        // Create the new design for the site model (note that SS and design types are different)
        var design = DIContext.Obtain<ISurveyedSurfaceManager>()
          .Add(siteModelUid, new VSS.TRex.Designs.Models.DesignDescriptor(designUid, string.Empty, localFileName), surveyedUtc, extents);

        DIContext.Obtain<IExistenceMaps>().SetExistenceMap(siteModelUid, Consts.EXISTENCE_SURVEYED_SURFACE_DESCRIPTOR, design.ID, TTM.SubGridOverlayIndex());
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
        AlignmentDesign alignmentDesign = new AlignmentDesign(SubGridTreeConsts.DefaultCellSize);
        alignmentDesign.LoadFromFile(Path.Combine(new[] { localPath, localFileName }));

        // todo when SDK avail
        BoundingWorldExtent3D extents = new BoundingWorldExtent3D();
        alignmentDesign.GetExtents(out extents.MinX, out extents.MinY, out extents.MaxX, out extents.MaxY);
        alignmentDesign.GetHeightRange(out extents.MinZ, out extents.MaxZ);

        // Create the new design for the site model
        var design = DIContext.Obtain<IAlignmentManager>()
          .Add(siteModelUid, new VSS.TRex.Designs.Models.DesignDescriptor(designUid, string.Empty, localFileName), extents);

        // todo when SDK avail
        //DIContext.Obtain<IExistenceMaps>().SetExistenceMap(siteModelUid, Consts.EXISTENCE_MAP_DESIGN_DESCRIPTOR, design.ID, alignmentDesign.SubGridOverlayIndex());
      }
      catch (Exception e)
      {
        throw new TRexException($"Exception writing alignment to siteModel:", e);
      }
    }

  }
}
