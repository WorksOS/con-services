using System;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Helpers
{
  public interface IDesignProfileRequestHandler
  {
    CompactionProfileDesignRequest CreateDesignProfileRequest(double latRadians1, double lngRadians1, double latRadians2, double lngRadians2);
  }
}