using System.Linq;
using VSS.TRex.Exports.Patches;
using VSS.TRex.Exports.Patches.GridFabric;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.GridFabric.Servers.Client;

namespace VSS.TRex.Exports.Servers.Client
{
  /// <summary>
  /// The server used to house tile rendering services
  /// </summary>
  public class PatchRequestServer : ApplicationServiceServer, IPatchRequestServer
  {
    /// <summary>
    /// Default no-arg constructor that creates a server with the default Application Service role and the specialize tile rendering role.
    /// </summary>
    public PatchRequestServer() : this(new[] {ApplicationServiceServer.DEFAULT_ROLE, ServerRoles.PATCH_REQUEST_ROLE})
    {
    }

    public PatchRequestServer(string[] roles) : base(roles)
    {
    }

    /// <summary>
    /// Generate a patch of sub grids given the supplied arguments
    /// </summary>
    /// <param name="argument"></param>
    /// <returns></returns>
    public PatchResult Execute(PatchRequestArgument argument)
    {
      PatchRequest request = new PatchRequest();

      PatchRequestResponse response = request.Execute(argument);

      PatchResult result = new PatchResult
      {
        TotalNumberOfPagesToCoverFilteredData = response.TotalNumberOfPagesToCoverFilteredData,
        MaxPatchSize = argument.DataPatchSize,
        PatchNumber = argument.DataPatchNumber,
        Patch = response?.SubGrids?.Select(x =>
        {
          SubgridDataPatchRecord_ElevationAndTime s = new SubgridDataPatchRecord_ElevationAndTime();
          s.Populate(x);
          return s;
        }).ToArray()
      };

      return result;
    }
  }
}
