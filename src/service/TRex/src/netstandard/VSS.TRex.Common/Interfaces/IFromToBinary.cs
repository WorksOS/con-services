using Apache.Ignite.Core.Binary;

namespace VSS.TRex.Common.Interfaces
{
  /// <summary>
  /// Defines a 'ToFromBinary' interface defining reader and writer methods for classes implementing IBinarizable
  /// serialization in the TRex Ignite grid
  /// </summary>
  public interface IFromToBinary
  {
    void ToBinary(IBinaryRawWriter writer);
    void FromBinary(IBinaryRawReader reader);
  }
}
