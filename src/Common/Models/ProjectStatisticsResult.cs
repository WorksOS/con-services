using System;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Contracts;

namespace VSS.Productivity3D.Common.Models
{
  /// <summary>
  /// A representation of a set of spatial and temporal stastics for the project as a whole
  /// </summary>
  public class ProjectStatisticsResult : ContractExecutionResult
  {
    /// <summary>
    /// Earlist time stamped data present in the project, including both production and surveyed surface data.
    /// </summary>
    public DateTime startTime;

    /// <summary>
    /// Latest time stamped data present in the project, including both production and surveyed surface data.
    /// </summary>
    public DateTime endTime;

    /// <summary>
    /// Size of spatial data cells in the project (the default value is 34cm)
    /// </summary>
    public double cellSize;

    /// <summary>
    /// The index origin offset from the absolute bottom left origin of the subgrid tree cartesian coordinate system to the centered origin of the cartesian
    /// grid coordinate system used in the project, and the centered origin cartesian coordinates of cell addresses.
    /// </summary>
    public int indexOriginOffset;

    /// <summary>
    /// The three dimensional extents of the project including both production and surveyed surface data.
    /// </summary>
    public BoundingBox3DGrid extents;

    public ProjectStatisticsResult(int code, string message = "") : base(code, message)
    {
    }

    protected ProjectStatisticsResult(string message) : base(message)
    {
    }

    public ProjectStatisticsResult()
        : base()
    {
    }

    /// <summary>
    /// Statistics parameters request help instance
    /// </summary>
    public static ProjectStatisticsResult HelpSample => new ProjectStatisticsResult
    {
      startTime = DateTime.Now,
      endTime = DateTime.Now.AddDays(1),
      cellSize = 10,
      Code = ContractExecutionStatesEnum.ExecutedSuccessfully,
      extents = BoundingBox3DGrid.HelpSample,
      indexOriginOffset = 55,
    };

    /// <summary>
    /// ToString override
    /// </summary>
    /// <returns>Formatted string representation of the project statistics information.</returns>
    public override string ToString()
    {
      return
        $"Start time:{startTime}, end time:{endTime}, cellsize:{cellSize}, indexOrifinOffset:{indexOriginOffset}, extents:{extents}";
    }
  }
}