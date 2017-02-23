using System;
using Newtonsoft.Json.Linq;
using VSS.Raptor.Service.Common.Filters.Authentication.Models;
using VSS.Raptor.Service.Common.Models;

namespace VSS.Raptor.Service.Common.JsonConverters
{
  public class ProjectIDConverter : JsonCreationConverter<ProjectID>
  {
    private readonly IAuthenticatedProjectsStore authProjectsStore;
    public ProjectIDConverter(IAuthenticatedProjectsStore authProjectsStore)
    {
      this.authProjectsStore = authProjectsStore;
    }
    protected override ProjectID Create(Type objectType, JObject jObject)
    {
      var projectID = jObject.ToObject<ProjectID>();
      if (!projectID.projectId.HasValue)
        projectID.projectId = ProjectID.GetProjectId(projectID.projectUid, authProjectsStore);
      return projectID;
    }
  }
}

