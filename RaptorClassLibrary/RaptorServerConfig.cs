using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor
{
    /// <summary>
    /// Raptor server config is intended to collect and represent configuration presented to this server instance
    /// in particular, such as its spatial subdivision role
    /// </summary>
    public class RaptorServerConfig
    {
        private static RaptorServerConfig instance = null;

        public static RaptorServerConfig Instance()
        {
            if (instance == null)
            {
                string[] args = Environment.CommandLine.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                instance = new RaptorServerConfig();
                instance.SpatialSubdivisionDescriptor = args.Where(x => x.Contains("SpatialDivision=")).Select(x => x.Split(new String[] { "=" }, StringSplitOptions.RemoveEmptyEntries)[1]).Select(x => Convert.ToUInt16(x)).FirstOrDefault();
            }

            return instance;
        }

        public RaptorServerConfig()
        {
            // Pick up the parameters from command line or other sources...
        }

        /// <summary>
        /// SpatialSubdivisionDescriptor records which division of the spatial data in the system this node instance is responsible
        /// for serving requests against.
        /// </summary>
        public uint SpatialSubdivisionDescriptor { get; set; } = 0;
    }
}
