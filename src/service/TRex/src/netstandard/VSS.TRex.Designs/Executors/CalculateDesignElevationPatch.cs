using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.Models;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Designs.Executors
{
    public class CalculateDesignElevationPatch
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger<CalculateDesignElevationPatch>();

        /// <summary>
        /// Default no-args constructor
        /// </summary>
        public CalculateDesignElevationPatch()
        {
        }

        /// <summary>
        /// Performs the donkey work of the elevation patch calculation
        /// </summary>
        /// <param name="Args"></param>
        /// <param name="CalcResult"></param>
        /// <returns></returns>
        private IClientHeightLeafSubGrid Calc(CalculateDesignElevationPatchArgument Args,
                                             out DesignProfilerRequestResult CalcResult)
        {
            CalcResult = DesignProfilerRequestResult.UnknownError;

            DesignBase Design = DesignFiles.Designs.Lock(Args.DesignDescriptor, Args.ProjectID, Args.CellSize, out DesignLoadResult LockResult);

            if (Design == null)
            {
                Log.LogWarning($"Failed to read design file {Args.DesignDescriptor.FullPath}");
                CalcResult = DesignProfilerRequestResult.FailedToLoadDesignFile;
                return null;
            }

            try
            {
                // Check to see if this subgrid has any design surface underlying it
                // from which to calculate an elevation patch. If not, don't bother...
                if (!Design.HasElevationDataForSubGridPatch(Args.OriginX >> SubGridTreeConsts.SubGridIndexBitsPerLevel,
                                                            Args.OriginY >> SubGridTreeConsts.SubGridIndexBitsPerLevel))
                {
                    CalcResult = DesignProfilerRequestResult.NoElevationsInRequestedPatch;
                    return null;
                }

                IClientHeightLeafSubGrid Result = new ClientHeightLeafSubGrid(null, null, SubGridTreeConsts.SubGridTreeLevels, Args.CellSize, SubGridTreeConsts.DefaultIndexOriginOffset);

                Result.SetAbsoluteOriginPosition((uint)(Args.OriginX & ~SubGridTreeConsts.SubGridLocalKeyMask),
                                                 (uint)(Args.OriginY & ~SubGridTreeConsts.SubGridLocalKeyMask));
                Result.CalculateWorldOrigin(out double WorldOriginX, out double WorldOriginY);

// Exclusive serialisation of the Design is not required in the Ignite POC
//                Design.AcquireExclusiveInterlock();
//                try
//                {
                    if (Design.InterpolateHeights(Result.Cells, WorldOriginX, WorldOriginY, Args.CellSize, Args.DesignDescriptor.Offset))
                    {
                        CalcResult = DesignProfilerRequestResult.OK;
                    }
                    else
                    {
                        CalcResult = DesignProfilerRequestResult.NoElevationsInRequestedPatch;
                    }
//                }
//                finally
//                {
//                    Design.ReleaseExclusiveInterlock();
//                }

                return Result;
            }
            finally
            {
                DesignFiles.Designs.UnLock(Args.DesignDescriptor, Design);
            }
        }

        /// <summary>
        /// Performs execution business logic for this executor
        /// </summary>
        /// <returns></returns>
        public IClientHeightLeafSubGrid Execute(CalculateDesignElevationPatchArgument args)
        {
            try
            {
                // Perform the design profile calculation
                try
                {
                    /* Test code to force all subgrids to have 0 elevations from a design
                    ClientHeightLeafSubGrid test = new ClientHeightLeafSubGrid(null, null, 6, 0.34, SubGridTreeConsts.DefaultIndexOriginOffset);
                    test.SetToZeroHeight();
                    return test;
                    */

                    // Calculate the patch of elevations and return it
                    IClientHeightLeafSubGrid result = Calc(args, out DesignProfilerRequestResult CalcResult);

                    if (result == null)
                    {
                        // TODO: Handle faulre to calculate a design elevation patch result
                    }

                    return result;
                }
                finally
                {
                    //if VLPDSvcLocations.Debug_PerformDPServiceRequestHighRateLogging then
                    //Log.LogInformation($"#Out# {nameof(CalculateDesignElevationPatch)}.Execute #Result# {CaleResult}");
                }
            }
            catch (Exception E)
            {
                Log.LogError($"Execute: Exception {E}");
                return null;
            }
        }
    }
}
