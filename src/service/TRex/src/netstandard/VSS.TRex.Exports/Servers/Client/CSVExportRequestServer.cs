﻿using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.GridFabric.Servers.Client;

namespace VSS.TRex.Exports.Servers.Client
{
  /// <summary>
  /// The server used to house csv export services
  /// </summary>
  public class CSVExportRequestServer : ApplicationServiceServer
  {
    /// <summary>
    /// Default no-arg constructor that creates a server with the specialised grid role only, as it has it's own service.
    /// </summary>
    public CSVExportRequestServer() : base(new[] {ServerRoles.REPORTING_ROLE})
    {
    }

    public CSVExportRequestServer(string[] roles) : base(roles)
    {
    }

    /// <summary>
    /// Creates a new instance of a csv export request server
    /// </summary>
    /// <returns></returns>
    public static CSVExportRequestServer NewInstance(string[] roles)
    {
      return new CSVExportRequestServer(roles);
    }
  }
}
