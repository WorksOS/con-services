using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MockProjectWebApi.Utils;
using VSS.MasterData.Models.Local.ResultHandling;
using VSS.MasterData.Models.Models;

namespace MockProjectWebApi.Controllers
{
  public class MockFilterController : Controller
  {
    /// <summary>
    /// Gets the list of filters used in the 3D Productivity service acceptance tests.
    /// The data is mocked.
    /// </summary>
    [Route("api/v4/mockfilter/{projectUid}/{filterUid}")]
    [HttpGet]
    public FilterData GetMockFilter(string projectUid, string filterUid)
    {
      Console.WriteLine("GetMockFilter: projectUid={0}, filterUid={1}", projectUid, filterUid);

      return GetFilter(projectUid, filterUid);
    }

    private FilterData GetFilter(string projectUid, string filterUid)
    {
      if (projectUid != ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1)
        return null;

      foreach (var filter in _filterList)
      {
        if (filterUid != null && filter.filterDescriptor.FilterUid == filterUid)
          return filter;
      }

      return null;
    }

    private readonly List<FilterData> _filterList = new List<FilterData>
    {
      new FilterData
      {
        filterDescriptor = new FilterDescriptor()
        {
          FilterUid = "3d9086f2-3c04-4d92-9141-5134932b1523",
          Name = "Filter 1",
          FilterJson = "{\"startUTC\":null,\"endUTC\":null,\"designUid\":\"220e12e5-ce92-4645-8f01-1942a2d5a57f\",\"contributingMachines\":null,\"onMachineDesignID\":null,\"elevationType\":null,\"vibeStateOn\":null,\"polygonLL\":null,\"forwardDirection\":null,\"layerNumber\":null,\"layerType\":null}"
        }
      },
      new FilterData
      {
      filterDescriptor = new FilterDescriptor()
      {
      FilterUid = "81422acc-9b0c-401c-9987-0aedbf153f1d",
      Name = "Filter 1",
      FilterJson = "{\"startUTC\":null,\"endUTC\":null,\"designUid\":\"dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff\",\"contributingMachines\":null,\"onMachineDesignID\":null,\"elevationType\":null,\"vibeStateOn\":null,\"polygonLL\":null,\"forwardDirection\":null,\"layerNumber\":null,\"layerType\":null}"
    }
  }
    };
  }
}
