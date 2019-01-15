using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using VSS.TRex.DI;
using VSS.TRex.Events;
using VSS.TRex.Machines.Interfaces;
using VSS.TRex.SiteModels;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.TAGFiles.Classes.Integrator;
using VSS.TRex.TAGFiles.Executors;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace TAGFiles.Tests
{

  public class AggregatedDataIntegratorWorkerTests : IClassFixture<DITagFileFixture>
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

    [Fact]
    public void Test_AggregatedDataIntegratorWorker_ProcessTask_SingleTAGFile()
    {
      // Convert a TAG file using a TAGFileConverter into a mini-site model
      TAGFileConverter converter = DITagFileFixture.ReadTAGFile("TestTAGFile.tag");


      // Create the site model and machine etc to aggregate the processed TAG file into
      //  DIContext.Obtain<ISiteModelFactory>().NewSiteModel(DITagFileFixture.NewSiteModelGuid);
      ISiteModel targetSiteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      IMachine targetMachine = targetSiteModel.Machines.CreateNew("Test Machine", "", 1, 1, false, Guid.NewGuid());

      converter.Machine.ID = targetMachine.ID;

      // Create the integrator and add the processed TAG file to its processing list
      AggregatedDataIntegrator integrator = new AggregatedDataIntegrator();

      integrator.AddTaskToProcessList(targetSiteModel, targetMachine, converter.SiteModelGridAggregator, converter.ProcessedCellPassCount, converter.MachineTargetValueChangesAggregator);

      // Construct an integration worker and ask it to perform the integration
      List<AggregatedDataIntegratorTask> ProcessedTasks = new List<AggregatedDataIntegratorTask>();

      AggregatedDataIntegratorWorker worker = new AggregatedDataIntegratorWorker(integrator.TasksToProcess);
      worker.ProcessTask(ProcessedTasks);

      ProcessedTasks.Count.Should().Be(1);
      targetSiteModel.Grid.CountLeafSubgridsInMemory().Should().Be(12);
    }

    [Fact]
    public void Test_AggregatedDataIntegratorWorker_ProcessTask_SingleTAGFileTwice()
    {
      // Convert a TAG file using a TAGFileConverter into a mini-site model
      TAGFileConverter converter1 = DITagFileFixture.ReadTAGFile("TestTAGFile.tag");
      TAGFileConverter converter2 = DITagFileFixture.ReadTAGFile("TestTAGFile.tag");

      // Create the site model and machine etc to aggregate the processed TAG file into
      //ISiteModel targetSiteModel = DIContext.Obtain<ISiteModelFactory>().NewSiteModel(DITagFileFixture.NewSiteModelGuid);
      ISiteModel targetSiteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      IMachine targetMachine = targetSiteModel.Machines.CreateNew("Test Machine", "", 1, 1, false, Guid.NewGuid());

      converter1.Machine.ID = targetMachine.ID;
      converter2.Machine.ID = targetMachine.ID;

      // Create the integrator and add the processed TAG file to its processing list
      AggregatedDataIntegrator integrator = new AggregatedDataIntegrator();

      integrator.AddTaskToProcessList(targetSiteModel, targetMachine, converter1.SiteModelGridAggregator, converter1.ProcessedCellPassCount, converter1.MachineTargetValueChangesAggregator);
      integrator.AddTaskToProcessList(targetSiteModel, targetMachine, converter2.SiteModelGridAggregator, converter2.ProcessedCellPassCount, converter2.MachineTargetValueChangesAggregator);

      // Construct an integration worker and ask it to perform the integration
      List<AggregatedDataIntegratorTask> ProcessedTasks = new List<AggregatedDataIntegratorTask>();

      AggregatedDataIntegratorWorker worker = new AggregatedDataIntegratorWorker(integrator.TasksToProcess);
      worker.ProcessTask(ProcessedTasks);

      ProcessedTasks.Count.Should().Be(2);
      targetSiteModel.Grid.CountLeafSubgridsInMemory().Should().Be(12);
    }

    [Theory]
    [InlineData("Dimensions2018-CaseMachine", 164, 164, 9)]
    public void Test_AggregatedDataIntegratorWorker_ProcessTask_TAGFileSet(string tagFileCollectionFolder, int expectedFileCount, int maxTAGFilesPerAggregation, int expectedSubGridCount)
    {
      // Convert TAG files using TAGFileConverters into mini-site models
      var converters = Directory.GetFiles(Path.Combine("TestData", "TAGFiles", tagFileCollectionFolder), "*.tag")
        .ToList().OrderBy(x => x).Select(DITagFileFixture.ReadTAGFileFullPath).ToArray();

      converters.Length.Should().Be(expectedFileCount);

      // Create the site model and machine etc to aggregate the processed TAG file into
      ISiteModel targetSiteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      IMachine targetMachine = targetSiteModel.Machines.CreateNew("Test Machine", "", 1, 1, false, Guid.NewGuid());

      // Create the integrator and add the processed TAG file to its processing list
      AggregatedDataIntegrator integrator = new AggregatedDataIntegrator();

      foreach (var c in converters)
      {
        c.Machine.ID = targetMachine.ID;
        integrator.AddTaskToProcessList(targetSiteModel, targetMachine, c.SiteModelGridAggregator, c.ProcessedCellPassCount, c.MachineTargetValueChangesAggregator);
      }

      // Construct an integration worker and ask it to perform the integration
      List<AggregatedDataIntegratorTask> ProcessedTasks = new List<AggregatedDataIntegratorTask>();

      AggregatedDataIntegratorWorker worker = new AggregatedDataIntegratorWorker(integrator.TasksToProcess)
      {
        MaxMappedTagFilesToProcessPerAggregationEpoch = maxTAGFilesPerAggregation
      };
      worker.ProcessTask(ProcessedTasks);

      // The number of TAG files processing in one aggregation epoch is limited by MaxMappedTagFilesToProcessPerAggregationEpoch
      ProcessedTasks.Count.Should().Be(maxTAGFilesPerAggregation);

      // Check the set of TAG files created the expected number of sub grids
      targetSiteModel.Grid.CountLeafSubgridsInMemory().Should().Be(expectedSubGridCount);
    }

    [Fact]
    public void Test_AggregatedDataIntegratorWorker_EventIntegrator_NoTargetDesignIds()
    {
      /*
       source list
       id   Value 
       0    'DesignName4'
       1    'DesignName2'

       target list
       id   Value        
      none
      */
      EventIntegrator eventIntegrator = new EventIntegrator();
      var sourceSiteModel = new SiteModel();
      sourceSiteModel.SiteModelMachineDesigns.CreateNew("DesignName4");
      sourceSiteModel.SiteModelMachineDesigns.CreateNew("DesignName2");
      var sourceEventList = new ProductionEventLists(sourceSiteModel, MachineConsts.kNullInternalSiteModelMachineIndex);
      sourceEventList.MachineDesignNameIDStateEvents.PutValueAtDate(DateTime.UtcNow.AddMinutes(-60), 1);
      sourceEventList.MachineDesignNameIDStateEvents.PutValueAtDate(DateTime.UtcNow.AddMinutes(-30), 0);
      Assert.Equal(2, sourceEventList.MachineDesignNameIDStateEvents.Count());

      var targetSiteModel = new SiteModel();
      var targetEventList = new ProductionEventLists(targetSiteModel, MachineConsts.kNullInternalSiteModelMachineIndex);

      eventIntegrator.IntegrateMachineEvents(sourceEventList, targetEventList, false, sourceSiteModel, targetSiteModel);
      Assert.Equal(2, targetSiteModel.SiteModelMachineDesigns.Count);

      // integration re-orders the event lists so cannot locate orig by []
      Assert.Equal(0, targetSiteModel.SiteModelMachineDesigns.Locate("DesignName2").Id);
      Assert.Equal(1, targetSiteModel.SiteModelMachineDesigns.Locate("DesignName4").Id);

      sourceEventList.MachineDesignNameIDStateEvents.GetStateAtIndex(0, out DateTime _, out int state);
      Assert.Equal(0, state);

      sourceEventList.MachineDesignNameIDStateEvents.GetStateAtIndex(1, out DateTime _, out state);
      Assert.Equal(1, state);
    }

    [Fact]
    public void Test_AggregatedDataIntegratorWorker_EventIntegrator_NoOverlappedDesignIds()
    {
      /*
       source list
       id   Value 
       0    'DesignName2'
       1    'DesignName4'

       target list
       id   Value        
       0    'DesignName5'
      */
      EventIntegrator eventIntegrator = new EventIntegrator();
      var sourceSiteModel = new SiteModel();
      sourceSiteModel.SiteModelMachineDesigns.CreateNew("DesignName2");
      sourceSiteModel.SiteModelMachineDesigns.CreateNew("DesignName4");
      var sourceEventList = new ProductionEventLists(sourceSiteModel, MachineConsts.kNullInternalSiteModelMachineIndex);
      sourceEventList.MachineDesignNameIDStateEvents.PutValueAtDate(DateTime.UtcNow.AddMinutes(-60), 0);
      sourceEventList.MachineDesignNameIDStateEvents.PutValueAtDate(DateTime.UtcNow.AddMinutes(-30), 1);
      Assert.Equal(2, sourceEventList.MachineDesignNameIDStateEvents.Count()); 

      var targetSiteModel = new SiteModel();
      targetSiteModel.SiteModelMachineDesigns.CreateNew("DesignName5");
      Assert.Equal(1, targetSiteModel.SiteModelMachineDesigns.Count);

      var targetEventList = new ProductionEventLists(targetSiteModel, MachineConsts.kNullInternalSiteModelMachineIndex);
      targetEventList.MachineDesignNameIDStateEvents.PutValueAtDate(DateTime.UtcNow.AddMinutes(-32), 0);
      Assert.Equal(1, targetEventList.MachineDesignNameIDStateEvents.Count());

      eventIntegrator.IntegrateMachineEvents(sourceEventList, targetEventList, false, sourceSiteModel, targetSiteModel);
      Assert.Equal(3, targetSiteModel.SiteModelMachineDesigns.Count);

      // integration re-orders the event lists so cannot locate orig by []
      Assert.Equal(0, targetSiteModel.SiteModelMachineDesigns.Locate("DesignName5").Id);
      Assert.Equal(1, targetSiteModel.SiteModelMachineDesigns.Locate("DesignName2").Id);
      Assert.Equal(2, targetSiteModel.SiteModelMachineDesigns.Locate("DesignName4").Id);
    }

    [Fact]
    public void Test_AggregatedDataIntegratorWorker_EventIntegrator_OverlappedDesignIds()
    {
      /*
       source list
       id   Value 
       0    'DesignName2'
       1    'DesignName4'

       target list
       id   Value        
       0    'DesignName4'
      */
      EventIntegrator eventIntegrator = new EventIntegrator();
      var sourceSiteModel = new SiteModel();
      sourceSiteModel.SiteModelMachineDesigns.CreateNew("DesignName2");
      sourceSiteModel.SiteModelMachineDesigns.CreateNew("DesignName4");
      var sourceEventList = new ProductionEventLists(sourceSiteModel, MachineConsts.kNullInternalSiteModelMachineIndex);
      sourceEventList.MachineDesignNameIDStateEvents.PutValueAtDate(DateTime.UtcNow.AddMinutes(-60), 0);
      sourceEventList.MachineDesignNameIDStateEvents.PutValueAtDate(DateTime.UtcNow.AddMinutes(-30), 1);
      Assert.Equal(2, sourceEventList.MachineDesignNameIDStateEvents.Count());

      var targetSiteModel = new SiteModel();
      targetSiteModel.SiteModelMachineDesigns.CreateNew("DesignName4");
      Assert.Equal(1, targetSiteModel.SiteModelMachineDesigns.Count);

      var targetEventList = new ProductionEventLists(targetSiteModel, MachineConsts.kNullInternalSiteModelMachineIndex);

      eventIntegrator.IntegrateMachineEvents(sourceEventList, targetEventList, false, sourceSiteModel, targetSiteModel);
      Assert.Equal(2, targetSiteModel.SiteModelMachineDesigns.Count);

      // integration re-orders the event lists so cannot locate orig by []
      Assert.Equal(1, targetSiteModel.SiteModelMachineDesigns.Locate("DesignName2").Id);
      Assert.Equal(0, targetSiteModel.SiteModelMachineDesigns.Locate("DesignName4").Id);
    }

    [Fact]
    public void Test_AggregatedDataIntegratorWorker_EventIntegrator_DifferentDesignIds()
    {
      /*
       source list
       id   Value 
       0    'DesignName2'
       1    'DesignName4'

       target list
       id   Value        
       0    'DesignName2'
       1    'DesignName4'
      */
      EventIntegrator eventIntegrator = new EventIntegrator();
      var sourceSiteModel = new SiteModel();
      sourceSiteModel.SiteModelMachineDesigns.CreateNew("DesignName2");
      sourceSiteModel.SiteModelMachineDesigns.CreateNew("DesignName4");
      var sourceEventList = new ProductionEventLists(sourceSiteModel, MachineConsts.kNullInternalSiteModelMachineIndex);
      sourceEventList.MachineDesignNameIDStateEvents.PutValueAtDate(DateTime.UtcNow.AddMinutes(-60), 0);
      sourceEventList.MachineDesignNameIDStateEvents.PutValueAtDate(DateTime.UtcNow.AddMinutes(-30), 1);
      Assert.Equal(2, sourceEventList.MachineDesignNameIDStateEvents.Count());

      var targetSiteModel = new SiteModel();
      targetSiteModel.SiteModelMachineDesigns.CreateNew("DesignName2");
      targetSiteModel.SiteModelMachineDesigns.CreateNew("DesignName4");
      Assert.Equal(2, targetSiteModel.SiteModelMachineDesigns.Count);

      var targetEventList = new ProductionEventLists(targetSiteModel, MachineConsts.kNullInternalSiteModelMachineIndex);

      eventIntegrator.IntegrateMachineEvents(sourceEventList, targetEventList, false, sourceSiteModel, targetSiteModel);
      Assert.Equal(2, targetSiteModel.SiteModelMachineDesigns.Count);

      // integration re-orders the event lists so cannot locate orig by []
      Assert.Equal(0, targetSiteModel.SiteModelMachineDesigns.Locate("DesignName2").Id);
      Assert.Equal(1, targetSiteModel.SiteModelMachineDesigns.Locate("DesignName4").Id);
    }
  }
}
