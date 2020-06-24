using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MockProjectWebApi.Utils;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models.Coords;
using VSS.Productivity3D.Models.ResultHandling.Coords;

namespace MockProjectWebApi.Controllers
{
  public class MockTRexV1CompactionDataController : BaseController
  {
    public MockTRexV1CompactionDataController(ILoggerFactory loggerFactory) : base(loggerFactory)
    { }

    [Route("api/v1/coordinateconversion")]
    [HttpPost]
    public ContractExecutionResult TRexConvertNEtoLL([FromBody] CoordinateConversionRequest coordinateConversionRequest)
    {
      Logger.LogInformation($"{nameof(TRexConvertNEtoLL)}: coordinateConversionRequest {JsonConvert.SerializeObject(coordinateConversionRequest)}");
      var coordinateConversionResult = new CoordinateConversionResult(new TwoDConversionCoordinate[0]);

      if (coordinateConversionRequest.ProjectUid.HasValue && coordinateConversionRequest.ProjectUid.Value.ToString() == ConstantsUtil.DIMENSIONS_PROJECT_UID)
      {
        var points = new[] { new TwoDConversionCoordinate(180, 15) };
        coordinateConversionResult = new CoordinateConversionResult(points);
      }

      Logger.LogInformation($"{nameof(TRexConvertNEtoLL)}: CoordinateConversionResult {JsonConvert.SerializeObject(coordinateConversionResult)}");
      return coordinateConversionResult;
    }

  }
}
