using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.GridFabric.Servers.Client;

namespace VSS.TRex.Exports.Servers.Client
{
  public class TINSurfaceExportRequestServer : ApplicationServiceServer
  {
      /// <summary>
      /// Default no-arg constructor that creates a server with the default Application Service role and the specialize tile rendering role.
      /// </summary>
      public TINSurfaceExportRequestServer() : this(new[]
      {
        ServerRoles.TIN_SURFACE_EXPORT_ROLE,
        ServerRoles.RECEIVES_SITEMODEL_CHANGE_EVENTS,
        ServerRoles.RECEIVES_DESIGN_CHANGE_EVENTS
      })
      {
      }

      public TINSurfaceExportRequestServer(string[] roles) : base(roles)
      {
      }
    }
}
