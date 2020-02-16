using System;
using System.Collections.Generic;
using System.Linq;
using VSS.UnitTest.Common.Contexts;

using VSS.Hosted.VLCommon;

namespace VSS.UnitTest.Common.EntityBuilder
{
  public class ServiceBuilder 
  {
    private ServiceTypeEnum _serviceType = ServiceTypeEnum.Essentials;
    private long id = IdGen.GetId();
    private string _bssPlanLineId = IdGen.GetId().ToString();
    private DateTime? _activationDate = DateTime.UtcNow;
    private DateTime? _cancellationDate = null;
    private bool _isFirstReportNeeded = false;
    private DateTime? _ownerVisibilityDate = DateTime.UtcNow.AddMonths(-13);
    private Guid _serviceUid = Guid.NewGuid();

    private Device _device;
    private IList<ServiceViewBuilder> _serviceViews = new List<ServiceViewBuilder>();

    private bool _syncWithRpt;
    
    public ServiceBuilder(ServiceTypeEnum serviceType)
    {
      _serviceType = serviceType;
    }

    public ServiceBuilder OwnerVisibilityDate(DateTime? ownerVisibilityDate)
    {
      _ownerVisibilityDate = ownerVisibilityDate;
      return this;
    }
    public ServiceBuilder BssPlanLineId(string bssPlanLineId)
    {
      _bssPlanLineId = bssPlanLineId;
      return this;
    }
    public ServiceBuilder ActivationDate(DateTime activationDate)
    {
      _activationDate = activationDate;
      return this;
    }
    public ServiceBuilder CancellationDate(DateTime cancellationDate)
    {
      _cancellationDate = cancellationDate;
      return this;
    }
    public ServiceBuilder ForDevice(Device device)
    {
      if(_serviceType != ServiceTypeEnum.ManualMaintenanceLog)
      {
        _device = device;
      }
      return this;
    }
    public ServiceBuilder WithView(Func<ServiceViewBuilder, object> serviceView)
    {
      var serviceViewBuilder = serviceView(new ServiceViewBuilder(this)) as ServiceViewBuilder;
      _serviceViews.Add(serviceViewBuilder);
      return this;
    }
    public ServiceBuilder SyncWithRpt()
    {
      _syncWithRpt = true;
      return this;
    }
    public ServiceBuilder SyncWithRpt(bool doSync)
    {
      _syncWithRpt = doSync;
      return this;
    }
    public Service Build()
    {
      ServiceType st = (from sts in ContextContainer.Current.OpContext.ServiceTypeReadOnly
                        where sts.ID == (int)_serviceType
                        select sts).FirstOrDefault();
      if (null == st)
      {
        st = new ServiceType { ID = (int)_serviceType, Name = _serviceType.ToString(), BSSPartNumber = _serviceType.ToString(), IsCore = GetIsCore(_serviceType) };
        ContextContainer.Current.OpContext.ServiceType.AddObject(st);
      }

      var service = new Service();
      service.ID = id;
      service.BSSLineID = _bssPlanLineId;
      service.fk_ServiceTypeID = (int)_serviceType;
      service.Device = _device;
      service.ActivationKeyDate = _activationDate.KeyDate();
      service.CancellationKeyDate = _cancellationDate.KeyDate();
      service.IsFirstReportNeeded = _isFirstReportNeeded;
      service.OwnerVisibilityKeyDate = _ownerVisibilityDate != null ? _ownerVisibilityDate.KeyDate() : (int?)null;
      service.ServiceUID = _serviceUid;

      foreach (var serviceView in _serviceViews)
      {
        var view = serviceView.Build();
        service.ServiceView.Add(view);
        ContextContainer.Current.OpContext.ServiceView.AddObject(view);
      }

      return service;
    }

    private bool GetIsCore(ServiceTypeEnum _serviceType)
    {
      switch (_serviceType)
      {
        case ServiceTypeEnum.Essentials:
        case ServiceTypeEnum.ManualMaintenanceLog:
          return true;
        default:
          return false;
      }
    }
    public Service Save()
    {
      var service = Build();

      ContextContainer.Current.OpContext.Service.AddObject(service);
      ContextContainer.Current.OpContext.SaveChanges();

      return service;
    }

    public ServiceBuilder IsFirstReportNeeded(bool p)
    {
      _isFirstReportNeeded = p;
      return this;
    }
  }
}
