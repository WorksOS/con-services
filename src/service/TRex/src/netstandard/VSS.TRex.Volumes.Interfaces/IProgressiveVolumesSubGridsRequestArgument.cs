using System;
using VSS.TRex.SubGrids.Interfaces;

namespace VSS.TRex.Volumes.Interfaces
{
  public interface IProgressiveVolumesSubGridsRequestArgument : ISubGridsRequestArgument
  {
    DateTime StartDate { get; set; }
    DateTime EndDate { get; set; }

    TimeSpan Interval { get; set; }
  }
}
