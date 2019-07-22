using System.Threading.Tasks;
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
      var func = new AddCoordinateSystemComputeFunc();

      // Send the appropriate response to the caller
      return Compute.Apply(func, arg);
    }

    public override Task<AddCoordinateSystemResponse> ExecuteAsync(AddCoordinateSystemArgument arg)
    {
      // Construct the function to be used
      var func = new AddCoordinateSystemComputeFunc();

      // Send the appropriate response to the caller
      return Compute.ApplyAsync(func, arg);
    }
  }
}
