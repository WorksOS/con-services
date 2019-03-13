using Apache.Ignite.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Interfaces;

namespace VSS.TRex.GridFabric.Servers.Client
{
    /// <summary>
    /// This class simply marks the named grid as being active when executed
    /// </summary>
    public class ActivatePersistentGridServer : IActivatePersistentGridServer
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger<ActivatePersistentGridServer>();

        /// <summary>
        /// Set the state of a grid to active. If the grid is available and is already active, or can be set active this returns true
        /// </summary>
        /// <param name="gridName">The name of the grid to be set to active</param>
        /// <returns>True if the grid was successfully set to active, or was already in an active state</returns>
        public bool SetGridActive(string gridName)
        {
            using (TRexClientServerFactory.NewClientNode(gridName, "Activator"))
            {
                try
                {
                    // Get an ignite reference to the named grid
                    IIgnite ignite = DIContext.Obtain<ITRexGridFactory>()?.Grid(gridName);

                    // If the grid exists, and it is not active, then set it to active
                    if (ignite != null && !ignite.GetCluster().IsActive())
                    {
                        ignite.GetCluster().SetActive(true);

                        Log.LogInformation($"Set grid '{gridName}' to active.");

                        return true;
                    }

                    Log.LogError($"Grid '{gridName}' is not available or is already active.");
                    return ignite != null && ignite.GetCluster().IsActive();
                }
                catch (Exception E)
                {
                    Log.LogError(E, "SetGridActive: Exception:");
                    return false;
                }
            }
        }

        /// <summary>
        /// Set the state of a grid to inactive. If the grid is available and is already inactive, or can be set inactive this returns true
        /// </summary>
        /// <param name="gridName">The name of the grid to be set to inactive</param>
        /// <returns>True if the grid was successfully set to inactive, or was already in an inactive state</returns>
            public bool SetGridInActive(string gridName)
        {
            try
            {
                // Get an ignite reference to the named grid
                IIgnite ignite = DIContext.Obtain<ITRexGridFactory>()?.Grid(gridName);

                // If the grid exists, and it is active, then set it to inactive
                if (ignite != null && ignite.GetCluster().IsActive())
                {
                    ignite.GetCluster().SetActive(false);
                    Log.LogInformation($"Set grid '{gridName}' to inactive.");

                    return true;
                }

                Log.LogError($"Grid '{gridName}' is not available or is already inactive.");
                return ignite != null && !ignite.GetCluster().IsActive();
            }
            catch (Exception E)
            {
                Log.LogError(E, "SetGridInActive: Exception:");
                return false;
            }
        }

        /// <summary>
        /// Wait until the grid reports itself as active
        /// </summary>
        /// <param name="gridName">The name of the grid to wait for</param>
        public bool WaitUntilGridActive(string gridName)
        {
            IIgnite ignite = DIContext.Obtain<ITRexGridFactory>()?.Grid(gridName);

            if (ignite == null)
            {
                Log.LogError($"Grid {gridName} not available to wait for.");
                return false;
            }

            bool isActive;
            do
            {
                isActive = ignite.GetCluster().IsActive();

                if (!isActive)
                    Thread.Sleep(1000);
            } while (!isActive);

            return ignite.GetCluster().IsActive();
        }
    }
}
