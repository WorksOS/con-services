using System;
using System.Linq;

namespace VSS.Hosted.VLCommon
{
  public class PMCompletedInstance
  {
    private long _pmIntervalID = 0;
    public long AssetID;
    public DateTime ServiceDate;
    public double RuntimeHours;
    public double OdometerMiles;
    public byte ServiceCompletionType;
    public bool IsCumulative;
    public int Rank;

    public string IntervalTitle;
    public long ID;
    public int TrackingType;
    public long DefaultIntervalID;
    public long PMIntervalAssetIntervalID;
    public DateTime UpdatedUTC;

    public long PMIntervalID
    {
      get
      {
        if (_pmIntervalID == 0)
        {
          _pmIntervalID = PMIntervalAssetIntervalID == 0 ? DefaultIntervalID : PMIntervalAssetIntervalID;
        }
        return _pmIntervalID;
      }
      set
      {
        _pmIntervalID = value;
      }
    }
    public double TrackingValue
    {
      get
      {
        switch (TrackingType)
        {
          case (int)PMTrackingTypeEnum.RuntimeHours:
            return (this.RuntimeHours);

          case (int)PMTrackingTypeEnum.Mileage:
            return (this.OdometerMiles);
        }

        return 0;
      }
    }   
  }
}
