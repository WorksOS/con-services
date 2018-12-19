using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common;
using VSS.TRex.Designs;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Exceptions;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.ExistenceMaps.Interfaces;
using VSS.TRex.Gateway.Common.Helpers;
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
    /// <param name="siteModelID">Grid to return status for</param>
    /// <returns></returns>
    [HttpGet("{siteModelID}")]
    public JsonResult GetDesignsForSiteModel(string siteModelID)
    {
      return new JsonResult(DIContext.Obtain<IDesignManager>().List(Guid.Parse(siteModelID)));
    }

    /// <summary>
    /// Deletes a design from a sitemodel.
    /// </summary>
    /// <param name="siteModelID">Grid to return status for</param>
    /// <param name="designID"></param>
    /// <returns></returns>
    [HttpDelete("{siteModelID}/{designID}")]
    public JsonResult DeleteDesignFromSiteModel(string siteModelID, string designID)
    {
      return new JsonResult(DIContext.Obtain<IDesignManager>().Remove(Guid.Parse(siteModelID), Guid.Parse(designID)));
    }

    /// <summary>
    /// Adds a new design to a sitemodel. 
    /// </summary>
    /// <param name="siteModelUid"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    [HttpPost("{siteModelUid}")]
    public async Task<JsonResult> AddDesignToSiteModel(
      string siteModelUid,
      [FromQuery] string fileName)
    {
      if (string.IsNullOrEmpty(siteModelUid))
        throw new ArgumentException($"Invalid siteModelUid (you need to have selected one first): {siteModelUid}");

      if (string.IsNullOrEmpty(fileName) || 
          !Path.HasExtension(fileName) || 
          (string.Compare(Path.GetExtension(fileName), ".ttm", StringComparison.OrdinalIgnoreCase) != 0))
        throw new ArgumentException($"Invalid [path]filename: {fileName}");

      if (!System.IO.File.Exists(fileName))
        throw new ArgumentException($"Unable to locate [path]fileName: {fileName}");
      
      var siteModelGuid = Guid.Parse(siteModelUid);
      var designUid = Guid.NewGuid();

      var fileNameOnly = Path.GetFileName(fileName);

      // copy local file to S3
      var designFileLoadedOk = S3FileTransfer.WriteFile(Path.GetDirectoryName(fileName), Guid.Parse(siteModelUid), fileNameOnly);
      if (!designFileLoadedOk)
        throw new ArgumentException($"Unable to copy design file to S3: {fileNameOnly}");

      // download to appropriate local location and add to site model
      string downloadLocalPath = DesignHelper.EstablishLocalDesignFilepath(siteModelUid);
      var downloadedok = await S3FileTransfer.ReadFile(Guid.Parse(siteModelUid), fileNameOnly, downloadLocalPath).ConfigureAwait(false);
      if (!downloadedok)
        throw new ArgumentException($"Unable to restore same design file from S3: {fileNameOnly}");
      AddTheDesignToSiteModel(siteModelGuid, designUid, downloadLocalPath, fileNameOnly);

      // upload indices
      var spatialUploadedOk = S3FileTransfer.WriteFile(downloadLocalPath, Guid.Parse(siteModelUid), fileNameOnly + ".$DesignSpatialIndex$");
      if (!spatialUploadedOk)
        throw new ArgumentException($"Unable to copy spatial index file to S3: {fileNameOnly + ".$DesignSpatialIndex$"}");
      var subgridUploadedOk = S3FileTransfer.WriteFile(downloadLocalPath, Guid.Parse(siteModelUid), fileNameOnly + ".$DesignSubgridIndex$");
      if (!subgridUploadedOk)
        throw new ArgumentException($"Unable to copy subgrid index file to S3: {fileNameOnly + ".$DesignSubgridIndex$"}");

      return new JsonResult(DIContext.Obtain<IDesignManager>().List(siteModelGuid).Locate(designUid));
    }

    
    private void AddTheDesignToSiteModel(Guid siteModelUid, Guid designUid, string localPath, string localFileName)
    {

      // Invoke the service to add the design
      try
      {
        // Load the file and extract its extents
        TTMDesign TTM = new TTMDesign(SubGridTreeConsts.DefaultCellSize);
        TTM.LoadFromFile(Path.Combine(new[] { localPath, localFileName }));

        BoundingWorldExtent3D extents = new BoundingWorldExtent3D();
        TTM.GetExtents(out extents.MinX, out extents.MinY, out extents.MaxX, out extents.MaxY);
        TTM.GetHeightRange(out extents.MinZ, out extents.MaxZ);

        // Create the new design for the site model
        var design = DIContext.Obtain<IDesignManager>().Add(siteModelUid,
           new DesignDescriptor(designUid, string.Empty, localFileName, 0),
          extents);

        DIContext.Obtain<IExistenceMaps>().SetExistenceMap(siteModelUid, Consts.EXISTENCE_MAP_DESIGN_DESCRIPTOR, design.ID, TTM.SubgridOverlayIndex());
      }
      catch (Exception e)
      {
        throw new TRexException($"Exception writing design to siteModel:", e);
      }
    }
  }
}
