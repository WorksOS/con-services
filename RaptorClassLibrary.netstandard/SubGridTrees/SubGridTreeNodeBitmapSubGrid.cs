using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;

namespace VSS.VisionLink.Raptor.SubGridTrees
{
    public class SubGridTreeNodeBitmapSubGrid : NodeSubGrid, INodeSubGrid
    {
      public SubGridTreeBitmapSubGridBits Bits;

        /// <summary>
        /// Writes the contents of the subgrid bit mask to the writer
        /// </summary>
        /// <param name="writer"></param>
        public override void Write(BinaryWriter writer, byte [] buffer)
        {
            Bits.Write(writer, buffer);
        }

        /// <summary>
        /// Reads the contents of the subgrid bit mask from the reader
        /// </summary>
        /// <param name="reader"></param>
        public override void Read(BinaryReader reader, byte [] buffer)
        {
            Bits.Read(reader, buffer);
        }

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public SubGridTreeNodeBitmapSubGrid()
        {
            Bits = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);
        }

        /// <summary>
        /// Constructor taking the tree reference, parent and level of the subgrid to be created
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="parent"></param>
        /// <param name="level"></param>
        public SubGridTreeNodeBitmapSubGrid(ISubGridTree owner,
            SubGrid parent,
            byte level /*,
            double cellSize,
            int indexOriginOffset*/) : base(owner, parent, level /*, cellSize, indexOriginOffset*/)
        {
            Bits = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);
        }

//      Function NonInstanceMemorySize : Integer; Override;
    }
}
