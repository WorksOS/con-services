using Newtonsoft.Json;
using System;
using System.Net;
using VSS.MasterData.Models.Handlers;

namespace VSS.Productivity3D.Filter.Common.Models
{
  public class ProjectGeofenceRequest
  {
    /// <summary>
    /// The BoundaryUid whose boundary is to be updated, empty for create.
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public string BoundaryUid { get; set; } = string.Empty;

    /// <summary>
    /// The FilterUid this boundary is attached to.
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public string ProjectUid { get; set; } = string.Empty;

    protected ProjectGeofenceRequest()
    { }

    /// <summary>
    /// Returns a new instance of <see cref="BoundaryRequest"/> using the provided inputs.
    /// </summary>
    public static ProjectGeofenceRequest Create(string projectUid, string boundaryUid)
    {
      return new ProjectGeofenceRequest
      {
        BoundaryUid = boundaryUid,
        ProjectUid = projectUid
      };
    }

    public void Validate(IServiceExceptionHandler serviceExceptionHandler)
    {
      if (string.IsNullOrEmpty(ProjectUid) || Guid.TryParse(ProjectUid, out Guid projectUidGuid) == false)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 1);
      }

      if (string.IsNullOrEmpty(BoundaryUid) || Guid.TryParse(BoundaryUid, out Guid boundaryUid) == false)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 5);
      }
    }
  }
}