using Apache.Ignite.Core.Binary;

namespace VSS.TRex.GridFabric.Interfaces
{
  /// <summary>
  /// Defines a 'ToFromBinary' interface defining reader and writer methods for classes implementing IBinarizable
  /// serialization in the TRex Ignite grid
  /// </summary>
  public interface IToFromBinary
  {
    void ToBinary(IBinaryRawWriter writer);
    void FromBinary(IBinaryRawReader reader);
  }
}
