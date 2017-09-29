﻿using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;

namespace MockProjectWebApi.Controllers
{
  public class MockRaptorController : Controller
  {

    /// <summary>
    /// Dummies the post.
    /// </summary>
    [Route("api/v1/mock/coordsystem/validation")]
    [HttpPost]
    public CoordinateSystemSettingsResult DummyCoordsystemValidationPost(
      [FromBody] CoordinateSystemFileValidationRequest request)
    {
      var cs = CoordinateSystemSettingsResult.CreateCoordinateSystemSettings
      (
        csName: "Mock generated by DummyCoordsystemValidationPost",
        csGroup: "Projection from Data Collector",
        csib: new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 },
        datumName: "Datum Grid",
        siteCalibration: false,
        geoidFileName: "NZ2009.GGF",
        geoidName: "New Zealand Geoid 2009",
        isDatumGrid: true,
        latitudeDatumGridFileName: "NZNATlat.DGF",
        longitudeDatumGridFileName: "NZNATlon.DGF",
        heightDatumGridFileName: null,
        shiftGridName: null,
        snakeGridName: null,
        verticalDatumName: null,
        unsupportedProjection: false
      );
      Console.WriteLine(
        "DummyCoordsystemValidationPost: CoordinateSystemFileValidationRequest {0}. CoordinateSystemSettings {1}",
        JsonConvert.SerializeObject(request), JsonConvert.SerializeObject(cs));
      return cs;
    }

    /// <summary>
    /// Dummies the post.
    /// </summary>
    [Route("api/v1/mock/coordsystem")]
    [HttpPost]
    public CoordinateSystemSettingsResult DummyCoordsystemPost([FromBody] CoordinateSystemFile request)
    {
      var cs = CoordinateSystemSettingsResult.CreateCoordinateSystemSettings
      (
        csName: "Mock generated by DummyCoordsystemPost",
        csGroup: "Projection from Data Collector",
        csib: new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 },
        datumName: "Datum Grid",
        siteCalibration: false,
        geoidFileName: "NZ2009.GGF",
        geoidName: "New Zealand Geoid 2009",
        isDatumGrid: true,
        latitudeDatumGridFileName: "NZNATlat.DGF",
        longitudeDatumGridFileName: "NZNATlon.DGF",
        heightDatumGridFileName: null,
        shiftGridName: null,
        snakeGridName: null,
        verticalDatumName: null,
        unsupportedProjection: false
      );
      Console.WriteLine("DummyCoordsystemPost: CoordinateSystemFile {0}. CoordinateSystemSettings {1}",
        JsonConvert.SerializeObject(request), JsonConvert.SerializeObject(cs));
      return cs;
    }

    /// <summary>
    /// Dummies the get.
    /// </summary>
    [Route("api/v2/notification/addfile")]
    [HttpGet]
    public BaseDataResult DummyAddFileGet(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid fileUid,
      [FromQuery] string fileDescriptor,
      [FromQuery] long importedFileId)
    {
      var res = new BaseDataResult();
      var message = $"DummyAddFileGet: res {res}. projectUid {projectUid} fileUid {fileUid} fileDescriptor {fileDescriptor} importedFileId {importedFileId}";
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
      [FromQuery] Guid fileUid,
      [FromQuery] string fileDescriptor,
      [FromQuery] long importedFileId)
    {
      var res = new BaseDataResult();
      var message = $"DummyDeleteFileGet: res {res}. projectUid {projectUid} fileUid {fileUid} fileDescriptor {fileDescriptor} importedFileId {importedFileId}";
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
    /// Dummies the project projectSettings validation.
    /// </summary>
    [Route("api/v2/compaction/validatesettings")]
    [HttpGet]
    public BaseDataResult DummyValidateProjectSettingsGet(
      [FromQuery] Guid projectUid,
      [FromQuery] string projectSettings)
    {
      BaseDataResult res = new BaseDataResult();
      var message = $"DummyValidateProjectSettingsGet: res {res}. projectSettings {projectSettings}";
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
  }
}