using System;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Helpers
{
  public abstract class ProfileConverterBase
  {
    public static bool CellGapExists(Velociraptor.PDSInterface.ProfileCell prevCell, Velociraptor.PDSInterface.ProfileCell currCell, out double prevStationIntercept)
    {
      prevStationIntercept = prevCell == null
        ? 0.0
        : prevCell.station + prevCell.interceptLength;

      return prevCell != null && Math.Abs(currCell.station - prevStationIntercept) > 0.001;
    }
  }
}