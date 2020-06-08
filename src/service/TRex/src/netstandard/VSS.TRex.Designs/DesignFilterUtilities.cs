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
        public static bool ProcessDesignElevationsForFilter(ISiteModel siteModel, //Guid siteModelID,
                                                            ICombinedFilter filter,
                                                            ISubGridTreeBitMask overallExistenceMap)
        {
            if (filter == null)
                return true;

            if (overallExistenceMap == null)
              return false;

            if (filter.AttributeFilter.HasElevationRangeFilter && filter.AttributeFilter.ElevationRangeDesign.DesignID != Guid.Empty)
            {
                var designExistenceMap = DIContext.Obtain<IExistenceMaps>().GetSingleExistenceMap
                    (siteModel.ID, Consts.EXISTENCE_MAP_DESIGN_DESCRIPTOR, filter.AttributeFilter.ElevationRangeDesign.DesignID);

                if (designExistenceMap != null)
                {
                    // Not sure this is really needed...
                    designExistenceMap.CellSize = SubGridTreeConsts.SubGridTreeDimension * siteModel.CellSize;
                    overallExistenceMap.SetOp_OR(designExistenceMap);
                }
            }

            return true;
        }
    }
}
