using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using VSS.Common.Abstractions.Configuration;
using VSS.TRex.DI;

namespace VSS.TRex.Common
{
    /// <summary>
    /// TRex server config is intended to collect and represent configuration presented to this server instance
    /// in particular, such as its spatial subdivision role or whether it handles mutable or immutable spatial data 
    /// (ie: read-write or read-only contexts)
    /// </summary>
    public class TRexServerConfig
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger<TRexServerConfig>();

        private static string PERSISTENT_CACHE_STORE_LOCATION = "PERSISTENT_CACHE_STORE_LOCATION";

        private static TRexServerConfig instance;

        /// <summary>
        /// Constructs a static instance of the configuration information supplied to the process on the command line
        /// </summary>
        /// <returns></returns>
        public static TRexServerConfig Instance()
        {
            if (instance == null)
            {
                Log.LogInformation("Creating TRexServerConfig");

                var args = Environment.CommandLine.Split(new [] { " " }, StringSplitOptions.RemoveEmptyEntries);

                Log.LogInformation($"Number of process args provided: {args.Length}");
                if (args.Length > 0)
                {
                    Log.LogInformation($"Process args: {args.Aggregate((s1, s2) => s1 + " " + s2 + "\n")}");
                }

                instance = new TRexServerConfig();
            }

            return instance;
        }

        private TRexServerConfig()
        {
            // Pick up the parameters from command line or other sources...
        }

        /// <summary>
        /// The file system location in which to store Ignite persistent data
        /// </summary>
        public static string PersistentCacheStoreLocation => DIContext.Obtain<IConfigurationStore>().GetValueString(PERSISTENT_CACHE_STORE_LOCATION, Path.Combine(Path.GetTempPath(), "TRexIgnitePersistentData"));
    }
}
