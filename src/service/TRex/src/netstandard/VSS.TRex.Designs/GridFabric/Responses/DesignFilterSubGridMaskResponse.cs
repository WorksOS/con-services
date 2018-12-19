using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.SubGridTrees;

namespace VSS.TRex.Designs.GridFabric.Responses
{
  public class DesignFilterSubGridMaskResponse : BaseDesignRequestResponse
  {
    private const byte VERSION_NUMBER = 1;

    public SubGridTreeBitmapSubGridBits Bits = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

    public override void ToBinary(IBinaryRawWriter writer)
    {
      writer.WriteByte(VERSION_NUMBER);

      writer.WriteArray(Bits.Bits);
    }

    public override void FromBinary(IBinaryRawReader reader)
    {
      byte version = reader.ReadByte();

      if (version != VERSION_NUMBER)
        throw new TRexSerializationVersionException(VERSION_NUMBER, version);

      Bits.Bits = reader.ReadArray<uint>();
    }
  }
}
