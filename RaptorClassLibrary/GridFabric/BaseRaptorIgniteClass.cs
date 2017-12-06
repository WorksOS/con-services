using Apache.Ignite.Core;
using Apache.Ignite.Core.Cluster;
using Apache.Ignite.Core.Compute;
using Apache.Ignite.Core.Resource;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.GridFabric.Grids;
using VSS.VisionLink.Raptor.Servers;

namespace VSS.VisionLink.Raptor.GridFabric
{
    public class BaseRaptorIgniteClass
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Injected Ignite instance
        /// </summary>
        [NonSerialized]
        [InstanceResource]
        private readonly IIgnite _ignite;

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

        private string Role { get; set; } = "";

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public BaseRaptorIgniteClass()
        {
            if (_ignite == null)
            {
                Log.InfoFormat($"Ignite grid instance not injected into {this}");

                try
                {
                    _ignite = Ignition.TryGetIgnite(RaptorGrids.RaptorGridName());

                    if (_ignite == null)
                    {
                        Log.InfoFormat($"Ignite grid instance still null after secondary attempt to locate grid");
                    }

                }
                catch (Exception E)
                {
                    Log.InfoFormat($"Exception: {E}");
                }
            }
        }

        /// <summary>
        /// Default no-arg constructor that sets up cluster and compute projections available for use
        /// </summary>
        public BaseRaptorIgniteClass(string role) : this()
        {
            Role = role;

            AcquireIgniteTopologyProjections();
        }

        /// <summary>
        /// Acquires references to group and compute topology projections on the Ignite grid that may accept requests from this requestor
        /// </summary>
        public void AcquireIgniteTopologyProjections()
        {
            if (!String.IsNullOrEmpty(Role))
            {
                _group = _ignite.GetCluster().ForRemotes().ForAttribute(ServerRoles.ROLE_ATTRIBUTE_NAME, Role);

                if (_group != null)
                {
                    _compute = _group.GetCompute();
                }
            }
        }
    }
}
