using VSS.TRex.Common.Interfaces;

namespace VSS.TRex.GridFabric.Interfaces
{
  /// <summary>
  /// Defines the obligation to include an argument of a type with an Ignite ComputeFunc class.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public interface IComputeFuncArgument<T>

  {
    /// <summary>
    /// The argument type
    /// </summary>
    T Argument { get; set; }
  }
}
