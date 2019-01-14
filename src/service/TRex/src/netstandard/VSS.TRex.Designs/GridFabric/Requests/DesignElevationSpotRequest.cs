﻿using Apache.Ignite.Core.Compute;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.ComputeFuncs;

namespace VSS.TRex.Designs.GridFabric.Requests
{
    public class DesignElevationSpotRequest : DesignProfilerRequest<CalculateDesignElevationSpotArgument, double>
    {
        public override double Execute(CalculateDesignElevationSpotArgument arg)
        {
            // Construct the function to be used
            IComputeFunc<CalculateDesignElevationSpotArgument, double> func = new CalculateDesignElevationSpotComputeFunc();

            return _Compute.Apply(func, arg);
        }
    }
}
