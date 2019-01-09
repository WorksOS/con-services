using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using VSS.TRex.Pipelines.Tasks;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Reports.Gridded.Executors.Tasks
{
  /// <summary>
  /// The task responsible for receiving subgrids to be aggregated into a grid response
  /// </summary>
  public class GriddedReportTask : PipelinedSubGridTask
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

    /// <summary>
    /// The action (via a delegate) this task will perform on each of the subgrids transferred to it
    /// </summary>
    public Action<ClientCellProfileLeafSubgrid> ProcessorDelegate { get; set; } = null;
  
    public GriddedReportTask()
    {
    }

    /// <summary>
    /// Constructs the grid task
    /// </summary>
    /// <param name="requestDescriptor"></param>
    /// <param name="tRexNodeId"></param>
    /// <param name="gridDataType"></param>
    public GriddedReportTask(Guid requestDescriptor, string tRexNodeId, GridDataType gridDataType) : base(requestDescriptor, tRexNodeId, gridDataType)
    {
    }

    /// <summary>
    /// Accept a subgrid response from the processing engine and incorporate into the result for the request.
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    public override bool TransferResponse(object response)
    {
      if (!base.TransferResponse(response))
      {
        Log.LogWarning("Base TransferResponse returned false");
        return false;
      }

      if (!(response is IClientLeafSubGrid[] subGridResponses) || subGridResponses.Length == 0)
      {
        Log.LogWarning("No subgrid responses returned");
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

