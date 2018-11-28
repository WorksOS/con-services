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

      if (obj is Exception)
      {
        writer.WriteObject("Exception", obj);
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

      if (obj is Exception)
      {
        var res = reader.ReadObject<object>("Exception");
        res.ShallowCloneTo(obj);
        return;
      }

      throw new TRexNonBinarizableException($"Not IBinarizable on ReadBinary: {obj.GetType()}");
    }
  }
}
