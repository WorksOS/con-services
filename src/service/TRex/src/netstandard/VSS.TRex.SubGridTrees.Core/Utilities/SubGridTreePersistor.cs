using System.IO;
using Microsoft.Extensions.Logging;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Common.Utilities.ExtensionMethods;

namespace VSS.TRex.SubGridTrees.Core.Utilities
{
  /// <summary>
  /// SubGridTreePersistor is a helper class that coordinates serializing and deserializing the 
  /// contents of sub grid trees.
  /// </summary>
  public static class SubGridTreePersistor
  {
    private static ILogger Log = Logging.Logger.CreateLogger("SubGridTreePersistor");

    /// <summary>
    /// Serializes all the sub grids in the tree out to the writer
    /// </summary>
    /// <param name="tree"></param>
    /// <param name="writer"></param>
    /// <returns></returns>
    static bool SerializeOut(ISubGridTree tree, BinaryWriter writer)
    {
      long SubGridCount = tree.CountLeafSubGridsInMemory();

      writer.Write(tree.ID.ToByteArray());
      writer.Write(SubGridCount);

      byte[] buffer = new byte[10000];

      return tree.ScanAllSubGrids(subGrid =>
      {
        // Write out the origin for the node
        writer.Write(subGrid.OriginX);
        writer.Write(subGrid.OriginY);

        subGrid.Write(writer, buffer);

        return true; // keep scanning
      });
    }

    /// <summary>
    /// Overloaded Write() method that does not accept a header or version to include into the serialized
    /// stream. Header will be set to string.Empty and version will be set to 0.
    /// This should only be used in contexts where the existence of the stream is transient and never written
    /// to a persistent location that may be sensitive to version considerations.
    /// </summary>
    /// <param name="tree"></param>
    /// <param name="writer"></param>
    /// <returns></returns>
    public static bool Write(ISubGridTree tree, BinaryWriter writer) => Write(tree, string.Empty, 0, writer);

    /// <summary>
    /// Provides Write() semantics for a sub grid tree against a BinaryWriter
    /// </summary>
    /// <param name="tree"></param>
    /// <param name="header"></param>
    /// <param name="version"></param>
    /// <param name="writer"></param>
    /// <returns></returns>
    public static bool Write(ISubGridTree tree, string header, int version, BinaryWriter writer)
    {
      writer.Write(header);
      writer.Write(version);

      // Write place holder for stream size
      long SizePosition = writer.BaseStream.Position;
      writer.Write(0L);

      bool serializationResult = SerializeOut(tree, writer);

      if (serializationResult)
      {
        // Write the size of the stream in to the header
        long Size = writer.BaseStream.Position;
        writer.BaseStream.Seek(SizePosition, SeekOrigin.Begin);
        writer.Write(Size);
      }

      return serializationResult;
    }

    /// <summary>
    /// Serializes the content of all the sub grids in the sub grid tree from the BinaryReader instance
    /// </summary>
    /// <param name="tree"></param>
    /// <param name="reader"></param>
    /// <returns></returns>
    static bool SerialiseIn(ISubGridTree tree, BinaryReader reader)
    {
      tree.ID = reader.ReadGuid();

      // Read in the number of sub grids
      long SubGridCount = reader.ReadInt64();

      byte[] buffer = new byte[10000];

      // Read in each sub grid and add it to the tree
      for (long I = 0; I < SubGridCount; I++)
      {
        // Read in the the origin for the node
        uint OriginX = reader.ReadUInt32();
        uint OriginY = reader.ReadUInt32();

        // Create a node to hold the bits
        ISubGrid SubGrid = tree.ConstructPathToCell(OriginX, OriginY, Types.SubGridPathConstructionType.CreateLeaf);
        SubGrid.Read(reader, buffer);
      }

      return true;
    }

    /// <summary>
    /// Overloaded Read() method that does not accept a header or version to verify in the deserialized
    /// stream. Header will be expected to be string.Empty and version will be expected to be 0.
    /// This should only be used in contexts where the existence of the stream is transient and never read from
    /// a persistent location that may be sensitive to version considerations.
    /// </summary>
    /// <param name="tree"></param>
    /// <param name="reader"></param>
    /// <returns></returns>
    public static bool Read(ISubGridTree tree, BinaryReader reader) => Read(tree, string.Empty, 0, reader);

    /// <summary>
    /// Provides Read() semantics for a sub grid tree against a BinaryReader
    /// </summary>
    /// <param name="tree"></param>
    /// <param name="header"></param>
    /// <param name="version"></param>
    /// <param name="reader"></param>
    /// <returns></returns>
    public static bool Read(ISubGridTree tree, string header, int version, BinaryReader reader)
    {
      string Header = reader.ReadString();
      int Version = reader.ReadInt32();
      long Size = reader.ReadInt64();

      if (Header != header || Version != version || Size == 0 || Size != reader.BaseStream.Length)
      {
        Log.LogError($"Header, version or stream size mismatch reading spatial sub grid index. Header={Header} (expected {header}), Version={Version} (expected {version}), Size={reader.BaseStream.Length} (expected {Size})");
        return false;
      }

      return SerialiseIn(tree, reader);
    }
  }
}
