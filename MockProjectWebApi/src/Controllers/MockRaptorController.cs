﻿using Microsoft.AspNetCore.Mvc;
using MockProjectWebApi.Utils;
using Newtonsoft.Json;
using System;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace MockProjectWebApi.Controllers
{
  public class MockRaptorController : Controller
  {
    [Route("api/v2/mock/export/veta")]
    [HttpGet]
    public ExportResult GetMockVetaExportData(
      [FromQuery] Guid projectUid,
      [FromQuery] string fileName,
      [FromQuery] string machineNames,
      [FromQuery] Guid? filterUid)
    {
      if (projectUid.ToString() == ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1 &&
          fileName == MockSchedulerController.SUCCESS_JOB_ID &&
          string.IsNullOrEmpty(machineNames) &&
          filterUid.ToString() == "81422acc-9b0c-401c-9987-0aedbf153f1d")
      {
        var result = @"{
          ""exportData"": ""UEsDBBQAAAgIAEsQe0tesHMI2AEAANUIAAAIAAAAVGVzdC5jc3bNlM1um0AQgO+V+g4op1aabPeH5Sc3Sn4q1SDLRpZ6stawjVcB1gXiqn21HvpIfYUu1CZSAlVRKsVcZhdmhk/fDvz68TNRhYRQ5nm8Lrp4ZeJVLveiUbo067mo6/i+2MgKZqJuFiJTetaU6Te4lLW6LWNhGkQi3apSwnInZba+K95tu+Sb+TLSmQQTgzRNdG76JaK6bXuG+r5sYCVylc11DTP1uelqwmjV5bSx3UeX827fxg7gcP+6kl/WH75366DYrYs/rZOtSu9KWdfmVQeqGykqWKmNXDaikV1BIk1F+voVxYSex3p/jrmF/QtuXzAHMQ8DoZgjwjBQj1LECQbue4i75glgiIK5hQmchUFihUvuvD8DijDMdNpZsxbJRwi1qGppvcGIYq94Cww+ydpUA8cmlTAX+UC6tbnaJAwLuZemBmLdWMFul6tUbHIJg5juA6btPsJ0qDuGySZi2m3BMzD5gE2PH2xye9ymPc0mRu5jzGtdfRVV9k+YQ4feY7Z6T+PQ+cCh95hs9NB9RKbatJ9jc2g2j5iOwRyx+fKzyRDxj7Ppj39C7jSbBNH/ZPMp5knOZo/J2QHTYadss8c8bZuee7T5l9/7y9vsMV06bpNPw/Qn2PwNUEsBAhQAFAAACAgASxB7S16wcwjYAQAA1QgAAAgAAAAAAAAAAAAgAAAAAAAAAFRlc3QuY3N2UEsFBgAAAAABAAEANgAAAP4BAAAAAA=="",
          ""resultCode"": 0,
          ""Code"": 0,
          ""Message"": ""success""
          }";
        return JsonConvert.DeserializeObject<ExportResult>(result);
      }

      return new ExportResult { ResultCode = 0, ExportData = null };
    }

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
      var res = new AddFileResult{MinZoomLevel = hasDxfTiles ? 15 : 0, MaxZoomLevel = hasDxfTiles ? 19 : 0};
      var message = $"DummyAddFileGet: res {res}. projectUid {projectUid} fileType {fileType} fileUid {fileUid} fileDescriptor {fileDescriptor} fileId {fileId} dXfUnitsType {dXfUnitsType}";
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
      var message = $"DummyDeleteFileGet: res {res}. projectUid {projectUid} fileType {fileType} fileUid {fileUid} fileDescriptor {fileDescriptor} fileId {fileId}";
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
    /// Dummies the project projectSettings validation.
    /// </summary>
    [Route("api/v2/validatesettings")]
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
    /// Dummies the project projectSettings validation.
    /// </summary>
    [Route("api/v2/validatesettings")]
    [Route("api/v2/compaction/validatesettings")]
    [HttpPost]
    public BaseDataResult DummyValidateProjectSettingsPost([FromBody] ProjectSettingsRequest request)
    {
      BaseDataResult res = new BaseDataResult();
      var message = $"DummyValidateProjectSettingsGet: res {res}. projectSettings {request.Settings}";
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