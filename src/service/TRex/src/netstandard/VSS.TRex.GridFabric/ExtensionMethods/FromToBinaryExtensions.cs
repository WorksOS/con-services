using System.Collections.Generic;
using System.Diagnostics;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Geometry;

namespace VSS.TRex.GridFabric.ExtensionMethods
{
  /// <summary>
  /// Provides extension methods supporting IBinarizable style Ignite grid serialisation for a collection of common object in TRex
  /// </summary>
  public static class FromToBinary
  {
    /// <summary>
    /// An extension method providing a ToBinary() semantic to Fence
    /// </summary>
    public static void ToBinary(this Fence item, IBinaryRawWriter writer)
    {
      const byte versionNumber = 1;

      writer.WriteByte(versionNumber);

      writer.WriteInt(item.NumVertices);
      foreach (var point in item.Points)
      {
        writer.WriteDouble(point.X);
        writer.WriteDouble(point.Y);
        writer.WriteDouble(point.Z);
      }
    }

    /// <summary>
    /// An extension method providing a FromBinary() semantic to Fence
    /// </summary>
    public static void FromBinary(this Fence item, IBinaryRawReader reader)
    {
      const byte versionNumber = 1;
      byte readVersionNumber = reader.ReadByte();

      Debug.Assert(readVersionNumber == versionNumber, $"Invalid version number: {readVersionNumber}, expecting {versionNumber}");

      item.Points = new List<FencePoint>(reader.ReadInt());
      for (int i = 0; i < item.Points.Capacity; i++)
         item.Points.Add(new FencePoint(reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble()));
    }

    /// <summary>
    /// An extension method providing a ToBinary() semantic to BoundingWorldExtent3D
    /// </summary>
    public static void ToBinary(this BoundingWorldExtent3D item, IBinaryRawWriter writer)
    {
      const byte versionNumber = 1;

      writer.WriteByte(versionNumber);

      writer.WriteDouble(item.MinX);
      writer.WriteDouble(item.MinY);
      writer.WriteDouble(item.MinZ);
      writer.WriteDouble(item.MaxX);
      writer.WriteDouble(item.MaxY);
      writer.WriteDouble(item.MaxZ);
    }

    /// <summary>
    /// An extension method providing a ToBinary() semantic to BoundingWorldExtent3D
    /// </summary>
    public static void FromBinary(this BoundingWorldExtent3D item, IBinaryRawReader reader)
    {
      const byte versionNumber = 1;
      byte readVersionNumber = reader.ReadByte();

      Debug.Assert(readVersionNumber == versionNumber, $"Invalid version number: {readVersionNumber}, expecting {versionNumber}");

      item.MinX = reader.ReadDouble();
      item.MinY = reader.ReadDouble();
      item.MinZ = reader.ReadDouble();
      item.MaxX = reader.ReadDouble();
      item.MaxY = reader.ReadDouble();
      item.MaxZ = reader.ReadDouble();
    }

    /// <summary>
    /// An extension method providing a ToBinary() semantic to BoundingIntegerExtent2D
    /// </summary>
    public static void ToBinary(this BoundingIntegerExtent2D item, IBinaryRawWriter writer)
    {
      const byte versionNumber = 1;

      writer.WriteByte(versionNumber);

      writer.WriteInt(item.MinX);
      writer.WriteInt(item.MinY);
      writer.WriteInt(item.MaxX);
      writer.WriteInt(item.MaxY);
    }

    /// <summary>
    /// An extension method providing a ToBinary() semantic to BoundingIntegerExtent2D
    /// </summary>
    public static BoundingIntegerExtent2D FromBinary(this BoundingIntegerExtent2D item, IBinaryRawReader reader)
    {
      const byte versionNumber = 1;
      byte readVersionNumber = reader.ReadByte();

      Debug.Assert(readVersionNumber == versionNumber, $"Invalid version number: {readVersionNumber}, expecting {versionNumber}");

      item.MinX = reader.ReadInt();
      item.MinY = reader.ReadInt();
      item.MaxX = reader.ReadInt();
      item.MaxY = reader.ReadInt();

      return item;
    }

    /// <summary>
    /// An extension method providing a ToBinary() semantic to XYZ
    /// </summary>
    public static void ToBinary(this XYZ item, IBinaryRawWriter writer)
    {
      const byte versionNumber = 1;

      writer.WriteByte(versionNumber);

      writer.WriteDouble(item.X);
      writer.WriteDouble(item.Y);
      writer.WriteDouble(item.Z);
    }

    /// <summary>
    /// An extension method providing a FromBinary() semantic to XYZ
    /// </summary>
    public static XYZ FromBinary(this XYZ item, IBinaryRawReader reader)
    {
      const byte versionNumber = 1;
      byte readVersionNumber = reader.ReadByte();

      Debug.Assert(readVersionNumber == versionNumber, $"Invalid version number: {readVersionNumber}, expecting {versionNumber}");

      item.X = reader.ReadDouble();
      item.Y = reader.ReadDouble();
      item.Z = reader.ReadDouble();

      return item;
    }
  }
}
