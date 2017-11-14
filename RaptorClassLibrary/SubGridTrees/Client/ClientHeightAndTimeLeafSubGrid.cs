using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Common;
using VSS.VisionLink.Raptor.Filters;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;
using VSS.VisionLink.Raptor.SubGridTrees.Utilities;

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

        /// <summary>
        /// Sets height and time components of the struct in a single operation
        /// </summary>
        /// <param name="height"></param>
        /// <param name="time"></param>
        public void Set(float height, DateTime time)
        {
            Height = height;
            Time = time;
        }
    }

    /// <summary>
    /// Client leaf sub grid that tracks height and time for each cell
    /// </summary>
    [Serializable]
    public class ClientHeightAndTimeLeafSubGrid : GenericClientLeafSubGrid<SubGridCellHeightAndTime>
    {
        [NonSerialized]
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The map of cells within this subgrid where the elevations are derived from a surveyed surface 
        /// rather than a machine cell pass
        /// </summary>
        public SubGridTreeBitmapSubGridBits SurveyedSurfaceMap = new SubGridTreeBitmapSubGridBits(SubGridTreeBitmapSubGridBits.SubGridBitsCreationOptions.Unfilled);

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

        /// <summary>
        /// Determine if a filtered height is valid (not null)
        /// </summary>
        /// <param name="filteredValue"></param>
        /// <returns></returns>
        public override bool AssignableFilteredValueIsNull(ref FilteredPassData filteredValue) => filteredValue.FilteredPass.Height == Consts.NullSingle;

        /// <summary>
        /// Assign filtered height value from a filtered pass to a cell
        /// </summary>
        /// <param name="cellX"></param>
        /// <param name="cellY"></param>
        /// <param name="Context"></param>
        public override void AssignFilteredValue(byte cellX, byte cellY, FilteredValueAssignmentContext Context)
        {
            Cells[cellX, cellY].Set(Context.FilteredValue.FilteredPassData.FilteredPass.Height, Context.FilteredValue.FilteredPassData.FilteredPass.Time);
        }

        /// <summary>
        /// Determines if the height at the cell location is null or not.
        /// </summary>
        /// <param name="cellX"></param>
        /// <param name="cellY"></param>
        /// <returns></returns>
        public override bool CellHasValue(byte cellX, byte cellY) => Cells[cellX, cellY].Height != Consts.NullHeight;

        /// <summary>
        /// An array containing the content of a fully null subgrid
        /// </summary>
        public static SubGridCellHeightAndTime[,] nullCells = NullHeightsAndTimes();

        private static SubGridCellHeightAndTime[,] NullHeightsAndTimes()
        {
            SubGridCellHeightAndTime[,] result = new SubGridCellHeightAndTime[SubGridTree.SubGridTreeDimension, SubGridTree.SubGridTreeDimension];

            ForEach((x, y) => result[x, y].Clear());

            return result;
        }

        /// <summary>
        /// Sets all cell heights to null and clears the surveyed surface pass map
        /// </summary>
        public override void Clear()
        {
            if (Cells == null)
            {
                base.Clear();
            }

            //Buffer.BlockCopy(nullCells, 0, Cells, 0, SubGridTree.SubGridTreeCellsPerSubgrid * sizeof(SubGridCellHeightAndTime));
            ForEach((x, y) => Cells[x, y].Clear()); // TODO: Optimisation: Use PassData_Height_Null assignment as in current gen;

            SurveyedSurfaceMap.Clear();
        }

        /// <summary>
        /// Write the contents of the Items array using the supplied writer
        /// This is an unimplemented override; a generic BinaryReader based implementation is not provided. 
        /// Override to implement if needed.
        /// </summary>
        /// <param name="writer"></param>
        public override void Write(BinaryWriter writer, byte[] buffer)
        {
            base.Write(writer, buffer);

            SurveyedSurfaceMap.Write(writer, buffer);

            /// Horribly slow... May need to separate height and time into separate vectors for performance...
            SubGridUtilities.SubGridDimensionalIterator((x, y) =>
            {
                writer.Write(Cells[x, y].Height);
                writer.Write(Cells[x, y].Time.ToBinary());
            });

            //Buffer.BlockCopy(Cells, 0, buffer, 0, SubGridTree.SubGridTreeCellsPerSubgrid * sizeof(float));
            //writer.Write(buffer, 0, SubGridTree.SubGridTreeCellsPerSubgrid * sizeof(float));
        }

        /// <summary>
        /// Fill the items array by reading the binary representation using the provided reader. 
        /// This is an unimplemented override; a generic BinaryReader based implementation is not provided. 
        /// Override to implement if needed.
        /// </summary>
        /// <param name="reader"></param>
        public override void Read(BinaryReader reader, byte[] buffer)
        {
            base.Read(reader, buffer);

            SurveyedSurfaceMap.Read(reader, buffer);

            /// Horribly slow... May need to separate height and time into separate vectors for performance...
            SubGridUtilities.SubGridDimensionalIterator((x, y) =>
            {
                Cells[x, y].Set(reader.ReadSingle(), DateTime.FromBinary(reader.ReadInt64()));
            });

            //reader.Read(buffer, 0, SubGridTree.SubGridTreeCellsPerSubgrid * sizeof(float));
            //Buffer.BlockCopy(buffer, 0, Cells, 0, SubGridTree.SubGridTreeCellsPerSubgrid * sizeof(float));
        }

        /// <summary>
        /// Sets all elevations in the height & time client leaf sub grid to zero (not null), and minimum time value
        /// </summary>
        public void SetToZeroHeight() => ForEach((x, y) => Cells[x, y].Set(0, DateTime.MinValue)); // TODO: Optimisation: Use single array assignment
    }
}
