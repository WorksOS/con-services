using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Common.Interfaces;

namespace VSS.TRex.GridFabric.ComputeFuncs
{
  /// <summary>
  /// The base class for compute functions. This provides common aspects such as the injected Ignite instance
  /// </summary>
  public class BaseComputeFunc : IBinarizable, IFromToBinary
  {
    private const byte VERSION_NUMBER = 1;

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

    public virtual void ToBinary(IBinaryRawWriter writer)
    {
      // Version the serialization, even if nothing additional is serialized
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);
    }

    public virtual void FromBinary(IBinaryRawReader reader)
    {
      // Version the serialization, even if nothing additional is serialized
      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);
    }
  }
}
