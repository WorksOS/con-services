using System;
using System.Net;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;

namespace VSS.Productivity3D.Filter.Common.Models
{
  /// <summary>
  /// All the parameters required for getting or deleting a boundary.
  /// </summary>
  public class BoundaryUidRequestFull : BaseRequestFull
  {
    /// <summary>
    /// The UID of the boundary to get or delete
    /// </summary>
    public string BoundaryUid { get; set; }
 
    /// <summary>
    /// Returns a new instance of <see cref="BoundaryUidRequestFull"/> using the provided inputs.
    /// </summary>
    public static BoundaryUidRequestFull Create(
      string customerUid,
      bool isApplicationContext,
      ProjectData projectData,
      string userUid,
      string boundaryUid)
    {
      return new BoundaryUidRequestFull
      {
        BoundaryUid = boundaryUid,
        IsApplicationContext = isApplicationContext,
        ProjectUid = projectData?.ProjectUid,
        CustomerUid = customerUid,
        UserUid = userUid
      };
    }

    public override void Validate(IServiceExceptionHandler serviceExceptionHandler)
    {
      base.Validate(serviceExceptionHandler);

      if (string.IsNullOrEmpty(BoundaryUid) || Guid.TryParse(BoundaryUid, out Guid boundaryUidGuid) == false)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 59);
      }
    }
  }
}
