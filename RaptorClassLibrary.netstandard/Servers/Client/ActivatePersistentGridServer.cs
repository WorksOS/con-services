using Apache.Ignite.Core;
using log4net;
using System;
using System.Reflection;
using System.Threading;
using VSS.TRex.GridFabric.Grids;

namespace VSS.TRex.Servers.Client
{
    /// <summary>
    /// This class simply marks the named grid as being active when executed
    /// </summary>
    public class ActivatePersistentGridServer 
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static ActivatePersistentGridServer instance;

        /// <summary>
        /// Default constructor with role
        /// </summary>
        /// <param name="role"></param>
        public ActivatePersistentGridServer(string role) //: base(role)
        {
        }

        /// <summary>
        /// Returns the singleton instance for the activator, and creates it if necessary
        /// </summary>
        /// <returns></returns>
        public static ActivatePersistentGridServer Instance() => instance ?? (instance = new ActivatePersistentGridServer("Activator"));

        /// <summary>
        /// Set the state of a grid to active. If the grid is available and is already active, or can be set active this returns true
        /// </summary>
        /// <param name="gridName">The name of the grid to be set to active</param>
        /// <returns>True if the grid was successfully set to active, or was already in an active state</returns>
        public bool SetGridActive(string gridName)
        {
            using (RaptorClientServerFactory.NewClientNode(gridName, "Activator"))
            {
                try
                {
                    // Get an ignite reference to the named grid
                    IIgnite ignite = RaptorGridFactory.Grid(gridName);

                    // If the grid exists, and it is not active, then set it to active
                    if (ignite != null && !ignite.GetCluster().IsActive())
                    {
                        ignite.GetCluster().SetActive(true);

                        Log.InfoFormat("Set grid '{0}' to active.", gridName);

                        return true;
                    }
                    else
                    {
                        Log.InfoFormat("Grid '{0}' is not available or is already active.", gridName);

                        return ignite != null && ignite.GetCluster().IsActive();
                    }
                }
                catch (Exception E)
                {
                    Log.ErrorFormat("SetGridActive: Exception: {0}", E);
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
                IIgnite ignite = RaptorGridFactory.Grid(gridName);

                // If the grid exists, and it is active, then set it to inactive
                if (ignite != null && ignite.GetCluster().IsActive())
                {
                    ignite.GetCluster().SetActive(false);
                    Log.InfoFormat("Set grid '{0}' to inactive.", gridName);

                    return true;
                }
                else
                {
                    Log.InfoFormat("Grid '{0}' is not available or is already inactive.", gridName);

                    return ignite != null && !ignite.GetCluster().IsActive();
                }
            }
            catch (Exception E)
            {
                Log.ErrorFormat("SetGridInActive: Exception: {0}", E);
                return false;
            }
        }

        /// <summary>
        /// Wait until the grid reports itself as active
        /// </summary>
        /// <param name="gridName">The name of the grid to wait for</param>
        public void WaitUntilGridActive(string gridName)
        {
            IIgnite ignite = RaptorGridFactory.Grid(gridName);

            if (ignite == null)
            {
                Log.ErrorFormat("Grid {0} not available to wait for.", gridName);
                return;
            }

            bool isActive = false;
            do
            {
                try
                {
                    isActive = ignite.GetCluster().IsActive();

                    if (!isActive)
                    {
                        Thread.Sleep(1000);
                    }
                }
                catch
                {
                    // Ignore it and spin
                    Thread.Sleep(1000);
                }
            } while (!isActive);
        }
    }
}
