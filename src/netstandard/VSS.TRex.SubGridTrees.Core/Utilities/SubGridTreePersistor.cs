using System;
using System.IO;
using Microsoft.Extensions.Logging;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Utilities.ExtensionMethods;

namespace VSS.TRex.SubGridTrees.Core.Utilities
{
    /// <summary>
    /// SubGridTreePersistor is a helper class that coordinates serialising and deserialising the 
    /// contents of subgrid trees.
    /// </summary>
    public static class SubGridTreePersistor
    {
        private static ILogger Log = Logging.Logger.CreateLogger("SubGridTreePersistor");

        /// <summary>
        /// Serialises all the subgrids in the tree out to the writer
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="writer"></param>
        /// <param name="subGridSerialiser"></param>
        /// <returns></returns>
        static bool SerialiseOut(ISubGridTree tree, BinaryWriter writer, Action<ISubGrid, BinaryWriter> subGridSerialiser)
        {
            long SubgridCount = tree.CountLeafSubgridsInMemory();

            writer.Write(tree.ID.ToByteArray());
            writer.Write(SubgridCount);

            byte[] buffer = new byte[10000];

            return tree.ScanAllSubGrids(subGrid =>
            {
                // Write out the origin for the node
                writer.Write(subGrid.OriginX);
                writer.Write(subGrid.OriginY);

                if (subGridSerialiser != null)
                  subGridSerialiser(subGrid, writer);
                else
                  subGrid.Write(writer, buffer);

                return true; // keep scanning
            });
        }

        /// <summary>
        /// Overloaded Write() method that does not accept a header or version to include into the serialised
        /// stream. Header will be set to String.Empty and version will be set to 0.
        /// This should only be used in contexts where the existence of the stream is transient and never written
        /// to a persistent location that may be sensitive to versioning considerations.
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="writer"></param>
        /// <returns></returns>
        public static bool Write(ISubGridTree tree, BinaryWriter writer) => Write(tree, string.Empty, 0, writer, null);

      /// <summary>
      /// Provides Write() semantics for a subgrid tree against a BinaryWriter
      /// </summary>
      /// <param name="tree"></param>
      /// <param name="header"></param>
      /// <param name="version"></param>
      /// <param name="writer"></param>
      /// <param name="subGridSerialiser"></param>
      /// <returns></returns>
      public static bool Write(ISubGridTree tree, string header, int version, BinaryWriter writer, Action<ISubGrid, BinaryWriter> subGridSerialiser)
        {
            writer.Write(header);
            writer.Write(version);

            // Write place holder for stream size
            long SizePosition = writer.BaseStream.Position;
            writer.Write((long) 0);

            if (!SerialiseOut(tree, writer, subGridSerialiser))
                return false;

            // Write the size of the stream in to the header
            long Size = writer.BaseStream.Position;
            writer.BaseStream.Seek(SizePosition, SeekOrigin.Begin);
            writer.Write(Size);

            return true;
        }

      /// <summary>
      /// Serialises the content of all the subgrids in the subgrid tree from the BinaryReader instance
      /// </summary>
      /// <param name="tree"></param>
      /// <param name="reader"></param>
      /// <returns></returns>
      private static bool SerialiseIn(ISubGridTree tree, BinaryReader reader)
      {
        return SerialiseIn(tree, reader, null);
      }

      /// <summary>
      /// Serialises the content of all the subgrids in the subgrid tree from the BinaryReader instance
      /// </summary>
      /// <param name="tree"></param>
      /// <param name="reader"></param>
      /// <param name="subGridSerialiser"></param>
      /// <returns></returns>
      static bool SerialiseIn(ISubGridTree tree, BinaryReader reader, Action<ISubGrid, BinaryReader> subGridSerialiser)
        {
            try
            {
                tree.ID = reader.ReadGuid();

                // Read in the number of subgrids
                long SubGridCount = reader.ReadInt64();

                byte[] buffer = new byte[10000];

                // Read in each subgrid and add it to the tree
                for (long I = 0; I < SubGridCount; I++)
                {
                    // Read in the the origin for the node
                    uint OriginX = reader.ReadUInt32();
                    uint OriginY = reader.ReadUInt32();

                    // Create a node to hold the bits
                    ISubGrid SubGrid = tree.ConstructPathToCell(OriginX, OriginY, Types.SubGridPathConstructionType.CreateLeaf);

                    if (subGridSerialiser != null)
                      subGridSerialiser(SubGrid, reader);
                    else 
                      SubGrid.Read(reader, buffer);
                }

                return true;
            }
            catch (Exception E)
            {
              Log.LogError($"Exception in {nameof(SerialiseIn)}: {E}");
              return false;
            }
        }

        /// <summary>
        /// Overloaded Read() method that does not accept a header or version to verify in the deserialised
        /// stream. Header will be expected to be String.Empty and version will be expected to be 0.
        /// This should only be used in contexts where the existence of the stream is transient and never read from
        /// a persistent location that may be sensitive to versioning considerations.
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static bool Read(ISubGridTree tree, BinaryReader reader) => Read(tree, string.Empty, 0, reader, null);

      /// <summary>
      /// Provides Read() semantics for a subgrid tree against a BinaryReader
      /// </summary>
      /// <param name="tree"></param>
      /// <param name="header"></param>
      /// <param name="version"></param>
      /// <param name="reader"></param>
      /// <param name="subGridSerialiser"></param>
      /// <returns></returns>
      public static bool Read(ISubGridTree tree, string header, int version, BinaryReader reader, Action<ISubGrid, BinaryReader> subGridSerialiser)
        {
            try
            {
                string Header = reader.ReadString();
                int Version = reader.ReadInt32();
                long Size = reader.ReadInt64();

                if (Header != header || Version != version || Size == 0 || Size != reader.BaseStream.Length)
                {
                  Log.LogError($"Header, version or stream size mismatch reading spatial subgrid index. Header={Header} (expected {header}), Version={Version} (expected {version}), Size={reader.BaseStream.Length} (expected {Size})");
                  return false;
                }

                return SerialiseIn(tree, reader, subGridSerialiser);
            }
            catch (Exception E)
            {
              Log.LogError($"Exception in {nameof(Read)}: {E}");
              return false;
            }
        }
    }
}
