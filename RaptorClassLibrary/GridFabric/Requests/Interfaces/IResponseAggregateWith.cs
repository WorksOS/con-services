using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.GridFabric.Requests.Interfaces
{
    /// <summary>
    /// Defines the interface for a response aggregator that reduces cluster compute responses into a single response
    /// </summary>
    public interface IResponseAggregateWith<Response>
    {
        /// <summary>
        /// Aggregates the state contained in other with the state in 'this' and returns the result.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
       Response AggregateWith(Response other);
    }
}
