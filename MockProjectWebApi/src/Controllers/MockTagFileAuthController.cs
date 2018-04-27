using Microsoft.AspNetCore.Mvc;
using MockProjectWebApi.Common;
using MockProjectWebApi.Utils;

// Mocking the Tagfile Auth Service
// Uses json files to provide a dynamic response for testing

namespace MockProjectWebApi.Controllers
{

  // Mock GetProjectID
  public class MockTagFileAuthController : Controller
  {
    [Route("api/v1/project/getId")]
    [HttpPost]
    public GetProjectIdResult GetProjectId([FromBody] GetProjectIdRequest request)
    {
      request.Validate();
      TagFileUtilsHelper.Init();
      // Return values are stored in Json files e.g. GetProjectId.json
      return TagFileUtilsHelper.LookupProjectId(request.assetId, request.tccOrgUid);
    }

    [Route("api/v1/asset/getId")]
    [HttpPost]
    public GetAssetIdResult GetAssetId([FromBody]GetAssetIdRequest request)
    {
      request.Validate();
      TagFileUtilsHelper.Init();
      return TagFileUtilsHelper.LookupAssetId(request.projectId, request.radioSerial);
    }

    [Route("api/v1/project/getBoundary")]
    [HttpPost]
    public GetProjectBoundaryAtDateResult PostProjectBoundary([FromBody]GetProjectBoundaryAtDateRequest request)
    {
      request.Validate();
      return TagFileUtilsHelper.LookupBoundary(request.projectId);
    }

    [Route("api/v1/project/getBoundaries")]
    [HttpPost]
    public GetProjectBoundariesAtDateResult PostProjectBoundaries([FromBody]GetProjectBoundariesAtDateRequest request)
    {
      request.Validate();
      return TagFileUtilsHelper.LookupBoundaries(request.assetId);
    }

    [Route("api/v2/notification/tagFileProcessingError")]
    [HttpPost]
    public TagFileProcessingErrorResult PostTagFileProcessingError([FromBody] TagFileProcessingErrorV2Request request)
    {
      TagFileUtilsHelper.Init();
      request.Validate();
      return TagFileUtilsHelper.ReportError();
    }

    /// <summary>
    /// Posts the application alarm.
    /// </summary>
    [Route("api/v1/notification/appAlarm")]
    [HttpPost]
    public ContractExecutionResult PostAppAlarm([FromBody] AppAlarmMessage request)
    {
      request.Validate();
      return new ContractExecutionResult();
    }
  }
}
