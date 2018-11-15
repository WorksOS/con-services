using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using VSS.TRex.Designs.Executors;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.Responses;

namespace VSS.TRex.Designs.GridFabric.ComputeFuncs
{
  /// <summary>
  /// Ignite ComputeFunc responsible for executing the design profile calculator
  /// </summary>
  public class CalculateDesignProfileComputeFunc : IComputeFunc<CalculateDesignProfileArgument, CalculateDesignProfileResponse>
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

    public CalculateDesignProfileResponse Invoke(CalculateDesignProfileArgument arg)
    {
      try
      {
        // Log.LogInformation($"CalculateDesignProfileComputeFunc: Arg = {arg}");

        CalculateDesignProfile Executor = new CalculateDesignProfile();

        return new CalculateDesignProfileResponse()
        {
          Profile = Executor.Execute(arg)
        };
      }
      catch (Exception E)
      {
        Log.LogError($"Exception: {E}");
        return null;
      }
    }
  }
}
