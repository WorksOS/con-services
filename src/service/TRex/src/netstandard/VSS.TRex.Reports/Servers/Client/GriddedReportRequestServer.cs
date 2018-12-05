using System.Collections.Generic;
using Force.DeepCloner;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.GridFabric.Servers.Client;
using VSS.TRex.Reports.Gridded;
using VSS.TRex.Reports.Gridded.GridFabric;
using VSS.TRex.SubGridTrees.Client;

namespace VSS.TRex.Reports.Servers.Client
{
  /// <summary>
  /// The server used to house gridded report services
  /// </summary>
  public class GriddedReportRequestServer : ApplicationServiceServer, IGriddedReportRequestServer
  {
    /// <summary>
    /// Default no-arg constructor that creates a server with the default Application Service role and the specialised grid role.
    /// </summary>
    public GriddedReportRequestServer() : base(new[] {ApplicationServiceServer.DEFAULT_ROLE, ServerRoles.REPORTING_ROLE})
    {
    }

    public GriddedReportRequestServer(string[] roles) : base(roles)
    {
    }

    /// <summary>
    /// Creates a new instance of a Grid request server
    /// </summary>
    /// <returns></returns>
    public static GriddedReportRequestServer NewInstance(string[] roles)
    {
      return new GriddedReportRequestServer(roles);
    }

    /// <summary>
    /// Generate a grid of values, given the supplied arguments
    /// </summary>
    /// <param name="argument"></param>
    /// <returns></returns>
    public GriddedReportResult Execute(GriddedReportRequestArgument argument)
    {
      GriddedReportRequest request = new GriddedReportRequest();

      GriddedReportRequestResponse computeResponse = request.Execute(argument);

      GriddedReportResult result = new GriddedReportResult
      {
        ReturnCode = ReportReturncode.NoError,
        ReportType = ReportType.Gridded,
        GriddedData = new GriddedReportData()
        {
          // todoJeannie why are these setup here rather than on return to WebAPI layer?
          ElevationReport = argument.ReportElevation,
          CutFillReport = argument.ReportCutFill,
          CmvReport = argument.ReportCMV,
          MdpReport = argument.ReportMDP,
          PassCountReport = argument.ReportPassCount,
          TemperatureReport = argument.ReportTemperature,
          Rows = ExtractRequiredValues(argument, computeResponse)
        }
      };
      result.GriddedData.NumberOfRows = result.GriddedData.Rows.Count;

      return result;
    }

    private GriddedReportDataRows ExtractRequiredValues(GriddedReportRequestArgument argument, GriddedReportRequestResponse computeResponse)
    {
      var result = new GriddedReportDataRows();
      foreach (var subGrid in computeResponse.SubGrids)
      {
        if (subGrid is ClientCellProfileLeafSubgrid sg)
        {
          foreach (var cell in sg.Cells)
          {
            result.Add(new GriddedReportDataRow()
            {
              // todoJeannie does the cellProfiler only provide each value if flag is set
              Easting = cell.CellXOffset + sg.CacheOriginX, // todoJeannie what unit to convert to?
              Northing = cell.CellYOffset + sg.CacheOriginY, // todoJeannie what unit to convert to?
              Elevation = argument.ReportElevation ? cell.Height : 0.0, // todoJeannie what is the default?
              // todoJeannie CutFill = argument.ReportCutFill ? cell.? : 0.0
              // todoJeannie Cmv = argument.ReportCMV ? cell.? : 0,
              Mdp = (short) (argument.ReportMDP ? cell.LastPassValidMDP : 0),
              PassCount = (short) (argument.ReportPassCount ? cell.PassCount : 0),
              Temperature = (short) (argument.ReportTemperature ? cell.LastPassValidTemperature : 0)
            });
          }
        }
      }

      return result;
    }
  }
}
