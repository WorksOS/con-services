using System;
using System.Collections.Generic;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;
using Filter = VSS.Productivity3D.Common.Models.Filter;

namespace VSS.Productivity3D.Common.Interfaces
{
  public interface ICompactionSettingsManager
  {
    LiftBuildSettings CompactionLiftBuildSettings(CompactionProjectSettings projectSettings);

    Filter CompactionFilter(DateTime? startUtc, DateTime? endUtc, long? onMachineDesignId, bool? vibeStateOn,
      ElevationType? elevationType,
      int? layerNumber, List<MachineDetails> machines, List<long> excludedSurveyedSurfaceIds, DesignDescriptor designDescriptor = null);

    Filter CompactionFilter(string filterUid, string projectUid,
      IDictionary<string, string> headers);

    CMVSettings CompactionCmvSettings(CompactionProjectSettings projectSettings);

    MDPSettings CompactionMdpSettings(CompactionProjectSettings projectSettings);
    TemperatureSettings CompactionTemperatureSettings(CompactionProjectSettings projectSettings, bool nativeValues = true);

    double[] CompactionCmvPercentChangeSettings(CompactionProjectSettings projectSettings);

    PassCountSettings CompactionPassCountSettings(CompactionProjectSettings projectSettings);

    List<ColorPalette> CompactionPalette(DisplayMode mode, ElevationStatisticsResult elevExtents,
      CompactionProjectSettings projectSettings);
  }
}