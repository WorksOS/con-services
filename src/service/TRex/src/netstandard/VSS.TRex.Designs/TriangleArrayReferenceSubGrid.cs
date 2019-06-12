using System.IO;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Core.Utilities;

namespace VSS.TRex.Designs
{
  public class TriangleArrayReferenceSubGrid : GenericLeafSubGrid<TriangleArrayReference>, ITriangleArrayReferenceSubGrid
  {
    /// <summary>
    /// Write the contents of the Items array using the supplied writer
    /// </summary>
    /// <param name="writer"></param>
    public override void Write(BinaryWriter writer)
    {
      SubGridUtilities.SubGridDimensionalIterator((x, y) =>
      {
        writer.Write(Items[x, y].Count);
        writer.Write(Items[x, y].TriangleArrayIndex);
      });
    }

    /// <summary>
    /// Fill the items array by reading the binary representation using the provided reader. 
    /// This is an unimplemented override; a generic BinaryReader based implementation is not provided. 
    /// Override to implement if needed.
    /// </summary>
    /// <param name="reader"></param>
    public override void Read(BinaryReader reader)
    {
      TriangleArrayReference arrayReference = new TriangleArrayReference();

      SubGridUtilities.SubGridDimensionalIterator((x, y) =>
      {
        arrayReference.Count = reader.ReadInt16();
        arrayReference.TriangleArrayIndex = reader.ReadInt32();
        Items[x, y] = arrayReference;
      });
    }
  }


}
