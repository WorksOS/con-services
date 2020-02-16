using System;
using System.Linq;

namespace VSS.Hosted.VLCommon
{
  public class PMIntervalWithNextDueInfo
  {
    public PMInterval Interval;

    public double HoursSinceLastService = 0;
    public double MilesSinceLastService = 0;

    public double DueAtHours = 0;
    public double DueAtMiles = 0;

    public double DueInHours = 0;
    public double DueInMiles = 0;

    public double PercentDue = 0;
    public DateTime NextIntervalDueUTC;
    public PMCompletedInstance LastServiceCompleted;
    public int AssetIconID;
    public long AssetID;
    public string AssetSerialNumber;
    public string AssetName;
    public string MakeModel;
    public double? CurrentRuntimeHours;
    public double? CurrentOdometerMiles;
    public bool reportsMileage;
    public double? Latitude;
    public double? Longitude;
    public DateTime? LastReportUTC;

    public PMTrackingTypeEnum TrackingType
    {
      get
      {
        return (PMTrackingTypeEnum)this.Interval.ifk_PMTrackingTypeID;
      }
    }

    public double SinceLastServiceByTrackingType
    {
      get
      {
        switch (TrackingType)
        {
          case PMTrackingTypeEnum.RuntimeHours:
            return HoursSinceLastService;

          case PMTrackingTypeEnum.Mileage:
            return MilesSinceLastService;
        }
        return -1;
      }
      set
      {
        switch (TrackingType)
        {
          case PMTrackingTypeEnum.RuntimeHours:
            this.HoursSinceLastService = value;
            break;

          case PMTrackingTypeEnum.Mileage:
            this.MilesSinceLastService = value;
            break;
        }
      }
    }

    public double DueInByTrackingType
    {
      get
      {
        switch (TrackingType)
        {
          case PMTrackingTypeEnum.RuntimeHours:
            return DueInHours;

          case PMTrackingTypeEnum.Mileage:
            return DueInMiles;
        }
        return -1;
      }
      set
      {
        switch (TrackingType)
        {
          case PMTrackingTypeEnum.RuntimeHours:
            this.DueInHours = value;
            break;

          case PMTrackingTypeEnum.Mileage:
            this.DueInMiles = value;
            break;
        }
      }
    }

    public double DueAtByTrackingType
    {
      get
      {
        switch (TrackingType)
        {
          case PMTrackingTypeEnum.RuntimeHours:
            return DueAtHours;

          case PMTrackingTypeEnum.Mileage:
            return DueAtMiles;
        }
        return -1;
      }
      set
      {
        switch (TrackingType)
        {
          case PMTrackingTypeEnum.RuntimeHours:
            this.DueAtHours = value;
            break;

          case PMTrackingTypeEnum.Mileage:
            this.DueAtMiles = value;
            break;
        }
      }
    }

    public double FirstIntervalByTrackingType
    {
      get
      {
        switch (TrackingType)
        {
          case PMTrackingTypeEnum.RuntimeHours:
            return Interval.TrackingValueHoursFirst;

          case PMTrackingTypeEnum.Mileage:
            return Interval.TrackingValueMilesFirst;
        }
        return -1;
      }
    }

    public double NextIntervalByTrackingType
    {
      get
      {
        switch (TrackingType)
        {
          case PMTrackingTypeEnum.RuntimeHours:
            return Interval.TrackingValueHoursNext;

          case PMTrackingTypeEnum.Mileage:
            return Interval.TrackingValueMilesNext;
        }
        return -1;
      }
    }

    public double? AssetCurrentMeterByTrackingType
    {
      get
      {
        switch (TrackingType)
        {
          case PMTrackingTypeEnum.RuntimeHours:
            return CurrentRuntimeHours;

          case PMTrackingTypeEnum.Mileage:
            return CurrentOdometerMiles;
        }
        return null;
      }
    }


    public PMIntervalWithNextDueInfo(PMInterval interval)
    {
      this.Interval = interval;
    }
  }
}
