using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaptorSvcAcceptTestsCommon.Models
{
    public class MachineSpeedTarget
    {
        /// <summary>
        /// Sets the minimum target machine speed.
        /// </summary>
        /// <value>
        /// The minimum target machine speed.
        /// </value>
        public short MinTargetMachineSpeed { get; set; }

        /// <summary>
        /// Sets the maximum target machine speed.
        /// </summary>
        /// <value>
        /// The maximum target machine speed.
        /// </value>
        public short MaxTargetMachineSpeed { get; set; }
    }
}
