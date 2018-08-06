using System.IO;
using VSS.TRex.Designs.TTM;
using VSS.TRex.Exports.Surfaces;
using VSS.TRex.Exports.Surfaces.GridFabric;

namespace VSS.TRex.Exports.Surfaces.Requestors
{
  public interface ITINSurfaceExportRequestor
  {
    /// <summary>
    /// Generate a decimated TIN surface from the supplied arguments
    /// </summary>
    /// <param name="argument"></param>
    /// <returns></returns>
    TINSurfaceResult Execute(TINSurfaceRequestArgument argument);
  }

  /// <summary>
    /// Provides a requestor to manage making TIN surface export requests.
    /// This class assume a running, activated Immutable Grid Client to be present in the local process
    /// </summary>
    public class TINSurfaceExportRequestor : ITINSurfaceExportRequestor
  {
      /// <summary>
      /// Generate a decimated TIN surface from the supplied arguments
      /// </summary>
      /// <param name="argument"></param>
      /// <returns></returns>
      public TINSurfaceResult Execute(TINSurfaceRequestArgument argument)
      {
        TINSurfaceRequest request = new TINSurfaceRequest();

        return request.Execute(argument);
      }
  }
}
