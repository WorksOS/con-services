using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Helpers
{
  public interface ICompactionReportStationOffsetRequestHelper
  {
    CompactionReportStationOffsetRequest CreateRequest(bool reportElevation, bool reportCmv, bool reportMdp, bool reportPassCount, bool reportTemperature, bool reportCutFill, DesignDescriptor cutFillDesignDescriptor, DesignDescriptor alignmentDescriptor, double crossSectionInterval, double startStation, double endStation, double[] offsets, UserPreferenceData userPreferences, string projectTimezone);
  }
}
