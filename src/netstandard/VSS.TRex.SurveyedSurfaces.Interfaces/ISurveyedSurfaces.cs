using System;
using System.Collections.Generic;
using VSS.TRex.Designs.Models;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Utilities.Interfaces;

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

    /// <summary>
    /// Create a new surveyed surface in the list based on the provided details
    /// </summary>
    /// <param name="ASurveyedSurfaceID"></param>
    /// <param name="ADesignDescriptor"></param>
    /// <param name="AAsAtDate"></param>
    /// <param name="AExtents"></param>
    /// <returns></returns>
    ISurveyedSurface AddSurveyedSurfaceDetails(Guid ASurveyedSurfaceID,
      DesignDescriptor ADesignDescriptor,
      DateTime AAsAtDate,
      BoundingWorldExtent3D AExtents);

    /// <summary>
    /// Remove a given surveyed surface from the list of surveyed surfaces for a site model
    /// </summary>
    /// <param name="ASurveyedSurfaceID"></param>
    /// <returns></returns>
    bool RemoveSurveyedSurface(Guid ASurveyedSurfaceID);

    /// <summary>
    /// Given a filter compute which of the surfaces in the list match any given time aspect
    /// of the filter, and the overall existance map of the surveyed surfaces that match the filter.
    /// ComparisonList denotes a possibly pre-filtered set of surfaces for another filter; if this is the same as the 
    /// filtered set of surfaces then the overall existence map for those surfaces will not be computed as it is 
    /// assumed to be the same.
    /// </summary>
    /// <param name="siteModelID"></param>
    /// <param name="Filter"></param>
    /// <param name="ComparisonList"></param>
    /// <param name="FilteredSurveyedSurfaces"></param>
    /// <param name="OverallExistenceMap"></param>
    /// <returns></returns>
    bool ProcessSurveyedSurfacesForFilter(Guid siteModelID,
      ICombinedFilter Filter,
      ISurveyedSurfaces ComparisonList,
      ISurveyedSurfaces FilteredSurveyedSurfaces,
      ISubGridTreeBitMask OverallExistenceMap);
  }
}
