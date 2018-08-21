using System;
using Microsoft.AspNetCore.Mvc;
using VSS.MasterData.Models.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace MockProjectWebApi.Controllers
{
  public class MockNotificationController : Controller
  {

    /// <summary>
    /// Dummies the add.
    /// </summary>
    [Route("api/v2/notification/addfile")]
    [HttpGet]
    public BaseDataResult DummyAddFileGet(
      [FromQuery] Guid projectUid,
      [FromQuery] ImportedFileType fileType,
      [FromQuery] Guid fileUid,
      [FromQuery] string fileDescriptor,
      [FromQuery] long fileId,
      [FromQuery] DxfUnitsType dXfUnitsType)
    {
      var hasDxfTiles = fileType == ImportedFileType.Linework || fileType == ImportedFileType.Alignment ||
                        fileType == ImportedFileType.DesignSurface;
      var res = new AddFileResult { MinZoomLevel = hasDxfTiles ? 15 : 0, MaxZoomLevel = hasDxfTiles ? 19 : 0 };
      var message =
        $"DummyAddFileGet: res {res}. projectUid {projectUid} fileType {fileType} fileUid {fileUid} fileDescriptor {fileDescriptor} fileId {fileId} dXfUnitsType {dXfUnitsType}";
      Console.WriteLine(message);
      return res;
    }

    /// <summary>
    /// Dummies the delete.
    /// </summary>
    [Route("api/v2/notification/deletefile")]
    [HttpGet]
    public BaseDataResult DummyDeleteFileGet(
      [FromQuery] Guid projectUid,
      [FromQuery] ImportedFileType fileType,
      [FromQuery] Guid fileUid,
      [FromQuery] string fileDescriptor,
      [FromQuery] long fileId)
    {
      var res = new BaseDataResult();
      var message =
        $"DummyDeleteFileGet: res {res}. projectUid {projectUid} fileType {fileType} fileUid {fileUid} fileDescriptor {fileDescriptor} fileId {fileId}";
      Console.WriteLine(message);
      return res;
    }

    /// <summary>
    /// Dummies the update.
    /// </summary>
    [Route("api/v2/notification/updatefiles")]
    [HttpGet]
    public BaseDataResult DummyUpdateFilesGet(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid[] fileUids)
    {
      var res = new BaseDataResult();
      var message = $"DummyUpdateFilesGet: res {res}. projectUid {projectUid} fileUids {fileUids}";
      Console.WriteLine(message);
      return res;
    }

    /// <summary>
    /// Dummies the nofication of change.
    /// </summary>
    [Route("api/v2/notification/importedfilechange")]
    [HttpGet]
    public BaseDataResult DummyNotifyImportedFileChange(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid fileUid)
    {
      var res = new BaseDataResult();
      var message = $"DummyNotifyImportedFileChange: res {res}. projectUid {projectUid} fileUid {fileUid}";
      Console.WriteLine(message);
      return res;
    }


    /// <summary>
    /// Dummies the notification that a filterUid has been updated/deleted
    /// </summary>
    [Route("api/v2/notification/filterchange")]
    [HttpGet]
    public BaseDataResult DummyNotifyFilterChangeGet(
      [FromQuery] Guid filterUid
    )
    {
      var res = new BaseDataResult();
      var message = $"DummyNotifyFilterChangeGet: res {res}. filterUid {filterUid}";
      Console.WriteLine(message);
      return res;
    }

    /// <summary>
    /// Dummies the notification that a projectUid has been updated/deleted
    /// </summary>
    [Route("api/v2/notification/invalidatecache")]
    [HttpGet]
    public BaseDataResult DummyNotifyProjectChangeGet(
      [FromQuery] Guid projectUid
    )
    {
      var res = new BaseDataResult();
      var message = $"DummyNotifyProjectChangeGet: res {res}. projectUid {projectUid}";
      Console.WriteLine(message);
      return res;
    }


  }
}
