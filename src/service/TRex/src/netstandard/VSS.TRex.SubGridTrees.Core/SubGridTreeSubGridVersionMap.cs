using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.SubGridTrees.Core
{
  public class SubGridTreeSubGridVersionMap : GenericSubGridTree_Long
  {
    public SubGridTreeSubGridVersionMap() : base (SubGridTreeConsts.SubGridTreeLevels, 0)
    {
    }

    public override string SerialisedHeaderName() => "SubGridVersionMap";

    public override int SerialisedVersion() => 1;
  }
}
