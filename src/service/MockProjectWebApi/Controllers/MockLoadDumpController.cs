using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MockProjectWebApi.Utils;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;

namespace MockProjectWebApi.Controllers
{
  public class MockLoadDumpController : BaseController
  {
    private static List<LoadDumpLocation> LoadDumpData;

    public MockLoadDumpController(ILoggerFactory loggerFactory) : base(loggerFactory)
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
    [Route("api/v1")]
    [HttpGet]
    public LoadDumpResult GetMockLoadDumpLocations([FromQuery] Guid projectUid)
    {
      Logger.LogInformation($"GetMockLoadDumpLocations: projectUid={projectUid}");

      return projectUid.ToString() == ConstantsUtil.DIMENSIONS_PROJECT_UID
        ? new LoadDumpResult { cycles = LoadDumpData }
        : new LoadDumpResult();
    }
  }
}
