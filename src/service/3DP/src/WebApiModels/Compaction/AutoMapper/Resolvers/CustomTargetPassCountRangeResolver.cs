using AutoMapper;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Productivity3D.Models.Compaction;

namespace VSS.Productivity3D.WebApi.Models.Compaction.AutoMapper
{
  public partial class AutoMapperUtility
  {
    public class CustomTargetPassCountRangeResolver : IValueResolver<CompactionProjectSettings, LiftBuildSettings, TargetPassCountRange>
    {
      public TargetPassCountRange Resolve(CompactionProjectSettings src, LiftBuildSettings dst, TargetPassCountRange member, ResolutionContext context)
      {
        return src.OverrideMachineTargetPassCount
          ? new TargetPassCountRange((ushort) src.customTargetPassCountMinimum, (ushort) src.customTargetPassCountMaximum)
          : null;
      }
    }
  }
}
