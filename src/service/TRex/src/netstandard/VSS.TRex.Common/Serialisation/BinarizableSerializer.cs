using System;
using Apache.Ignite.Core.Binary;

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

      throw new Exception($"Not IBinarizable on WriteBinary: {obj.GetType()}");
    }

    public void ReadBinary(object obj, IBinaryReader reader)
    {
      if (obj is IBinarizable bin)
      {
        bin.ReadBinary(reader);
        return;
      }

      throw new Exception($"Not IBinarizable on ReadBinary: {obj.GetType()}");
    }
  }
}
