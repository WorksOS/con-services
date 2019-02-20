using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Common.Extensions;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Designs.GridFabric.Responses
{
  public class DesignFilterSubGridMaskResponse : BaseDesignRequestResponse
  {
    private const byte VERSION_NUMBER = 1;

    public SubGridTreeBitmapSubGridBits Bits = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

    public override void ToBinary(IBinaryRawWriter writer)
    {
      writer.WriteByte(VERSION_NUMBER);

      int[] buffer = new int[SubGridTreeConsts.SubGridTreeDimension];
      for (int i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
        buffer[i] = unchecked((int) Bits.Bits[i]);

      writer.WriteIntArray(buffer);
    }

    public override void FromBinary(IBinaryRawReader reader)
    {
      byte version = reader.ReadByte();

      if (version != VERSION_NUMBER)
        throw new TRexSerializationVersionException(VERSION_NUMBER, version);

      int[] buffer = reader.ReadIntArray();
      for (int i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
        Bits.Bits[i] = unchecked((uint)buffer[i]);
    }
  }
}
