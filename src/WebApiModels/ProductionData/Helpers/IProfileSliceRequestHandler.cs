using System;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Helpers
{
  public interface IProfileSliceRequestHandler
  {
    ProfileProductionDataRequest CreateSlicerProfileRequest(Guid projectUid,
      double startLatDegrees, double startLonDegrees, double endLatDegrees, double endLonDegrees,
      Guid filterUid, Guid customerUid, Guid? cutfillDesignUid);
  }
}