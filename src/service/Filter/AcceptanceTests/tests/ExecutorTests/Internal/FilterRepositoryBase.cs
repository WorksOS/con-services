using System;
using System.Collections.Generic;
using VSS.Productivity3D.Filter.Abstractions.Models;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace ExecutorTests.Internal
{
  public class FilterRepositoryBase : TestControllerBase
  {
    public void Setup()
    {
      SetupDI();     
    }

    protected FilterRequestFull CreateAndValidateRequest(
      bool isApplicationContext = false,
      string userId = null,
      string projectUid = null,
      string filterUid = null,
      string name = null,
      string filterJson = "",
      string boundaryUid = null,
      string customerUid = null,
      FilterType filterType = FilterType.Transient,
      bool onlyFilterUid = false)
    {
      var request = FilterRequestFull.Create(
        new Dictionary<string, string>(),
        customerUid ?? Guid.NewGuid().ToString(),
        isApplicationContext,
        userId ?? Guid.NewGuid().ToString(),
        new ProjectData { ProjectUID = projectUid ?? Guid.NewGuid().ToString() },
        new FilterRequest
        {
          FilterUid = filterUid ?? Guid.NewGuid().ToString(),
          Name = name,
          FilterJson = filterJson,
          FilterType = filterType
        });

      request.Validate(ServiceExceptionHandler);

      return request;
    }



    protected FilterRequestFull CreateAndValidateRequest(
      ProjectData projectData,
      bool isApplicationContext = false,
      string userId = null,
      string filterUid = null,
      string name = null,
      string filterJson = "",
      string boundaryUid = null,
      string customerUid = null,
      FilterType filterType = FilterType.Transient,
      bool onlyFilterUid = false)
    {
      var request = FilterRequestFull.Create(
        new Dictionary<string, string>(),
        customerUid ?? Guid.NewGuid().ToString(),
        isApplicationContext,
        userId ?? Guid.NewGuid().ToString(),
        projectData ?? new ProjectData() { ProjectUID = Guid.NewGuid().ToString() },
        new FilterRequest
        {
          FilterUid = filterUid ?? Guid.NewGuid().ToString(),
          Name = name,
          FilterType = filterType,
          FilterJson = filterJson
        });

      request.Validate(ServiceExceptionHandler, onlyFilterUid);

      return request;
    }
  }
}
