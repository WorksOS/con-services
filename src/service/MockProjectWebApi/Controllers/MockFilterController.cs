using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
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
    [Route("api/v1/mock/filter/{projectUid}")]
    [Route("api/v1/filter/{projectUid}")]
    [HttpGet]
    public FilterData GetMockFilter(string projectUid, [FromUri] string filterUid)
    {
      Logger.LogInformation($"{nameof(GetMockFilter)}: projectUid={projectUid}, filterUid={filterUid}");

      return filtersService.GetFilter(projectUid, filterUid);
    }

    /// <summary>
    /// Gets the filters for a given project.
    /// </summary>
    [Route("api/v1/mock/filters/{projectUid}")]
    [Route("api/v1/filters/{projectUid}")]
    [HttpGet]
    public FilterListData GetMockFilters(string projectUid)
    {
      Logger.LogInformation($"{nameof(GetMockFilters)}: projectUid={projectUid}");

      return filtersService.GetFilters(projectUid);
    }
  }
}
