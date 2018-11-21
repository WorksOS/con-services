using Apache.Ignite.Core.Compute;
using VSS.TRex.CoordinateSystems.Executors;
using VSS.TRex.CoordinateSystems.GridFabric.Arguments;
using VSS.TRex.CoordinateSystems.GridFabric.Responses;
using VSS.TRex.GridFabric.ComputeFuncs;

namespace VSS.TRex.CoordinateSystems.GridFabric.ComputeFuncs
{
  public class AddCoordinateSystemComputeFunc : BaseComputeFunc, IComputeFunc<AddCoordinateSystemArgument, AddCoordinateSystemResponse>
  {
    /// <summary>
    /// Default no-arg constructor that orients the request to the available TAG processing server nodes on the mutable grid projection
    /// </summary>
    public AddCoordinateSystemComputeFunc()
    {
    }

    /// <summary>
    /// The Invoke method for the compute func - calls the TAG file processing executor to do the work
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    public AddCoordinateSystemResponse Invoke(AddCoordinateSystemArgument arg)
    {
      var executor = new AddCoordinateSystemExecutor();

      return new AddCoordinateSystemResponse {Succeeded = executor.Execute(arg.ProjectID, arg.CSIB)};
    }
  }
}
