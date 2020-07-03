using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.GridFabric.Requests;
using VSS.TRex.SurveyedSurface.GridFabric.Responses;
using VSS.TRex.SurveyedSurfaces.GridFabric.Arguments;
using VSS.TRex.SurveyedSurfaces.GridFabric.ComputeFuncs;

namespace VSS.TRex.SurveyedSurfaces.GridFabric.Requests
{
  public class RemoveSurveyedSurfaceRequest : GenericASNodeRequest<RemoveSurveyedSurfaceArgument, RemoveSurveyedSurfaceComputeFunc, RemoveSurveyedSurfaceResponse>
  {
    public RemoveSurveyedSurfaceRequest() : base(TRexGrids.MutableGridName(), ServerRoles.DATA_MUTATION_ROLE)
    {
    }
  }
}
