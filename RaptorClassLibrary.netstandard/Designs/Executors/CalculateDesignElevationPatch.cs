using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.Velociraptor.DesignProfiling.GridFabric.Arguments;
using VSS.VisionLink.Raptor;
using VSS.VisionLink.Raptor.SubGridTrees.Client;

namespace VSS.Velociraptor.DesignProfiling.Executors
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
        /// <param name="CalcResult"></param>
        /// <returns></returns>
        private ClientHeightLeafSubGrid Calc(CalculateDesignElevationPatchArgument Args,
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
                if (!Design.HasElevationDataForSubGridPatch(Args.OriginX >> SubGridTree.SubGridIndexBitsPerLevel,
                                                            Args.OriginY >> SubGridTree.SubGridIndexBitsPerLevel))
                {
                    CalcResult = DesignProfilerRequestResult.NoElevationsInRequestedPatch;
                    return null;
                }

                ClientHeightLeafSubGrid Result = new ClientHeightLeafSubGrid(null, null, SubGridTree.SubGridTreeLevels, Args.CellSize, SubGridTree.DefaultIndexOriginOffset);

                Result.SetAbsoluteOriginPosition((uint)(Args.OriginX & ~SubGridTree.SubGridLocalKeyMask),
                                                 (uint)(Args.OriginY & ~SubGridTree.SubGridLocalKeyMask));
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
        public ClientHeightLeafSubGrid Execute(CalculateDesignElevationPatchArgument args)
        {
            try
            {
                // Perform the design profile calculation
                try
                {
                    // Check we are still active
                    // TODO.... 

                    /* Test code to force all subgrids to have 0 elevations from a design
                    ClientHeightLeafSubGrid test = new ClientHeightLeafSubGrid(null, null, 6, 0.34, SubGridTree.DefaultIndexOriginOffset);
                    test.SetToZeroHeight();
                    return test;
                    */

                    // Calculate the patch of elevations and return it
                    ClientHeightLeafSubGrid result = Calc(args, out DesignProfilerRequestResult CalcResult);

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
