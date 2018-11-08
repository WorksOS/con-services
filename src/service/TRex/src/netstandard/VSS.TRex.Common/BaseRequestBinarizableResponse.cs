using Apache.Ignite.Core.Binary;
using VSS.TRex.Common.Interfaces;

namespace VSS.TRex.Common
{
  public abstract class BaseRequestBinarizableResponse : IBinarizable, IFromToBinary
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
