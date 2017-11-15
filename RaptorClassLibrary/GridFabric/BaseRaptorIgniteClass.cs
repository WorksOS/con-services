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
        protected readonly IIgnite _ignite;

        /// <summary>
        /// The cluster group of nodes in the grid that are available for responding to design/profile requests
        /// </summary>
        [NonSerialized]
        protected readonly IClusterGroup _group = null;

        /// <summary>
        /// The compute interface from the cluster group projection
        /// </summary>
        [NonSerialized]
        protected readonly ICompute _compute = null;

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
        public BaseRaptorIgniteClass(string Role) : this()
        {
            if (!String.IsNullOrEmpty(Role))
            {
                _group = _ignite.GetCluster().ForRemotes().ForAttribute("Role", Role);

                if (_group != null)
                {
                    _compute = _group.GetCompute();
                }
            }
        }
    }
}
