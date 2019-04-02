using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using VSS.TRex.Pipelines.Tasks;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace VSS.TRex.Exports.CSV.Executors.Tasks
{
  /// <summary>
  /// The task responsible for receiving sub grids to be aggregated into a grid response
  /// </summary>
  public class CSVExportTask : PipelinedSubGridTask
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<CSVExportTask>();
    
    public CSVExportSubGridProcessor SubGridExportProcessor { get; set; }
    public List<string> DataRows { get; } = new List<string>();

    /// <summary>
    /// Accept a sub grid response from the processing engine and incorporate into the result for the request.
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    public override bool TransferResponse(object response)
    {
      bool result = false;

      if (!SubGridExportProcessor.RecordCountLimitReached() && base.TransferResponse(response))
      {
        if (!(response is IClientLeafSubGrid[] subGridResponses) || subGridResponses.Length == 0)
        {
          Log.LogWarning("No sub grid responses returned");
        }
        else
        {
          result = true;
          foreach (var subGrid in subGridResponses)
          {
            if (subGrid != null)
            {
              List<string> rows;
              if (subGrid is ClientCellProfileLeafSubgrid grid)
                rows = SubGridExportProcessor.ProcessSubGrid(grid);
              else
                rows = SubGridExportProcessor.ProcessSubGrid(subGrid as ClientCellProfileAllPassesLeafSubgrid);
              DataRows.AddRange(rows);

              if (SubGridExportProcessor.RecordCountLimitReached())
              {
                Log.LogWarning("CSVExportTask: exceeded row limit");
                result = false;
                break;
              }
            }
          }
        }
      }

      return result;
    }
  }
}

