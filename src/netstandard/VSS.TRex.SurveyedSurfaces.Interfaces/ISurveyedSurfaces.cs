using System;
using System.Collections.Generic;

namespace VSS.TRex.SurveyedSurfaces.Interfaces
{
  public interface ISurveyedSurfaces : IList<ISurveyedSurface>
  {
    void FilterSurveyedSurfaceDetails(bool HasTimeFilter,
      DateTime StartTime, DateTime EndTime,
      bool ExcludeSurveyedSurfaces,
      ISurveyedSurfaces FilteredSurveyedSurfaceDetails,
      Guid[] ExclusionList);

    void Assign(ISurveyedSurfaces surveyedSurfaces);

    void SortChronologically(bool Descending = true);

    /// <summary>
    /// Determines if there is at least one surveyed surface with an as at date later than the data provided as a DateTime
    /// Optimal performance will be observed if the list is sorted in ascending chronological order
    /// </summary>
    /// <param name="TimeStamp"></param>
    /// <returns></returns>
    bool HasSurfaceLaterThan(DateTime TimeStamp);

    /// <summary>
    /// Determines if there is at least one surveyed surface with an as at date later than the data provided as a DateTime.ToBinary() Int64
    /// Optimal performance will be observed if the list is sorted in ascending chronological order
    /// </summary>
    /// <param name="TimeStamp"></param>
    /// <returns></returns>
    bool HasSurfaceLaterThan(long TimeStamp);

    /// <summary>
    /// Determines if there is at least one surveyed surface with an as at date earlier than the data provided as a DateTime
    /// Optimal performance will be observed if the list is sorted in ascending chronological order
    /// </summary>
    /// <param name="TimeStamp"></param>
    /// <returns></returns>
    bool HasSurfaceEarlierThan(DateTime TimeStamp);

    /// <summary>
    /// Determines if there is at least one surveyed surface with an as at date earlier than the data provided as a DateTime.ToBinary() Int64
    /// Optimal performance will be observed if the list is sorted in ascending chronological order
    /// </summary>
    /// <param name="TimeStamp"></param>
    /// <returns></returns>
    bool HasSurfaceEarlierThan(long TimeStamp);
  }
}
