using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;

using VSS.Hosted.VLCommon;


using log4net;

namespace VSS.Hosted.VLCommon.PMDuePopulator
{
  public class PMIntervalInstanceWrapper
  {
    public PMIntervalInstance thisInstance;
    public int trackingType;

    public PMIntervalInstanceWrapper()
    { }

    public PMIntervalInstanceWrapper(PMIntervalInstance _thisInstance, int _trackingType)
    {
      thisInstance = _thisInstance;
      trackingType = _trackingType;
    }

    public double trackingValue
    {
      set
      {
        switch (trackingType)
        {
          case (int)PMTrackingTypeEnum.RuntimeHours:
            thisInstance.RuntimeHours = value;
            thisInstance.OdometerMiles = null;
            break;

          case (int)PMTrackingTypeEnum.Mileage:
            thisInstance.OdometerMiles = value;
            thisInstance.RuntimeHours = null;
            break;
        }
      }

      get
      {
        switch (trackingType)
        {
          case (int)PMTrackingTypeEnum.RuntimeHours:
            return (thisInstance.RuntimeHours.HasValue ? thisInstance.RuntimeHours.Value : 0);

          case (int)PMTrackingTypeEnum.Mileage:
            return (thisInstance.OdometerMiles.HasValue ? thisInstance.OdometerMiles.Value : 0);
        }

        return 0;
      }
    }
  }
}
