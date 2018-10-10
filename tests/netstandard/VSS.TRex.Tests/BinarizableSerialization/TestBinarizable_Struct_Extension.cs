using Apache.Ignite.Core.Binary;
using VSS.TRex.GridFabric.ExtensionMethods;

namespace VSS.TRex.Tests.BinarizableSerialization
{
  public class TestBinarizable_Struct_Extension<T> : IBinarizable where T : struct
  {
    public T member;

    public TestBinarizable_Struct_Extension()
    {
      member = new T();
    }

    public void WriteBinary(IBinaryWriter writer)
    {
      var method = typeof(FromToBinary).GetMethod("ToBinary", new[] { typeof(T), typeof(IBinaryRawWriter) });
      method.Invoke(null, new object[] { member, writer.GetRawWriter() });
    }

    public void ReadBinary(IBinaryReader reader)
    {
      var method = typeof(FromToBinary).GetMethod("FromBinary", new[] { typeof(T), typeof(IBinaryRawReader) });

      member = new T();
      member = (T)method.Invoke(null, new object[] { member, reader.GetRawReader() });
    }
  }
}
