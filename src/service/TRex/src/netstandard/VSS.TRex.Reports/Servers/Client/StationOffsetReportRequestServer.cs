using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.GridFabric.Servers.Client;

namespace VSS.TRex.Reports.Servers.Client
{
  /// <summary>
  /// The server used to house gridded report services
  /// </summary>
  public class StationOffsetReportRequestServer : ApplicationServiceServer
  {
    /// <summary>
    /// Default no-arg constructor that creates a server with the specialised grid role only, as it has it's own service.
    /// </summary>
    public StationOffsetReportRequestServer() : base(new[] {ServerRoles.REPORTING_ROLE})
    {
    }

    public StationOffsetReportRequestServer(string[] roles) : base(roles)
    {
    }

    /// <summary>
    /// Creates a new instance of a Grid request server
    /// </summary>
    /// <returns></returns>
    public static StationOffsetReportRequestServer NewInstance(string[] roles)
    {
      return new StationOffsetReportRequestServer(roles);
    }
  }
}
