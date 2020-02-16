//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace VSS.Hosted.VLCommon
{
  public partial class Service
  {
    #region Primitive Properties

    public virtual long ID
    {
      get;
      set;
    }

    public virtual int fk_ServiceTypeID
    {
      get;
      set;
    }

    public virtual long fk_DeviceID
    {
      get { return _fk_DeviceID; }
      set
      {
        if (_fk_DeviceID != value)
        {
          if (Device != null && Device.ID != value)
          {
            Device = null;
          }
          _fk_DeviceID = value;
        }
      }
    }
    private long _fk_DeviceID;

    public virtual string BSSLineID
    {
      get;
      set;
    }

    public virtual bool IsFirstReportNeeded
    {
      get;
      set;
    }

    public virtual int ActivationKeyDate
    {
      get;
      set;
    }

    public virtual int CancellationKeyDate
    {
      get;
      set;
    }

    public virtual Nullable<int> OwnerVisibilityKeyDate
    {
      get;
      set;
    }

    public virtual long fk_StoreID
    {
      get { return _fk_StoreID; }
      set { _fk_StoreID = value; }
    }
    private long _fk_StoreID = 1;

    public virtual Nullable<System.Guid> ServiceUID
    {
      get;
      set;
    }

    public virtual System.DateTime UpdateUTC
    {
      get { return _updateUTC; }
      set { _updateUTC = value; }
    }
    private System.DateTime _updateUTC = DateTime.UtcNow;

    public virtual bool IsVirtual
    {
      get;
      set;
    }

    #endregion

    #region Navigation Properties

    public virtual Device Device
    {
      get { return _device; }
      set
      {
        if (!ReferenceEquals(_device, value))
        {
          var previousValue = _device;
          _device = value;
          FixupDevice(previousValue);
        }
      }
    }
    private Device _device;

    public virtual ICollection<ServiceView> ServiceView
    {
      get
      {
        if (_serviceView == null)
        {
          var newCollection = new FixupCollection<ServiceView>();
          newCollection.CollectionChanged += FixupServiceView;
          _serviceView = newCollection;
        }
        return _serviceView;
      }
      set
      {
        if (!ReferenceEquals(_serviceView, value))
        {
          var previousValue = _serviceView as FixupCollection<ServiceView>;
          if (previousValue != null)
          {
            previousValue.CollectionChanged -= FixupServiceView;
          }
          _serviceView = value;
          var newValue = value as FixupCollection<ServiceView>;
          if (newValue != null)
          {
            newValue.CollectionChanged += FixupServiceView;
          }
        }
      }
    }
    private ICollection<ServiceView> _serviceView;

    #endregion

    #region Association Fixup

    private void FixupDevice(Device previousValue)
    {
      if (previousValue != null && previousValue.Service.Contains(this))
      {
        previousValue.Service.Remove(this);
      }

      if (Device != null)
      {
        if (!Device.Service.Contains(this))
        {
          Device.Service.Add(this);
        }
        if (fk_DeviceID != Device.ID)
        {
          fk_DeviceID = Device.ID;
        }
      }
    }

    private void FixupServiceView(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.NewItems != null)
      {
        foreach (ServiceView item in e.NewItems)
        {
          item.Service = this;
        }
      }

      if (e.OldItems != null)
      {
        foreach (ServiceView item in e.OldItems)
        {
          if (ReferenceEquals(item.Service, this))
          {
            item.Service = null;
          }
        }
      }
    }

    #endregion

  }
}
