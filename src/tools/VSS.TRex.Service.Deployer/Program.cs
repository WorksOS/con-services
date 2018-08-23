using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common.Utilities;
using VSS.TRex.DI;
using VSS.TRex.Logging;
using VSS.TRex.TAGFiles.GridFabric.Services;
using VSS.TRex.Servers.Client;

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
        DIBuilder.New().AddLogging().Complete();
      }

      static void Main(string[] args)
        {
            DependencyInjection();

            // Make sure all our assemblies are loaded...
            AssembliesHelper.LoadAllAssembliesForExecutingContext();

            Log = Logger.CreateLogger<Program>();

            // Active local client Ignite node
            MutableClientServer deployServer = new MutableClientServer("ServiceDeployer");

            Log.LogInformation($"Obtaining proxy for TAG file buffer queue service");

            // Ensure the continuous query service is installed that supports TAG file processing
            TAGFileBufferQueueServiceProxy proxy = new TAGFileBufferQueueServiceProxy();
            try
            {
                Log.LogInformation($"Deploying TAG file buffer queue service");
                proxy.Deploy();
            }
            catch (Exception e)
            {
                Log.LogError($"Exception occurred deploying service: {e}");
            }

            Log.LogInformation($"Complected service deployment for TAG file buffer queue service");
        }
    }
}
