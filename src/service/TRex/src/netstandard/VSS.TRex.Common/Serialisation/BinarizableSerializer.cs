using System;
using Apache.Ignite.Core.Binary;
using Force.DeepCloner;
using VSS.TRex.Common.Exceptions;

namespace VSS.TRex.Common.Serialisation
{
  /// <summary>
  /// Provides a class that is registered with Ignite to enforce exclusive of IBinarizable based serialization
  /// </summary>
  public class BinarizableSerializer : IBinarySerializer
  {
    public void WriteBinary(object obj, IBinaryWriter writer)
    {
      if (obj is IBinarizable bin)
      {
        bin.WriteBinary(writer);
        return;
      }

      if (obj is Exception e)
      {
        writer.WriteObject("Exception", e);
        return;
      }

      throw new TRexNonBinarizableException($"Not IBinarizable on WriteBinary: {obj.GetType()}");
    }

    public void ReadBinary(object obj, IBinaryReader reader)
    {
      if (obj is IBinarizable bin)
      {
        bin.ReadBinary(reader);
        return;
      }

      if (obj is Exception e)
      {
        var res = reader.ReadObject<Exception>("Exception");
        res.ShallowCloneTo(e);
        return;
      }

      throw new TRexNonBinarizableException($"Not IBinarizable on ReadBinary: {obj.GetType()}");
    }
  }
}
