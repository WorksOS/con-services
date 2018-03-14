using Apache.Ignite.Core.Compute;
using System.Drawing;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.GridFabric.Requests;
using VSS.VisionLink.Raptor.Rendering.GridFabric.Arguments;
using VSS.VisionLink.Raptor.Rendering.GridFabric.ComputeFuncs;

namespace VSS.VisionLink.Raptor.Rendering.GridFabric.Requests
{
    /// <summary>
    /// Sends a request to the grid for a tile to be rendered
    /// </summary>
    public class TileRenderRequest : ApplicationServicePoolRequest
    {
        /// <summary>
        /// Renders a bitmap according to the parameters in its argument
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public Bitmap Execute(TileRenderRequestArgument arg)
        {
            // Construct the function to be used
            IComputeFunc<TileRenderRequestArgument, Bitmap> func = new TileRenderRequestComputeFunc();

            Task<Bitmap> taskResult = _Compute.ApplyAsync(func, arg);

            //Bitmap result = compute.Apply(func, arg);

            // Send the appropriate response to the caller
            return taskResult.Result;
        }
    }
}
