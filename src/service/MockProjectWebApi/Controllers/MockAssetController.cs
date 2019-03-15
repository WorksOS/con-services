using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;

namespace MockProjectWebApi.Controllers
{
  public class MockAssetController : Controller
  {
    [HttpGet("api/v1/mock/Asset/List")]
    public AssetDataResult GetAssetList(
      [FromQuery] string customerUid, 
      [FromQuery] string pageSize
      )
    {
      Console.WriteLine("Get MockAssetList for Customer");

      var result = new AssetDataResult
      {
        Assets = new List<AssetData>(1)
        {
          new AssetData(Guid.Parse(customerUid), Guid.NewGuid(), "Asset Name", 666,
            "serial Number", "CAT", "D7", "Asset Type",
            "", 1, 1977)
        }
      };

      return result;
    }
  }
}
