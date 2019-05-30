using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VSS.Common.Abstractions.Configuration;
using VSS.ConfigurationStore;
using VSS.TRex.Common;
using VSS.TRex.DI;
using VSS.TRex.TAGFiles.Classes.Integrator;
using VSS.TRex.TAGFiles.GridFabric.Arguments;
using VSS.TRex.TAGFiles.GridFabric.Responses;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Executors
{
    /// <summary>
    /// Provides an executor that accepts a set of TAG files to be processed and orchestrates their processing using
    /// appropriate converters and aggregator/integrator workers.
    /// </summary>
    public static class ProcessTAGFilesExecutor
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger("ProcessTAGFilesExecutor");

        private static readonly int batchSize = DIContext.Obtain<IConfigurationStore>().GetValueInt("MAX_MAPPED_TAG_FILES_TO_PROCESS_PER_AGGREGATION_EPOCH", Consts.MAX_MAPPED_TAG_FILES_TO_PROCESS_PER_AGGREGATION_EPOCH);

        public static ProcessTAGFileResponse Execute(Guid ProjectID, Guid AssetID, IEnumerable<ProcessTAGFileRequestFileItem> TAGFiles)
        {
            var _TAGFiles = TAGFiles.ToArray(); // Enumerate collection just once

            Log.LogInformation($"ProcessTAGFileResponse.Execute. Processing {_TAGFiles.Count()} TAG files into project {ProjectID}, asset {AssetID}");

            var response = new ProcessTAGFileResponse();

            int batchCount = 0;

            // Create the machinery responsible for tracking tasks and integrating them into the database
            var integrator = new AggregatedDataIntegrator();
            var worker = new AggregatedDataIntegratorWorker(integrator.TasksToProcess, ProjectID);
            var ProcessedTasks = new List<AggregatedDataIntegratorTask>();

            // Create the site model and machine etc to aggregate the processed TAG file into
            // Note: This creates these elements within the project itself, not just class instances...
            // SiteModel siteModel = SiteModels.SiteModels.Instance(StorageMutability.Mutable).GetSiteModel(ProjectUID, true);
            // Machine machine = new Machine(null, "TestName", "TestHardwareID",  0, 0, Guid.NewGuid(), 0, false);

            // Create a list of tasks to represent conversion of each of the TAGFiles into a min-model

            Log.LogInformation($"#Progress# Initiating task based conversion of TAG files into project {ProjectID}");
            var tagFileConversions = _TAGFiles.Select(x => Task.Run(() =>
            {
                Log.LogInformation($"#Progress# Processing TAG file {x.FileName} into project {ProjectID}");

                var converter = new TAGFileConverter();

                using (var fs = new MemoryStream(x.TagFileContent))
                {
                  converter.Execute(fs);
                
                  Log.LogInformation(
                    $"#Progress# TAG file {x.FileName} generated {converter.ProcessedCellPassCount} cell passes from {converter.ProcessedEpochCount} epochs");
                }

                return (x, converter);
            })).ToList();

            // Pick off the tasks as they complete and pass the results into the integrator processor
            while (tagFileConversions.Count > 0)
            {
              async Task<(ProcessTAGFileRequestFileItem, TAGFileConverter)> nextTaskCompleted()
              {
                var theTask = await Task.WhenAny(tagFileConversions);
                tagFileConversions.Remove(theTask);
                return await theTask;
              };

              var convertedTask = nextTaskCompleted();

              var converter = convertedTask.Result.Item2;
              var TAGFile = convertedTask.Result.Item1;
              try
              {
                converter.SiteModel.ID = ProjectID;
                converter.Machine.ID = AssetID;
                converter.Machine.IsJohnDoeMachine = TAGFile.IsJohnDoe;

                integrator.AddTaskToProcessList(converter.SiteModel, ProjectID,
                  converter.Machine, AssetID,
                  converter.SiteModelGridAggregator,
                  converter.ProcessedCellPassCount,
                  converter.MachineTargetValueChangesAggregator);

                if (++batchCount >= batchSize)
                {
                  worker.ProcessTask(ProcessedTasks);
                  batchCount = 0;
                }

                response.Results.Add(new ProcessTAGFileResponseItem { FileName = TAGFile.FileName, Success = converter.ReadResult == TAGReadResult.NoError });
              }
              catch (Exception E)
              {
                response.Results.Add(new ProcessTAGFileResponseItem { FileName = TAGFile.FileName, Success = false, Exception = E.ToString() });
              }
            }

            if (batchCount > 0)
            {
                worker.ProcessTask(ProcessedTasks);
            }

            worker.CompleteTaskProcessing();

            Log.LogInformation($"#Progress# Completed task based conversion of TAG files into project {ProjectID}");

            return response;
        }
    }
}
