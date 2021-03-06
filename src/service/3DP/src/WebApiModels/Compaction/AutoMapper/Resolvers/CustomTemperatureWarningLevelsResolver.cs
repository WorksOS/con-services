﻿using AutoMapper;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;

namespace VSS.Productivity3D.WebApi.Models.Compaction.AutoMapper
{
  public partial class AutoMapperUtility
  {
    public class CustomTemperatureWarningLevelsResolver : IValueResolver<CompactionProjectSettings, LiftBuildSettings, TemperatureWarningLevels>
    {
      public TemperatureWarningLevels Resolve(CompactionProjectSettings src, LiftBuildSettings dst, TemperatureWarningLevels member, ResolutionContext context)
      {
        return src.OverrideMachineTargetTemperature
          ? new TemperatureWarningLevels(src.CustomTargetTemperatureWarningLevelMinimum, src.CustomTargetTemperatureWarningLevelMaximum) : null;
      }
    }
  }
}
