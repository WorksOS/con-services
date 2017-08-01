using System;
using VSS.Productivity3D.WebApiModels.ProductionData.Models;

namespace VSS.Productivity3D.WebApiModels.ProductionData.Helpers
{
  public interface IProfileSliceRequestHandler
  {
    ProfileProductionDataRequest CreateSlicerProfileResponse(Guid projectUid, double startLatDegrees, double startLonDegrees, double endLatDegrees, double endLonDegrees, DateTime? startUtc, DateTime? endUtc, Guid? cutfillDesignUid);
  }
}