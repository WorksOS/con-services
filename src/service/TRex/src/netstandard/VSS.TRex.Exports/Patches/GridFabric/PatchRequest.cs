using System.Linq;
using System.Threading.Tasks;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.GridFabric.Requests;

namespace VSS.TRex.Exports.Patches.GridFabric
{
  /// <summary>
  /// Sends a request to the grid for a patch of sub grids
  /// </summary>
  public class PatchRequest : GenericASNodeRequest<PatchRequestArgument, PatchRequestComputeFunc, PatchRequestResponse>
  {
    public PatchRequest() : base(TRexGrids.ImmutableGridName(), ServerRoles.ASNODE)
    {
    }

    public async Task<PatchResult> ExecuteAndConvertToResult(PatchRequestArgument argument)
    {
      var response = await ExecuteAsync(argument);

      var result = new PatchResult
      {
        TotalNumberOfPagesToCoverFilteredData = response.TotalNumberOfPagesToCoverFilteredData,
        MaxPatchSize = argument.DataPatchSize,
        PatchNumber = argument.DataPatchNumber,
        Patch = response?.SubGrids?.Select(x =>
        {
          var s = new SubgridDataPatchRecord_ElevationAndTime();
          s.Populate(x);
          return s;
        }).ToArray()
      };

      return result;
    }
  }
}
