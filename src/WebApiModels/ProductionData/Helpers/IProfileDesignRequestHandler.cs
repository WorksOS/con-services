using System;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Helpers
{
  public interface IProfileDesignRequestHandler
  {
    ProfileProductionDataRequest CreateDesignProfileResponse(Guid projectUid, double latRadians1, double lngRadians1, double latRadians2, double lngRadians2, Guid customerUid, Guid importedFileUid, int importedFileTypeid, Guid filterUid, Guid? cutfillDesignUid);
  }
}