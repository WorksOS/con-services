using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.Velociraptor.DesignProfiling
{
    public enum DesignLoadResult
    {
        Success, 
        UnknownFailure,
        NoAlignmentsFound,
        UnableToLoadSubgridIndex
    }
}
