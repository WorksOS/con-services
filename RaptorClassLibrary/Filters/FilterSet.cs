using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.Filters
{
    /// <summary>
    /// FilterSet represents a set of filters to be applied to each subgrid in a query within a single operation
    /// </summary>
    public class FilterSet
    {
        /// <summary>
        /// The list of combined attribute and spatial filters to be used
        /// </summary>
        public CombinedFilter[] Filters { get; set; } = null;

        /// <summary>
        /// Default no-arg constructor that creates a zero-sized array of combined filters
        /// </summary>
        public FilterSet()
        {
            Filters = new CombinedFilter[0];
        }
    }
}
