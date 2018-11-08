using System.IO;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.SubGridTrees.Core
{
    public class SubGridTreeNodeBitmapSubGrid : NodeSubGrid
    {
        //Node level bit aggregation currently not required.
        //public SubGridTreeBitmapSubGridBits _Bits;

        /// <summary>
        /// Writes the contents of the subgrid bit mask to the writer
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="buffer"></param>
        public override void Write(BinaryWriter writer, byte [] buffer)
        {
          base.Write(writer, buffer);
          //Node level bit aggregation currently not required.
          //Bits.Write(writer, buffer);
        }

        /// <summary>
        /// Reads the contents of the subgrid bit mask from the reader
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="buffer"></param>
        public override void Read(BinaryReader reader, byte [] buffer)
        {
          base.Read(reader, buffer);
          //Node level bit aggregation currently not required.
          //Bits.Read(reader, buffer);
        }

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public SubGridTreeNodeBitmapSubGrid()
        {
          //Node level bit aggregation currently not required.
          //Bits = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);
        }

        /// <summary>
        /// Constructor taking the tree reference, parent and level of the subgrid to be created
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="parent"></param>
        /// <param name="level"></param>
        public SubGridTreeNodeBitmapSubGrid(ISubGridTree owner, ISubGrid parent, byte level) : base(owner, parent, level)
        {
          //Node level bit aggregation currently not required.
          //Bits = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);
        }
    }
}
