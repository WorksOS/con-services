using Apache.Ignite.Core.Binary;

namespace VSS.TRex.GridFabric.ComputeFuncs
{
  /// <summary>
  /// The base class for compute functions. This provides common aspects such as the injected Ignite instance
  /// </summary>
  public class BaseComputeFunc : BaseIgniteClass, IBinarizable
  {
    public BaseComputeFunc()
    {
    }

    /// <summary>
    /// Constructor accepting a role for the compute func that can identity a cluster group in the grid to perform the operation
    /// </summary>
    public BaseComputeFunc(string gridName, string role) : base(gridName, role)
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
    public void WriteBinary(IBinaryWriter writer)
    {
    }

    /// <summary>
    /// By convention in TRex, compute functions derive their state from the supplied argument to
    /// their Invoke() method. State derived from BaseIgniteClass is intended to allow the representation
    /// of the compute function on the invoking side the ability to target appropriate grid resources
    /// for execution of the function. Thus, IBinarizable serialization for base compute func in TRex is a
    /// null function.
    /// </summary>
    /// <param name="reader"></param>
    public void ReadBinary(IBinaryReader reader)
    {
    }
  }
}
