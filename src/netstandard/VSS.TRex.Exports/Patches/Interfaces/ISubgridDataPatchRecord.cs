using VSS.TRex.SubGridTrees.Client.Interfaces;

namespace VSS.TRex.Exports.Patches.Interfaces
{
  public interface ISubgridDataPatchRecord
  {
    void Populate(IClientLeafSubGrid subGrid);
  }
}
