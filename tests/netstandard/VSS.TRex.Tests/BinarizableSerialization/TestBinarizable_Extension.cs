using Apache.Ignite.Core.Binary;
using VSS.TRex.GridFabric.ExtensionMethods;

namespace VSS.TRex.Tests.BinarizableSerialization
{
  /// <summary>
  /// Provides a wrapper class to hold a member that implements a type that has IBinarizable serialisation supported through the
  /// FromToBytes extension methods rather than by directly implementing hte IBinarizable interface
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class TestBinarizable_Extension<T> : IBinarizable where T : class, new()
  {
    public T member;

    public TestBinarizable_Extension()
    {
      member = new T();
    }

    public void WriteBinary(IBinaryWriter writer)
    {
      var method = typeof(FromToBinary).GetMethod("ToBinary", new[] {typeof(T), typeof(IBinaryRawWriter)});
      method.Invoke(null, new object[] {member, writer.GetRawWriter()});
    }

    public void ReadBinary(IBinaryReader reader)
    {
      var method = typeof(FromToBinary).GetMethod("FromBinary", new[] {typeof(T), typeof(IBinaryRawReader)});

      method.Invoke(null, new object[] { member ?? (member = new T()), reader.GetRawReader()});
    }
  }

}
