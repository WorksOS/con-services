using System;
using System.Collections.Generic;
using System.Text;
using VSS.TRex.Types;

namespace VSS.TRex.TAGFiles.Models
{
    public enum RequestResult
    {
        None,
        Ok,
        BadRequest,
        TFAError,
        Unexpected
    }

    public class WGS84FenceContainer
    {
        public WGS84Point[] FencePoints = null;
    }

    public class ProjectBoundaryPackage
    {
        public long ProjectID;
        public WGS84FenceContainer Boundary;
    }
}
