using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;
using VSS.TRex.Exports.CSV.GridFabric;
using VSS.TRex.Pipelines.Tasks;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Reports.Gridded.Executors.Tasks
{
  /// <summary>
  /// The task responsible for receiving sub grids to be aggregated into a grid response
  /// </summary>
  public class CSVExportTask : PipelinedSubGridTask
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

  
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

      foreach (var subGrid in subGridResponses)
      {
        // todoJeannie may be profile (for last pass),
        //     or something else for all passes
        //     what is a half-pass?
        if (subGrid is ClientCellProfileLeafSubgrid leafSubGrid)
          ExtractRequiredValues(null, leafSubGrid);
      }

      return true;
    }


    private string[] ExtractRequiredValues(CSVExportRequestArgument CSVExportRequestArgument, ClientCellProfileLeafSubgrid subGrid)
    {
      // TICPassCountExportCalculator.ProcessAllPasses
      // TICPassCountExportCalculator.InitComponentStrings
      //                              .UpdateComponentStrings

      var result = new string[0];

      //var result = new List<GriddedReportDataRow>();
      //IClientHeightLeafSubGrid designHeights = null;

      //if (_griddedReportRequestArgument.ReferenceDesignUID != Guid.Empty)
      //{
      //  IDesign cutFillDesign = DIContext.Obtain<ISiteModels>().GetSiteModel(_griddedReportRequestArgument.ProjectID).Designs.Locate(_griddedReportRequestArgument.ReferenceDesignUID);
      //  if (cutFillDesign == null)
      //  {
      //    throw new ArgumentException($"Design {_griddedReportRequestArgument.ReferenceDesignUID} not a recognized design in project {_griddedReportRequestArgument.ProjectID}");
      //  }

      //  cutFillDesign.GetDesignHeights(_griddedReportRequestArgument.ProjectID, subGrid.OriginAsCellAddress(),
      //    subGrid.CellSize, out designHeights, out var errorCode);

      //  if (errorCode != DesignProfilerRequestResult.OK || designHeights == null)
      //  {
      //    string errorMessage;
      //    if (errorCode == DesignProfilerRequestResult.NoElevationsInRequestedPatch)
      //    {
      //      errorMessage = "Gridded Report. Call to RequestDesignElevationPatch failed due to no elevations in requested patch.";
      //      Log.LogInformation(errorMessage);
      //    }
      //    else
      //    {
      //      errorMessage = $"Gridded Report. Call to RequestDesignElevationPatch failed due to no TDesignProfilerRequestResult return code {errorCode}.";
      //      Log.LogWarning(errorMessage);
      //    }
      //  }
      //}

      //subGrid.CalculateWorldOrigin(out double subgridWorldOriginX, out double subgridWorldOriginY);
      //SubGridUtilities.SubGridDimensionalIterator((x, y) =>
      //{
      //  var cell = subGrid.Cells[x, y];

      //  if (cell.PassCount == 0) // Nothing for us to do, as cell is not in our areaControlSet...
      //    return;

      //  result.Add(new GriddedReportDataRow
      //  {
      //    Easting = cell.CellXOffset + subgridWorldOriginX,
      //    Northing = cell.CellYOffset + subgridWorldOriginY,
      //    Elevation = griddedReportRequestArgument.ReportElevation ? cell.Height : Consts.NullHeight,
      //    CutFill = (griddedReportRequestArgument.ReportCutFill && (designHeights != null) &&
      //               designHeights.Cells[x, y] != Consts.NullHeight)
      //      ? cell.Height - designHeights.Cells[x, y]
      //      : Consts.NullHeight,

      //    // CCV is equiv to CMV in this instance
      //    Cmv = (short)(griddedReportRequestArgument.ReportCmv ? cell.LastPassValidCCV : CellPassConsts.NullCCV),
      //    Mdp = (short)(griddedReportRequestArgument.ReportMdp ? cell.LastPassValidMDP : CellPassConsts.NullMDP),
      //    PassCount = (short)(griddedReportRequestArgument.ReportPassCount ? cell.PassCount : CellPassConsts.NullPassCountValue),
      //    Temperature = (short)(griddedReportRequestArgument.ReportTemperature ? cell.LastPassValidTemperature : CellPassConsts.NullMaterialTemperatureValue)
      //  });
      //});

      return result;
    }
  }
}

