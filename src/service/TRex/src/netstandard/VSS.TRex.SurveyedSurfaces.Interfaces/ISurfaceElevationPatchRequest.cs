using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.SubGridTrees.Client.Interfaces;

namespace VSS.TRex.SurveyedSurfaces.Interfaces
{
  public interface ISurfaceElevationPatchRequest : IBaseRequest<ISurfaceElevationPatchArgument, IClientLeafSubGrid>
  {
  }
}
