using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Productivity3D.Scheduler.Abstractions;
using VSS.Productivity3D.Scheduler.WebAPI.JobRunner;

namespace VSS.Productivity3D.Scheduler.Tests
{
  [TestClass]
  public class JobFactoryTests
  {
    private ILoggerFactory loggerFactory;
    private IServiceProvider serviceProvider;

    [TestInitialize]
    public void TestInitialize()
    {
      var services = new ServiceCollection();
      services.AddSingleton<IVSSJobFactory, VSSJobFactory>();
      serviceProvider = services
        .AddLogging()
        .BuildServiceProvider();

      loggerFactory = serviceProvider.GetService<ILoggerFactory>();
    }

    [TestMethod]
    public void CanRegisterJob()
    {
      var factory = serviceProvider.GetService<IVSSJobFactory>();
      Assert.IsNotNull(factory);

      // We should be able to register the job with different guids
      factory.RegisterJob(Guid.NewGuid(), typeof(MockDxfTileGenerationJob));
      factory.RegisterJob(Guid.NewGuid(), typeof(MockDxfTileGenerationJob));
      factory.RegisterJob(Guid.NewGuid(), typeof(MockDxfTileGenerationJob));
    }

    [TestMethod]
    public void CanGetJob()
    {
      var factory = serviceProvider.GetService<IVSSJobFactory>();
      Assert.IsNotNull(factory);

      var id = Guid.Parse("7E91D04B-781D-4767-8486-FBD0B0B5F44B");
      factory.RegisterJob(id, typeof(MockDxfTileGenerationJob));

      var job = factory.GetJob(id);

      Assert.IsInstanceOfType(job, typeof(MockDxfTileGenerationJob));
    }

    [TestMethod]
    public void InvalidJobThrowsError()
    {
      var factory = serviceProvider.GetService<IVSSJobFactory>();
      Assert.IsNotNull(factory);

      var id = Guid.Parse("EB6150FC-0305-4C5F-B299-5A11F50A057A");
      
      Assert.ThrowsException<ArgumentException>(() =>
      {
        var job = factory.GetJob(id);
        Assert.IsNull(job);
      }, $"Job Factory should throw {nameof(ArgumentException)} when a job is not registered");

      factory.RegisterJob(id, typeof(MockDxfTileGenerationJob));
   
      var jobRegistered = factory.GetJob(id);
      Assert.IsInstanceOfType(jobRegistered, typeof(MockDxfTileGenerationJob));

    }
  }
}