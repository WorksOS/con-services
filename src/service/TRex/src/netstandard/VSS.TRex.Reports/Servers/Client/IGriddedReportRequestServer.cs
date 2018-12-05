using VSS.TRex.Reports.Gridded;
using VSS.TRex.Reports.Gridded.GridFabric;

namespace VSS.TRex.Reports.Servers.Client
{
  public interface IGriddedReportRequestServer
  {
    /// <summary>
    /// Generate a grid of cell values, given the supplied arguments
    /// </summary>
    /// <param name="argument"></param>
    /// <returns></returns>
    GriddedReportResult Execute(GriddedReportRequestArgument argument);
  }
}
