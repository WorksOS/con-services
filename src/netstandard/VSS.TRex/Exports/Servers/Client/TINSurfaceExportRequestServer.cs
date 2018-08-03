using VSS.TRex.Exports.Surfaces;
using VSS.TRex.Exports.Surfaces.GridFabric;
using VSS.TRex.Servers;
using VSS.TRex.Servers.Client;

namespace VSS.TRex.Exports.Servers.Client
{
    public class TINSurfaceExportRequestServer : ApplicationServiceServer
  {
      /// <summary>
      /// Default no-arg constructor that creates a server with the default Application Service role and the specialise tile rendering role.
      /// </summary>
      public TINSurfaceExportRequestServer() : base(new[] { ApplicationServiceServer.DEFAULT_ROLE, ServerRoles.TIN_SURFACE_EXPORT_ROLE })
      {
      }

      public TINSurfaceExportRequestServer(string[] roles) : base(roles)
      {
      }

      /// <summary>
      /// Creates a new instance of a Patch request server
      /// </summary>
      /// <returns></returns>
      public static TINSurfaceExportRequestServer NewInstance(string[] roles)
      {
        return new TINSurfaceExportRequestServer(roles);
      }

      /// <summary>
      /// Generate a patch of subgrids given the supplied arguments
      /// </summary>
      /// <param name="argument"></param>
      /// <returns></returns>
      public TINSurfaceResult Execute(TINSurfaceRequestArgument argument)
      {
        TINSurfaceRequest request = new TINSurfaceRequest();

        TINSurfaceRequestResponse response = request.Execute(argument);

        TINSurfaceResult result = new TINSurfaceResult
        {
          // ????
        };

        return result;
      }
    }
}
