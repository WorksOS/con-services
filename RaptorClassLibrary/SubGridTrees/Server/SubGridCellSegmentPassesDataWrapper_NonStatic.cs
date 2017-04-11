using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Cells;

namespace VSS.VisionLink.Raptor.SubGridTrees.Server
{
    public class SubGridCellSegmentPassesDataWrapper_NonStatic
    {
        public int SegmentPassCount { get; set; } = 0;

        public Cell[,] PassData = new Cell[SubGridTree.SubGridTreeDimension, SubGridTree.SubGridTreeDimension];

        public SubGridCellSegmentPassesDataWrapper_NonStatic()
        {
        }
    }
}
