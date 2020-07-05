using System;
using System.Collections.Generic;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.SurveyedSurfaces.Interfaces
{
  public interface ISurveyedSurfaces : IList<ISurveyedSurface>, IBinaryReaderWriter
  {
    void FilterSurveyedSurfaceDetails(bool HasTimeFilter,
      DateTime StartTime, DateTime EndTime,
      bool ExcludeSurveyedSurfaces,
      ISurveyedSurfaces FilteredSurveyedSurfaceDetails,
      Guid[] ExclusionList);

    void Assign(ISurveyedSurfaces surveyedSurfaces);

    void SortChronologically(bool Descending);

    /// <summary>
    /// Determines if there is at least one surveyed surface with an as at date later than the data provided as a DateTime
    /// Optimal performance will be observed if the list is sorted in ascending chronological order
    /// </summary>
    bool HasSurfaceLaterThan(DateTime timeStamp);

    /// <summary>
    /// Determines if there is at least one surveyed surface with an as at date later than the data provided as a DateTime.ToBinary() Int64
    /// Optimal performance will be observed if the list is sorted in ascending chronological order
    /// </summary>
    bool HasSurfaceLaterThan(long timeStamp);

    /// <summary>
    /// Determines if there is at least one surveyed surface with an as at date earlier than the data provided as a DateTime
    /// Optimal performance will be observed if the list is sorted in ascending chronological order
    /// </summary>
    bool HasSurfaceEarlierThan(DateTime timeStamp);

    /// <summary>
    /// Determines if there is at least one surveyed surface with an as at date earlier than the data provided as a DateTime.ToBinary() Int64
    /// Optimal performance will be observed if the list is sorted in ascending chronological order
    /// </summary>
    bool HasSurfaceEarlierThan(long timeStamp);

    /// <summary>
    /// Create a new surveyed surface in the list based on the provided details
    /// </summary>
    ISurveyedSurface AddSurveyedSurfaceDetails(Guid surveyedSurfaceUid,
      DesignDescriptor designDescriptor,
      DateTime asAtDate,
      BoundingWorldExtent3D extents);

    /// <summary>
    /// Remove a given surveyed surface from the list of surveyed surfaces for a site model
    /// </summary>
    bool RemoveSurveyedSurface(Guid surveyedSurfaceUid);

    /// <summary>
    /// Given a filter compute which of the surfaces in the list match any given time aspect
    /// of the filter, and the overall existence map of the surveyed surfaces that match the filter.
    /// ComparisonList denotes a possibly pre-filtered set of surfaces for another filter; if this is the same as the 
    /// filtered set of surfaces then the overall existence map for those surfaces will not be computed as it is 
    /// assumed to be the same.
    /// </summary>
    bool ProcessSurveyedSurfacesForFilter(Guid surveyedSurfaceUid,
      ICombinedFilter Filter,
      ISurveyedSurfaces ComparisonList,
      ISurveyedSurfaces FilteredSurveyedSurfaces,
      ISubGridTreeBitMask OverallExistenceMap);

    /// <summary>
    /// Locates a surveyed surface in the list with the given GUID
    /// </summary>
    ISurveyedSurface Locate(Guid surveyedSurfaceUid);

    /// <summary>
    /// Determine if the surveyed surfaces in this list are the same as the surveyed surfaces in the other list, based on ID comparison
    /// </summary>
    bool IsSameAs(ISurveyedSurfaces other);
  }
}
