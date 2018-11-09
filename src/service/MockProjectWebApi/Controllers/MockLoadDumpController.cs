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
            loadLatitude = 36.206,
            loadLongitude = -115.0230,
            dumpLatitude = 36.208,
            dumpLongitude = -115.0200
          },
          new LoadDumpLocation
          {
          loadLatitude = 36.206,
          loadLongitude = -115.0235,
          dumpLatitude = 36.208,
          dumpLongitude = -115.0205
        },
          new LoadDumpLocation
          {
            loadLatitude = 36.206,
            loadLongitude = -115.0220,
            dumpLatitude = 36.208,
            dumpLongitude = -115.0190
          },
          new LoadDumpLocation
          {
            loadLatitude = 36.206,
            loadLongitude = -115.0225,
            dumpLatitude = 36.208,
            dumpLongitude = -115.0195
          },
          new LoadDumpLocation
          {
            loadLatitude = 36.207,
            loadLongitude = -115.0230,
            dumpLatitude = 36.209,
            dumpLongitude = -115.0200
          },
          new LoadDumpLocation
          {
            loadLatitude = 36.207,
            loadLongitude = -115.0235,
            dumpLatitude = 36.209,
            dumpLongitude = -115.0205
          },
          new LoadDumpLocation
          {
            loadLatitude = 36.207,
            loadLongitude = -115.0220,
            dumpLatitude = 36.209,
            dumpLongitude = -115.0190
          },
          new LoadDumpLocation
          {
            loadLatitude = 36.207,
            loadLongitude = -115.0225,
            dumpLatitude = 36.209,
            dumpLongitude = -115.0195
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
