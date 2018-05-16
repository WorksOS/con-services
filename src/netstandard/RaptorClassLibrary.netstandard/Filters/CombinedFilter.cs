using System;

namespace VSS.TRex.Filters
{
    /// <summary>
    /// Combined filter represents both spatial and attribute based filtering considerations
    /// </summary>
    [Serializable]
    public class CombinedFilter
    {
        /// <summary>
        /// The filter reponsible for selection of cell passes based on attribute filtering criteria related to cell passes
        /// </summary>
        public CellPassAttributeFilter AttributeFilter;

        /// <summary>
        /// The filter responsible for selection of cells based on spatial filtering criteria related to cell location
        /// </summary>
        public CellSpatialFilter SpatialFilter;

        /// <summary>
        /// Defautl no-arg constructor
        /// </summary>
        public CombinedFilter()
        {
            AttributeFilter = new CellPassAttributeFilter();
            SpatialFilter = new CellSpatialFilter();
        }

/*
 /// <summary>
        /// Constructor that takes a Sitemodel owner and creates default attribute and spatial filters
        /// </summary>
        /// <param name="Owner"></param>
        public CombinedFilter(SiteModel Owner) : this()
        {
            AttributeFilter = new CellPassAttributeFilter(Owner);
            SpatialFilter = new CellSpatialFilter();
        }
*/

        /// <summary>
        /// Constructor accepting attribute and spatial filters
        /// </summary>
        /// <param name="attributeFilter"></param>
        /// <param name="spatialFilter"></param>
        public CombinedFilter(CellPassAttributeFilter attributeFilter, CellSpatialFilter spatialFilter) : this()
        {
            AttributeFilter = attributeFilter;
            SpatialFilter = spatialFilter;
        }
    }
}
