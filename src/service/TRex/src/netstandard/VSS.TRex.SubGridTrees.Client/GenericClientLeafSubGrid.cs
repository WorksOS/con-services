using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.SubGridTrees.Client
{
    public abstract class GenericClientLeafSubGrid<T> : ClientLeafSubGrid, IGenericClientLeafSubGrid<T>
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger<GenericClientLeafSubGrid<T>>();

        /// <summary>
        /// The array of cell values this sub grid client class maintains
        /// </summary>
        public T[,] Cells { get; } = new T[SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension];

        /// <summary>
        /// The array of null values to be used to set all cell values to their client grid respective value
        /// It is the responsibility of the derived class to proved a class constructor to initialise the
        /// values NullCells to the correct nu ll values
        /// </summary>
        public static readonly T[,] NullCells = new T[SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension];

        /// <summary>
        /// Represented a T element configured as the null value for cells is this client leaf sub grid
        /// </summary>
        public abstract T NullCell();

        /// <summary>
        /// Constructs a default client sub grid with no owner or parent, at the standard leaf bottom sub grid level,
        /// and using the default cell size and index origin offset
        /// </summary>
        protected GenericClientLeafSubGrid() : base(null, null, SubGridTreeConsts.SubGridTreeLevels, SubGridTreeConsts.DefaultCellSize, SubGridTreeConsts.DefaultIndexOriginOffset)
        {
        }

        /// <summary>
        /// Main constructor. Creates the local generic Items[,] array and delegates to base(...)
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="parent"></param>
        /// <param name="level"></param>
        /// <param name="cellSize"></param>
        /// <param name="indexOriginOffset"></param>
        protected GenericClientLeafSubGrid(ISubGridTree owner, ISubGrid parent, byte level, double cellSize, int indexOriginOffset) : base(owner, parent, level, cellSize, indexOriginOffset)
        {
        }

        /// <summary>
        /// Returns a direct copy of the cells content of this sub grid.
        /// </summary>
        /// <returns></returns>
        public T[,] Clone2DArray()
        {
          var result = new T[SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension];
          Array.Copy(Cells, 0, result, 0, SubGridTreeConsts.SubGridTreeCellsPerSubGrid);
       
          return result;
        }

        /// <summary>
        /// Iterates over all the cells in the leaf sub grid calling functor on each of them.
        /// Both non-null and null values are presented to functor. If functor returns false
        /// the ForEach terminates and returns false.
        /// </summary>
        /// <param name="functor"></param>
        /// <returns></returns>
        public bool ForEach(Func<T, bool> functor)
        {
            for (byte i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
            {
                for (byte j = 0; j < SubGridTreeConsts.SubGridTreeDimension; j++)
                {
                    if (!functor(Cells[i, j]))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Iterates over all the cells in the leaf sub grid calling functor on each of them.
        /// Both non-null and null values are presented to functor.
        /// </summary>
        /// <param name="functor"></param>
        /// <returns></returns>
        public void ForEach(Action<byte, byte, T> functor)
        {
            for (byte i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
            {
                for (byte j = 0; j < SubGridTreeConsts.SubGridTreeDimension; j++)
                {
                    functor(i, j, Cells[i, j]);
                }
            }
        }

        public override void Clear()
        {
          Array.Copy(NullCells, 0, Cells, 0, SubGridTreeConsts.SubGridTreeCellsPerSubGrid);

          // Recreate the array. .Net will initialise the memory used to zero's effecting the clear
          // Cells = new T[SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension];
        }

        /// <summary>
        /// Assign cell information from a previously cached result held in the general sub grid result cache
        /// using the supplied map to control which cells from the caches sub grid should be copied into this
        /// client leaf sub grid
        /// </summary>
        /// <param name="source"></param>
        public override void AssignFromCachedPreProcessedClientSubGrid(ISubGrid source)
        {
          Array.Copy(((GenericClientLeafSubGrid<T>)source).Cells, Cells, SubGridTreeConsts.CellsPerSubGrid);
        }

        /// <summary>
        /// Assign cell information from a previously cached result held in the general sub grid result cache
        /// using the supplied map to control which cells from the caches sub grid should be copied into this
        /// client leaf sub grid
        /// </summary>
        /// <param name="source"></param>
        /// <param name="map"></param>
        public override void AssignFromCachedPreProcessedClientSubGrid(ISubGrid source, SubGridTreeBitmapSubGridBits map)
        {
          if (map.IsFull())
            AssignFromCachedPreProcessedClientSubGrid(source);
          else
          {
            var subGrid = (GenericClientLeafSubGrid<T>)source;
            map.ForEachSetBit((x, y) => Cells[x, y] = subGrid.Cells[x, y]);
          }
        }

        /// <summary>
        /// Dumps the contents of this client leaf sub grid into the log in a human readable form
        /// </summary>
        public override void DumpToLog(string title)
        {
          Log.LogDebug($"Sub grid {Moniker()}: {title}");
        }
    }
}
