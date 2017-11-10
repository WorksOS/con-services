using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VSS.Velociraptor.DesignProfiling.GridFabric.Arguments;
using VSS.Velociraptor.DesignProfiling.GridFabric.Requests;
using VSS.VisionLink.Raptor.Common;
using VSS.VisionLink.Raptor.Rendering;
using VSS.VisionLink.Raptor.SubGridTrees.Client;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;
using VSS.VisionLink.Raptor.SubGridTrees.Utilities;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.Executors.Tasks
{
    /// <summary>
    /// A Task specialised towards rendering subgrid based information onto Plan View Map tiles
    /// </summary>
    public class PVMRenderingTask : PipelinedSubGridTask
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The tile renderer responsible for processing subgrid information into tile based thematic rendering
        /// </summary>
        public PlanViewTileRenderer TileRenderer { get; set; } = null;

        public DesignDescriptor CutFillDesign { get; set; } = DesignDescriptor.Null();

        public PVMRenderingTask(long requestDescriptor, string raptorNodeID, GridDataType gridDataType) : base(requestDescriptor, raptorNodeID, gridDataType)
        {
        }

        public PVMRenderingTask(long requestDescriptor, 
                                string raptorNodeID, 
                                GridDataType gridDataType, 
                                PlanViewTileRenderer tileRenderer, 
                                DesignDescriptor cutFillDesign) : this(requestDescriptor, raptorNodeID, gridDataType)
        {
            TileRenderer = tileRenderer;
            CutFillDesign = cutFillDesign;
        }

        public override bool TransferResponse(object response)
        {
            // Log.InfoFormat("Received a SubGrid to be processed: {0}", (response as IClientLeafSubGrid).Moniker());

            if (!base.TransferResponse(response))
            {
                Log.Warn("Base TransferResponse returned false");
                return false;
            }

            if (TileRenderer.Mode == DisplayMode.CutFill)
            {
                ClientHeightLeafSubGrid ProductionElevations = response as ClientHeightLeafSubGrid;

                // This is the old Legacy pattern of extracting surface elevations and calculating the cutfill, Alan has implemented
                // cut/fill computatation on the PSNodes now, modify this to the new approach when things are brought up to date

                // 1. Request the elevations for the matching subgrid from the grid
                // ...

                ProductionElevations.CalculateWorldOrigin(out double worldOriginX, out double worldOriginY);
                CalculateDesignElevationPatchArgument arg =
                    new CalculateDesignElevationPatchArgument(TileRenderer.DataModelID,
                                                              ProductionElevations.OriginX,
                                                              ProductionElevations.OriginY,
                                                              ProductionElevations.CellSize,
                                                              TileRenderer.CutFillDesign,
                                                              ProductionElevations.FilterMap);

                ClientHeightLeafSubGrid DesignElevations = DesignElevationPatchRequest.Execute(arg);

                // 2. Adjust the heights to be isopac elevations
                // ..
                if (DesignElevations == null)
                {
                    return false; //???? Or true?
                }

                SubGridUtilities.SubGridDimensionalIterator((x, y) =>
                {
                    if (ProductionElevations.Cells[x, y] != Consts.NullHeight && DesignElevations.Cells[x, y] != Consts.NullHeight)
                    {
                        ProductionElevations.Cells[x, y] = DesignElevations.Cells[x, y] - ProductionElevations.Cells[x, y];
                    }
                });
            }

            return TileRenderer.Displayer.RenderSubGrid(response as IClientLeafSubGrid);
        }
    }
}
