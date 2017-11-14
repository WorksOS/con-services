using Apache.Ignite.Core;
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
        [InstanceResource]
        protected readonly IIgnite _ignite;

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
    }
}
