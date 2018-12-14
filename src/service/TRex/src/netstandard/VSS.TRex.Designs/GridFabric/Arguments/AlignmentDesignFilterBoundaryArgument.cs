using Apache.Ignite.Core.Binary;
using VSS.TRex.Common.Exceptions;

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
      writer.WriteByte(VERSION_NUMBER);

      writer.WriteDouble(StartStation);
      writer.WriteDouble(EndStation);
      writer.WriteDouble(LeftOffset);
      writer.WriteDouble(RightOffset);
    }

    public override void FromBinary(IBinaryRawReader reader)
    {
      byte version = reader.ReadByte();

      if (version != VERSION_NUMBER)
        throw new TRexSerializationVersionException(VERSION_NUMBER, version);

      StartStation = reader.ReadDouble();
      EndStation = reader.ReadDouble();
      LeftOffset = reader.ReadDouble();
      RightOffset = reader.ReadDouble();
    }
  }
}
