using System;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Productivity3D.Models;
using VSS.Productivity3D.Productivity3D.Models.Notification.ResultHandling;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Models;

namespace MockProjectWebApi.Controllers
{
  public class MockProductivity3dV2NotificationController : Controller
  {

    /// <summary>
    /// Dummies the add.
    /// </summary>
    [Route("api/v2/notification/addfile")]
    [HttpGet]
    public ContractExecutionResult DummyAddFileGet(
      [FromQuery] Guid projectUid,
      [FromQuery] ImportedFileType fileType,
      [FromQuery] Guid fileUid,
      [FromQuery] string fileDescriptor,
      [FromQuery] long fileId,
      [FromQuery] DxfUnitsType dXfUnitsType)
    {
      var hasDxfTiles = fileType == ImportedFileType.Linework || fileType == ImportedFileType.Alignment ||
                        fileType == ImportedFileType.DesignSurface;
      var res = new AddFileResult(ContractExecutionStatesEnum.ExecutedSuccessfully, ContractExecutionResult.DefaultMessage)
      {
        MinZoomLevel = hasDxfTiles ? 15 : 0,
        MaxZoomLevel = hasDxfTiles ? 19 : 0,
        FileDescriptor = JsonConvert.DeserializeObject<FileDescriptor>(fileDescriptor),
        FileUid = fileUid,
        UserEmailAddress = "dummy@xyz.com"
      };
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
    public BaseMasterDataResult DummyDeleteFileGet(
      [FromQuery] Guid projectUid,
      [FromQuery] ImportedFileType fileType,
      [FromQuery] Guid fileUid,
      [FromQuery] string fileDescriptor,
      [FromQuery] long fileId)
    {
      var res = new BaseMasterDataResult();
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
    public BaseMasterDataResult DummyUpdateFilesGet(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid[] fileUids)
    {
      var res = new BaseMasterDataResult();
      var message = $"DummyUpdateFilesGet: res {res}. projectUid {projectUid} fileUids {fileUids}";
      Console.WriteLine(message);
      return res;
    }

    /// <summary>
    /// Dummies the nofication of change.
    /// </summary>
    [Route("api/v2/notification/importedfilechange")]
    [HttpGet]
    public BaseMasterDataResult DummyNotifyImportedFileChange(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid fileUid)
    {
      var res = new BaseMasterDataResult();
      var message = $"DummyNotifyImportedFileChange: res {res}. projectUid {projectUid} fileUid {fileUid}";
      Console.WriteLine(message);
      return res;
    }


    /// <summary>
    /// Dummies the notification that a filterUid has been updated/deleted
    /// </summary>
    [Route("api/v2/notification/filterchange")]
    [HttpGet]
    public BaseMasterDataResult DummyNotifyFilterChangeGet(
      [FromQuery] Guid filterUid
    )
    {
      var res = new BaseMasterDataResult();
      var message = $"DummyNotifyFilterChangeGet: res {res}. filterUid {filterUid}";
      Console.WriteLine(message);
      return res;
    }

    /// <summary>
    /// Dummies the notification that a projectUid has been updated/deleted
    /// </summary>
    [Route("api/v2/notification/invalidatecache")]
    [HttpGet]
    public BaseMasterDataResult DummyNotifyProjectChangeGet(
      [FromQuery] Guid projectUid
    )
    {
      var res = new BaseMasterDataResult();
      var message = $"DummyNotifyProjectChangeGet: res {res}. projectUid {projectUid}";
      Console.WriteLine(message);
      return res;
    }
  }
}
