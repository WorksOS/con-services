using System.Net;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Repositories.DBModels;
using VSS.Productivity3D.Project.Abstractions.Models;

namespace VSS.Productivity3D.Filter.Common.Models
{
  /// <summary>
  /// All the parameters reequired for creating or updating a boundary.
  /// </summary>
  public class BoundaryRequestFull : BaseRequestFull
  {
    public GeofenceType GeofenceType => GeofenceType.Filter;
    public BoundaryRequest Request { get; set; }
    
    /// <summary>
    /// Returns a new instance of <see cref="BoundaryRequestFull"/> using the provided inputs.
    /// </summary>
    public static BoundaryRequestFull Create(
      string customerUid,
      bool isApplicationContext,
      ProjectData projectData,
      string userUid,
      BoundaryRequest request)
    {
      return new BoundaryRequestFull
      {
        IsApplicationContext = isApplicationContext,
        ProjectUid = projectData?.ProjectUID,
        CustomerUid = customerUid,
        UserUid = userUid,
        Request = request
      };
    }

    public override void Validate(IServiceExceptionHandler serviceExceptionHandler)
    {
      base.Validate(serviceExceptionHandler);
      if (Request == null)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 60);
      }
      Request.Validate(serviceExceptionHandler);
    }
  }
}
