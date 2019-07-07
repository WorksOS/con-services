using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Contracts
{
  public interface ITerrainContract
  {
    Task<IActionResult> Get(int x, int y, int z, string formatExtension, [FromQuery] Guid projectUid, [FromQuery] long filterId);
  }
}
