using Microsoft.AspNetCore.Mvc;
using MockProjectWebApi.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using VSS.MasterData.Models.Models;

namespace MockProjectWebApi.Controllers
{
  public class MockFilterController : Controller
  {
    /// <summary>
    /// Get a filter used in the 3D Productivity service acceptance tests.
    /// The data is mocked.
    /// </summary>
    [Route("api/v1/mock/filter/{projectUid}")]
    [HttpGet]
    public FilterData GetMockFilter(string projectUid, [FromUri] string filterUid)
    {
      Console.WriteLine("GetMockFilter: projectUid={0}, filterUid={1}", projectUid, filterUid);

      return GetFilter(projectUid, filterUid);
    }

    /// <summary>
    /// Gets the list of filters used in the 3D Productivity service acceptance tests.
    /// The data is mocked.
    /// </summary>
    [Route("api/v1/mock/filters/{projectUid}")]
    [HttpGet]
    public FilterListData GetMockFilters(string projectUid)
    {
      Console.WriteLine("GetMockFilters: projectUid={0}", projectUid);

      return GetFilters(projectUid);
    }

    private FilterData GetFilter(string projectUid, string filterUid)
    {
      var filters = GetFilters(projectUid);
      var filter = filters.filterDescriptors.SingleOrDefault(f => f.FilterUid == filterUid);
      if (filter == null)
      {
        return new FilterData
        {
          Code = 36,
          Message =
            "GetFilter By filterUid. The requested filter does exist, or does not belong to the requesting customer; project or user."
        };
      }
      return new FilterData {filterDescriptor = filter};
    }

    private FilterListData GetFilters(string projectUid)
    {
      if (projectUid == ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1)
      {
        return new FilterListData {filterDescriptors = _filterList};
      }

      if (projectUid == ConstantsUtil.DIMENSIONS_PROJECT_UID)
      {
        return new FilterListData
        {
          filterDescriptors = new List<FilterDescriptor>
          {
            new FilterDescriptor
            {
              FilterUid = "200c7b47-b5e6-48ee-a731-7df6623412da",
              Name = "Elevation Range and Palette No Data Filter",
              FilterJson =
                "{\"startUTC\":\"2017-01-01\",\"endUTC\":\"2017-01-01\",\"designUid\":null,\"contributingMachines\":null,\"onMachineDesignID\":null,\"elevationType\":null,\"vibeStateOn\":null,\"polygonLL\":null,\"forwardDirection\":null,\"layerNumber\":null,\"layerType\":null}"
            }
          }
        };
      }

      return new FilterListData
      {
        filterDescriptors = new List<FilterDescriptor>()
      };
    }

    private readonly List<FilterDescriptor> _filterList = new List<FilterDescriptor>
    {
      new FilterDescriptor
      {
        FilterUid = "3d9086f2-3c04-4d92-9141-5134932b1523",
        Name = "Filter 1",
        FilterJson =
          "{\"startUTC\": null,\"endUTC\": null,\"designUid\": \"220e12e5-ce92-4645-8f01-1942a2d5a57f\",\"contributingMachines\": null,\"onMachineDesignID\": null,\"elevationType\": null,\"vibeStateOn\": null,\"polygonName\": null,\"polygonUid\": null,\"polygonLL\": null,\"forwardDirection\": null,\"layerNumber\": null,\"layerType\": null}"
      },
      new FilterDescriptor
      {
        FilterUid = "81422acc-9b0c-401c-9987-0aedbf153f1d",
        Name = "Filter 2",
        FilterJson =
          "{\"startUTC\":null,\"endUTC\":null,\"designUid\":\"dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff\",\"contributingMachines\":null,\"onMachineDesignID\":null,\"elevationType\":null,\"vibeStateOn\":null,\"polygonName\": null,\"polygonUid\": \"94dc0ec6-32ef-4b54-a7f8-e8e3eee65642\",\"polygonLL\":null,\"forwardDirection\":null,\"layerNumber\":null,\"layerType\":null}"
      },
      new FilterDescriptor
      {
        FilterUid = "1cf81668-1739-42d5-b068-ea025588796a",
        Name = "Filter 3",
        FilterJson =
          "{\"startUTC\":null,\"endUTC\":null,\"designUid\":\"ea97efb9-c0c4-4a7f-9eee-e2b0ef0b0916\",\"contributingMachines\":null,\"onMachineDesignID\":null,\"elevationType\":null,\"vibeStateOn\":null,\"polygonLL\":null,\"forwardDirection\":null,\"layerNumber\":null,\"layerType\":null}"
      },
      new FilterDescriptor
      {
        FilterUid = "d15e65e0-3cb1-476f-8fc6-08507a14a269",
        Name = "Filter 4",
        FilterJson =
          "{\"startUTC\":\"2012-11-05\",\"endUTC\":\"2012-11-06\",\"designUid\":null,\"contributingMachines\":null,\"onMachineDesignID\":null,\"elevationType\":null,\"vibeStateOn\":null,\"polygonLL\":null,\"forwardDirection\":null,\"layerNumber\":null,\"layerType\":null}"
      },
      new FilterDescriptor
      {
        FilterUid = "d7cb424d-b012-4618-b3bc-e526ca84bbd6",
        Name = "Filter 5",
        FilterJson =
          "{\"startUTC\":\"2017-11-05\",\"endUTC\":\"2017-11-06\",\"designUid\":null,\"contributingMachines\":null,\"onMachineDesignID\":null,\"elevationType\":null,\"vibeStateOn\":null,\"polygonLL\":null,\"forwardDirection\":null,\"layerNumber\":null,\"layerType\":null}"
      }  
    };
  }
}