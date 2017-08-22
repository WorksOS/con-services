using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Interfaces
{
  public interface ICompactionProfileResultHelper
  {
    void FindCutFillElevations(CompactionProfileResult<CompactionProfileCell> slicerProfileResult,
      CompactionProfileResult<CompactionProfileVertex> slicerDesignResult);
  }
}
