using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.NodeFilters;
using VSS.TRex.Logging;
using VSS.TRex.TAGFiles.GridFabric.Services;
using VSS.TRex.Servers.Client;
using VSS.TRex.Storage.Models;
using VSS.TRex.TAGFiles.GridFabric.NodeFilters;

namespace VSS.TRex.Service.Deployer
{
    /// <summary>
    /// This command line tool deploys all grid deployed services into the appropriate mutable and immutable
    /// data grids in TRex.
    /// </summary>
    class Program
    {
        private static ILogger Log;

      private static void DependencyInjection()
      {
        DIBuilder.New()
          .AddLogging()
          .Add(x => x.AddSingleton<ITRexGridFactory>(new TRexGridFactory()))
          .Build()
          .Add(x => x.AddSingleton(new MutableClientServer("ServiceDeployer")))
          .Add(x => x.AddSingleton(new ImmutableClientServer("ServiceDeployer")))
          .Complete();
      }

      static void Main(string[] args)
        {
            DependencyInjection();

            Log = Logger.CreateLogger<Program>();

            // Active local client Ignite node
//            MutableClientServer deployServer = new MutableClientServer("ServiceDeployer");

            Log.LogInformation("Obtaining proxy for TAG file buffer queue service");

            // Ensure the continuous query service is installed that supports TAG file processing
            var tagBufferFileQueueProxy = new TAGFileBufferQueueServiceProxy();
            try
            {
                Log.LogInformation("Deploying TAG file buffer queue service");
                tagBufferFileQueueProxy.Deploy();
            }
            catch (Exception e)
            {
                Log.LogError($"Exception occurred deploying service: {e}");
            }

            Log.LogInformation($"Complected service deployment for TAG file buffer queue service");

            var segmentRetirementProxyMutable = new SegmentRetirementQueueServiceProxyMutable();
            try
            {
              Log.LogInformation("Deploying segment retirement queue service to the mutable grid");
              segmentRetirementProxyMutable.Deploy();
            }
            catch (Exception e)
            {
              Log.LogError($"Exception occurred deploying service: {e}");
            }

            Log.LogInformation("Complected service deployment for mutable segment retirement queue service");

          /* Currently the segment retirement service is only deployed to the mutable grid
            var segmentRetirementProxyImmutable = new SegmentRetirementQueueServiceProxyImmutable();
            try
            {
              Log.LogInformation("Deploying segment retirement queue service to the immutable grid");
              segmentRetirementProxyImmutable.Deploy();
            }
            catch (Exception e)
            {
              Log.LogError($"Exception occurred deploying service: {e}");
            }
         
            Log.LogInformation("Complected service deployment for immutable segment retirement queue service");
*/
          }
    }
}
