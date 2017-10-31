using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;

namespace VSS.Productivity3D.WebApiModels.Compaction.Helpers
{
  public interface IProductionDataProfileRequestHelper
  {
    CompactionProfileProductionDataRequest CreateProductionDataProfileRequest(double startLatDegrees, double startLonDegrees, double endLatDegrees, double endLonDegrees);
  }
}