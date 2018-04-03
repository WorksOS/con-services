using Apache.Ignite.Core;
using Apache.Ignite.Core.Cluster;
using Apache.Ignite.Core.Compute;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Servers;

namespace VSS.VisionLink.Raptor.GridFabric
{
    [Serializable]
    public class BaseRaptorIgniteClass
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Ignite instance.
        /// Note: This was previous an [InstanceResource] but this does not work well with more than one Grid active in the process
        /// </summary>
        [NonSerialized]
        private IIgnite _ignite;

        protected IIgnite _Ignite { get { return _ignite; } }

        /// <summary>
        /// The cluster group of nodes in the grid that are available for responding to design/profile requests
        /// </summary>
        [NonSerialized]
        private IClusterGroup _group = null;

        protected IClusterGroup _Group { get { return _group; } }

        /// <summary>
        /// The compute interface from the cluster group projection
        /// </summary>
        [NonSerialized]
        private ICompute _compute = null;

        protected ICompute _Compute { get { return _compute; } }

        public string Role { get; set; } = "";

        public string GridName { get; set; }

        /// <summary>
        /// Initialises the GridName and Role parameters and uses them to establish grid connectivity and compute projections
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
        public BaseRaptorIgniteClass(string gridName, string role)
        {
            InitialiseIgniteContext(gridName, role);
        }

        /// <summary>
        /// Default no-arg constructor that throws an exception as the two arg constructor should be used
        /// </summary>
        public BaseRaptorIgniteClass()
        {
            Log.Info($"No-arg constructor BaseRaptorIgniteClass() called");
            // throw new ArgumentException("No-arg constructor invalid for BaseRaptorIgniteClass, use two-arg constructor");
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
                    Log.InfoFormat($"Ignite grid instance null after attempt to locate grid: '{GridName}'");
                }

            }
            catch (Exception E)
            {
                Log.InfoFormat($"Exception: {E}");
            }
        }

        /// <summary>
        /// Acquires references to group and compute topology projections on the Ignite grid that may accept requests from this requestor
        /// </summary>
        public void AcquireIgniteTopologyProjections()
        {
            if (!string.IsNullOrEmpty(Role))
            {
                _group = _ignite?.GetCluster().ForRemotes().ForAttribute($"{ServerRoles.ROLE_ATTRIBUTE_NAME}-{Role}", "True");
                _compute = _group?.GetCompute();
            }
        }
    }
}
