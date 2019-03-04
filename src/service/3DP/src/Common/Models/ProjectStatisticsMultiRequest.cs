using System;

namespace VSS.Productivity3D.Common.Models
{
  /// <summary>
  /// Request representation for interfacing with the ProjectStatisticsExecutor
  /// Both ProjectUid/Id should be present
  ///   May need to convert SSIds
  /// </summary>
  public class ProjectStatisticsMultiRequest 
  {
    public long ProjectId { get; set; }

    public Guid? ProjectUid { get; set; }

    public long[] ExcludedSurveyedSurfaceIds { get; private set; }
    public Guid[] ExcludedSurveyedSurfaceUids { get; private set; }


    public ProjectStatisticsMultiRequest(Guid? projectUid, long projectId, 
      Guid[] excludedSurveyedSurfaceUids, long[] excludedSurveyedSurfaceIds)
    {
      ProjectUid = projectUid;
      ProjectId = projectId;
      ExcludedSurveyedSurfaceUids = excludedSurveyedSurfaceUids;
      ExcludedSurveyedSurfaceIds = excludedSurveyedSurfaceIds;
    }

  }
}
