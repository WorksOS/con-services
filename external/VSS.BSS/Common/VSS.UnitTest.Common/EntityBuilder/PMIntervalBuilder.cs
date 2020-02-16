using System;
using System.Collections.Generic;
using VSS.UnitTest.Common.Contexts;

using VSS.Hosted.VLCommon;

namespace VSS.UnitTest.Common.EntityBuilder
{
  public class PMIntervalBuilder 
  {
    #region PMInterval Fields

    private long _id = IdGen.GetId();
    private string _description = String.Empty;
    private PMTrackingTypeEnum _trackingType = (int)PMTrackingTypeEnum.RuntimeHours;
    private long? _externalID = 0;
    private short? _jobCode = 0;
    private string _compCode;
    private string _quantityCode;
    private string _modCode;
    private double _trackingValueHoursFirst = 0;
    private double _trackingValueHoursNext = 0;
    private double _trackingValueMilesFirst = 0;
    private double _trackingValueMilesNext = 0;
    private PMSalesModel _pmSalesModel;
    private long _pmSalesModelID;
    private bool _isCustom = false;
    private bool _isDeleted = false;
    private DateTime _updateUtc = DateTime.UtcNow;
    private bool _isMetric = false;
    private string _title = String.Empty;
    private bool _isCumulative = true;
    private int _rank = 1;
    private bool _isTrackingEnabled = true;
    private bool _isMajorComponent = false;

    #endregion

    public PMIntervalBuilder Id(long id)
    {
      _id = id;
      return this;
    }
    public PMIntervalBuilder Description(string description)
    {
      _description = description;
      return this;
    }
    public PMIntervalBuilder ExternalID(long? externalID)
    {
      _externalID = externalID;
      return this;
    }
    public PMIntervalBuilder JobCode(short? jobCode)
    {
      _jobCode = jobCode;

      return this;
    }
    public PMIntervalBuilder CompCode(string compCode)
    {
      _compCode = compCode;
      return this;
    }
    public PMIntervalBuilder QuantityCode(string quantityCode)
    {
      _quantityCode = quantityCode;
      return this;
    }
    public PMIntervalBuilder ModCode(string modCode)
    {
      _modCode = modCode;
      return this;
    }
    public PMIntervalBuilder TrackingValueHoursFirst(double trackingValueHoursFirst)
    {
      _trackingValueHoursFirst = trackingValueHoursFirst;
      return this;
    }
    public PMIntervalBuilder TrackingValueHoursNext(double trackingValueHoursNext)
    {
      _trackingValueHoursNext = trackingValueHoursNext;
      return this;
    }
    public PMIntervalBuilder TrackingValueMilesFirst(double trackingValueMilesFirst)
    {
      _trackingValueMilesFirst = trackingValueMilesFirst;
      return this;
    }
    public PMIntervalBuilder TrackingValueMilesNext(double trackingValueMilesNext)
    {
      _trackingValueMilesNext = trackingValueMilesNext;
      return this;
    }
    public PMIntervalBuilder PMSalesModel(PMSalesModel pmSalesModel)
    {
      _pmSalesModel = pmSalesModel;
      _pmSalesModelID = pmSalesModel.ID;
      return this;
    }

    public PMIntervalBuilder PMSalesModelID(long pmSalesModelID)
    {
      _pmSalesModelID = pmSalesModelID;
      return this;
    }
    public PMIntervalBuilder IsCustom(bool isCustom)
    {
      _isCustom = isCustom;
      return this;
    }
    public PMIntervalBuilder IsTrackingEnabled(bool isTrackingEnabled)
    {
      _isTrackingEnabled = isTrackingEnabled;
      return this;
    }

    public PMIntervalBuilder IsMajorComponent(bool isMajorComponent)
    {
        _isMajorComponent = isMajorComponent;
        return this;
    }
    public PMIntervalBuilder IsDeleted(bool isDeleted)
    {
      _isDeleted = isDeleted;
      return this;
    }
    public PMIntervalBuilder UpdateUtc(DateTime updateUtc)
    {
      _updateUtc = updateUtc;
      return this;
    }
    public PMIntervalBuilder IsMetric(bool isMetric)
    {
      _isMetric = isMetric;
      return this;
    }
    public PMIntervalBuilder Title(string title)
    {
      _title = title;
      return this;
    }
    public PMIntervalBuilder PMTrackingType(PMTrackingTypeEnum pmTrackingTypeEnum)
    {
      _trackingType = pmTrackingTypeEnum;
      return this;
    }
    public PMIntervalBuilder IsCumulative(bool isCumulative)
    {
      _isCumulative = isCumulative;

      if (!isCumulative)
        _rank = 0;

      return this;
    }

   
    public PMIntervalBuilder Rank(int rank)
    {
      _rank = rank;
      return this;
    }

    public PMInterval Build()
    {
      PMInterval interval =  new PMInterval();

      interval.Description = _description;
      interval.ifk_PMTrackingTypeID = (int)_trackingType;
      interval.ExternalID = _externalID;
      interval.CompCode = _compCode;
      interval.JobCode = _jobCode;
      interval.QuantityCode = _quantityCode;
      interval.ModCode = _modCode;
      interval.TrackingValueHoursFirst = _trackingValueHoursFirst;
      interval.TrackingValueHoursNext = _trackingValueHoursNext;
      interval.TrackingValueMilesFirst = _trackingValueMilesFirst;
      interval.TrackingValueMilesNext = _trackingValueMilesNext;
      if (_pmSalesModel != null)
      {
        interval.fk_PMSalesModelID = _pmSalesModel.ID;
      }
      interval.fk_PMSalesModelID = _pmSalesModelID;
      interval.IsCustom = _isCustom;
      interval.IsDeleted = _isDeleted;
      interval.UpdateUTC = _updateUtc;
      interval.IsMetric = _isMetric;
      interval.Title = _title;
      interval.IsCumulative = _isCumulative;
      interval.Rank = (short)_rank;
      interval.IsTrackingEnabled = _isTrackingEnabled;
      interval.IsMajorComponent = _isMajorComponent;
      
      return interval;
    }
    public PMInterval Save()
    {
      PMInterval interval = Build();

      ContextContainer.Current.OpContext.PMInterval.AddObject(interval);
      ContextContainer.Current.OpContext.SaveChanges();

      return interval;
    }
  }
}
