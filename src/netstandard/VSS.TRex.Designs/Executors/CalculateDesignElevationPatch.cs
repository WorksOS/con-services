﻿using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.Models;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Designs.Executors
{
    public class CalculateDesignElevationPatch
    {
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

            DesignBase Design = DesignFiles.Designs.Lock(Args.DesignDescriptor, Args.SiteModelID, Args.CellSize, out DesignLoadResult LockResult);

            if (Design == null)
            {
                // TODO: readd when logging available
                //SIGLogMessage.PublishNoODS(Nil, Format('Failed to read design file %s', [DesignDescriptor.ToString]), slmcWarning);
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
