using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.SubGridTrees;

namespace VSS.VisionLink.Raptor
{
    /// <summary>
    /// ProductionDataExistanceMap stores a map of existing subgrids that contain data processed into the datamodel
    /// </summary>
    public class ProductionDataExistanceMap
    {
        public long DataModelID { get; set; } = -1;
        public SubGridTreeSubGridExistenceBitMask Map { get; set; } = null;

        public ProductionDataExistanceMap(long dataModelID, double cellSize)
        {
            DataModelID = dataModelID;

            Map = new SubGridTreeSubGridExistenceBitMask(); //, kICFSSubgridSpatialExistanceMapHeader, kICFSSubgridSpatialExistanceMapVersion);
        }    
    }
}
