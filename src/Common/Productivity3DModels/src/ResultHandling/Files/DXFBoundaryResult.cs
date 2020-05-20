using System;
using System.Collections.Generic;
using System.Text;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models.Files;

namespace VSS.TRex.Gateway.Common.ResultHandling
{
  public class DXFBoundaryResultItem : ContractExecutionResult
  {
    /// <summary>
    /// Boundary as list of points.
    /// </summary>
    public List<WGSPoint> Fence { get; private set; }

    public DXFLineWorkBoundaryType Type { get; private set; }

    public string Name { get; private set; }

    /// <summary>
    /// Override constructor with parameters.
    /// </summary>
    /// <param name="fence"></param>
    /// <param name="type"></param>
    /// <param name="name"></param>
    public DXFBoundaryResultItem(List<WGSPoint> fence, DXFLineWorkBoundaryType type, string name)
    {
      Fence = fence;
      Type = type;
      Name = name;
    }
  }

  public class DXFBoundaryResult : ContractExecutionResult
  {
    public List<DXFBoundaryResultItem> Boundaries { get; set; }

    /// <summary>
    /// Override constructor with parameters.
    /// </summary>
    public DXFBoundaryResult(int code, string message, List<DXFBoundaryResultItem> boundaries) : base(code, message)
    {
      Boundaries = boundaries;
    }
  }
}
