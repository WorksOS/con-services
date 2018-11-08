using VSS.Productivity3D.WebApi.Models.Compaction.Models;

namespace VSS.Productivity3D.WebApiModels.Compaction.Helpers
{
  public interface IDesignProfileRequestHandler
  {
    CompactionProfileDesignRequest CreateDesignProfileRequest(double latRadians1, double lngRadians1, double latRadians2, double lngRadians2);
  }
}