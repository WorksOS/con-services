using System.Threading.Tasks;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.ComputeFuncs;

namespace VSS.TRex.Designs.GridFabric.Requests
{
  public class DesignElevationSpotRequest : DesignProfilerRequest<CalculateDesignElevationSpotArgument, double>
  {
    public override double Execute(CalculateDesignElevationSpotArgument arg)
    {
      // Construct the function to be used
      var func = new CalculateDesignElevationSpotComputeFunc();

      return Compute.Apply(func, arg);
    }

    public override Task<double> ExecuteAsync(CalculateDesignElevationSpotArgument arg)
    {
      // Construct the function to be used
      var func = new CalculateDesignElevationSpotComputeFunc();

      return Compute.ApplyAsync(func, arg);
    }
  }
}
