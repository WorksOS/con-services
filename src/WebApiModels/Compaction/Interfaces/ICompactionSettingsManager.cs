using System;
using System.Collections.Generic;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApiModels.Report.Models;
using VSS.Productivity3D.WebApiModels.Report.ResultHandling;

namespace VSS.Productivity3D.WebApiModels.Compaction.Interfaces
{
  public interface ICompactionSettingsManager
  {
    LiftBuildSettings CompactionLiftBuildSettings(CompactionProjectSettings projectSettings);

    Filter CompactionFilter(DateTime? startUtc, DateTime? endUtc, long? onMachineDesignId, bool? vibeStateOn,
      ElevationType? elevationType,
      int? layerNumber, List<MachineDetails> machines, List<long> excludedSurveyedSurfaceIds);

    CMVSettings CompactionCmvSettings(CompactionProjectSettings projectSettings);

    MDPSettings CompactionMdpSettings(CompactionProjectSettings projectSettings);
    TemperatureSettings CompactionTemperatureSettings(CompactionProjectSettings projectSettings);

    double[] CompactionCmvPercentChangeSettings(CompactionProjectSettings projectSettings);

    PassCountSettings CompactionPassCountSettings(CompactionProjectSettings projectSettings);

    List<ColorPalette> CompactionPalette(DisplayMode mode, ElevationStatisticsResult elevExtents,
      CompactionProjectSettings projectSettings);

  }
}
