using Apache.Ignite.Core.Binary;
using VSS.TRex.Common.Interfaces;

namespace VSS.TRex.GridFabric.Arguments
{
  /// <summary>
  /// Defines a base class implementation of the Ignite binarizable serialization. It also defines a separate
  /// set of methods from IFromToBinary based on the Ignite raw read/write serialisation as the preferred performant serialisation
  /// approach
  /// </summary>
  public abstract class BaseRequestArgument : IBinarizable, IFromToBinary
  {
    public abstract void ToBinary(IBinaryRawWriter writer);

    public abstract void FromBinary(IBinaryRawReader reader);

    /// <summary>
    /// Implements the Ignite IBinarizable.WriteBinary interface Ignite will call to serialise this object.
    /// </summary>
    /// <param name="writer"></param>
    public void WriteBinary(IBinaryWriter writer) => ToBinary(writer.GetRawWriter());

    /// <summary>
    /// Implements the Ignite IBinarizable.ReadBinary interface Ignite will call to serialise this object.
    /// </summary>
    public void ReadBinary(IBinaryReader reader) => FromBinary(reader.GetRawReader());
  }
}
