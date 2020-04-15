using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.Extensions.Logging;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.TagFileDump.Controllers
{
  public class TagFileController : Controller
  {
    private readonly ITransferProxy _transferProxy;
    private readonly ILogger<TagFileController> _logger;

    public TagFileController(ITransferProxy transferProxy, ILogger<TagFileController> logger)
    {
      _transferProxy = transferProxy;
      _logger = logger;
    }

    [Route("api/v2/tagfiles")]
    [HttpPost]
    public IActionResult PostTagFileNonDirectSubmission([FromBody] CompactionTagFileRequest request)
    {
      return Json(ProcessTagFile(request, "non-direct"));
    }

    [Route("api/v2/tagfiles/direct")]
    [HttpPost]
    public IActionResult PostTagFileDirectSubmission([FromBody] CompactionTagFileRequest request)
    {
      return Json(ProcessTagFile(request, "direct"));
    }

    private ContractExecutionResult ProcessTagFile(CompactionTagFileRequest request, string method)
    {
      if (request == null)
      {
        _logger.LogWarning("Empty request passed");
        return ContractExecutionResult.ErrorResult("Empty Request");
      }

      request.Validate();

      _logger.LogInformation($"Received Tag File (via {method}) with filename: {request.FileName}. TCC Org: {request.OrgId}. Data Length: {request.Data.Length}");

      using (var data = new MemoryStream(request.Data))
      {
        _logger.LogInformation($"Uploading Tag File {request.FileName}");
        var path = GetS3Key(method,request.FileName, request.OrgId);
        // S3 needs a full path including file, but TCC needs a path and filename as two separate variables
        var s3FullPath = path + request.FileName;

        _transferProxy.Upload(data, s3FullPath);
        _logger.LogInformation($"Successfully uploaded Tag File {request.FileName}");
      }

      return new ContractExecutionResult(0);
    }


    private string GetS3Key(string method, string tagFileName, string tccOrgId)
    {
      //Example tagfile name: 0415J010SW--HOUK IR 29 16--170731225438.tag
      //Format: <display or ECM serial>--<machine name>--yyMMddhhmmss.tag
      //Required folder structure is <TCC org id>/<serial>--<machine name>/<archive folder>/<serial--machine name--date>/<tagfile>
      //e.g. 0415J010SW--HOUK IR 29 16/Production-Data (Archived)/0415J010SW--HOUK IR 29 16--170731/0415J010SW--HOUK IR 29 16--170731225438.tag
      const string separator = "--";
      string[] parts = tagFileName.Split(new string[] {separator}, StringSplitOptions.None);
      var nameWithoutTime = tagFileName.Substring(0, tagFileName.Length - 10);
      //TCC org ID is not provided with direct submission from machines
      var prefix = string.IsNullOrEmpty(tccOrgId) ? string.Empty : $"{tccOrgId}/";

      return $"{DateTime.Today:yyyy-MM-dd}/{method}/{prefix}{parts[0]}{separator}{parts[1]}/{nameWithoutTime}/";
    }

  }
}
