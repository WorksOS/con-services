using Apache.Ignite.Core.Binary;

namespace VSS.TRex.Common.Interfaces
{
  public interface IVersionCheckedBinarizableSerializationBase
  {
//    byte[] GetVersionNumbers();

    void InternalToBinary(IBinaryRawWriter writer);
    void InternalFromBinary(IBinaryRawReader reader);
  }
}
