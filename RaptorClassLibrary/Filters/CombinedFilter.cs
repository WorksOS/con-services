using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.SiteModels;

namespace VSS.VisionLink.Raptor.Filters
{
    /// <summary>
    /// Combined filter represents both spatial and attribute based filtering considerations
    /// </summary>
    public class CombinedFilter
    {
        public CellPassAttributeFilter AttributeFilter = null;
        public CellSpatialFilter SpatialFilter = null;

        public CombinedFilter(SiteModel Owner)
        {
            AttributeFilter = new CellPassAttributeFilter(Owner);
            SpatialFilter = new CellSpatialFilter();
        }

        /// <summary>
        /// Constructor accepting attribute and spatial filters
        /// </summary>
        /// <param name="attributeFilter"></param>
        /// <param name="spatialFilter"></param>
        public CombinedFilter(CellPassAttributeFilter attributeFilter, CellSpatialFilter spatialFilter)
        {
            AttributeFilter = attributeFilter;
            SpatialFilter = spatialFilter;
        }
    }
}
