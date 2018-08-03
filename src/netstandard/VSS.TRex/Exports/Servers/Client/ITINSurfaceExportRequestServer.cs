using VSS.TRex.Exports.Surfaces;
using VSS.TRex.Exports.Surfaces.GridFabric;

namespace VSS.TRex.Exports.Servers.Client
{
  public interface ITINSurfaceExportRequestServer
  {
    /// <summary>
    /// Generate a patch of subgrids given the supplied arguments
    /// </summary>
    /// <param name="argument"></param>
    /// <returns></returns>
    TINSurfaceResult Execute(TINSurfaceRequestArgument argument);
  }
}
