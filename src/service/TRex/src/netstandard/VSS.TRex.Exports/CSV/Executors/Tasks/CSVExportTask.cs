using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Common;
using VSS.TRex.Exports.CSV.GridFabric;
using VSS.TRex.Pipelines.Tasks;
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
    private static readonly ILogger Log = Logging.Logger.CreateLogger<CSVExportTask>();

    public CSVExportRequestArgument requestArgument;

    public Formatter formatter;

    // todoJeannie a CSVExportRequestResponse is defined as the pipeline response object
    //    can I access that instead of creating another   local?
    public CSVExportRequestResponse taskResponse = new CSVExportRequestResponse();

    private long DataLength = 0;
    private float RunningHeight = Consts.NullHeight;
    

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
      formatter = new Formatter(requestArgument.UserPreferences, requestArgument.OutputType, /* todoJeannie implement isRawDataAs3*/ false);
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
        taskResponse.dataRows.Add(UpdateComponentStrings(cell, subgridWorldOriginX, subgridWorldOriginY));
      });

      return true;
    }


    private string UpdateComponentStrings(ClientCellProfileLeafSubgridRecord cell, double subGridWorldOriginX, double subGridWorldOriginY)
    {
      var heightString = string.Empty;

      var easting = cell.CellXOffset + subGridWorldOriginX;
      var northing = cell.CellYOffset + subGridWorldOriginY;

      if (!cell.Height.Equals(RunningHeight))
      {
        heightString = formatter.FormatElevation(cell.Height);
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

      string resultString = $"{heightString},";
      DataLength += resultString.Length - 1; // Forget the last ',' character
      return resultString;
    }

    //private string BuildRow()
    //{
    //  //AddStringToBuffer(LastPassTimeString);
    //  //AddStringToBuffer(CoordString);
    //  //AddStringToBuffer(HeightString);
    //  //AddStringToBuffer(PassCountString);
    //  //AddStringToBuffer(LastPassValidRadioLatencyString);
    //  //AddStringToBuffer(DesignNameString);
    //  //AddQuotedStringToBuffer(MachineNameString);
    //  //AddStringToBuffer(MachineSpeedString);
    //  //AddStringToBuffer(LastPassValidGPSModeString);
    //  //AddStringToBuffer(GPSAccuracyToleranceString);
    //  //AddStringToBuffer(TargetPassCountString);
    //  //AddStringToBuffer('Yes');
    //  //AddStringToBuffer(LayerIDString);
    //  //AddStringToBuffer(LastPassValidCCVString);
    //  //AddStringToBuffer(TargetCCVString);
    //  //AddStringToBuffer(LastPassValidMDPString);
    //  //AddStringToBuffer(TargetMDPString);
    //  //AddStringToBuffer(LastPassValidRMVString);
    //  //AddStringToBuffer(LastPassValidFreqString);
    //  //AddStringToBuffer(LastPassValidAmpString);
    //  //AddStringToBuffer(TargetThicknessString);
    //  //AddStringToBuffer(EventMachineGearString);
    //  //AddStringToBuffer(EventVibrationStateString);
    //  //AddStringToBuffer(LastPassValidTemperatureString);

    //  DataLength += resultString.Length - 1; // Forget the last ',' character
    //  return resultString;
    //}

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

