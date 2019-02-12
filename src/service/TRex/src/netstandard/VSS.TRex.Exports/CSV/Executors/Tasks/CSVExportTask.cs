using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Common;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.Exports.CSV.GridFabric;
using VSS.TRex.Pipelines.Tasks;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Client.Types;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.Types;

namespace VSS.TRex.Exports.CSV.Executors.Tasks
{
  /// <summary>
  /// The task responsible for receiving sub grids to be aggregated into a grid response
  /// </summary>
  public class CSVExportTask : PipelinedSubGridTask
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

    public CSVExportRequestArgument requestArgument;

    public Formatter formatter;

    public CSVExportRequestResponse taskResponse = new CSVExportRequestResponse();

    public float RunningHeight = Consts.NullHeight;
    public string HeightString = string.Empty;

    public CSVExportTask()
    {
    }

    /// <summary>
    /// Constructs the grid task
    /// </summary>
    /// <param name="requestDescriptor"></param>
    /// <param name="tRexNodeId"></param>
    /// <param name="gridDataType"></param>
    public CSVExportTask(Guid requestDescriptor, string tRexNodeId, GridDataType gridDataType) : base(requestDescriptor, tRexNodeId, gridDataType)
    {
      // todoJeannie formatter = new Formatter(requestArgument.userPreferences, requestArgument.OutputType, requestDescriptor.rawDataAsWhatever);

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


      var requiredGridDataType = requestArgument.OutputType == OutputTypes.PassCountLastPass || requestArgument.OutputType == OutputTypes.VedaFinalPass ? 
        GridDataType.CellProfile : GridDataType.CellPasses;

      foreach (var subGrid in subGridResponses)
      {
        if (requiredGridDataType == GridDataType.CellProfile &&
            subGrid is ClientCellProfileLeafSubgrid profileSubGrid)
          ExtractRequiredValues(profileSubGrid);

        //if (requiredGridDataType == GridDataType.CellPasses &&
        //    subGrid is ClientCellProfileAllPassesLeafSubgrid allPassesSubGrid)
        //  ExtractRequiredValues(allPassesSubGrid);
      }

      return true;
    }

    private bool ExtractRequiredValues(ClientCellProfileLeafSubgrid subGrid)
    {
      var result = new List<string>();

      subGrid.CalculateWorldOrigin(out double subgridWorldOriginX, out double subgridWorldOriginY);
      SubGridUtilities.SubGridDimensionalIterator((x, y) =>
      {
        var cell = subGrid.Cells[x, y];

        if (cell.PassCount == 0) // Nothing for us to do, as cell is empty
          return;

        // todoJeannie if dealing with formatted strings, what about ordering
        result.Add(UpdateComponentStrings(cell, subgridWorldOriginX, subgridWorldOriginY));
      });

      return true;
    }


    private string UpdateComponentStrings(ClientCellProfileLeafSubgridRecord cell, double subgridWorldOriginX, double subgridWorldOriginY)
    {
      string resultString = string.Empty;
      var easting = cell.CellXOffset + subgridWorldOriginX;
      var northing = cell.CellYOffset + subgridWorldOriginY;

      if (!cell.Height.Equals(RunningHeight))
      {
        HeightString = formatter.FormatElevation(cell.Height);
        RunningHeight = cell.Height;
      }

      //    CutFill = (designHeights != null &&
      //               designHeights.Cells[x, y] != Consts.NullHeight)
      //      ? cell.Height - designHeights.Cells[x, y]
      //      : Consts.NullHeight,

      var Cmv = (short)cell.LastPassValidCCV;
      var Mdp = (short)cell.LastPassValidMDP;
      var PassCount = (short)cell.PassCount;
      var temperature = (short)cell.LastPassValidTemperature;
      return resultString;
    }

    //private string[] ExtractRequiredValues(ClientCellProfileAllPassesLeafSubgrid allPassesSubGrid)
    //{
    //  // For half-pass (e.g. 2-drum compactor), make up a single pass for each pair

    //  //  // TICPassCountExportCalculator.ProcessAllPasses
    //  //  // TICPassCountExportCalculator.InitComponentStrings
    //  //  //                              .UpdateComponentStrings
    //  //  var result = new string[0];

    //  //  string machineNameString;
    //  //  string CoordString;
    //  //  string elevation;

    //  return result;
    //}
  }
}

