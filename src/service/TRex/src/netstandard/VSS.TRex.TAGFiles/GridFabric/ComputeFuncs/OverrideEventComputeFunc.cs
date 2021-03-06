﻿using System;
using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx.Synchronous;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.TAGFiles.Executors;
using VSS.TRex.TAGFiles.GridFabric.Arguments;
using VSS.TRex.TAGFiles.GridFabric.Responses;

namespace VSS.TRex.TAGFiles.GridFabric.ComputeFuncs
{
  public class OverrideEventComputeFunc : BaseComputeFunc, IComputeFunc<OverrideEventRequestArgument, OverrideEventResponse>
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<OverrideEventComputeFunc>();

    /// <summary>
    /// Default no-arg constructor that orients the request to the available TAG processing server nodes on the mutable grid projection
    /// </summary>
    public OverrideEventComputeFunc()
    {
    }

    /// <summary>
    /// The Invoke method for the compute func - calls the override event executor to do the work
    /// </summary>
    public OverrideEventResponse Invoke(OverrideEventRequestArgument arg)
    {
      _log.LogInformation("In OverrideEventComputeFunc.Invoke()");

      if (arg == null)
        throw new ArgumentException("Argument for ComputeFunc must be provided");

      try
      {
        IOverrideEventExecutor executor;
        if (arg.Undo)
          executor = new RemoveOverrideEventExecutor();
        else
          executor = new AddOverrideEventExecutor();

        _log.LogInformation("Executing OverrideEventComputeFunc.ExecuteAsync()");

        return executor.ExecuteAsync(arg).WaitAndUnwrapException();
      }
      catch (Exception e)
      {
        _log.LogError(e, "Exception processing override event request");
        return new OverrideEventResponse { Success = false, Message = "Excepgtion"};
      }
      finally
      {
        _log.LogInformation("Exiting OverrideEventComputeFunc.Invoke()");
      }
    }
  }
}
