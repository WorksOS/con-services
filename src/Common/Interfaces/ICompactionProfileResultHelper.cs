using System;
using System.Collections.Generic;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Common.ResultHandling;

namespace VSS.Productivity3D.Common.Interfaces
{
  public interface ICompactionProfileResultHelper
  {
    void FindCutFillElevations(CompactionProfileResult<CompactionProfileDataResult> slicerProfileResult,
      CompactionProfileResult<CompactionProfileVertex> slicerDesignResult, string type, VolumeCalcType calcType);

    CompactionProfileResult<CompactionProfileDataResult> RearrangeProfileResult(
      CompactionProfileResult<CompactionProfileCell> slicerProfileResult);

    CompactionProfileDataResult RearrangeProfileResult(
      CompactionProfileResult<CompactionSummaryVolumesProfileCell> slicerProfileResult, VolumeCalcType? calcType);

    void RemoveRepeatedNoData(CompactionProfileResult<CompactionProfileDataResult> result, VolumeCalcType? calcType);

    void AddMidPoints(CompactionProfileResult<CompactionProfileDataResult> profileResult);

    void InterpolateEdges(CompactionProfileResult<CompactionProfileDataResult> profileResult, VolumeCalcType? calcType);

    CompactionProfileResult<CompactionDesignProfileResult> ConvertProfileResult(
      Dictionary<Guid, CompactionProfileResult<CompactionProfileVertex>> slicerProfileResults);

    void AddSlicerEndPoints(CompactionProfileResult<CompactionDesignProfileResult> profile);

  }
}
