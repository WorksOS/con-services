using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using VSS.Project.Service.Interfaces;
using VSS.Project.Service.Utils.Kafka;
using KafkaConsumer;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using MasterDataConsumer;
using VSS.Project.Service.Repositories;
using VSS.Project.Data;
using VSS.Customer.Data;
using VSS.Project.Service.Utils;

namespace VSS.UnifiedProductivity.Service.Datafeed.Tests
{

  [TestClass]
  public class MasterDataConsumerTests
  {

    [TestMethod]
    public void CanCreateCustomerEventConsumer()
    {
      CreateCollection();

      var customerConsumer = DependencyInjectionProvider.ServiceProvider.GetService<IKafkaConsumer<ICustomerEvent>>();
      Assert.IsNotNull(customerConsumer);

      customerConsumer.SetTopic("VSS.Interfaces.Events.MasterData.ICustomerEvent");
      var customerReturn = customerConsumer.StartProcessingAsync(new CancellationTokenSource());      
      Assert.IsNotNull(customerReturn);

      CleanCollection();
    }
        

    private void CreateCollection()
    {
      var serviceProvider = new ServiceCollection()
          .AddTransient<IKafka, RdKafkaDriver>()
          .AddTransient<IKafkaConsumer<ISubscriptionEvent>, KafkaConsumer<ISubscriptionEvent>>()
          .AddTransient<IKafkaConsumer<IProjectEvent>, KafkaConsumer<IProjectEvent>>()
          .AddTransient<IKafkaConsumer<ICustomerEvent>, KafkaConsumer<ICustomerEvent>>()
          .AddTransient<IMessageTypeResolver, MessageResolver>()
          .AddTransient<IRepositoryFactory, RepositoryFactory>()
          .AddTransient<IRepository<ISubscriptionEvent>, SubscriptionRepository>()
          .AddTransient<IRepository<IProjectEvent>, ProjectRepository>()
          .AddTransient<IRepository<ICustomerEvent>, CustomerRepository>()
          .AddSingleton<IConfigurationStore, GenericConfiguration>()
          .BuildServiceProvider();
      new DependencyInjectionProvider(serviceProvider);
    }

    private void CleanCollection()
    {
      DependencyInjectionProvider.CleanDependencyInjection();
    }

  }
}
