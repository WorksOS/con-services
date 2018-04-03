using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;

namespace VSS.VisionLink.Raptor.SubGridTrees
{
    /// <summary>
    /// GenericLeafSubGrid<T> implements a leaf subgrid where all the cells in the leaf are generic type T.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GenericLeafSubGrid<T> : SubGrid, ILeafSubGrid
    {
        public T[,] Items;

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public GenericLeafSubGrid()
        {
            Clear();
        }

        /// <summary>
        /// Main constructor. Creates the lcoal generic Items[,] array and delegates to base(...)
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="parent"></param>
        /// <param name="level"></param>
        public GenericLeafSubGrid(ISubGridTree owner, ISubGrid parent, byte level) : base(owner, parent, level)
        {
            Clear();
        }

        /// <summary>
        /// Iterates over all the cells in the leaf subgrid calling functor on each of them.
        /// Both non-null and null values are presented to functor.
        /// </summary>
        /// <param name="functor"></param>
        /// <returns></returns>
        public bool ForEach(Func<T, bool> functor)
        {
            for (byte I = 0; I < SubGridTree.SubGridTreeDimension; I++)
            {
                for (byte J = 0; J < SubGridTree.SubGridTreeDimension; J++)
                {
                    if (!functor(Items[I, J]))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public override void Clear()
        {
            // Recreate the array. .Net will initialise the memory used to zero's effecting the clear
            Items = new T[SubGridTree.SubGridTreeDimension, SubGridTree.SubGridTreeDimension];
        }

        /// <summary>
        /// Write the contents of the Items array using the supplied writer
        /// This is an unimplemented override; a generic BinaryReader based implementation is not provided. 
        /// Override to implement if needed.
        /// </summary>
        /// <param name="writer"></param>
        public override void Write(BinaryWriter writer, byte [] buffer)
        {
            throw new Exception("Generic BinaryWriter based implementation not provided. Override to implement if needed.");
        }

        /// <summary>
        /// Fill the items array by reading the binary representation using the provided reader. 
        /// This is an unimplemented override; a generic BinaryReader based implementation is not provided. 
        /// Override to implement if needed.
        /// </summary>
        /// <param name="reader"></param>
        public override void Read(BinaryReader reader, byte[] buffer)
        {
            throw new Exception ("Generic BinaryReader based implementation not provided. Override to implement if needed.");
        }

        /// <summary>
        /// Write the contents of the Items array using the supplied writer
        /// </summary>
        /// <param name="formatter"></param>
        /// <param name="stream"></param>
        public override void Write(BinaryFormatter formatter, Stream stream)
        {
            formatter.Serialize(stream, Items);
        }

        /// <summary>
        /// Fill the items array by reading the binary representation using the provided formatter
        /// </summary>
        /// <param name="formatter"></param>
        /// <param name="stream"></param>
        public override void Read(BinaryFormatter formatter, Stream stream)
        {
            Items = (T[,])formatter.Deserialize(stream);
        }

    }
}
