using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Filters;
using VSS.VisionLink.Raptor.Geometry;
using VSS.VisionLink.Raptor.Rendering;
using VSS.VisionLink.Raptor.SubGridTrees;
using VSS.VisionLink.Raptor.Types;
using VSS.VisionLink.Raptor.Utilities;
using VSS.VisionLink.Raptor.SiteModels;

namespace VSS.VisionLink.Raptor.Executors
{
    public class RenderOverlayTile
    {
        /// <summary>
        /// The Raptor application service node performing the request
        /// </summary>
        string RequestingRaptorNodeID { get; set; } = String.Empty;

        long DataModelID = -1;
        long MachineID = -1;
        //FExternalDescriptor :TASNodeRequestDescriptor;

        DisplayMode Mode = DisplayMode.Height;

        bool CoordsAreGrid = true;

        XYZ BLPoint; // : TWGS84Point;
        XYZ TRPoint; // : TWGS84Point;

        ushort NPixelsX = 0;
        ushort NPixelsY = 0;

        CombinedFilter Filter1 = null;
        CombinedFilter Filter2 = null;

        //    FFilter1IsLocal : Boolean;
        //    FFilter2IsLocal : Boolean;
        //    FDesignDescriptor : TVLPDDesignDescriptor;
        //    ComputeICVolumesType ReferenceVolumeType = ComputeICVolumesType.None;
        //    FColourPalettes: TColourPalettes;
        //    ICOptions ICOptions = new ICOptions();
        //    Color RepresentColor = Color.Black;

        PlanViewTileRenderer Renderer = null;

        public RenderOverlayTile(long ADataModelID,
                                  //AExternalDescriptor :TASNodeRequestDescriptor;
                                  DisplayMode AMode,
                                 XYZ ABLPoint, // : TWGS84Point;
                                 XYZ ATRPoint, // : TWGS84Point;
                                 bool ACoordsAreGrid,
                                 ushort ANPixelsX,
                                 ushort ANPixelsY,
                                 CombinedFilter AFilter1,
                                 CombinedFilter AFilter2,
                                 //ADesignDescriptor : TVLPDDesignDescriptor;
                                 //AReferenceVolumeType : TComputeICVolumesType;
                                 //AColourPalettes: TColourPalettes;
                                 //AICOptions: TSVOICOptions;
                                 //ARepresentColor: LongWord
                                 string requestingRaptorNodeID
                                 )
        {
            DataModelID = ADataModelID;
            // ExternalDescriptor = AExternalDescriptor
            Mode = AMode;
            BLPoint = ABLPoint;
            TRPoint = ATRPoint;
            CoordsAreGrid = ACoordsAreGrid;
            NPixelsX = ANPixelsX;
            NPixelsY = ANPixelsY;
            Filter1 = AFilter1;
            Filter2 = AFilter2;
            //DesignDescriptor = ADesignDescriptor;
            //ReferenceVolumeType = AReferenceVolumeType;
            //ColourPalettes = AColourPalettes;
            //ICOptions = AICOptions;
            //RepresentColor = ARepresentColor
            RequestingRaptorNodeID = requestingRaptorNodeID;
        }

        SubGridTreeBitMask OverallExistenceMap = null;

        RequestErrorStatus ResultStatus = RequestErrorStatus.Unknown;

        BoundingWorldExtent3D RotatedTileBoundingExtents;

        bool SurveyedSurfacesExludedViaTimeFiltering = true;

        /* TODO
         private TICDisplayPaletteBaseClass ColourPaletteClassType()
                {
            case FMode of
              icdmHeight                  : Result  = TICDisplayPalette_Height;
              icdmCCV                     : Result  = TICDisplayPalette_CCV;
              icdmCCVPercent              : Result  = TICDisplayPalette_CCVPercent;
              icdmLatency                 : Result  = TICDisplayPalette_RadioLatency;
              icdmPassCount               : Result  = TICDisplayPalette_PassCount;
              icdmPassCountSummary        : Result  = TICDisplayPalette_PassCountSummary;  // Palettes are fixed three colour palettes - displayer will use direct transitions
              icdmRMV                     : Result  = TICDisplayPalette_RMV;
              icdmFrequency               : Result  = TICDisplayPalette_Frequency;
              icdmAmplitude               : Result  = TICDisplayPalette_Amplitude;
              icdmCutFill                 : Result  = TICDisplayPalette_CutFill;
              icdmMoisture                : Result  = TICDisplayPalette_Moisture;
              icdmTemperatureSummary      : Result  = TICDisplayPaletteBase; //TICDisplayPalette_Temperature;
              icdmGPSMode                 : Result  = TICDisplayPaletteBase; //TICDisplayPalette_GPSMode;
              icdmCCVSummary              : Result  = TICDisplayPaletteBase; //TICDisplayPalette_CCVSummary;
              icdmCCVPercentSummary       : Result  = TICDisplayPalette_CCVPercent;
              icdmCompactionCoverage      : Result  = TICDisplayPalette_CoverageOverlay;
              icdmVolumeCoverage          : Result  = TICDisplayPalette_VolumeOverlay;
              icdmMDP                     : Result  = TICDisplayPalette_MDP; // ajr15167
              icdmMDPSummary              : Result  = TICDisplayPaletteBase;
              icdmMDPPercent              : Result  = TICDisplayPalette_MDPPercent;
              icdmMDPPercentSummary       : Result  = TICDisplayPalette_MDPPercent;
              icdmMachineSpeed            : Result  = TICDisplayPalette_MachineSpeed;
              icdmCCVPercentChange        : Result  = TICDisplayPalette_CCVPercent;
              icdmTargetThicknessSummary  : Result  = TICDisplayPalette_VolumeOverlay;
              icdmTargetSpeedSummary      : Result  = TICDisplayPalette_SpeedSummary;
              icdmCCVChange               : Result  = TICDisplayPalette_CCVChange;
              icdmCCA                     : Result  = TICDisplayPalette_CCA;
              icdmCCASummary              : Result  = TICDisplayPalette_CCASummary;

            else
              SIGLogMessage.PublishNoODS(Self, Format('ColourPaletteClassType: Unknown display type: %d', [Ord(FMode)]), slmcAssert);
              Result  = TICDisplayPaletteBase;
            end;
          end;
        */

        /* TODO
          function ComputeCCAPalette :Boolean;
          var
            I, J, K               :Integer;
            ResponseVerb        :TRPCVerbBase;
            ServerResult        :TICServerRequestResult;
            ResponseDataStream  :TStream;
            CCAMinimumPasses    :TICCCAMinPassesValue;
            CCAColorScale       :TCCAColorScale;
            CCAPalette          :TColourPalettes;

          begin
            Result  = False;

            ResponseVerb  = nil;
            try
              if Length(FFilter1.Machines) > 0 then
                FMachineID  = FFilter1.Machines[0].ID // Must be set by caller
              else
                FMachineID  = -1; // will fail call
              if not Assigned(ASNodeImplInstance) or ASNodeImplInstance.ServiceStopped then
                begin
                  SIGLogMessage.PublishNoODS(Self, Format('%s.Execute: Aborting request as service has been stopped', [Self.ClassName]), slmcWarning);
                  Exit;
                end;

              ASNodeImplInstance.PSLoadBalancer.LoadBalancedPSService.GetMachineCCAMinimumPassesValue(FDataModelID, FMachineID, FFilter1.StartTime, FFilter1.EndTime, FFilter1.LayerID, ResponseVerb);
              if Assigned(ResponseVerb) then
                with ResponseVerb as TVLPDRPCVerb_SendResponse do
                  begin
                    ServerResult  = TICServerRequestResult(ResponseCode);
                ResponseDataStream  = ResponseData;
                    if (ServerResult = icsrrNoError) and assigned(ResponseData) then
                      begin
                        CCAMinimumPasses  = ReadSmallIntFromStream(ResponseDataStream);

                Result  = CCAMinimumPasses > 0;

                        if not Result then
                          Exit;

                CCAColorScale  = TCCAColorScaleManager.CreateCoverageScale(CCAMinimumPasses);
                        try
                          SetLength(CCAPalette.Transitions, CCAColorScale.TotalColors);

                J  = Low(CCAPalette.Transitions);
                k  = High(CCAPalette.Transitions);
                          for I  = J to K do
                            begin
                              CCAPalette.Transitions[I].Colour  = CCAColorScale.ColorSegments[K - I].Color;
                CCAPalette.Transitions[I].Value   = I+1;
                            end;
                          CCAPalette.ConvertRGBToBGR; // gets done again but needed to match Anatoli palette test :)
                          WorkingColourPalette.PopulateFromPaletteColours(CCAPalette);
                          WorkingColourPalette.TransitionColours.ValuesCount  = Length(CCAPalette.Transitions);
                        finally
                          if Assigned(CCAColorScale) then
                            FreeAndNil(CCAColorScale);
                end;
                      end
                    else
                      SIGLogMessage.PublishNoODS(Self, Format('%s.Execute: GetMachineCCAMinimumPassesValue Failed for MachineID: %d. ReturnCode:%d', [Self.ClassName, FMachineID, Ord(ServerResult)]), slmcWarning);
                  end;
            finally
              if Assigned(ResponseVerb) then
                FreeAndNil(ResponseVerb);
                end;
          end;
        */

        /* TODO
         function CreateAndInitialiseWorkingColourPalette :Boolean;
         begin
           Result  = True;

           // Create a scaled palette to use when rendering the data
           try
             if ColourPaletteClassType<> Nil then
              begin

                 WorkingColourPalette  = ColourPaletteClassType.Create;
                 WorkingColourPalette.SmoothPalette  = FMode = icdmCutFill;

                 // CCASummary is done per machineid
                 if FMode in [icdmCCA, icdmCCASummary]
               then
                   Result  = ComputeCCAPalette
                 else
                   begin
                     if Length(FColourPalettes.Transitions) = 0 then
                       WorkingColourPalette.SetToDefaults
                     else
                       WorkingColourPalette.PopulateFromPaletteColours(FColourPalettes);
               end;

                 if Result then
                   WorkingColourPalette.ComputePalette;
               end
             else
               WorkingColourPalette  = Nil;

           Except
             On e:Exception do
               SIGLogMessage.PublishNoODS(Self, Format('%s.Execute: Error: %s ', [Self.ClassName, e.Message]), slmcException);
           end;
         end;
       */

        /* TODO
          Function ProcessGroundSurfacesForFilter(const Filter : TICFilterSettings;
            const ComparisonList : TICGroundSurfaceDetailsList;
                                                  const FilteredGroundSurfaces : TICGroundSurfaceDetailsList) : Boolean;
          var
            GroundSurfaceExistanceMap : TSubGridTreeBitMask;
            ShouldFreeGroundSurfaceMap : Boolean;
          begin
            Result  = False;
            ShouldFreeGroundSurfaceMap  = False;
            GroundSurfaceExistanceMap  = Nil;

            // Filter out any ground surfaces which don't match current filter (if any) - realistically, this is time filters we're thinking of here
            GroundSurfaces.FilterGroundSurfaceDetails(Filter.HasTimeComponent, Filter.StartTime, Filter.EndTime,
                                                      Filter.ExcludeSurveyedSurfaces,
                                                      FilteredGroundSurfaces,
                                                      Filter.SurveyedSurfaceExclusionList);

            if FilteredGroundSurfaces.IsSameAs(ComparisonList) then
              Result  = True
            else
              try
                if ASNodeImplInstance.DesignProfilerService.RequestCombinedDesignSubgridIndexMap(FDataModelID, CellSize, FilteredGroundSurfaces, GroundSurfaceExistanceMap, ShouldFreeGroundSurfaceMap) = dppiOK then
                  OverallExistenceMap.SetOp_OR(GroundSurfaceExistanceMap)
                else
                  Exit;

                Result  = True;
              finally
                if Assigned(GroundSurfaceExistanceMap) and ShouldFreeGroundSurfaceMap then
                  FreeAndNil(GroundSurfaceExistanceMap);
                end;
          end;
        */

        public Bitmap Execute()
        {
            XYZ[] NEECoords = null;
            XYZ[] LLHCoords = null;
            BoundingWorldExtent3D SpatialExtents;
            double CellSize;
            int IndexOriginOffset;
            MemoryStream ResultStream = null;
            //  WorkingColourPalette  : TICDisplayPaletteBase;
            //  CoordConversionResult : TCoordServiceErrorStatus;
            long RequestDescriptor;
            bool ScheduledWithGovernor = false;
            // DesignProfilerResult  : TDesignProfilerRequestResult;
            SubGridTreeBitMask ProdDataExistenceMap = null; //: TProductionDataExistanceMap;
            FileSystemErrorStatus ReturnCode;
            SubGridTreeBitMask DesignSubgridOverlayMap = null;
            SubGridTreeBitMask LiftDesignSubgridOverlayMap = null;
            // GroundSurfaces: TICGroundSurfaceDetailsList;
            // Filter1GroundSurfaces : TICGroundSurfaceDetailsList;
            // Filter2GroundSurfaces : TICGroundSurfaceDetailsList;
            bool ShouldFreeDesignSubgridIndexMap = false;
            bool ShouldFreeLiftDesignSubgridIndexMap = false;
            double TileRotation;
            double WorldTileWidth, WorldTileHeight;
            long[] SurveyedSurfaceExclusionList = new long[0];

            double dx, dy;

            /*
               if not Assigned(ASNodeImplInstance) or ASNodeImplInstance.ServiceStopped then
                begin
                  SIGLogMessage.PublishNoODS(Self, Format('%s.Execute: Aborting request as service has been stopped', [Self.ClassName]), slmcWarning);
                  Exit;
                end;
            */

            // TODO readd whenlogging available
            //SIGLogMessage.PublishNoODS(Self, Format('Performing %s.Execute for DataModel:%d, Mode=%d', [Self.ClassName, FDataModelID, Integer(FMode)]), slmcMessage);

            // TODO InterlockedIncrement64(ASNodeRequestStats.NumMapTileRequests);

            /* TODO
           if Assigned(ASNodeImplInstance.RequestCancellations) and
              ASNodeImplInstance.RequestCancellations.IsRequestCancelled(FExternalDescriptor) then
             begin
               if VLPDSvcLocations.Debug_LogDebugRequestCancellationToFile then
                 SIGLogMessage.PublishNoODS(Self, 'Request cancelled: ' + FExternalDescriptor.ToString, slmcDebug);

               ResultStatus  = asneRequestHasBeenCancelled;
               InterlockedIncrement64(ASNodeRequestStats.NumMapTileRequestsCancelled);
             Exit;
             end;
             */

            // The governor is intended to restrict the numbers of heavy weight processes
            // such as pipelines that interact with the PC layer to request subgrids
            /* TODO
            ScheduledWithGovernor  = ASNodeImplInstance.Governor.Schedule(FExternalDescriptor, Self, gqWMS, ResultStatus);
            if not ScheduledWithGovernor then
              Exit;
             */

            RequestDescriptor = Guid.NewGuid().GetHashCode(); // TODO ASNodeImplInstance.NextDescriptor;

            /* TODO Readd wen logging available
          if VLPDSvcLocations.Debug_EmitTileRenderRequestParametersToLog then
            begin
              if FCoordsAreGrid then
                begin
                  if Assigned(FFilter2) then
                    SIGLogMessage.PublishNoODS(Self,
                                               Format('RenderPlanViewTiles Execute: Performing render for Descriptor=%d ' +
                                                      'Args: Project=%d, Mode=%d, Filter1=%d, Filter2=%d, Design=''%s'', VolType=%d, Bound[BL/TR:X/Y]=(%.10f, %.10f, %.10f, %.10f), Width=%d, Height=%d',
                                                      [RequestDescriptor,
                                                       FDataModelID,
                                                       Integer(FMode),
                                                       FFilter1.FilterID, FFilter2.FilterID,
                                                       FDesignDescriptor.ToString,
                                                       Ord(FReferenceVolumeType),
                                                       FBLPoint.Lon, FBLPoint.Lat, FTRPoint.Lon, FTRPoint.Lat,
                                                       FNPixelsX, FNPixelsY]),
                                               slmcMessage)
                  else
                    SIGLogMessage.PublishNoODS(Self,
                                               Format('RenderPlanViewTiles Execute: Performing render for Descriptor=%d ' +
                                                      'Args: Project=%d, Mode=%d, Filter1=%d Design=''%s'', VolType=%d, Bound[BL/TR:X/Y]=(%.10f, %.10f, %.10f, %.10f), Width=%d, Height=%d',
                                                      [RequestDescriptor,
                                                       FDataModelID,
                                                       Integer(FMode),
                                                       FFilter1.FilterID,
                                                       FDesignDescriptor.ToString,
                                                       Ord(FReferenceVolumeType),
                                                       FBLPoint.Lon, FBLPoint.Lat, FTRPoint.Lon, FTRPoint.Lat,
                                                       FNPixelsX, FNPixelsY]),
                                               slmcMessage);
                end
              else
                begin
                  if Assigned(FFilter2) then
                    SIGLogMessage.PublishNoODS(Self,
                                               Format('RenderPlanViewTiles Execute: Performing render for Descriptor=%d ' +
                                                      'Args: Project=%d, Mode=%d, Filter1=%d, Filter2=%d, Design=''%s'', VolType=%d, Bound[BL/TR:Lon/Lat]=(%.10f, %.10f, %.10f, %.10f), Width=%d, Height=%d',
                                                      [RequestDescriptor,
                                                       FDataModelID,
                                                       Integer(FMode),
                                                       FFilter1.FilterID, FFilter2.FilterID,
                                                       FDesignDescriptor.ToString,
                                                       Ord(FReferenceVolumeType),
                                                       FBLPoint.Lon * (180/PI), FBLPoint.Lat* (180/PI), FTRPoint.Lon* (180/PI), FTRPoint.Lat* (180/PI),
                                                       FNPixelsX, FNPixelsY]),
                                               slmcMessage)
                  else
                    SIGLogMessage.PublishNoODS(Self,
                                               Format('RenderPlanViewTiles Execute: Performing render for Descriptor=%d ' +
                                                      'Args: Project=%d, Mode=%d, Filter1=%d, Design=''%s'', VolType=%d, Bound[BL/TR:Lon/Lat]=(%.10f, %.10f, %.10f, %.10f), Width=%d, Height=%d',
                                                      [RequestDescriptor,
                                                       FDataModelID,
                                                       Integer(FMode),
                                                       FFilter1.FilterID,
                                                       FDesignDescriptor.ToString,
                                                       Ord(FReferenceVolumeType),
                                                       FBLPoint.Lon * (180/PI), FBLPoint.Lat* (180/PI), FTRPoint.Lon* (180/PI), FTRPoint.Lat* (180/PI),
                                                       FNPixelsX, FNPixelsY]),
                                               slmcMessage);
                end;

              // Include the details of the filter with the logged tile parameters
              SIGLogMessage.PublishNoODS(Self, Format('Filter1: %s', [IfThen(FFilter1.IsNull, 'Null', FFilter1.ToString)]), slmcMessage);
              if Assigned(FFilter2) then
                SIGLogMessage.PublishNoODS(Self, Format('Filter2: %s', [IfThen(FFilter2.IsNull, 'Null', FFilter2.ToString)]), slmcMessage);
            end;
            */

            // Determine the grid (NEE) coordinates of the bottom/left, top/right WGS-84 positions
            // given the coordinate system assigned to the project. If there is no coordinate system
            // then this is where we exit

            LLHCoords = new XYZ[4]
            {
              new XYZ(BLPoint.X, BLPoint.Y),    // BL
              new XYZ(TRPoint.X, TRPoint.Y),    // TR
              new XYZ(BLPoint.X, TRPoint.Y),    // TL
              new XYZ(TRPoint.X, BLPoint.Y)     // BR
            };

            if (CoordsAreGrid)
            {
                NEECoords = LLHCoords;
            }
            else
            {
                /* TODO
                CoordConversionResult = ASNodeImplInstance.CoordService.RequestCoordinateConversion(RequestDescriptor, DataModelID, cctLLHtoNEE, LLHCoords, EmptyStr, NEECoords);
                if (CoordConversionResult != csOK)
                {
                    ResultStatus = RequestErrorStatus.FailedToConvertClientWGSCoords;
                    return;
                }
                */
            }

            WorldTileHeight = MathUtilities.Hypot(NEECoords[0].X - NEECoords[2].X, NEECoords[0].Y - NEECoords[2].Y);
            WorldTileWidth = MathUtilities.Hypot(NEECoords[0].X - NEECoords[3].X, NEECoords[0].Y - NEECoords[3].Y);

            dx = NEECoords[2].X - NEECoords[0].X;
            dy = NEECoords[2].Y - NEECoords[0].Y;
            TileRotation = (Math.PI / 2) - Math.Atan2(dy, dx);

            RotatedTileBoundingExtents.SetInverted();
            foreach (XYZ xyz in NEECoords)
            {
                RotatedTileBoundingExtents.Include(xyz.X, xyz.Y); 
            }

            if (Filter1 != null)
            {   
                // TODO not supporting surveyed surfaces ATM
                // SurveyedSurfaceExclusionList = Copy(Filter1.SurveyedSurfaceExclusionList);
            }

            // Get the SiteModel for the request
            SiteModel SiteModel = SiteModels.SiteModels.Instance().GetSiteModel(DataModelID);
            if (SiteModel == null)
            {
                throw new ArgumentException(String.Format("Unable to acquire site model instance for ID:{0}", DataModelID));
            }

            SpatialExtents = SiteModel.GetAdjustedDataModelSpatialExtents(SurveyedSurfaceExclusionList);

            if (!SpatialExtents.IsValidPlanExtent)
            {
                ResultStatus = RequestErrorStatus.FailedToRequestDatamodelStatistics; // TODO: Or there was no date in the model
                return null;
            }

            // Get the current production data existance map from the sitemodel
            ProdDataExistenceMap = SiteModel.GetProductionDataExistanceMap();

            if (ProdDataExistenceMap == null)
            {
                ResultStatus = RequestErrorStatus.FailedToRequestSubgridExistenceMap;
                return null;
            }

            /*
            if (ASNodeImplInstance.PSLoadBalancer.LoadBalancedPSService.GetDataModelSpatialExtents(DataModelID, SurveyedSurfaceExclusionList, SpatialExtent, CellSize, IndexOriginOffset) != icsrrNoError)
            {
                // TODO readd whenlogging available
                //SIGLogMessage.PublishNoODS(Self, Format('Tile render rejected due to FailedToRequestDatamodelStatistics: Datamodel=%d',                
                //                                        [FDataModelID]), slmcError);
                ResultStatus = RequestErrorStatus.FailedToRequestDatamodelStatistics;
                return;
            }
            */

            // Intersect the site model extents with the extents requested by the caller
            if (!SpatialExtents.Intersect(RotatedTileBoundingExtents).IsValidPlanExtent)
            {
                ResultStatus = RequestErrorStatus.InvalidCoordinateRange;

                // RenderTransparentTile();
                return new Bitmap(NPixelsX, NPixelsY);
            }

            // Obtain the subgrid existence map for the project
            // Retrieve the existence map for the datamodel
            OverallExistenceMap = new SubGridTreeBitMask(SubGridTree.SubGridTreeLevels - 1, SubGridTree.SubGridTreeDimension * SiteModel.Grid.CellSize);

            /* TODO - surveyed surfaces not supported
            if (DisplayModeRequireSurveyedSurfaceInformation(Mode))
            {
                GroundSurfaces = TICGroundSurfaceDetailsList.Create;
                Filter1GroundSurfaces = TICGroundSurfaceDetailsList.Create;
                Filter2GroundSurfaces = TICGroundSurfaceDetailsList.Create;
                try
                {
                    if ASNodeImplInstance.PSLoadBalancer.LoadBalancedPSService.GetKnownGroundSurfaceFileDetails(FDataModelID, GroundSurfaces) <> icsrrNoError then
                      begin
                      ResultStatus = asneFailedToRequestSubgridExistenceMap;
                    Exit;
                    end;

                    if not ProcessGroundSurfacesForFilter(FFilter1, Filter2GroundSurfaces, Filter1GroundSurfaces) then
                      begin
                      ResultStatus = asneFailedToRequestSubgridExistenceMap;
                    Exit;
                    end;

                    if Assigned(FFilter2) then
                    if not ProcessGroundSurfacesForFilter(FFilter2, Filter1GroundSurfaces, Filter2GroundSurfaces) then
                    begin
                        ResultStatus = asneFailedToRequestSubgridExistenceMap;
                    Exit;
                    end;

                    FSurveyedSurfacesExludedViaTimeFiltering = (Filter1GroundSurfaces.Count = 0) and(Filter2GroundSurfaces.Count = 0);
                }
                finally
                {
                    GroundSurfaces.Free;
                    Filter1GroundSurfaces.Free;
                    Filter2GroundSurfaces.Free;
                }
            }
            */

            OverallExistenceMap.SetOp_OR(ProdDataExistenceMap);

            if (Filter1 != null && Filter1.AttributeFilter.HasElevationRangeFilter && !Filter1.AttributeFilter.ElevationRangeDesign.IsNull)
            {
                /* TODO Design profiler access not supported
                DesignProfilerResult = ASNodeImplInstance.DesignProfilerService.RequestDesignSubgridIndexMap(DataModelID, CellSize, Filter1.AttributeFilter.ElevationRangeDesign, LiftDesignSubgridOverlayMap, ShouldFreeLiftDesignSubgridIndexMap);

                if (DesignProfilerResult == dppiOK)
                {
                    OverallExistenceMap.SetOp_AND(LiftDesignSubgridOverlayMap);
                }
                */
            }

            if (Filter1 != null && !Filter1.AttributeFilter.AnyFilterSelections)
            {
                if (Filter1.AttributeFilter.OverrideTimeBoundary && Filter1.AttributeFilter.EndTime == DateTime.MinValue)
                {
                    if (Filter2 != null && !Filter2.AttributeFilter.AnyFilterSelections)
                    {
                        // fix SV bug. Setup Filter 1 to look for early cell pass
                        Filter1.AttributeFilter.StartTime = DateTime.MinValue;
                        Filter1.AttributeFilter.EndTime = Filter2.AttributeFilter.StartTime;
                    }
                }

                ResultStatus = FilterUtilities.PrepareFilterForUse(Filter1, DataModelID);
                if (ResultStatus != RequestErrorStatus.OK)
                {
                    return null; 
                }
            }

            if (Filter2 != null && !Filter2.AttributeFilter.AnyFilterSelections)
            {
                ResultStatus = FilterUtilities.PrepareFilterForUse(Filter2, DataModelID);
                if (ResultStatus != RequestErrorStatus.OK)
                {
                    return null;
                }
            }

            /* TODO Readd when logging available
               {$IFDEF DEBUG}
                SIGLogMessage.PublishNoODS(Self,
                                          Format('(%d) Tile render executing across tile: [Rotation:%.3fdeg] [BL:%.3f, %.3f, TL:%.3f, %.3f, TR:%.3f, %.3f, BR:%.3f, %.3f](%.3f x %.3f), Lat/Lon = [%.8f, %.8f, %.8f, %.8f](%.8f x %.8f)',
                                                 [FDataModelID,
                                                  TileRotation * (180/PI),
                                                  NEECoords[0].X, NEECoords[0].Y,
                                                  NEECoords[2].X, NEECoords[2].Y,
                                                  NEECoords[1].X, NEECoords[1].Y,
                                                  NEECoords[3].X, NEECoords[3].Y,
                                                  NEECoords[1].X - NEECoords[0].X, NEECoords[1].Y - NEECoords[0].Y,
                                                  FBLPoint.Lon* (180/PI), FBLPoint.Lat* (180/PI), FTRPoint.Lon* (180/PI), FTRPoint.Lat* (180/PI),
                                                  (FTRPoint.Lon - FBLPoint.Lon) * (180/PI), (FTRPoint.Lat - FBLPoint.Lat) * (180/PI)]),
                                          slmcDebug);
               {$ENDIF}
            */

            /* TODO Design file access is not yet supported
            // If this render involves a relationship with a design then ensure the existance map
            // for the design is loaded in to memory to allow the request pipeline to confine
            // subgrid requests that overlay the actual design
            if (RequestRequiresAccessToDesignFileExistanceMap(Mode, ReferenceVolumeType))
            {
                if (FDesignDescriptor.IsNull)
                {
                    // TODO readd when logging available
                    //SIGLogMessage.PublishNoODS(Self, Format('No design provided to cut fill, summary volume or thickness overlay render request for datamodel %d', [FDataModelID]), slmcError);
                    ResultStatus = RequestErrorStatus.NoDesignProvided;
                    return;
                }

                DesignProfilerResult = ASNodeImplInstance.DesignProfilerService.RequestDesignSubgridIndexMap(FDataModelID, CellSize, FDesignDescriptor, FDesignSubgridOverlayMap, ShouldFreeDesignSubgridIndexMap);

                if (DesignProfilerResult == dppiOK)
                {
                    FDesignSubgridOverlayMap.CellSize = kSubGridTreeDimension * CellSize;
                }
                else
                {
                    // TODO readd whenloggin available
                   // SIGLogMessage.PublishNoODS(Self, Format('Failed to request subgrid overlay index for design %s in datamodel %d (error %s)', [FDesignDescriptor.ToString, FDataModelID, DesignProfilerErrorStatusName(DesignProfilerResult)]), slmcError);
                    return;
                }
            }
            */

            // Construct the renderer, configure it, and set it on its way
            //  WorkingColourPalette = Nil;
            Renderer = new PlanViewTileRenderer(RequestingRaptorNodeID);
            try
            {
                /* TODO 
                // Create a scaled palette to use when rendering the data
                if not CreateAndInitialiseWorkingColourPalette then
                  begin
                  SIGLogMessage.PublishNoODS(Self, Format('Failed to create and initialise working colour palette for data: %s in datamodel %d', [TypInfo.GetEnumName(TypeInfo(TICDisplayMode), Ord(FMode)), FDataModelID]), slmcWarning);
                  Exit;
                  end;
                */

                ////////////////////// CONFIGURE THE RENDERER - START

                Renderer.DataModelID = DataModelID;
                Renderer.RequestDescriptor = RequestDescriptor;
                // Renderer.ExternalDescriptor  = FExternalDescriptor;
                Renderer.SpatialExtents = SpatialExtents;
                Renderer.CellSize = SiteModel.Grid.CellSize;

                Renderer.Mode = Mode;
                // Renderer.ICOptions  = FICOptions;

                /* TODO Lift build settings not supported
                if (Filter1 != null && Filter1.AttributeFilter.HasLayerStateFilter || Filter1.AttributeFilter.LayerMethod != TFilterLayerMethod.flmNone)
                {
                    Renderer.LiftBuildSettings = ICOptions.GetLiftBuildSettings(Filter1.LayerMethod);
                }
                else
                {
                    Renderer.LiftBuildSettings = ICOptions.LiftBuildSettings;
                }
                */

                Renderer.OverallExistenceMap = OverallExistenceMap;
                Renderer.ProdDataExistenceMap = ProdDataExistenceMap;
                Renderer.DesignSubgridOverlayMap = DesignSubgridOverlayMap;

                // Renderer.WorkingPalette = WorkingColourPalette;

                Renderer.Filter1 = Filter1;
                Renderer.Filter2 = Filter2;

                Renderer.IsWhollyInTermsOfGridProjection = true; // Ensure the renderer knows we are using grid projection coordinates
                Renderer.SetBounds(NEECoords[0].X, NEECoords[0].Y,
                                   WorldTileWidth, WorldTileHeight,
                                   NPixelsX, NPixelsY);

                // Renderer.CutFillDesignDescriptor = FDesignDescriptor;
                // Renderer.ReferenceVolumeType = FReferenceVolumeType;

                Renderer.TileRotation = TileRotation;
                Renderer.RotatedTileBoundingExtents = RotatedTileBoundingExtents;

                Renderer.WorldTileWidth = WorldTileWidth;
                Renderer.WorldTileHeight = WorldTileHeight;

                Renderer.SurveyedSurfacesExludedViaTimeFiltering = SurveyedSurfacesExludedViaTimeFiltering;

                ////////////////////// CONFIGURE THE RENDERER - END
                ResultStatus = Renderer.PerformRender();

                if (ResultStatus == RequestErrorStatus.OK)
                {
                    /*
                    // Draw diagonal cross and top left corner indicators
                    Renderer.Displayer.MapView.DrawLine(BLPoint.X, BLPoint.Y, TRPoint.X, TRPoint.Y, Color.Red);
                    Renderer.Displayer.MapView.DrawLine(BLPoint.X, TRPoint.Y, TRPoint.X, BLPoint.Y, Color.Red);

                    Renderer.Displayer.MapView.DrawLine(BLPoint.X, TRPoint.Y, BLPoint.X, TRPoint.Y - 50, Color.Red);
                    Renderer.Displayer.MapView.DrawLine(BLPoint.X, TRPoint.Y, BLPoint.X + 50, TRPoint.Y - 0.1, Color.Red);
                    */

                    return Renderer.Displayer.MapView.BitmapCanvas;

                    // PackageRenderedTileIntoPNG(Renderer.Displayer);
                    // TODO - report the final rendered result back
                    //throw new NotImplementedException();
                }
            }
            catch
            {
                // TODO add when logging available
                // SIGLogMessage.PublishNoODS(Self, Format('%s.Execute: Exception "%s" occurred', [Self.ClassName, E.Message]), slmcException);
                ResultStatus = RequestErrorStatus.Exception;
            }

            return null;
        }
    }
}
