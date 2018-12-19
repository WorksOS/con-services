using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common;
using VSS.TRex.Profiling.Interfaces;

namespace VSS.TRex.Profiling
{
  public class SummaryVolumeProfileCell : ProfileCellBase, ISummaryVolumeProfileCell
  {

    private static ILogger Log = Logging.Logger.CreateLogger<ProfileCell>();

    public float LastCellPassElevation1;
    public float LastCellPassElevation2;

    public SummaryVolumeProfileCell()
    {
      LastCellPassElevation1 = Consts.NullHeight;
      LastCellPassElevation2 = Consts.NullHeight;
      DesignElev = Consts.NullHeight;
    }

  }
}
