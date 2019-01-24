using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using VSS.TRex.DI;
using VSS.TRex.Machines.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.TAGFiles.Classes.Integrator;

namespace VSS.TRex.Tests.TestFixtures
{
  public class DITAGFileAndSubGridRequestsFixture : DITagFileFixture, IDisposable
  {
    public DITAGFileAndSubGridRequestsFixture() : base()
    {
      DIBuilder
        .Continue();
      //.Add()
    }

    public void Dispose()
    {
      DIBuilder.Eject();
    }

    /// <summary>
    /// Takes a list of TAG files and constructs an ephemeral site model that may be queried
    /// </summary>
    /// <param name="tagFiles"></param>
    /// <param name="ProcessedTasks"></param>
    /// <returns></returns>
    public static ISiteModel BuildModel(IEnumerable<string> tagFiles, out List<AggregatedDataIntegratorTask> ProcessedTasks)
    {
      var _tagFiles = tagFiles.ToList();

      // Convert TAG files using TAGFileConverters into mini-site models
      var converters = _tagFiles.Select(DITagFileFixture.ReadTAGFileFullPath).ToArray();

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
      ProcessedTasks = new List<AggregatedDataIntegratorTask>();
      AggregatedDataIntegratorWorker worker = new AggregatedDataIntegratorWorker(integrator.TasksToProcess)
      {
        MaxMappedTagFilesToProcessPerAggregationEpoch = _tagFiles.Count
      };
      worker.ProcessTask(ProcessedTasks);

      return targetSiteModel;
    }
  }
}
