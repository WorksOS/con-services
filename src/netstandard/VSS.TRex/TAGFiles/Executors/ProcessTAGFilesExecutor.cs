using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using VSS.TRex.Executors;
using VSS.TRex.Executors.Executors;
using VSS.TRex.TAGFiles.Classes.Integrator;
using VSS.TRex.TAGFiles.GridFabric.Arguments;
using VSS.TRex.TAGFiles.GridFabric.Responses;
using VSS.TRex.Machines;
using VSS.TRex.SiteModels;
using VSS.TRex.Storage;

namespace VSS.TRex.TAGFiles.Executors
{
    /// <summary>
    /// Provides an executor that accepts a set of TAG files to be processed and orchestrates their processing using
    /// appropriate converters and aggregator/integrator workers.
    /// </summary>
    public static class ProcessTAGFilesExecutor
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

        public static ProcessTAGFileResponse Execute(Guid ProjectID, Guid AssetID, IEnumerable<ProcessTAGFileRequestFileItem> TAGFiles)
        {
            Log.LogInformation($"ProcessTAGFileResponse.Execute. Processing {TAGFiles.Count()} TAG files into project {ProjectID}, asset {AssetID}");

            ProcessTAGFileResponse response = new ProcessTAGFileResponse();

            const int batchSize = 20;
            int batchCount = 0;

            // Create the integration machinery responsibvle for tracking tasks and integrating them into the database
            AggregatedDataIntegrator integrator = new AggregatedDataIntegrator();
            AggregatedDataIntegratorWorker worker = new AggregatedDataIntegratorWorker(integrator.TasksToProcess);
            List<AggregatedDataIntegratorTask> ProcessedTasks = new List<AggregatedDataIntegratorTask>();

            // Create the site model and machine etc to aggregate the processed TAG file into
            // Note: This creates these elements within the project itself, not jsut class instances...
            // SiteModel siteModel = SiteModels.SiteModels.Instance(StorageMutability.Mutable).GetSiteModel(ProjectID, true);
            // Machine machine = new Machine(null, "TestName", "TestHardwareID",  0, 0, Guid.NewGuid(), 0, false);

            // Process each file into a task, and batch tasks into groups for integration to reduce the number of cache 
            // updates made for subgrid changes
            foreach (ProcessTAGFileRequestFileItem item in TAGFiles)
            {
                try
                {
                    Log.LogInformation($"#Progress# Processing TAG file {item.FileName} into project {ProjectID}");

                    TAGFileConverter converter = new TAGFileConverter();

                    using (MemoryStream fs = new MemoryStream(item.TagFileContent))
                    {
                        converter.Execute(fs);

                        Log.LogInformation($"#Progress# TAG file generated {converter.ProcessedCellPassCount} cell passes from {converter.ProcessedEpochCount} epochs");
                    }

                    converter.SiteModel.ID = ProjectID;
                    converter.Machine.ID = AssetID;
                    converter.Machine.IsJohnDoeMachine = item.IsJohnDoe;

                    integrator.AddTaskToProcessList(converter.SiteModel, converter.Machine, converter.SiteModelGridAggregator, converter.ProcessedCellPassCount, converter.MachineTargetValueChangesAggregator);

                    if (++batchCount >= batchSize)
                    {
                        worker.ProcessTask(ProcessedTasks);
                        batchCount = 0;
                    }

                    response.Results.Add(new ProcessTAGFileResponseItem { FileName = item.FileName, Success = true });
                }
                catch (Exception E)
                {
                    response.Results.Add(new ProcessTAGFileResponseItem { FileName = item.FileName, Success = false, Exception = E.ToString() });
                }
            }

            if (batchCount > 0)
            {
                worker.ProcessTask(ProcessedTasks);
            }

            return response;
        }
    }
}
