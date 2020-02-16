using System;
using VSS.UnitTest.Common.Contexts;

using VSS.Hosted.VLCommon;

namespace VSS.UnitTest.Common.EntityBuilder
{
  public class ServiceProviderBuilder 
  {
    private long id = IdGen.GetId();
    private string _address;
    private string _providerName;
    
    public ServiceProviderBuilder ProviderName(string providerName)
    {
      _providerName = providerName;
      return this;
    }
    public ServiceProviderBuilder ServerIPAddress(string address)
    {
      _address = address;
      return this;
    }
    public ServiceProvider Build()
    {
      var serviceProvider = new ServiceProvider();
      serviceProvider.ID = id;
      serviceProvider.ProviderName = _providerName;
      serviceProvider.ServerIPAddress = _address;
      serviceProvider.UpdateUTC = DateTime.UtcNow;
      return serviceProvider;
    }

    public ServiceProvider Save()
    {
      var service = Build();

      ContextContainer.Current.OpContext.ServiceProvider.AddObject(service);
      ContextContainer.Current.OpContext.SaveChanges();

      return service;
    }
  }
}
