using VSS.TRex.GridFabric.Servers.Client;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.Reports.Gridded;
using VSS.TRex.Reports.Gridded.GridFabric;

namespace VSS.TRex.Reports.Services.Client
{
  /// <summary>
  /// The server used to house tile rendering services
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
    /// Generate a patch of subgrids given the supplied arguments
    /// </summary>
    /// <param name="argument"></param>
    /// <returns></returns>
    public GriddedReportResult Execute(GriddedReportRequestArgument argument)
    {
      GriddedReportRequest request = new GriddedReportRequest();

      GriddedReportRequestResponse response = request.Execute(argument);

      GriddedReportResult result = new GriddedReportResult
      {
        //TotalNumberOfPagesToCoverFilteredData = response.TotalNumberOfPagesToCoverFilteredData,
        //MaxPatchSize = argument.DataPatchSize,
        //PatchNumber = argument.DataPatchNumber,
        //Patch = response?.SubGrids?.Select(x =>
        //{
        //  SubgridDataPatchRecord_ElevationAndTime s = new SubgridDataPatchRecord_ElevationAndTime();
        //  s.Populate(x);
        //  return s;
        //}).ToArray()
      };

      return result;
    }
  }
}
