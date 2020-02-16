using System;

using VSS.Hosted.VLCommon;

namespace VSS.UnitTest.Common.EntityBuilder
{
  public class ServiceViewBuilder
  {
    private DateTime _startDateTimeUtc = DateTime.UtcNow.AddMonths(-13);
    private DateTime? _endDateTimeUtc = null;
    
    protected readonly ServiceBuilder _serviceBuilder;
    private Customer _customer;
    private Asset _asset;
    private Guid _serviceViewUid = Guid.NewGuid();

    public ServiceViewBuilder(ServiceBuilder serviceBuilder)
    {
      _serviceBuilder = serviceBuilder;
    }
    public ServiceViewBuilder ForCustomer(Customer customer)
    {
      _customer = customer;
      return this;
    }
    public ServiceViewBuilder ForAsset(Asset asset)
    {
      _asset = asset;
      return this;
    }
    public ServiceViewBuilder StartsOn(DateTime startDateUtc)
    {
      _startDateTimeUtc = startDateUtc.Date;
      return this;
    }
    public ServiceViewBuilder EndsOn(DateTime endDateUtc)
    {
      _endDateTimeUtc = endDateUtc;
      return this;
    }

    public ServiceView Build()
    {
      var serviceView = new ServiceView();

      serviceView.Customer = _customer;
      serviceView.Asset = _asset;
      serviceView.StartKeyDate = _startDateTimeUtc.KeyDate();
      serviceView.EndKeyDate = _endDateTimeUtc.KeyDate();
      serviceView.UpdateUTC = DateTime.UtcNow;
      serviceView.ServiceViewUID = _serviceViewUid;
      return serviceView;
    }
  }
}