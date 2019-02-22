using Apache.Ignite.Core.Binary;
using VSS.TRex.Common.Interfaces;

namespace VSS.TRex.Tests.BinarizableSerialization
{
  /// <summary>
  /// Provides a wrapper class to hold a member that implements a type that has IFromToBinary only serialisation supported
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class TestFromToBinary_Class<T> where T : class, IFromToBinary, new()
  {
    public T member;

    public TestFromToBinary_Class()
    {
      member = new T();
    }

    public void WriteBinary(IBinaryWriter writer)
    {
      member.ToBinary(writer.GetRawWriter());
    }

    public void ReadBinary(IBinaryReader reader)
    {
      (member ?? (member = new T())).FromBinary(reader.GetRawReader());
    }
  }
}
