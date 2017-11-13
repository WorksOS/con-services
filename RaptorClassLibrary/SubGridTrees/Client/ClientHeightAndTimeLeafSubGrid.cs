using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Common;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;

namespace VSS.VisionLink.Raptor.SubGridTrees.Client
{
    /// <summary>
    /// The content of each cell in a 'Height and time' client leaf sub grid. Each cell stores an elevation and 
    /// the time at which the elevation measurement relates to (either the time stamp on a cell pass or the time
    /// stamp from the surveyed surface that this elevation came from
    /// </summary>
    public struct SubGridCellHeightAndTime
    {
        /// <summary>
        /// Measure height at the cell location
        /// </summary>
        public float Height { get; set; }

        /// <summary>
        /// UTC time at which the measurement is relevant
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// Set Height and Time values to null
        /// </summary>
        public void Clear()
        {
            Time = DateTime.MinValue;
            Height = Consts.NullHeight;
        }

        public void Set(float height, DateTime time)
        {
            Height = height;
            Time = time;
        }
    }

    /// <summary>
    /// Client leaf sub grid that tracks height and time for each cell
    /// </summary>
    public class ClientHeightAndTimeLeafSubGrid : GenericClientLeafSubGrid<SubGridCellHeightAndTime>
    {
        /// <summary>
        /// The map of cells within this subgrid where the elevations are derived from a surveyed surface 
        /// rather than a machine cell pass
        /// </summary>
        public SubGridTreeBitmapSubGridBits SurveyedSurfaceMap { get; set; }

        /// <summary>
        /// Constructor. Set the grid to HeightAndTime.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="parent"></param>
        /// <param name="level"></param>
        /// <param name="cellSize"></param>
        /// <param name="indexOriginOffset"></param>
        public ClientHeightAndTimeLeafSubGrid(ISubGridTree owner, ISubGrid parent, byte level, double cellSize, uint indexOriginOffset) : base(owner, parent, level, cellSize, indexOriginOffset)
        {
            GridDataType = Raptor.Types.GridDataType.HeightAndTime;
        }
    }

}
