using System;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using MockProjectWebApi.Services;
using VSS.MasterData.Models.Models;

namespace MockProjectWebApi.Controllers
{
  public class MockFilterController
  {
    private readonly FiltersService filtersService;

    public MockFilterController(IFiltersService filtersService)
    {
      this.filtersService = (FiltersService) filtersService;
    }

    /// <summary>
    /// Get a filter for a project by filter id.
    /// </summary>
    [HttpGet("api/v1/mock/filter/{projectUid}")]
    public FilterData GetMockFilter(string projectUid, [FromUri] string filterUid)
    {
      Console.WriteLine("GetMockFilter: projectUid={0}, filterUid={1}", projectUid, filterUid);

      return filtersService.GetFilter(projectUid, filterUid);
    }

    /// <summary>
    /// Gets the filters for a given project.
    /// </summary>
    [HttpGet("api/v1/mock/filters/{projectUid}")]
    public FilterListData GetMockFilters(string projectUid)
    {
      Console.WriteLine($"{nameof(GetMockFilters)}: projectUid={projectUid}");

      return filtersService.GetFilters(projectUid);
    }
  }
}
