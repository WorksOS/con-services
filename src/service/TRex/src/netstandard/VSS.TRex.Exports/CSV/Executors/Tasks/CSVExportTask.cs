using System;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.DI;
using VSS.TRex.Exports.CSV.GridFabric;
using VSS.TRex.Pipelines.Tasks;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Exports.CSV.Executors.Tasks
{
  /// <summary>
  /// The task responsible for receiving sub grids to be aggregated into a grid response
  /// </summary>
  public class CSVExportTask : PipelinedSubGridTask
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<CSVExportTask>();

    public CSVExportRequestArgument requestArgument;

    public Formatter formatter;
    public CSVExportRequestResponse taskResponse = new CSVExportRequestResponse();
    
    public CSVExportTask()
    {
      // todoJeannie does it come in here?
    }

    /// <summary>
    /// Constructs the grid task
    /// </summary>
    /// <param name="requestDescriptor"></param>
    /// <param name="tRexNodeId"></param>
    /// <param name="gridDataType"></param>
    public CSVExportTask(Guid requestDescriptor, string tRexNodeId, GridDataType gridDataType) : base(requestDescriptor, tRexNodeId, gridDataType)
    {
      //todoJeannie does it actually come in here?
      formatter = new Formatter(requestArgument.UserPreferences, requestArgument.OutputType, requestArgument.RawDataAsDBase);
    }

    /// <summary>
    /// Accept a sub grid response from the processing engine and incorporate into the result for the request.
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
        Log.LogWarning("No sub grid responses returned");
        return false;
      }

      var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(requestArgument.ProjectID);
      
      var requiredGridDataType = requestArgument.OutputType == OutputTypes.PassCountLastPass || requestArgument.OutputType == OutputTypes.VedaFinalPass ? 
        GridDataType.CellProfile : GridDataType.CellPasses;

      // todoJeannie what to do if this goes over?
      int runningRowCount = 0;

      foreach (var subGrid in subGridResponses)
      {
        //if (requestArgument.CoordType == CoordType.LatLon)
        //  SetupLLPositions(csibName, subGrid); // todoJeannie validate in executor that CSIB is loaded and avail
        if (requiredGridDataType == GridDataType.CellProfile &&
            subGrid is ClientCellProfileLeafSubgrid )
          /*ExtractRequiredValues(profileSubGrid)*/;
        {
          var subGridExportProcessor = new SubGridExportProcessor(formatter, requestArgument, siteModel, runningRowCount);
          var rows = subGridExportProcessor.ProcessSubGrid(subGrid as ClientCellProfileLeafSubgrid);
          runningRowCount += rows.Count;
          taskResponse.dataRows.AddRange(rows);
        }

        // todojeannie CellPasses
        //if (requiredGridDataType == GridDataType.CellPasses &&
        //    subGrid is ClientCellProfileAllPassesLeafSubgrid allPassesSubGrid)
        //  ExtractRequiredValues(allPassesSubGrid);
      }

      return true;
    }
  }
}

