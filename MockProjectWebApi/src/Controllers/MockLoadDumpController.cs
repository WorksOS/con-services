using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using MockProjectWebApi.Utils;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;

namespace MockProjectWebApi.Controllers
{
  public class MockLoadDumpController : Controller
  {
    private static List<LoadDumpLocation> LoadDumpData = null;

    public MockLoadDumpController()
    {
      if (LoadDumpData == null)
      {
        LoadDumpData = new List<LoadDumpLocation>
        {
          new LoadDumpLocation
          {
            loadLatitude = 36.2062,
            loadLongitude = -115.0258,
            dumpLatitude = 36.2068,
            dumpLongitude = -115.0262
          },
          new LoadDumpLocation
          {
          loadLatitude = 36.2062,
          loadLongitude = -115.0179,
          dumpLatitude = 36.2068,
          dumpLongitude = -115.0177
        },
          new LoadDumpLocation
          {
            loadLatitude = 36.2072,
            loadLongitude = -115.0258,
            dumpLatitude = 36.2078,
            dumpLongitude = -115.0262
          },
          new LoadDumpLocation
          {
            loadLatitude = 36.2072,
            loadLongitude = -115.0179,
            dumpLatitude = 36.2078,
            dumpLongitude = -115.0177
          },
          new LoadDumpLocation
          {
            loadLatitude = 36.2082,
            loadLongitude = -115.0258,
            dumpLatitude = 36.2088,
            dumpLongitude = -115.0262
          },
          new LoadDumpLocation
          {
            loadLatitude = 36.2082,
            loadLongitude = -115.0179,
            dumpLatitude = 36.2088,
            dumpLongitude = -115.0177
          },
          new LoadDumpLocation
          {
            loadLatitude = 36.2092,
            loadLongitude = -115.0258,
            dumpLatitude = 36.2098,
            dumpLongitude = -115.0262
          },
          new LoadDumpLocation
          {
            loadLatitude = 36.2092,
            loadLongitude = -115.0179,
            dumpLatitude = 36.2098,
            dumpLongitude = -115.0177
          }
        };
      }
    }

    [Route("api/v1/mock/loaddump")]
    [HttpGet]
    public LoadDumpResult GetMockLoadDumpLocations([FromQuery] Guid projectUid)
    {
      Console.WriteLine($"GetMockLoadDumpLocations: projectUid={projectUid}");

      if (projectUid.ToString() == ConstantsUtil.DIMENSIONS_PROJECT_UID)
        return new LoadDumpResult {cycles = LoadDumpData };

      return new LoadDumpResult();
    }
  }
}
