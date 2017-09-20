using Microsoft.AspNetCore.Mvc;
using MockProjectWebApi.Utils;
using System;
using System.Collections.Generic;
using System.Web.Http;
using VSS.MasterData.Models.Models;

namespace MockProjectWebApi.Controllers
{
  public class MockFilterController : Controller
  {
    /// <summary>
    /// Gets the list of filters used in the 3D Productivity service acceptance tests.
    /// The data is mocked.
    /// </summary>
    [Route("api/v4/mockfilter/{projectUid}")]
    [HttpGet]
    public FilterData GetMockFilter(string projectUid, [FromUri] string filterUid)
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
        filterDescriptor = new FilterDescriptor
        {
          FilterUid = "3d9086f2-3c04-4d92-9141-5134932b1523",
          Name = "Filter 1",
          FilterJson = "{\"startUTC\": null,\"endUTC\": null,\"designUid\": \"220e12e5-ce92-4645-8f01-1942a2d5a57f\",\"contributingMachines\": null,\"onMachineDesignID\": null,\"elevationType\": null,\"vibeStateOn\": null,\"polygonName\": null,\"polygonUid\": null,\"polygonLL\": null,\"forwardDirection\": null,\"layerNumber\": null,\"layerType\": null}"
        }
      },
      new FilterData
      {
        filterDescriptor = new FilterDescriptor
        {
          FilterUid = "81422acc-9b0c-401c-9987-0aedbf153f1d",
          Name = "Filter 2",
          FilterJson = "{\"startUTC\":null,\"endUTC\":null,\"designUid\":\"dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff\",\"contributingMachines\":null,\"onMachineDesignID\":null,\"elevationType\":null,\"vibeStateOn\":null,\"polygonName\": null,\"polygonUid\": \"94dc0ec6-32ef-4b54-a7f8-e8e3eee65642\",\"polygonLL\": \"POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))\",\"forwardDirection\":null,\"layerNumber\":null,\"layerType\":null}"
        }
      },
      new FilterData
      {
        filterDescriptor = new FilterDescriptor
        {
          FilterUid = "1cf81668-1739-42d5-b068-ea025588796a",
          Name = "Filter 3",
          FilterJson = "{\"startUTC\":null,\"endUTC\":null,\"designUid\":\"ea97efb9-c0c4-4a7f-9eee-e2b0ef0b0916\",\"contributingMachines\":null,\"onMachineDesignID\":null,\"elevationType\":null,\"vibeStateOn\":null,\"polygonLL\":null,\"forwardDirection\":null,\"layerNumber\":null,\"layerType\":null}"
        }
      },
      new FilterData
      {
        filterDescriptor = new FilterDescriptor
        {
          FilterUid = "d15e65e0-3cb1-476f-8fc6-08507a14a269",
          Name = "Filter 4",
          FilterJson = "{\"startUTC\":\"2012-11-05\",\"endUTC\":\"2012-11-06\",\"designUid\":null,\"contributingMachines\":null,\"onMachineDesignID\":null,\"elevationType\":null,\"vibeStateOn\":null,\"polygonLL\":null,\"forwardDirection\":null,\"layerNumber\":null,\"layerType\":null}"
        }
      },
      new FilterData
      {
        filterDescriptor = new FilterDescriptor
        {
          FilterUid = "d7cb424d-b012-4618-b3bc-e526ca84bbd6",
          Name = "Filter 5",
          FilterJson = "{\"startUTC\":\"2017-11-05\",\"endUTC\":\"2017-11-06\",\"designUid\":null,\"contributingMachines\":null,\"onMachineDesignID\":null,\"elevationType\":null,\"vibeStateOn\":null,\"polygonLL\":null,\"forwardDirection\":null,\"layerNumber\":null,\"layerType\":null}"
        }
      }
    };
  }
}