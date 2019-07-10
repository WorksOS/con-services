using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog;
using VSS.Productivity3D.Scheduler.Abstractions;
using VSS.Productivity3D.Scheduler.WebAPI.JobRunner;
using VSS.Serilog.Extensions;

namespace VSS.Productivity3D.Scheduler.Tests
{
  [TestClass]
  public class JobFactoryTests
  {
    private IServiceProvider serviceProvider;

    [TestInitialize]
    public void TestInitialize()
    {
      var logger = new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.Scheduler.UnitTests"));
      var serviceCollection = new ServiceCollection();

      serviceProvider = serviceCollection.AddLogging()
                                         .AddSingleton(logger)
                                         .AddSingleton<IJobFactory, JobFactory>()
                                         .BuildServiceProvider();
    }

    [TestMethod]
    public void CanRegisterJob()
    {
      var factory = serviceProvider.GetService<IJobFactory>();
      Assert.IsNotNull(factory);

      // We should be able to register the job with different guids
      factory.RegisterJob(Guid.NewGuid(), typeof(MockTileGenerationJob));
      factory.RegisterJob(Guid.NewGuid(), typeof(MockTileGenerationJob));
      factory.RegisterJob(Guid.NewGuid(), typeof(MockTileGenerationJob));
    }

    [TestMethod]
    public void CanGetJob()
    {
      var factory = serviceProvider.GetService<IJobFactory>();
      Assert.IsNotNull(factory);

      var id = Guid.Parse("7E91D04B-781D-4767-8486-FBD0B0B5F44B");
      factory.RegisterJob(id, typeof(MockTileGenerationJob));

      var job = factory.GetJob(id);

      Assert.IsInstanceOfType(job, typeof(MockTileGenerationJob));
    }

    [TestMethod]
    public void InvalidJobThrowsError()
    {
      var factory = serviceProvider.GetService<IJobFactory>();
      Assert.IsNotNull(factory);

      var id = Guid.Parse("EB6150FC-0305-4C5F-B299-5A11F50A057A");
      
      Assert.ThrowsException<ArgumentException>(() =>
      {
        var job = factory.GetJob(id);
        Assert.IsNull(job);
      }, $"Job Factory should throw {nameof(ArgumentException)} when a job is not registered");

      factory.RegisterJob(id, typeof(MockTileGenerationJob));
   
      var jobRegistered = factory.GetJob(id);
      Assert.IsInstanceOfType(jobRegistered, typeof(MockTileGenerationJob));

    }
  }
}
