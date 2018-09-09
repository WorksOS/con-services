using System;
using System.Diagnostics;
using System.IO;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.SubGridTrees
{
    /// <summary>
    /// GenericLeafSubGrid in T implements a leaf subgrid where all the cells in the leaf are generic type T.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GenericLeafSubGrid<T> : SubGrid, ILeafSubGrid
    {
        public T[,] Items = new T[SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension];

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public GenericLeafSubGrid()
        {
        }
    
        /// <summary>
        /// Creates a new generic leaf and instantiates its content with the provided cell array
        /// </summary>
        public GenericLeafSubGrid(T[,] items)
        {
          Items = items;
        }

        /// <summary>
        /// Main constructor. Creates the lcoal generic Items[,] array and delegates to base(...)
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="parent"></param>
        /// <param name="level"></param>
        public GenericLeafSubGrid(ISubGridTree owner, ISubGrid parent, byte level) : base(owner, parent, level)
        {
        }

        /// <summary>
        /// Iterates over all the cells in the leaf subgrid calling functor on each of them.
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

        public override void Clear()
        {
        }

        /// <summary>
        /// Write the contents of the Items array using the supplied writer
        /// This is an unimplemented override; a generic BinaryReader based implementation is not provided. 
        /// Override to implement if needed.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="buffer"></param>
        public override void Write(BinaryWriter writer, byte [] buffer)
        {
            Debug.Assert(false, "Generic BinaryWriter based implementation not provided. Override to implement if needed.");
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
          Debug.Assert(false, "Generic BinaryReader based implementation not provided. Override to implement if needed.");
        }
    }
}
