using System;
using System.Collections.Generic;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Helpers
{
  public interface IProfileSliceRequestHandler
  {
    CompactionProfileProductionDataRequest CreateSlicerProfileRequest(Guid projectUid,
      double startLatDegrees, double startLonDegrees, double endLatDegrees, double endLonDegrees,
      Guid? filterUid, Guid customerUid, IDictionary<string, string> headers, Guid? cutfillDesignUid);
  }
}