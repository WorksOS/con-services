using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.GridFabric.Arguments;

namespace VSS.TRex.Designs.GridFabric.Arguments
{
  public class AlignmentDesignFilterBoundaryArgument : DesignSubGridRequestArgumentBase
  {
    private const byte VERSION_NUMBER = 1;

    public double StartStation { get; set; }
    public double EndStation { get; set; }
    public double LeftOffset { get; set; }
    public double RightOffset { get; set; }


    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteDouble(StartStation);
      writer.WriteDouble(EndStation);
      writer.WriteDouble(LeftOffset);
      writer.WriteDouble(RightOffset);
    }

    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      StartStation = reader.ReadDouble();
      EndStation = reader.ReadDouble();
      LeftOffset = reader.ReadDouble();
      RightOffset = reader.ReadDouble();
    }
  }
}
