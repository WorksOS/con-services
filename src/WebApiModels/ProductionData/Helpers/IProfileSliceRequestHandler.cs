using System;
using System.Collections.Generic;
using VSS.Productivity3D.WebApiModels.ProductionData.Models;

namespace VSS.Productivity3D.WebApiModels.ProductionData.Helpers
{
  public interface IProfileSliceRequestHandler
  {
    ProfileProductionDataRequest CreateSlicerProfileResponse(Guid projectUid,
      double startLatDegrees, double startLonDegrees, double endLatDegrees, double endLonDegrees,
      Guid filterUid, Guid customerUid, IDictionary<string, string> headers, Guid? cutfillDesignUid);
  }
}