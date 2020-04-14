using System.Net;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Utilities;

namespace VSS.MasterData.Project.WebAPI.Common.Helpers
{
  /// <summary>
  ///
  /// </summary>
  public partial class ProjectRequestHelper
  {
    public static void ValidateProjectBoundary(string projectBoundary, IServiceExceptionHandler serviceExceptionHandler)
    {
      var result = GeofenceValidation.ValidateWKT(projectBoundary);
      if (string.CompareOrdinal(result, GeofenceValidation.ValidationOk) != 0)
      {
        if (string.CompareOrdinal(result, GeofenceValidation.ValidationNoBoundary) == 0)
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 23);
        }

        if (string.CompareOrdinal(result, GeofenceValidation.ValidationLessThan3Points) == 0)
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 24);
        }

        if (string.CompareOrdinal(result, GeofenceValidation.ValidationInvalidFormat) == 0)
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 25);
        }

        if (string.CompareOrdinal(result, GeofenceValidation.ValidationInvalidPointValue) == 0)
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 111);
        }
      }
    }
  }
}
