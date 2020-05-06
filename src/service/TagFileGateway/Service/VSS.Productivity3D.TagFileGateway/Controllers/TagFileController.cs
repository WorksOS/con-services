using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.Extensions.Logging;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.TagFileForwarder.Controllers
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
    
    }


  }
}
