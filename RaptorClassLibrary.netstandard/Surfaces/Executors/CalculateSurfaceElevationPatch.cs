using Apache.Ignite.Core;
using Apache.Ignite.Core.Cluster;
using Apache.Ignite.Core.Compute;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.Velociraptor.DesignProfiling;
using VSS.VisionLink.Raptor.Designs;
using VSS.VisionLink.Raptor.GridFabric.Grids;
using VSS.VisionLink.Raptor.SubGridTrees;
using VSS.VisionLink.Raptor.SubGridTrees.Client;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;
using VSS.VisionLink.Raptor.Surfaces.GridFabric.Arguments;
using VSS.VisionLink.Raptor.Surfaces.GridFabric.ComputeFuncs;

namespace VSS.VisionLink.Raptor.Surfaces.Executors
{
    public class CalculateSurfaceElevationPatch
    {
        /// <summary>
        /// Local reference to the client subgrid factory
        /// </summary>
        private static IClientLeafSubgridFactory ClientLeafSubGridFactory = ClientLeafSubgridFactoryFactory.GetClientLeafSubGridFactory();

        /// <summary>
        /// Private reference to the arguments provided to the executor
        /// </summary>
        private SurfaceElevationPatchArgument Args { get; set; } = null;

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public CalculateSurfaceElevationPatch()
        {
        }

        /// <summary>
        /// Constructor for the executor accepting the arguments for its operation
        /// </summary>
        /// <param name="args"></param>
        public CalculateSurfaceElevationPatch(SurfaceElevationPatchArgument args) : this()
        {
            this.Args = args;
        }

        /// <summary>
        /// Performs the donkey work of the elevation patch calculation
        /// </summary>
        /// <param name="CalcResult"></param>
        /// <returns></returns>
        private ClientHeightAndTimeLeafSubGrid Calc(out DesignProfilerRequestResult CalcResult)
        {
            CalcResult = DesignProfilerRequestResult.UnknownError;

            DesignBase Design = null;
            ClientHeightAndTimeLeafSubGrid Patch = null;
            object Hint = null;
            DesignLoadResult LockResult;
            SurveyedSurface ThisSurveyedSurface;

            try
            {
                // if VLPDSvcLocations.Debug_PerformDPServiceRequestHighRateLogging then
                //   SIGLogMessage.PublishNoODS(Self, Format('In %s.Execute for DataModel:%d  OTGCellBottomLeftX:%d  OTGCellBottomLeftY:%d', [Self.ClassName, Args.DataModelID, Args.OTGCellBottomLeftX, Args.OTGCellBottomLeftY]), slmcDebug);
                // InterlockedIncrement64(DesignProfilerRequestStats.NumSurfacePatchesComputed);

                try
                {
                    Patch = ClientLeafSubGridFactory.GetSubGrid(Types.GridDataType.HeightAndTime) as ClientHeightAndTimeLeafSubGrid;

                    if (Patch == null)
                    {
                        return null;
                    }

                    Patch.CellSize = Args.CellSize;
                    Patch.SetAbsoluteOriginPosition(Args.OTGCellBottomLeftX, Args.OTGCellBottomLeftY);
                    Patch.CalculateWorldOrigin(out double OriginX, out double OriginY);

                    double CellSize = Args.CellSize;
                    double HalfCellSize = CellSize / 2;
                    double OriginXPlusHalfCellSize = OriginX + HalfCellSize;
                    double OriginYPlusHalfCellSize = OriginY + HalfCellSize;

                    // Work down through the list of surfaces in the time ordering provided by the caller
                    for (int i = 0; i < Args.IncludedSurveyedSurfaces.Count; i++)
                    {
                        if (Args.ProcessingMap.IsEmpty())
                        {
                            break;
                        }

                        ThisSurveyedSurface = Args.IncludedSurveyedSurfaces[i];

                        // Lock & load the design
                        Design = DesignFiles.Designs.Lock(ThisSurveyedSurface.DesignDescriptor, Args.SiteModelID, Args.CellSize, out LockResult);

                        if (Design == null)
                        {
                            // TODO: Readd when logging available
                            // SIGLogMessage.PublishNoODS(Self, Format('Failed to read design file %s in %s', [ThisSurveyedSurface.DesignDescriptor.ToString, Self.ClassName]), slmcWarning);
                            CalcResult = DesignProfilerRequestResult.FailedToLoadDesignFile;
                            return null;
                        }

                        try
                        {
                            Design.AcquireExclusiveInterlock();
                            try
                            {
                                if (!Design.HasElevationDataForSubGridPatch(Args.OTGCellBottomLeftX >> SubGridTree.SubGridIndexBitsPerLevel,
                                                                            Args.OTGCellBottomLeftY >> SubGridTree.SubGridIndexBitsPerLevel))
                                {
                                    continue;
                                }

                                long AsAtDate = ThisSurveyedSurface.AsAtDate.ToBinary();
                                double Offset = ThisSurveyedSurface.DesignDescriptor.Offset;

                                // Walk across the subgrid checking for a design elevation for each appropriate cell
                                // based on the processing bit mask passed in
                                Args.ProcessingMap.ForEachSetBit((x, y) =>
                                {
                                    if (Design.InterpolateHeight(ref Hint,
                                                                 OriginXPlusHalfCellSize + (CellSize * x),
                                                                 OriginYPlusHalfCellSize + (CellSize * y),
                                                                 Offset,
                                                                 out double z))
                                    {
                                        // If we can interpolate a height for the requested cell, then update the cell height
                                        // and decrement the bit count so that we know when we've handled all the requested cells
                                        Patch.Cells[x, y] = (float)z;
                                        Patch.Times[x, y] = AsAtDate;
                                    }

                                    Args.ProcessingMap.ClearBit(x, y);

                                    return true;
                                });
                            }
                            finally
                            {
                                Design.ReleaseExclusiveInterlock();
                            }
                        }
                        finally
                        {
                            DesignFiles.Designs.UnLock(ThisSurveyedSurface.DesignDescriptor, Design);
                        }
                    }

                    CalcResult = DesignProfilerRequestResult.OK;

                    return Patch;
                }
                finally
                {
                    //if VLPDSvcLocations.Debug_PerformDPServiceRequestHighRateLogging then
                    //SIGLogMessage.PublishNoODS(Self, Format('Out %s.Execute', [Self.ClassName]), slmcMessage);
                }
            }
            catch // (Exception E)
            {
                // TODO readd when logging available
                // SIGLogMessage.PublishNoODS(Self, Format('%s.Execute: Exception "%s"', [Self.ClassName, E.Message]), slmcException);
            }

            return null;
        }

        /// <summary>
        /// Performs execution business logic for this executor
        /// </summary>
        /// <returns></returns>
        public ClientHeightAndTimeLeafSubGrid Execute()
        {
            try
            {
                // Perform the design profile calculation
                try
                {
                    // Check we are still active
                    // TODO.... 

                    // Calculate the patch of elevations and return it
                    ClientHeightAndTimeLeafSubGrid result = Calc(out DesignProfilerRequestResult CalcResult);

                    if (result == null)
                    {
                        // TODO: ....
                    }

                    return result;
                }
                finally
                {
                    // TODO: Readd when logging available
                    //if VLPDSvcLocations.Debug_PerformDPServiceRequestHighRateLogging then
                    //  SIGLogMessage.PublishNoODS(Self, Format('#Out# %s.Execute #Result#%s', [Self.ClassName, DPErrorStatusName(Ord(CalcResult))]), slmcMessage);
                }
            }
            catch // (Exception E)
            {
                // TODO: Readd when logging available
                // SIGLogMessage.PublishNoODS(Self, Format('%s.Execute: Exception "%s"', [Self.ClassName, E.Message]), slmcException);
                return null;
            }
        }
    }
}
