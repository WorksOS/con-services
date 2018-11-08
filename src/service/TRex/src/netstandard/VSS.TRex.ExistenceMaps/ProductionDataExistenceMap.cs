using System;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.ExistenceMaps
{
    /// <summary>
    /// ProductionDataExistenceMap stores a map of existing subgrids that contain data processed into the datamodel
    /// </summary>
    public class ProductionDataExistenceMap
    {
        public Guid DataModelID { get; set; } 

        public ISubGridTreeBitMask Map { get; set; }

        public ProductionDataExistenceMap(Guid dataModelID, double cellSize)
        {
            DataModelID = dataModelID;

            Map = new SubGridTreeSubGridExistenceBitMask
            {
                CellSize = cellSize
            };
        }    
    }
}
