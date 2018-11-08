using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.AWS.TransferProxy;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.TRex.Designs;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Exceptions;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.ExistenceMaps.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

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
    /// <param name="siteModelID"></param>
    /// <param name="fileName"></param>
    /// <param name="minX"></param>
    /// <param name="minY"></param>
    /// <param name="maxX"></param>
    /// <param name="maxY"></param>
    /// <returns></returns>
    [HttpPost("{siteModelID}")]
    public JsonResult AddDesignToSiteModel(string siteModelID,
      [FromQuery] string fileName,
      [FromQuery] double minX,
      [FromQuery] double minY,
      [FromQuery] double maxX,
      [FromQuery] double maxY)
    {
      return new JsonResult(DIContext.Obtain<IDesignManager>().Add
        (Guid.Parse(siteModelID), 
        new DesignDescriptor(Guid.NewGuid(), "", fileName, 0),  
        new BoundingWorldExtent3D(minX, minY, maxX, maxY)));
    }

    /// <summary>
    /// Adds a new design to a sitemodel.
    ///   temporarily using s3
    ///     path less bucket name and filename
    ///     e.g. https://s3-us-west-2.amazonaws.com/vss-project3dp-stg/{projectID}/bowlfill+1290+6-5-18.ttm
    ///          is where ProjectSvc/Scheduler currently puts this file
    ///    Bucket:   vss-project3dp-stg 
    ///    Path:     projectID
    ///    Filename: "bowlfill 1290 6-5-18.ttm"   (is + for a space)
    /// 
    /// </summary>
    /// <param name="siteModelID"></param>
    /// <param name="designID"></param>
    /// <param name="fileName"></param>
    /// <param name="fileType"></param>
    /// <returns></returns>
    [HttpPost("{siteModelID}/{fileName}")]
    public async Task<JsonResult> AddDesignSurfaceFromAwsS3ToSiteModel(
      [FromRoute] string siteModelID,
      [FromRoute] string fileName, 
      [FromQuery] string designID
      )
    {
      var siteModelUID = Guid.Parse(siteModelID);
      var designUID = Guid.Parse(designID);

      var s3Path = $"{siteModelID}";
      string downloadLocalPath = Path.GetTempPath(); 
      var downladedok = await DownloadFileFromS3Async(s3Path, downloadLocalPath, fileName).ConfigureAwait(false);
      var loadedDesign = AddTheDesignToSiteModel(siteModelUID, designUID, downloadLocalPath, fileName);

      // todojeannie should this go into IDesignManager().Add?
      var spatialUploadedOk = UploadFileToS3(downloadLocalPath, fileName + ".$DesignSpatialIndex$", s3Path);
      var subgridUploadedOk = UploadFileToS3(downloadLocalPath, fileName + ".$DesignSubgridIndex$", s3Path);

      // todojeannie should remove delete the file/s from s3? ProjectSvc probably the orig and Trex the indices
      return new JsonResult(DIContext.Obtain<IDesignManager>().List(Guid.Parse(siteModelID)).Locate(Guid.Parse(designID))); 
    }

    ///// <summary>
    ///// Adds a new design to a sitemodel, round trip
    /////     i.e. , from a local file upload to s3, then the process which downloads it etc
    /////   temporarily using s3
    /////     path less bucket name and filename
    /////     e.g. https://s3-us-west-2.amazonaws.com/vss-exports-stg/3dpm/100015/bowlfill+1290+6-5-18.ttm.json
    /////          is where ProjectSvc/Scheduler currently puts this file
    /////    Bucket:   vss-project3dp-stg
    /////    Path:     3dpm/100015  or project/importedFile   (eventually may be ProjectUID/DesignUID) 
    /////    Filename: bowlfill+1290+6-5-18.ttm (is + for a space?)
    ///// 
    ///// </summary>
    ///// <param name="siteModelID"></param>
    ///// <param name="designID"></param>
    ///// <param name="localPath"></param>
    ///// <param name="fileName"></param>
    ///// <param name="fileType"></param>
    ///// <returns></returns>
    //[HttpPost("{siteModelID}/{designID}")]
    //public async Task<JsonResult> AddDesignSurfaceToSiteModel(
    //  [FromRoute] string siteModelID,
    //  [FromRoute] string designID,
    //  [FromQuery] string localPath,
    //  [FromQuery] string fileName,
    //  [FromQuery] ImportedFileType fileType
    //)
    //{
    //  var siteModelUID = Guid.Parse(siteModelID);
    //  var designUID = Guid.Parse(designID);

    //  var s3Path = $"{siteModelID}";
    //  var uploadedOk = UploadFileToS3(localPath, fileName, s3Path);

    //  string downloadLocalPath = @"C:\Temp\TRex Designs\downloads";
    //  var downladedok = await DownloadFileFromS3Async(s3Path, downloadLocalPath, fileName).ConfigureAwait(false);
    //  var loadedDesign = AddTheDesignToSiteModel(siteModelUID, designUID, downloadLocalPath, fileName);

    //  return new JsonResult(DIContext.Obtain<IDesignManager>().List(Guid.Parse(siteModelID)).Locate(Guid.Parse(designID)));
    //}


    private bool UploadFileToS3(string localPath, string fileName, string s3Path)
    {
      var transferProxy = DIContext.Obtain<ITransferProxy>();

      var localFullPath = Path.Combine(localPath, fileName);
      var s3FullPath = $"{s3Path}/{fileName}";
      try
      {
        var fileStream = System.IO.File.Open(localFullPath, FileMode.Open, FileAccess.Read);
        // TransferUtility will create the 'directory' if not already there
        transferProxy.Upload(fileStream, s3FullPath);
      }
      catch (Exception e)
      {
        throw new TRexException($"Exception writing design to s3: {e}");
      }
      return true;
    }

    private async Task<bool> DownloadFileFromS3Async(string s3path, string downloadLocalPath, string fileName)
    {
      var transferProxy = DIContext.Obtain<ITransferProxy>();

      var s3FullPath = $"{s3path}/{fileName}";
      FileStreamResult fileStreamResult;
      try
      {
        fileStreamResult = await transferProxy.Download(s3FullPath).ConfigureAwait(false);
      }
      catch (Exception e)
      {
        throw new TRexException($"Exception reading design from s3: {e}");
      }

      if (string.IsNullOrEmpty(fileStreamResult.ContentType))
      {
        throw new TRexException($"Exception setting up download from S3.ContentType unknown.");
      }

      try
      {
        var downloadFullPath = Path.Combine(downloadLocalPath, fileName);
        using (var targetFileStream = System.IO.File.Create(downloadFullPath, (int) fileStreamResult.FileStream.Length))
        {
          fileStreamResult.FileStream.CopyTo(targetFileStream);
        }
      }
      catch (Exception e)
      {
        throw new TRexException($"Exception writing design file locally: {e}");
      }

      return true;
    }

    private IDesign AddTheDesignToSiteModel(Guid siteModelUID, Guid designUID, string localPath, string localFileName)
    {
      IDesign design = null;

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
        design = DIContext.Obtain<IDesignManager>().Add(siteModelUID,
           new DesignDescriptor(designUID, string.Empty, localFileName, 0),
          extents);

        DIContext.Obtain<IExistenceMaps>().SetExistenceMap(siteModelUID, Consts.EXISTENCE_MAP_DESIGN_DESCRIPTOR, design.ID, TTM.SubgridOverlayIndex());
      }
      catch (Exception e)
      {
        throw new TRexException($"Exception writing design to siteModel: {e}");
      }

      return design;
    }
  }
}
