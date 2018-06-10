using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Utilities;

namespace VSS.TRex.SubGridTrees.Client
{
  [Serializable]
    public abstract class GenericClientLeafSubGrid<T> : ClientLeafSubGrid, IGenericClientLeafSubGrid<T>
  {
        private static ILogger Log = Logging.Logger.CreateLogger("GenericClientLeafSubGrid");

        /// <summary>
        /// The array of cell values this subgrid client class maintains
        /// </summary>
        public T[,] Cells { get; set; } = new T[SubGridTree.SubGridTreeDimension, SubGridTree.SubGridTreeDimension];

        /// <summary>
        /// The array of null values to be used to set all cell values to their client grid respective value
        /// It is the responsibility of the derived class to proved a class constructor to initialise the
        /// values NullCells to the correct nu ll values
        /// </summary>
        protected static T[,] NullCells = new T[SubGridTree.SubGridTreeDimension, SubGridTree.SubGridTreeDimension];

        /// <summary>
        /// Represented a T element configured as the null value for cells is this client leafe subgrid
        /// </summary>
        public abstract T NullCell(); // => default(T);

        /// <summary>
        /// Main constructor. Creates the local generic Items[,] array and delegates to base(...)
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="parent"></param>
        /// <param name="level"></param>
        /// <param name="cellSize"></param>
        /// <param name="indexOriginOffset"></param>
        public GenericClientLeafSubGrid(ISubGridTree owner, ISubGrid parent, byte level, double cellSize, uint indexOriginOffset) : base(owner, parent, level, cellSize, indexOriginOffset)
        {
        }

        /// <summary>
        /// Iterates over all the cells in the leaf subgrid calling functor on each of them.
        /// Both non-null and null values are presented to functor. If functor returns false
        /// the ForEach terminates and returns false.
        /// </summary>
        /// <param name="functor"></param>
        /// <returns></returns>
        public bool ForEach(Func<T, bool> functor)
        {
            for (byte I = 0; I < SubGridTree.SubGridTreeDimension; I++)
            {
                for (byte J = 0; J < SubGridTree.SubGridTreeDimension; J++)
                {
                    if (!functor(Cells[I, J]))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Iterates over all the cells in the leaf subgrid calling functor on each of them.
        /// Both non-null and null values are presented to functor.
        /// </summary>
        /// <param name="functor"></param>
        /// <returns></returns>
        public void ForEach(Action<byte, byte, T> functor)
        {
            for (byte I = 0; I < SubGridTree.SubGridTreeDimension; I++)
            {
                for (byte J = 0; J < SubGridTree.SubGridTreeDimension; J++)
                {
                    functor(I, J, Cells[I, J]);
                }
            }
        }

        /// <summary>
        /// Iterates over all the cells in the leaf subgrid calling functor on each of them.
        /// Both non-null and null values are presented to functor.
        /// </summary>
        /// <param name="functor"></param>
        /// <returns></returns>
        public static void ForEach(Action<byte, byte> functor) => SubGridUtilities.SubGridDimensionalIterator((x, y) => functor((byte)x, (byte)y));

        public override void Clear()
        {
          Array.Copy(NullCells, 0, Cells, 0, SubGridTree.SubGridTreeCellsPerSubgrid);

          // Recreate the array. .Net will initialise the memory used to zero's effecting the clear
          // Cells = new T[SubGridTree.SubGridTreeDimension, SubGridTree.SubGridTreeDimension];
        }

    /*
            /// <summary>
            /// Write the contents of leaf sub grid using the supplied formatter
            /// </summary>
            /// <param name="formatter"></param>
            /// <param name="stream"></param>
            public override void Write(BinaryFormatter formatter, Stream stream)
            {
                base.Write(formatter, stream);

                formatter.Serialize(stream, Cells);
            }

            /// <summary>
            /// Fill the contents of the leaf sub grid reading the binary representation using the provided formatter
            /// </summary>
            /// <param name="formatter"></param>
            /// <param name="stream"></param>
            public override void Read(BinaryFormatter formatter, Stream stream)
            {
                base.Read(formatter, stream);

                Cells = (T[,])formatter.Deserialize(stream);
            }
    */

    /// <summary>
    /// Dumps the contents of this client leaf subgrid into the log in a human readable form
    /// </summary>
    public override void DumpToLog(string title)
        {
          Log.LogDebug($"Subgrid {Moniker()}: {title}");
        }

        /// <summary>
        /// Assign 
        /// </summary>
        /// <param name="source"></param>
        public void Assign(GenericClientLeafSubGrid<T> source)
        {
            base.Assign(source);

            // Derived classes are responsible for performing assignation of the Cells structure as they can use optimal methods such as BlockCopy()
            // ForEach((x, y) => Cells[x, y] = source.Cells[x, y]);
        }
    }
}
