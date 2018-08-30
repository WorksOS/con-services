using System;
using System.IO;
using VSS.TRex.Designs.TTM.Optimised.Exceptions;

namespace VSS.TRex.Designs.TTM.Optimised
{
  /// <summary>
  /// Contains the set of triangles that form the edge of the TIN. Note, in the optimised model, while these are read
  /// neighbour information is not meaning there is not understanding of which side of the triangle is the edge
  /// </summary>
  public class TTMEdges
  {
    /// <summary>
    /// The collection of edge triangles
    /// </summary>
    public int[] Items;

    /// <summary>
    /// Reads in the collection of esged from the TIN model usign the provided reader
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="header"></param>
    public void Read(BinaryReader reader, TTMHeader header)
    {
      Items = new int[header.NumberOfEdgeRecords];

      try
      {
        int loopLimit = header.NumberOfEdgeRecords;
        for (int i = 0; i < loopLimit; i++)
        {
          long RecPos = reader.BaseStream.Position;
          Items[i] = Utilities.ReadInteger(reader, header.TriangleNumberSize) - 1;
          reader.BaseStream.Position = RecPos + header.EdgeRecordSize;
        }
      }
      catch (Exception E)
      {
        throw new TTMFileReadException($"Failed to read edges", E);
      }
    }
  }
}
