using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.Pipelines.Tasks;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;

namespace VSS.TRex.Reports.Gridded.Executors.Tasks
{
  /// <summary>
  /// The task responsible for receiving sub grids to be aggregated into a grid response
  /// </summary>
  public class GriddedReportTask : PipelinedSubGridTask
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<GriddedReportTask>();

    /// <summary>
    /// The action (via a delegate) this task will perform on each of the sub grids transferred to it
    /// </summary>
    public Action<ClientCellProfileLeafSubgrid> ProcessorDelegate { get; set; }
  
    public GriddedReportTask()
    {
    }

    /// <summary>
    /// Accept a sub grid response from the processing engine and incorporate into the result for the request.
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    public override bool TransferResponse(object response)
    {
      if (!base.TransferResponse(response))
        return false;

      if (!(response is IClientLeafSubGrid[] subGridResponses) || subGridResponses.Length == 0)
      {
        Log.LogWarning("No sub grid responses returned");
        return false;
      }

      foreach (var subGrid in subGridResponses)
      {
        if (subGrid is ClientCellProfileLeafSubgrid leafSubGrid)
          ProcessorDelegate(leafSubGrid);
      }

      return true;
    }
  }
}

