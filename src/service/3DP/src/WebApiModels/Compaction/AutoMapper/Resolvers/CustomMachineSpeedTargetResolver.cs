using AutoMapper;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.AutoMapper
{
  public partial class AutoMapperUtility
  {
    public class CustomMachineSpeedTargetResolver : IValueResolver<CompactionProjectSettings, LiftBuildSettings, MachineSpeedTarget>
    {
      public MachineSpeedTarget Resolve(CompactionProjectSettings src, LiftBuildSettings dst, MachineSpeedTarget member, ResolutionContext context)
      {
        return new MachineSpeedTarget(src.CustomTargetSpeedMinimum, src.CustomTargetSpeedMaximum);
      }
    }
  }
}
