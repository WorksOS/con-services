using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.GridFabric.Requests;
using VSS.TRex.SurveyedSurfaces.GridFabric.Arguments;
using VSS.TRex.SurveyedSurfaces.GridFabric.ComputeFuncs;
using VSS.TRex.SurveyedSurfaces.GridFabric.Responses;

namespace VSS.TRex.SurveyedSurfaces.GridFabric.Requests
{
  public class AddSurveyedSurfaceRequest : GenericASNodeRequest<AddSurveyedSurfaceArgument, AddSurveyedSurfaceComputeFunc, AddSurveyedSurfaceResponse>
  {
    public AddSurveyedSurfaceRequest() : base(TRexGrids.MutableGridName(), ServerRoles.DATA_MUTATION_ROLE)
    {
    }
  }
}
