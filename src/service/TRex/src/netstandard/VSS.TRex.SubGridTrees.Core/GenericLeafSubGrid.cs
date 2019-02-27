using System;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.SubGridTrees
{
    /// <summary>
    /// GenericLeafSubGrid in T implements a leaf sub grid where all the cells in the leaf are generic type T.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GenericLeafSubGrid<T> : LeafSubGridBase, IGenericLeafSubGrid<T>
    {
        public T[,] Items { get; set; }
    
        private void AllocateItems() => Items = new T[SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension];
        
        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public GenericLeafSubGrid()
        {
          AllocateItems();
        }
    
        /// <summary>
        /// Creates a new generic leaf and instantiates its content with the provided cell array
        /// </summary>
        public GenericLeafSubGrid(T[,] items)
        {
          Items = items;
        }

        /// <summary>
        /// Main constructor. Creates the local generic Items[,] array and delegates to base(...)
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="parent"></param>
        /// <param name="level"></param>
        public GenericLeafSubGrid(ISubGridTree owner, ISubGrid parent, byte level) : base(owner, parent, level)
        {
          AllocateItems();
        }

        /// <summary>
        /// Iterates over all the cells in the leaf sub grid calling functor on each of them.
        /// Both non-null and null values are presented to functor.
        /// </summary>
        /// <param name="functor"></param>
        /// <returns></returns>
        public bool ForEach(Func<T, bool> functor)
        {
            for (byte I = 0; I < SubGridTreeConsts.SubGridTreeDimension; I++)
            {
                for (byte J = 0; J < SubGridTreeConsts.SubGridTreeDimension; J++)
                {
                    if (!functor(Items[I, J]))
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// By default the cells in a generic leaf sub grid are said to contain values
        /// </summary>
        /// <param name="cellX"></param>
        /// <param name="cellY"></param>
        /// <returns></returns>
        public override bool CellHasValue(byte cellX, byte cellY) => true;

        /// <summary>
        /// Clear the generic items by setting all cell values to the default generic value
        /// </summary>
        public override void Clear() => ForEach((x, y) => Items[x, y] = default(T));
    }
}
