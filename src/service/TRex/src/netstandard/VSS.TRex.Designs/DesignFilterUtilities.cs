using System;
using VSS.TRex.DI;
using VSS.TRex.ExistenceMaps.Interfaces;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Designs
{
    /// <summary>
    /// Utilities relating to interactions between filters and design surfaces
    /// </summary>
    public static class DesignFilterUtilities
    {
        /// <summary>
        /// Given a design used as an elevation range filter aspect, retrieve the existence map for the design and
        /// includes it in the supplied overall existence map for the query
        /// </summary>
        /// <param name="siteModel"></param>
        /// <param name="filter"></param>
        /// <param name="overallExistenceMap"></param>
        /// <returns></returns>
        public static bool ProcessDesignElevationsForFilter(ISiteModel siteModel, //Guid siteModelID,
                                                            ICombinedFilter filter,
                                                            ISubGridTreeBitMask overallExistenceMap)
        {
            if (filter == null)
                return true;

            if (overallExistenceMap == null)
              return false;

            if (filter.AttributeFilter.HasElevationRangeFilter && filter.AttributeFilter.ElevationRangeDesignUID != Guid.Empty)
            {
                ISubGridTreeBitMask DesignExistenceMap = DIContext.Obtain<IExistenceMaps>().GetSingleExistenceMap
                    (siteModel.ID, Consts.EXISTENCE_MAP_DESIGN_DESCRIPTOR, filter.AttributeFilter.ElevationRangeDesignUID);

                if (DesignExistenceMap != null)
                {
                    // Not sure this is really needed...
                    DesignExistenceMap.CellSize = SubGridTreeConsts.SubGridTreeDimension * siteModel.Grid.CellSize;
                    overallExistenceMap.SetOp_OR(DesignExistenceMap);
                }
            }

            return true;
        }
    }
}
