using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;
using System.Collections.Generic;

namespace VSS.Productivity3D.WebApiTests.Compaction.Filters
{
  public class ActionFilterAttributeTestsBase
  {
    protected ActionExecutingContext CreateActionExecutingContext(HttpContext httpContext)
    {
      return new ActionExecutingContext(
        new ActionContext
        {
          HttpContext = httpContext,
          RouteData = new RouteData(),
          ActionDescriptor = new ActionDescriptor(),
        },
        new List<IFilterMetadata>(),
        new Dictionary<string, object>(),
        new Mock<Controller>().Object);
    }
  }
}