using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Common;
using VSS.VisionLink.Raptor.Filters;
using VSS.VisionLink.Raptor.Geometry;
using VSS.VisionLink.Raptor.Rendering;
using VSS.VisionLink.Raptor.SubGridTrees;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.Executors
{
    public class RenderOverlayTile_Old
    {
        long DataModelID = -1;

        DisplayMode Mode = DisplayMode.Height;

        DateTime FromUTC = DateTime.MinValue;
        DateTime ToUTC = DateTime.MinValue;

        public double OriginX = Consts.NullDouble;
        public double OriginY = Consts.NullDouble;
        public double Width = Consts.NullDouble;
        public double Height = Consts.NullDouble;

        public ushort NPixelsX = 0;
        public ushort NPixelsY = 0;

//        FileSystemErrorStatus ReturnCode = FileSystemErrorStatus.UnknownErrorReadingFromFS;
//        ProductionDataExistanceMap ProdDataExistenceMap;

        // FICOptions : TSVOICOptions;

        // FTempDisplayPalette : TICDisplayPalettes;

        RequestErrorStatus ResultStatus = RequestErrorStatus.Unknown;

        BoundingWorldExtent3D RotatedTileBoundingExtents;
        // FRepresentationalDisplayer : TSVOICPVMDisplayerBase;

        public RenderOverlayTile_Old(double originX, double originY, double width, double height, ushort nPixelsX, ushort nPixelsY) : base()
        {
            OriginX = originX;
            OriginY = originY;
            Width = width;
            Height = height;
            NPixelsX = nPixelsX;
            NPixelsY = nPixelsY;

            // FICOptions := TSVOICOptions.Create;

            // Create a local palette and fill it in until we have pallette information
            // coming in through the verb.
            //FTempDisplayPalette := TICDisplayPalettes.Create;
        }

        public void Execute()
        {
            PlanViewTileRenderer Renderer;
            // DummyLiftBuildSettings: TICLiftBuildSettings;
            long RequestDescriptor = -1;
            // GroundSurfaceDetailsList GroundSurfaces = null;
            long[] SurveyedSurfaceExclusionList = new long[0];
            //MemoryStream ResultStream = null;
            ResultStatus = RequestErrorStatus.Unknown;
            // RepresentationalDisplayer = null;

            try
            {
                try
                {
                    // Get the SiteModel for the request
                    SiteModel SiteModel = SiteModels.Instance().GetSiteModel(DataModelID);
                    if (SiteModel == null)
                    {
                        throw new ArgumentException(String.Format("Unable to acquire site model instance for ID:{0}", DataModelID));
                    }

                    BoundingWorldExtent3D SpatialExtents = SiteModel.GetAdjustedDataModelSpatialExtents(SurveyedSurfaceExclusionList);

                    if (!SpatialExtents.IsValidPlanExtent)
                    {
                        ResultStatus = RequestErrorStatus.FailedToRequestDatamodelStatistics; // TODO: Or there was no date in the model
                        return;
                    }

                    // Get the current production data existance map from the sitemodel
                    SubGridTreeBitMask ProdDataExistenceMap = SiteModel.GetProductionDataExistanceMap();

                    if (ProdDataExistenceMap != null)
                    {
                        /* TODO - surveyed surfaces not supported
                         * GroundSurfaces = TICGroundSurfaceDetailsList.Create;

                          if (ASNodeImplInstance.PSLoadBalancer.LoadBalancedPSService.GetKnownGroundSurfaceFileDetails(DataModelID, GroundSurfaces) != icsrrNoError)
                            {
                               ResultStatus = RequestErrorStatus.FailedToRequestSubgridExistenceMap;
                               return;
                            }
                         */

                        RotatedTileBoundingExtents = new BoundingWorldExtent3D(OriginX, OriginY, OriginX + Width, OriginY + Height);

                        // Test to see if the tile can be satisfied with a representational render indicating where
                        // data is but not what it is (this is useful when the zoom level is far enough away that we
                        // cannot meaningfully render the data). If the size of s subgrid is smaller than
                        // the size of a pixel in the requested tile then do this. Just check the X dimension
                        // as the data display is isotropic.
                        /* TODO...
                        if (SubgridShouldBeRenderedAsRepresentationalDueToScale) then
                          begin
                            RenderTileAsRepresentationalDueToScale;
                             Exit; // There is no need to do anything else
                          end;
                        */

                        // DummyLiftBuildSettings = TICLiftBuildSettings.Create; // Dummy lift build settings

                        Renderer = new PlanViewTileRenderer();
                        Renderer.Filter1 = new CombinedFilter(SiteModel);

                        Renderer.IsWhollyInTermsOfGridProjection = true;

                        Renderer.DataModelID = DataModelID;
                        Renderer.RequestDescriptor = RequestDescriptor;
                        Renderer.SpatialExtents = SpatialExtents;
                        Renderer.CellSize = SiteModel.Grid.CellSize;

                        Renderer.Mode = Mode;
                        // Renderer.ICOptions = ICOptions;
                        // Renderer.LiftBuildSettings := DummyLiftBuildSettings;

                        Renderer.OverallExistenceMap = ProdDataExistenceMap;
                        Renderer.ProdDataExistenceMap = ProdDataExistenceMap;

                        // Renderer.DisplayPalettes := FTempDisplayPalette;

                        if (FromUTC != DateTime.MinValue && ToUTC != DateTime.MinValue && FromUTC < ToUTC)
                        {
                            Renderer.Filter1.AttributeFilter.StartTime = FromUTC;
                            Renderer.Filter1.AttributeFilter.EndTime = ToUTC;
                            Renderer.Filter1.AttributeFilter.HasTimeFilter = true;
                        }

                        Renderer.RotatedTileBoundingExtents = RotatedTileBoundingExtents;
                        Renderer.WorldTileWidth = Width;
                        Renderer.WorldTileHeight = Height;

                        Renderer.SetBounds(OriginX, OriginY, Width, Height, NPixelsX, NPixelsY);

                        ResultStatus = Renderer.PerformRender();
                        if (ResultStatus == RequestErrorStatus.OK)
                        {
                            // TODO...
                            // PackageRenderedTileIntoPNG(Renderer.Displayer);
                        }
                    }
                    else
                    {
                        ResultStatus = RequestErrorStatus.NoSuchDataModel;
                    }
                }
                finally
                {
                    throw new NotImplementedException("Renderer does not yet return the actual rendered image");
                    // SendResponseVerbToDestination(AContext, Construct_SendResponse_Args(FVerb, msmASNodeService, Ord(ResultStatus), [ResultStream], Nil, -1));
                }
            }
            catch
            {
                // TODO readd when logging available
                // SIGLogMessage.PublishNoODS(Self, Format('%s.Execute: Exception "%s"', [Self.ClassName, E.Message]), slmcException);
            }
        }
    }
}
