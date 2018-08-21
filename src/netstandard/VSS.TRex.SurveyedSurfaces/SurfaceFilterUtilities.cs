using System;
using System.Linq;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SurveyedSurfaces.Interfaces;

namespace VSS.TRex.Surfaces
{
    /// <summary>
    /// Utilities relating to interactions between filters and sets of surveyed surfaces
    /// </summary>
    public static class SurfaceFilterUtilities
    {
        /// <summary>
        /// Given a set of surveyed surfaces and a filter compute which of the surfaces match any given time aspect
        /// of the filter, and the overall existance map of the surveyed surfaces that match the filter.
        /// ComparisonList denotes a possibly pre-filtered set of surfaces for another filter; if this is the same as the 
        /// filtered set of surfaces then the overall existence map for those surfaces will not be computed as it is 
        /// assumed to be the same.
        /// </summary>
        /// <param name="siteModelID"></param>
        /// <param name="SurveyedSurfaces"></param>
        /// <param name="Filter"></param>
        /// <param name="ComparisonList"></param>
        /// <param name="FilteredSurveyedSurfaces"></param>
        /// <param name="OverallExistenceMap"></param>
        /// <returns></returns>
        public static bool ProcessSurveyedSurfacesForFilter(Guid siteModelID,
                                                            ISurveyedSurfaces SurveyedSurfaces,
                                                            ICombinedFilter Filter,
                                                            ISurveyedSurfaces ComparisonList,
                                                            ISurveyedSurfaces FilteredSurveyedSurfaces,
                                                            SubGridTreeSubGridExistenceBitMask OverallExistenceMap)
        {
            if (SurveyedSurfaces == null)
                return true;

            // Filter out any surveyed surfaces which don't match current filter (if any) - realistically, this is time filters we're thinking of here
            SurveyedSurfaces.FilterSurveyedSurfaceDetails(Filter.AttributeFilter.HasTimeFilter,
                                                          Filter.AttributeFilter.StartTime, Filter.AttributeFilter.EndTime,
                                                          Filter.AttributeFilter.ExcludeSurveyedSurfaces(),
                                                          FilteredSurveyedSurfaces,
                                                          Filter.AttributeFilter.SurveyedSurfaceExclusionList);

            if (FilteredSurveyedSurfaces?.Equals(ComparisonList) == true)
                return true;

            if (FilteredSurveyedSurfaces.Count > 0)
            {
                SubGridTreeSubGridExistenceBitMask SurveyedSurfaceExistanceMap = ExistenceMaps.ExistenceMaps.GetCombinedExistenceMap(siteModelID,
                FilteredSurveyedSurfaces.Select(x => new Tuple<long, Guid>(ExistenceMaps.Consts.EXISTANCE_SURVEYED_SURFACE_DESCRIPTOR, x.ID)).ToArray());

                if (OverallExistenceMap == null)
                    return false;

                if (SurveyedSurfaceExistanceMap != null)
                    OverallExistenceMap.SetOp_OR(SurveyedSurfaceExistanceMap);
            }

            return true;
        }
    }
}
