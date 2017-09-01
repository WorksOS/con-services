using System;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Helpers
{
  public interface IProductionDataProfileRequestHelper
  {
    CompactionProfileProductionDataRequest CreateProductionDataProfileRequest(Guid projectUid,
      double startLatDegrees, double startLonDegrees, double endLatDegrees, double endLonDegrees, Guid customerUid, Guid? cutfillDesignUid);
  }
}