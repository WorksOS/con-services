using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace MockProjectWebApi.Controllers
{
  public class MockTileController : Controller
  {
    [Route("/internal/v1/mock/generatedxftiles")]
    [HttpGet]
    public Task<MockDataOceanController.TileMetadata> GenerateDxfTiles([FromQuery] string dcFileName,
      [FromQuery] string dxfFileName, [FromQuery] DxfUnitsType dxfUnitsType)
    {
      Console.WriteLine($"{nameof(GenerateDxfTiles)}: {Request.QueryString}");

      return Task.FromResult(MockDataOceanController.tileMetadata);
    }

    [Route("/internal/v1/mock/deletedxftiles")]
    [HttpDelete]
    public Task<bool> DeleteDxfTiles([FromQuery] string dxfFileName)
    {
      Console.WriteLine($"{nameof(DeleteDxfTiles)}: {Request.QueryString}");

      return Task.FromResult(true);
    }
  }
}
