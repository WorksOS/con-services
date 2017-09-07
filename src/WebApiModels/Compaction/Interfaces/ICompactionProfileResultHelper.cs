using System;
using System.Collections.Generic;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Interfaces
{
  public interface ICompactionProfileResultHelper
  {
    void FindCutFillElevations(CompactionProfileResult<CompactionProfileCell> slicerProfileResult,
      CompactionProfileResult<CompactionProfileVertex> slicerDesignResult);

    CompactionProfileResult<CompactionProfileDataResult> ConvertProfileResult(
      CompactionProfileResult<CompactionProfileCell> slicerProfileResult);

    CompactionProfileResult<CompactionDesignProfileResult> ConvertProfileResult(
      Dictionary<Guid, CompactionProfileResult<CompactionProfileVertex>> slicerProfileResults);

    void RemoveRepeatedNoData(CompactionProfileResult<CompactionProfileDataResult> result);

    void AddSlicerEndPoints(CompactionProfileResult<CompactionDesignProfileResult> profile);
  }
}
