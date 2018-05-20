using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Reflection;

namespace VSS.TRex
{
    /// <summary>
    /// TRex server config is intended to collect and represent configuration presented to this server instance
    /// in particular, such as its spatial subdivision role or whether it handles mutable or immutable spatial data 
    /// (ie: read-write or read-only contexts)
    /// </summary>
    public class TRexServerConfig
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

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

                string[] args = Environment.CommandLine.Split(new [] { " " }, StringSplitOptions.RemoveEmptyEntries);

                Log.LogInformation($"Number of process args provided: {args.Length}");
                if (args.Length > 0)
                {
                    Log.LogInformation($"Process args: {args.Aggregate((s1, s2) => s1 + " " + s2 + "\n")}");
                }

                instance = new TRexServerConfig();
            }

            return instance;
        }

        public TRexServerConfig()
        {
            // Pick up the parameters from command line or other sources...
        }

        /// <summary>
        /// UseMutableCellPassSegments controls whether the subgrid segments containing cell passes use a mutable structure 
        /// that permits addition/removal of cell passes (eg: in the context of processing in-bound TAG files and other 
        /// changes), or an immutable structure that favours memory allocation efficiency given read-only operations
        /// </summary>
        public bool UseMutableSpatialData { get; set; } = true;

        /// <summary>
        /// Defines whether spatial data (eg: cell pass sets for subgrid segments) should be compressed
        /// </summary>
        public bool CompressImmutableSpatialData { get; set; } = true;

        /// <summary>
        /// UseMutableSpatialData controls whether the event list and other non-spatial information in a datamodel
        /// use a mutable structure that permits addition/removal of non-spatial information or an immutable structure 
        /// that favours memory allocation efficiency given read-only operations
        /// </summary>
        public bool UseMutableNonSpatialData { get; set; } = true;

        /// <summary>
        /// Defines whether non spatial data should be compressed in it's immutable form
        /// </summary>
        public bool CompressImmutableNonSpatialData { get; set; } = true;
    }
}
