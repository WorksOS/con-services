using System.Linq;
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

    public PatchResult ExecuteAndConvertToResult(PatchRequestArgument argument)
    {
      PatchRequestResponse response = base.Execute(argument);

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
