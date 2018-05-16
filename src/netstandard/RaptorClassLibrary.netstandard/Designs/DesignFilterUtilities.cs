using System;
using VSS.TRex.Filters;
using VSS.TRex.SubGridTrees;

namespace VSS.TRex.Designs
{
    /// <summary>
    /// Utilitites relating to interactions between filters and design surfaces
    /// </summary>
    public static class DesignFilterUtilities
    {
        /// <summary>
        /// Given a design used as an elevation range filter aspect, retrieve the existence map for the design and
        /// including it in the supplied overall existence map for the query
        /// </summary>
        /// <param name="siteModelID"></param>
        /// <param name="filter"></param>
        /// <param name="overallExistenceMap"></param>
        /// <returns></returns>
        public static bool ProcessDesignElevationsForFilter(Guid siteModelID,
                                                            CombinedFilter filter,
                                                            SubGridTreeSubGridExistenceBitMask overallExistenceMap)
        {
            if (filter == null)
            {
                return true;
            }

            if (filter.AttributeFilter.HasElevationRangeFilter && filter.AttributeFilter.ElevationRangeDesignID != long.MinValue)
            {
                SubGridTreeSubGridExistenceBitMask DesignExistanceMap = ExistenceMaps.ExistenceMaps.GetSingleExistenceMap
                    (siteModelID, ExistenceMaps.Consts.EXISTANCE_MAP_DESIGN_DESCRIPTOR, filter.AttributeFilter.ElevationRangeDesignID);

                if (overallExistenceMap == null)
                {
                    return false;
                }

                if (DesignExistanceMap != null)
                {
                    overallExistenceMap.SetOp_OR(DesignExistanceMap);
                }
            }

            return true;
        }
    }
}
