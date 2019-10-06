using AutoMapper;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Productivity3D.Models.Compaction;

namespace VSS.Productivity3D.WebApi.Models.Compaction.AutoMapper
{
  public partial class AutoMapperUtility
  {
    public class CustomCCVRangePercentageResolver : IValueResolver<CompactionProjectSettings, LiftBuildSettings, CCVRangePercentage>
    {
      public CCVRangePercentage Resolve(CompactionProjectSettings src, LiftBuildSettings dst, CCVRangePercentage member, ResolutionContext context)
      {
        return src.OverrideDefaultTargetRangeCmvPercent ? 
          new CCVRangePercentage(src.CustomTargetCmvPercentMinimum, src.CustomTargetCmvPercentMaximum) :
          new CCVRangePercentage(CompactionProjectSettings.DefaultSettings.CustomTargetCmvPercentMinimum, CompactionProjectSettings.DefaultSettings.CustomTargetCmvPercentMaximum);
      }
    }
  }
}
