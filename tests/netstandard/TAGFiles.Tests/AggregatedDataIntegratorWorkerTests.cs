using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VSS.TRex.DI;
using VSS.TRex.Executors.Executors;
using VSS.TRex.SiteModels;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.TAGFiles.Classes.Integrator;
using Xunit;

namespace TAGFiles.Tests
{

  public static class LocalState
  {
  public static Guid NewSiteModelGuid = Guid.NewGuid();
}

public class TAGFileTestsDIFixture : IDisposable
  {
    private static object Lock = new object();

    public static Guid NewSiteModelGuid = Guid.NewGuid();

    public TAGFileTestsDIFixture()
    {
      lock (Lock)
      {
        var moqStorageProxy = new Mock<IStorageProxy>();

        var moqStorageProxyFactory = new Mock<IStorageProxyFactory>();
        moqStorageProxyFactory.Setup(mk => mk.Storage(StorageMutability.Immutable)).Returns(moqStorageProxy.Object);
        moqStorageProxyFactory.Setup(mk => mk.Storage(StorageMutability.Mutable)).Returns(moqStorageProxy.Object);

        ISiteModel mockedSiteModel = new SiteModel(NewSiteModelGuid);

        var moqSiteModels = new Mock<ISiteModels>();
        moqSiteModels.Setup(mk => mk.GetSiteModel(NewSiteModelGuid)).Returns(mockedSiteModel);

        // Mock the new sitemodel creation API to return jsut a new sitemodel
        moqSiteModels.Setup(mk => mk.GetSiteModel(moqStorageProxy.Object, NewSiteModelGuid, true)).Returns(mockedSiteModel);

        DIBuilder
          .New()
          .AddLogging()
          .Add(x => x.AddSingleton<IStorageProxyFactory>(moqStorageProxyFactory.Object))
          .Add(x => x.AddSingleton<ISiteModels>(moqSiteModels.Object))
          .Complete();
      }
    }
    public void Dispose() { } // Nothing needing doing 
  }

  public class AggregatedDataIntegratorWorkerTests : IClassFixture<TAGFileTestsDIFixture>
    {
        [Fact(Skip = "Not Implemented")]
        public void Test_AggregatedDataIntegratorWorker_AggregatedDataIntegratorWorkerTest()
        {
            Assert.True(false);
        }

        [Fact(Skip = "Not Implemented")]
        public void Test_AggregatedDataIntegratorWorker_AggregatedDataIntegratorWorkerTest1()
        {
            Assert.True(false);
        }

        [Fact()]
        public void Test_AggregatedDataIntegratorWorker_ProcessTask()
        {
            // Convert a TAG file using a TAGFileConverter into a mini-site model
            TAGFileConverter converter = new TAGFileConverter();

            Assert.True(converter.Execute(new FileStream(Path.Combine("TestData", "TAGFiles", "TestTAGFile.tag"), FileMode.Open, FileAccess.Read)),
                "Converter execute returned false");

            // Create the site model and machine etc to aggregate the processed TAG file into
            SiteModel siteModel = new SiteModel("TestName", "TestDesc", TAGFileTestsDIFixture.NewSiteModelGuid, 1.0);
            VSS.TRex.Machines.Machine machine = new VSS.TRex.Machines.Machine(null, "TestName", "TestHardwareID", 0, 0, Guid.NewGuid(), VSS.TRex.Machines.Machine.kNullInternalSiteModelMachineIndex, false);

            // Create the integrator and add the processed TAG file to its processing list
            AggregatedDataIntegrator integrator = new AggregatedDataIntegrator();

            integrator.AddTaskToProcessList(siteModel, machine, converter.SiteModelGridAggregator, converter.ProcessedCellPassCount, converter.MachineTargetValueChangesAggregator);

            // Construct an integration worker and ask it to perform the integration
            List<AggregatedDataIntegratorTask> ProcessedTasks = new List<AggregatedDataIntegratorTask>();

            AggregatedDataIntegratorWorker worker = new AggregatedDataIntegratorWorker(integrator.TasksToProcess);
            worker.ProcessTask(ProcessedTasks);

            Assert.True(1 == ProcessedTasks.Count);
        }
    }
}
