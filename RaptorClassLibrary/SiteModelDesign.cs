using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Geometry;

namespace VSS.VisionLink.Raptor
{
    /// <summary>
    /// Describes a single design used in the site model. It's chief purpose is to record the name of the design 
    /// and the plan extents of the cell pass information stored within the sitemodel for that design.
    /// </summary>
    [Serializable]
    public class SiteModelDesign : IEquatable<string>
    {
        private string name = "";
        public string Name { get { return name; } }

        public BoundingWorldExtent3D Extents { get; set; } 

        //FWorkingExtents is used as a working area for computing modified
        //design extents by operations such as data deletion. It is not persisted
        //in the design description
        public BoundingWorldExtent3D WorkingExtents { get; set; } 

        public SiteModelDesign()
        {
            Extents.Clear();
            WorkingExtents.Clear();
        }

        public SiteModelDesign(string name, BoundingWorldExtent3D extents) : this()
        {
            this.name = name;
            Extents = extents;
        }

        public bool MatchesDesignName(string other) => (other != null) && name.Equals(other);

        public bool Equals(string other) => (other != null) && name.Equals(other);
    }
}
