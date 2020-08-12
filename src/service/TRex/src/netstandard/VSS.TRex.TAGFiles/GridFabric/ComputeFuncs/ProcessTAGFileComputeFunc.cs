using System;
using System.Collections.Generic;
using System.Linq;
using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.TAGFiles.Executors;
using VSS.TRex.TAGFiles.GridFabric.Arguments;
using VSS.TRex.TAGFiles.GridFabric.Responses;
using VSS.TRex.TAGFiles.Models;

namespace VSS.TRex.TAGFiles.GridFabric.ComputeFuncs
{
  public class ProcessTAGFileComputeFunc : BaseComputeFunc,
    IComputeFunc<ProcessTAGFileRequestArgument, ProcessTAGFileResponse>
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<ProcessTAGFileComputeFunc>();

    /// <summary>
    /// Default no-arg constructor that orients the request to the available TAG processing server nodes on the mutable grid projection
    /// </summary>
    public ProcessTAGFileComputeFunc()
    {
    }

    /// <summary>
    /// The Invoke method for the compute func - calls the TAG file processing executor to do the work
    /// </summary>
    public ProcessTAGFileResponse Invoke(ProcessTAGFileRequestArgument arg)
    {
      try
      {
        _log.LogInformation($"Processing {arg.TAGFiles.Count} tag files in project {arg.ProjectID}");

        return ProcessTAGFilesExecutor.Execute(arg.ProjectID, arg.TAGFiles);
      }
      catch (Exception e)
      {
        _log.LogError(e, $"{nameof(ProcessTAGFileComputeFunc)}.{nameof(Invoke)} failed with exception processing {arg.TAGFiles} TAF files:");

        return new ProcessTAGFileResponse {Results = new List<IProcessTAGFileResponseItem>(arg.TAGFiles.Select(x => new ProcessTAGFileResponseItem
        {
          FileName = x.FileName,
          AssetUid = x.AssetId,
          Success = false,
          Exception = e.Message,
          SubmissionFlags = x.SubmissionFlags,
          OriginSource = x.OriginSource
        }).ToList())};
      }
    }
  }
}
