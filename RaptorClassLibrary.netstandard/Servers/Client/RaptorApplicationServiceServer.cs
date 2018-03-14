﻿using Apache.Ignite.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.Servers.Client
{
    /// <summary>
    /// Represents a server instance that client servers implmenting application service type capabilities such as
    /// tile rendering should descend from
    /// </summary>
    public class RaptorApplicationServiceServer : RaptorImmutableClientServer
    {
        public static string DEFAULT_ROLE = ServerRoles.ASNODE;

        public RaptorApplicationServiceServer() : base(new string[] { DEFAULT_ROLE })
        {

        }

        public RaptorApplicationServiceServer(string [] roles) : base(roles)
        {

        }

    }
}
