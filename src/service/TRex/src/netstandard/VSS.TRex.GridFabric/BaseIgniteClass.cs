using Apache.Ignite.Core;
using Apache.Ignite.Core.Cluster;
using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using System;
using VSS.TRex.GridFabric.Models.Servers;

namespace VSS.TRex.GridFabric
{
    public class BaseIgniteClass
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger<BaseIgniteClass>();

        /// <summary>
        /// Ignite instance.
        /// Note: This was previous an [InstanceResource] but this does not work well with more than one Grid active in the process
        /// </summary>
        private IIgnite _ignite;

      protected IIgnite _Ignite
      {
        get
        {
          if (_ignite == null)
          {
            AcquireIgniteGridReference();
            AcquireIgniteTopologyProjections();
          }

          return _ignite;
        }
      }

      /// <summary>
        /// The cluster group of nodes in the grid that are available for responding to design/profile requests
        /// </summary>
        private IClusterGroup _group;

        protected IClusterGroup _Group { get { return _group; } }

        /// <summary>
        /// The compute interface from the cluster group projection
        /// </summary>
        private ICompute _compute;

        protected ICompute _Compute { get { return _compute; } }

        public string Role { get; set; } = "";

        public string GridName { get; set; }

        /// <summary>
        /// Initializes the GridName and Role parameters and uses them to establish grid connectivity and compute projections
        /// </summary>
        /// <param name="gridName"></param>
        /// <param name="role"></param>
        public void InitialiseIgniteContext(string gridName, string role)
        {
            GridName = gridName;
            Role = role;

            AcquireIgniteGridReference();
            AcquireIgniteTopologyProjections();
        }

        /// <summary>
        /// Default no-arg constructor that sets up cluster and compute projections available for use
        /// </summary>
        public BaseIgniteClass(string gridName, string role)
        {
            InitialiseIgniteContext(gridName, role);
        }

        /// <summary>
        /// Default no-arg constructor that throws an exception as the two arg constructor should be used
        /// </summary>
        public BaseIgniteClass()
        {
            Log.LogInformation("No-arg constructor BaseIgniteClass() called");
            // throw new ArgumentException("No-arg constructor invalid for BaseIgniteClass, use two-arg constructor");
        }

        public void AcquireIgniteGridReference()
        {
            if (_ignite != null)
            {
                return; // The reference has already been acquired.
            }

            try
            {
                _ignite = Ignition.TryGetIgnite(GridName);

                if (_ignite == null)
                {
                    Log.LogInformation($"Ignite grid instance null after attempt to locate grid: '{GridName}'");
                }

            }
            catch (Exception E)
            {
                Log.LogInformation($"Exception: {E}");
            }
        }

        /// <summary>
        /// Acquires references to group and compute topology projections on the Ignite grid that may accept requests from this requestor
        /// </summary>
        public void AcquireIgniteTopologyProjections()
        {
            if (!string.IsNullOrEmpty(Role))
            {
                if (_ignite == null)
                {
                    Log.LogInformation("Ignite reference is null in AcquireIgniteTopologyProjections");
                }

                //_group = _ignite?.GetCluster().ForRemotes().ForAttribute($"{ServerRoles.ROLE_ATTRIBUTE_NAME}-{Role}", "True");
                _group = _ignite?.GetCluster().ForAttribute($"{ServerRoles.ROLE_ATTRIBUTE_NAME}-{Role}", "True");

                if (_group == null)
                {
                    Log.LogInformation($"Cluster group reference is null in AcquireIgniteTopologyProjections for role {Role} on grid {GridName}");
                }

                if (_group?.GetNodes().Count == 0)
                {
                    Log.LogInformation($"_group cluster topology is empty for role {Role} on grid {GridName}");
                }

                _compute = _group?.GetCompute();

                if (_compute == null)
                {
                    Log.LogInformation($"_compute projection is null in AcquireIgniteTopologyProjections on grid {GridName}");
                }
            }
            else
            {
                Log.LogInformation("Role name not defined when acquiring topology projection");
            }
        }
    }
}
