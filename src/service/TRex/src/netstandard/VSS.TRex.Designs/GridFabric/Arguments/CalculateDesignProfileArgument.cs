using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.ExtensionMethods;

namespace VSS.TRex.Designs.GridFabric.Arguments
{
  public class CalculateDesignProfileArgument : BaseApplicationServiceRequestArgument
  {
    private const byte VERSION_NUMBER = 1;

    /// <summary>
    /// The path along which the profile will be calculated
    /// </summary>
    public XYZ[] ProfilePath { get; set; } = new XYZ[0];

    /// <summary>
    /// The cell stepping size to move between points in the patch being interpolated
    /// </summary>
    public double CellSize { get; set; }

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public CalculateDesignProfileArgument()
    {
    }

    /// <summary>
    /// Constructor taking the full state of the elevation patch computation operation
    /// </summary>
    /// <param name="projectUid"></param>
    /// <param name="cellSize"></param>
    /// <param name="designUid"></param>
    /// <param name="offset"></param>
    /// <param name="profilePath"></param>
    // /// <param name="processingMap"></param>
    public CalculateDesignProfileArgument(Guid projectUid,
                                          double cellSize,
                                          Guid designUid,
                                          double offset,
                                          XYZ[] profilePath) : this()
    {
      ProjectID = projectUid;
      CellSize = cellSize;
      ReferenceDesignUID = designUid;
      ReferenceOffset = offset;
      ProfilePath = profilePath;
    }

    /// <summary>
    /// Overloaded ToString to add argument properties
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return base.ToString() + $" -> ProjectUID:{ProjectID}, CellSize:{CellSize}, Design:{ReferenceDesignUID}, Offset: {ReferenceOffset}, {ProfilePath.Length} vertices";
    }

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteDouble(CellSize);

      writer.WriteInt(ProfilePath.Length);
      foreach (var pt in ProfilePath)
      {
        pt.ToBinaryUnversioned(writer);
      }
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      CellSize = reader.ReadDouble();

      var count = reader.ReadInt();

      ProfilePath = new XYZ[count];
      for (int i = 0; i < count; i++)
      {       
        ProfilePath[i] = ProfilePath[i].FromBinaryUnversioned(reader);
      }
    }
  }
}
