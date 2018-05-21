using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Reflection;
using VSS.TRex.Filters;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.SubGridTrees.Client
{
    /*
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
    */

    /// <summary>
    /// Client leaf sub grid that tracks height and time for each cell
    /// </summary>
    [Serializable]
    public class ClientHeightAndTimeLeafSubGrid : ClientHeightLeafSubGrid
    {
        [NonSerialized]
        private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

        /// <summary>
        /// Time values for the heights stored in the height and time structure. Times are expressed as the binary DateTime format to promote efficient copying of arrays
        /// </summary>
        public long[,] Times = new long[SubGridTree.SubGridTreeDimension, SubGridTree.SubGridTreeDimension];

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
            _gridDataType = TRex.Types.GridDataType.HeightAndTime;
        }

        /// <summary>
        /// Assign filtered height value from a filtered pass to a cell
        /// </summary>
        /// <param name="cellX"></param>
        /// <param name="cellY"></param>
        /// <param name="Context"></param>
        public override void AssignFilteredValue(byte cellX, byte cellY, FilteredValueAssignmentContext Context)
        {
            base.AssignFilteredValue(cellX, cellY, Context);

            Times[cellX, cellY] = Context.FilteredValue.FilteredPassData.FilteredPass.Time.ToBinary();
        }

        /// <summary>
        /// An array containing the content of null times for all the cell in the subgrid
        /// </summary>
        public static long[,] nullTimes = NullTimes();

        private static long[,] NullTimes()
        {
            long[,] result = new long[SubGridTree.SubGridTreeDimension, SubGridTree.SubGridTreeDimension];

            DateTime min = DateTime.MinValue;
            long nullValue = min.ToBinary();

            ForEach((x, y) => result[x, y] = nullValue);

            return result;
        }

        /// <summary>
        /// Sets all cell heights to null and clears the surveyed surface pass map
        /// </summary>
        public override void Clear()
        {
            base.Clear();

            Buffer.BlockCopy(nullTimes, 0, Times, 0, SubGridTree.SubGridTreeCellsPerSubgrid * sizeof(long));
        }

        /// <summary>
        /// Write the contents of the Items array using the supplied writer
        /// This is an unimplemented override; a generic BinaryReader based implementation is not provided. 
        /// Override to implement if needed.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="buffer"></param>
        public override void Write(BinaryWriter writer, byte[] buffer)
        {
            base.Write(writer, buffer);

            Buffer.BlockCopy(Times, 0, buffer, 0, SubGridTree.SubGridTreeCellsPerSubgrid * sizeof(long));
            writer.Write(buffer, 0, SubGridTree.SubGridTreeCellsPerSubgrid * sizeof(long));
        }

        /// <summary>
        /// Fill the items array by reading the binary representation using the provided reader. 
        /// This is an unimplemented override; a generic BinaryReader based implementation is not provided. 
        /// Override to implement if needed.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="buffer"></param>
        public override void Read(BinaryReader reader, byte[] buffer)
        {
            base.Read(reader, buffer);

            reader.Read(buffer, 0, SubGridTree.SubGridTreeCellsPerSubgrid * sizeof(long));
            Buffer.BlockCopy(buffer, 0, Times, 0, SubGridTree.SubGridTreeCellsPerSubgrid * sizeof(long));
        }
    }
}
