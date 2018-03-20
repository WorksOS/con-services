using log4net;
using System.Reflection;
using VSS.Velociraptor.DesignProfiling.GridFabric.Arguments;
using VSS.Velociraptor.DesignProfiling.GridFabric.Requests;
using VSS.VisionLink.Raptor.Common;
using VSS.VisionLink.Raptor.SubGridTrees.Client;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;
using VSS.VisionLink.Raptor.SubGridTrees.Utilities;
using VSS.VisionLink.Raptor.Types;
using VSS.VisionLink.Raptor.Services.Designs;
using VSS.VisionLink.Raptor.Designs;
using VSS.VisionLink.Raptor.Executors.Tasks;
using VSS.VisionLink.Raptor.GridFabric.Requests;
using System;

namespace VSS.VisionLink.Raptor.Rendering.Executors.Tasks
{
    /// <summary>
    /// A Task specialised towards rendering subgrid based information onto Plan View Map tiles
    /// </summary>
    public class PVMRenderingTask : PipelinedSubGridTask
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Local instance of the design elevation calculation argument for use by this task
        /// </summary>
        private CalculateDesignElevationPatchArgument arg = null;

        /// <summary>
        /// Local instance of the design elevation patch request for use by this task
        /// </summary>
        private DesignElevationPatchRequest request = null;

        /// <summary>
        /// The tile renderer responsible for processing subgrid information into tile based thematic rendering
        /// </summary>
        public PlanViewTileRenderer TileRenderer { get; set; } = null;

        /// <summary>
        /// The design descriptor derived from the design ID to be used by the elevation calculation engine
        /// </summary>
        public DesignDescriptor CutFillDesign { get; set; } = DesignDescriptor.Null();

        /// <summary>
        /// Constructs the PVM renderer as well as an argument and request to be used if needing to request elevations to support cut/fill operations
        /// </summary>
        /// <param name="requestDescriptor"></param>
        /// <param name="raptorNodeID"></param>
        /// <param name="gridDataType"></param>
        /// <param name="tileRenderer"></param>
        /// <param name="cutFillDesignID"></param>
        public PVMRenderingTask(long requestDescriptor, 
                                string raptorNodeID, 
                                GridDataType gridDataType, 
                                PlanViewTileRenderer tileRenderer, 
                                long cutFillDesignID /*DesignDescriptor cutFillDesign*/) : base(requestDescriptor, raptorNodeID, gridDataType)
        {
            TileRenderer = tileRenderer;

            if (TileRenderer.Mode == DisplayMode.CutFill)
            {
                var Designs = DesignsService.Instance().List(tileRenderer.DataModelID);

                arg = new CalculateDesignElevationPatchArgument()
                {
                    SiteModelID = tileRenderer.DataModelID,
                    DesignDescriptor = Designs.Find(x => x.ID == cutFillDesignID).DesignDescriptor
                };

                request = new DesignElevationPatchRequest();
            }
        }

        public override bool TransferResponse(object response)
        {
            // Log.InfoFormat("Received a SubGrid to be processed: {0}", (response as IClientLeafSubGrid).Moniker());

            if (!base.TransferResponse(response))
            {
                Log.Warn("Base TransferResponse returned false");
                return false;
            }

            // If the display mode is cut/fill, perform a side lookup to convert the height information into cut/fill...
            if (TileRenderer.Mode == DisplayMode.CutFill)
            {
                IClientLeafSubGrid[] ProductionElevationsList = response as IClientLeafSubGrid[];

                if (ProductionElevationsList == null || ProductionElevationsList.Length == 0)
                {
                    Log.Warn($"Response is null or does not contain a cut/fill subgrid");

                    if (response != null)
                    {
                        Log.Info($"Type of reponse object provided to TransferResponse: {response.GetType()}");
                    }

                    return false;
                }

                ClientHeightLeafSubGrid ProductionElevations = (ClientHeightLeafSubGrid)ProductionElevationsList[0];

                if (ProductionElevations == null)
                {
                    Log.Info("Failed to get ProductionElevations from IClientLeafSubGrid");
                    return false;
                }

                // This is the old Legacy pattern of extracting surface elevations and calculating the cutfill, Alan Rose has implemented
                // cut/fill computatation on the PSNodes now, modify this to the new approach when things are brought up to date

                // 1. Request the elevations for the matching subgrid from the grid
                arg.OriginX = ProductionElevations.OriginX;
                arg.OriginY = ProductionElevations.OriginY;
                arg.CellSize = ProductionElevations.CellSize;
                // arg.ProcessingMap = ProductionElevations.FilterMap; 

                ClientHeightLeafSubGrid DesignElevations = request.Execute(arg);

                // 2. Adjust the heights to be isopac elevations
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

            return TileRenderer.Displayer.RenderSubGrid((response as IClientLeafSubGrid[])[0]);
        }

        /// <summary>
        /// Transfers a set of subgrids responses into the processing task
        /// </summary>
        /// <param name="responses"></param>
        /// <returns></returns>
        public override bool TransferResponses(object [] responses)
        {
            foreach (Object response in responses)
            {
                if (!TransferResponse(response))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
