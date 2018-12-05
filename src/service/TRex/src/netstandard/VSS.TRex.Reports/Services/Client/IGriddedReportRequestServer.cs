using VSS.TRex.Reports.Gridded;
using VSS.TRex.Reports.Gridded.GridFabric;

namespace VSS.TRex.Reports.Services.Client
{
  public interface IGriddedReportRequestServer
  {
    /// <summary>
    /// Generate a patch of subgrids given the supplied arguments
    /// </summary>
    /// <param name="argument"></param>
    /// <returns></returns>
    GriddedReportResult Execute(GriddedReportRequestArgument argument);
  }
}
