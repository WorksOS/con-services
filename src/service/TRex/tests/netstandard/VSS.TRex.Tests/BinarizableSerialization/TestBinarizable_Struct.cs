using Apache.Ignite.Core.Binary;
using VSS.TRex.Common.Interfaces;

namespace VSS.TRex.Tests.BinarizableSerialization
{
  /// <summary>
  /// Provides a wrapper class to hold a member that implements a type that has IBinarizable serialisation supported through the
  /// IBinarizable interface
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class TestBinarizable_Struct<T> : IBinarizable where T : struct, IFromToBinary
  {
    public T member;

    public TestBinarizable_Struct()
    {
      member = new T();
    }

    public void WriteBinary(IBinaryWriter writer)
    {
      member.ToBinary(writer.GetRawWriter());
    }

    public void ReadBinary(IBinaryReader reader)
    {
      member.FromBinary(reader.GetRawReader());
    }
  }
}
