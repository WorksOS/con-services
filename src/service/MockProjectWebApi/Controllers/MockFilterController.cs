using Mvc = Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MockProjectWebApi.Services;
using VSS.Productivity3D.Filter.Abstractions.Models;

namespace MockProjectWebApi.Controllers
{
  public class MockFilterController : BaseController
  {
    private readonly FiltersService filtersService;

    public MockFilterController(ILoggerFactory loggerFactory, IFiltersService filtersService) : base(loggerFactory)
    {
      this.filtersService = (FiltersService)filtersService;
    }

    /// <summary>
    /// Get a filter for a project by filter id.
    /// </summary>
    [Mvc.Route("api/v1/filter/{projectUid}")]
    [Mvc.HttpGet]
    public FilterData GetMockFilter(string projectUid, [Mvc.FromQuery] string filterUid)
    {
      Logger.LogInformation($"{nameof(GetMockFilter)}: projectUid={projectUid}, filterUid={filterUid}");

      return filtersService.GetFilter(projectUid, filterUid);
    }

    /// <summary>
    /// Gets the filters for a given project.
    /// </summary>
    [Mvc.Route("api/v1/filters/{projectUid}")]
    [Mvc.HttpGet]
    public FilterListData GetMockFilters(string projectUid)
    {
      Logger.LogInformation($"{nameof(GetMockFilters)}: projectUid={projectUid}");

      return filtersService.GetFilters(projectUid);
    }
  }
}
