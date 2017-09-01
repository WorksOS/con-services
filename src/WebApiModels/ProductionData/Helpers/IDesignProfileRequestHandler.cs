using System;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Helpers
{
  public interface IDesignProfileRequestHandler
  {
    DesignProfileRequestHelper SetRaptorClient(IASNodeClient raptorClient);

    CompactionProfileDesignRequest CreateDesignProfileRequest(Guid projectUid, double latRadians1, double lngRadians1, double latRadians2, double lngRadians2, Guid customerUid, Guid importedFileUid);
  }
}