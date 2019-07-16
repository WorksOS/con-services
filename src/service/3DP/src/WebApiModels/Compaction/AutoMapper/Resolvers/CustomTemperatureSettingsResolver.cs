using AutoMapper;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.AutoMapper
{
  public partial class AutoMapperUtility
  {
    public class CustomTemperatureSettingsResolver : IValueResolver<LiftBuildSettings, OverridingTargets, TemperatureSettings>
    {
      public TemperatureSettings Resolve(LiftBuildSettings src, OverridingTargets dst, TemperatureSettings member, ResolutionContext context)
      {
        var min = 0.0;
        var max = 0.0;
        var overrideTemp = src.OverridingTemperatureWarningLevels != null;
        if (overrideTemp)
        {
          min = src.OverridingTemperatureWarningLevels.Min / 10.0;
          max = src.OverridingTemperatureWarningLevels.Max / 10.0;
        }

        return new TemperatureSettings(max, min, overrideTemp);
      }
    }
  }
}
