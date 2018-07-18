namespace VSS.TRex.Profiling.Interfaces
{
  public interface IProfilerBuilder
  {
    /// <summary>
    /// Builder responsible fopr per-cell profile analysis
    /// </summary>
    ICellLiftBuilder CellLiftBuilder { get; set; }

    /// <summary>
    /// Builder responsible for constructing cell vector from profile line
    /// </summary>
    ICellProfileBuilder CellProfileBuilder { get; set; }

    /// <summary>
    /// Buidler responsibler from building overall profile informationk from cell vector
    /// </summary>
    IProfileLiftBuilder ProfileLiftBuilder { get; set; }
  }
}
