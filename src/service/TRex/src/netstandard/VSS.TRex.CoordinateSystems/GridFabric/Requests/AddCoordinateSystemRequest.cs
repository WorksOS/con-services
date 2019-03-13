using Apache.Ignite.Core.Compute;
using VSS.TRex.CoordinateSystems.GridFabric.Arguments;
using VSS.TRex.CoordinateSystems.GridFabric.ComputeFuncs;
using VSS.TRex.CoordinateSystems.GridFabric.Responses;

namespace VSS.TRex.CoordinateSystems.GridFabric.Requests
{
  public class AddCoordinateSystemRequest : CoordinateSystemRequest<AddCoordinateSystemArgument, AddCoordinateSystemResponse>
  {
    public override AddCoordinateSystemResponse Execute(AddCoordinateSystemArgument arg)
    {
      // Construct the function to be used
      IComputeFunc<AddCoordinateSystemArgument, AddCoordinateSystemResponse> func = new AddCoordinateSystemComputeFunc();

      // Send the appropriate response to the caller
      return Compute.Apply(func, arg);
    }
  }
}
