using System;
using VSS.TRex.DI;
using VSS.TRex.ExistenceMaps.Interfaces;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Designs
{
    /// <summary>
    /// Utilitites relating to interactions between filters and design surfaces
    /// </summary>
    public static class DesignFilterUtilities
    {
        /// <summary>
        /// DI'ed context for access to ExistenceMaps functionality
        /// </summary>
        private static IExistenceMaps ExistenceMaps = DIContext.Obtain<IExistenceMaps>();

        /// <summary>
        /// Given a design used as an elevation range filter aspect, retrieve the existence map for the design and
        /// including it in the supplied overall existence map for the query
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

            if (filter.AttributeFilter.HasElevationRangeFilter && filter.AttributeFilter.ElevationRangeDesignID != Guid.Empty)
            {
                ISubGridTreeBitMask DesignExistanceMap = ExistenceMaps.GetSingleExistenceMap
                    (siteModel.ID, Consts.EXISTANCE_MAP_DESIGN_DESCRIPTOR, filter.AttributeFilter.ElevationRangeDesignID);

                if (overallExistenceMap == null)
                    return false;

                if (DesignExistanceMap != null)
                    overallExistenceMap.SetOp_OR(DesignExistanceMap);

              DesignExistanceMap.CellSize = SubGridTreeConsts.SubGridTreeDimension * siteModel.Grid.CellSize;
            }

      return true;
        }
    }
}
