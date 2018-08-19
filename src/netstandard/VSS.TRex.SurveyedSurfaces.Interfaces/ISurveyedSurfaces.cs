using System;
using System.Collections.Generic;

namespace VSS.TRex.Surfaces.Interfaces
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
  }
}
