using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.ExtensionMethods;

namespace VSS.TRex.Designs.GridFabric.Arguments
{
  public class CalculateDesignProfileArgument_ClusterCompute : BaseApplicationServiceRequestArgument
  {
    private const byte VERSION_NUMBER = 1;

    /// <summary>
    /// The path along which the profile will be calculated
    /// </summary>
    public XYZ[] ProfilePathNEE { get; set; } = new XYZ[0];


    /// <summary>
    /// The cell stepping size to move between points in the patch being interpolated
    /// </summary>
    public double CellSize { get; set; }

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public CalculateDesignProfileArgument_ClusterCompute()
    {
    }


    /// <summary>
    /// Overloaded ToString to add argument properties
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return base.ToString() + $" -> ProjectUID:{ProjectID}, CellSize:{CellSize}, Design:{ReferenceDesign?.DesignID}, Offset: {ReferenceDesign?.Offset}, ProfilePathNEE: {string.Concat(ProfilePathNEE)}";
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

      var count = ProfilePathNEE?.Length ?? 0;
      writer.WriteInt(count);
      for (int i = 0; i < count; i++)
        ProfilePathNEE[i].ToBinary(writer);


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
      ProfilePathNEE = new XYZ[count];
      for (int i = 0; i < count; i++)
        ProfilePathNEE[i] = ProfilePathNEE[i].FromBinary(reader);

    }
  }
}
