using System;
using System.Linq;
using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.TAGFiles.Executors;
using VSS.TRex.TAGFiles.GridFabric.Arguments;
using VSS.TRex.TAGFiles.GridFabric.Responses;

namespace VSS.TRex.TAGFiles.GridFabric.ComputeFuncs
{
  public class ProcessTAGFileComputeFunc : BaseComputeFunc,
    IComputeFunc<ProcessTAGFileRequestArgument, ProcessTAGFileResponse>
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<ProcessTAGFileComputeFunc>();

    /// <summary>
    /// Default no-arg constructor that orients the request to the available TAG processing server nodes on the mutable grid projection
    /// </summary>
    public ProcessTAGFileComputeFunc()
    {
    }

    /// <summary>
    /// The Invoke method for the compute func - calls the TAG file processing executor to do the work
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    public ProcessTAGFileResponse Invoke(ProcessTAGFileRequestArgument arg)
    {
      try
      {
        Log.LogInformation($"Processing {arg.TAGFiles.Count} tag files in project {arg.ProjectID}");

        return ProcessTAGFilesExecutor.Execute(arg.ProjectID, arg.TAGFiles);
      }
      catch (Exception e)
      {
        Log.LogError(e, $"{nameof(ProcessTAGFileComputeFunc)}.{nameof(Invoke)} failed with exception: {e.Message}, stack trace: {e.StackTrace}");

        return new ProcessTAGFileResponse {Results = arg.TAGFiles.Select(x => new ProcessTAGFileResponseItem
        {
          FileName = x.FileName,
          AssetUid = x.AssetId,
          Success = false,
          Exception = e.Message
        }).ToList()};
      }
    }
  }
}
