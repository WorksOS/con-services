using Apache.Ignite.Core.Binary;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Common.Interfaces;

namespace VSS.TRex.GridFabric.ComputeFuncs
{
  /// <summary>
  /// The base class for compute functions. This provides common aspects such as the injected Ignite instance
  /// </summary>
  public abstract class BaseComputeFunc : IBinarizable, IFromToBinary
  {
    private const byte kVersionNumber = 1;

    public BaseComputeFunc()
    {
    }

    /// <summary>
    /// By convention in TRex, compute functions derive their state from the supplied argument to
    /// their Invoke() method. State derived from BaseIgniteClass is intended to allow the representation
    /// of the compute function on the invoking side the ability to target appropriate grid resources
    /// for execution of the function. Thus, IBinarizable serialization for base compute func in TRex is a
    /// null function.
    /// </summary>
    /// <param name="writer"></param>
    public void WriteBinary(IBinaryWriter writer) => ToBinary(writer.GetRawWriter());

    /// <summary>
    /// By convention in TRex, compute functions derive their state from the supplied argument to
    /// their Invoke() method. State derived from BaseIgniteClass is intended to allow the representation
    /// of the compute function on the invoking side the ability to target appropriate grid resources
    /// for execution of the function. Thus, IBinarizable serialization for base compute func in TRex is a
    /// null function.
    /// </summary>
    /// <param name="reader"></param>
    public void ReadBinary(IBinaryReader reader) => FromBinary(reader.GetRawReader());

    public void ToBinary(IBinaryRawWriter writer)
    {
      // Version the serialization, even if nothing additional is serialized
      writer.WriteByte(kVersionNumber);
    }

    public void FromBinary(IBinaryRawReader reader)
    {
      // Version the serialization, even if nothing additional is serialized
      var version = reader.ReadByte();

      if (version != kVersionNumber)
        throw new TRexSerializationVersionException(kVersionNumber, version);
    }
  }
}
