using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Designs.Executors
{
    public class CalculateDesignElevationPatch
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger<CalculateDesignElevationPatch>();

        private static IDesignFiles designs = null;

        private IDesignFiles Designs => designs ?? (designs = DIContext.Obtain<IDesignFiles>());

        /// <summary>
        /// Default no-args constructor
        /// </summary>
        public CalculateDesignElevationPatch()
        {
        }

      /// <summary>
      /// Performs the donkey work of the elevation patch calculation
      /// </summary>
      /// <param name="offset"></param>
      /// <param name="CalcResult"></param>
      /// <param name="projectUID"></param>
      /// <param name="referenceDesignUID"></param>
      /// <param name="cellSize"></param>
      /// <param name="originX"></param>
      /// <param name="originY"></param>
      /// <returns></returns>
      private IClientHeightLeafSubGrid Calc(Guid projectUID, Guid referenceDesignUID, double cellSize, uint originX, uint originY, double offset,
          out DesignProfilerRequestResult CalcResult)
        {
            CalcResult = DesignProfilerRequestResult.UnknownError;

            IDesignBase Design = Designs.Lock(referenceDesignUID, projectUID, cellSize, out DesignLoadResult LockResult);

            if (Design == null)
            {
                Log.LogWarning($"Failed to read design file for design {referenceDesignUID}");
                CalcResult = DesignProfilerRequestResult.FailedToLoadDesignFile;
                return null;
            }

            try
            {
                // Check to see if this subgrid has any design surface underlying it
                // from which to calculate an elevation patch. If not, don't bother...
                if (!Design.HasElevationDataForSubGridPatch(originX >> SubGridTreeConsts.SubGridIndexBitsPerLevel,
                                                            originY >> SubGridTreeConsts.SubGridIndexBitsPerLevel))
                {
                    CalcResult = DesignProfilerRequestResult.NoElevationsInRequestedPatch;
                    return null;
                }

                IClientHeightLeafSubGrid Result = new ClientHeightLeafSubGrid(null, null, SubGridTreeConsts.SubGridTreeLevels, cellSize, SubGridTreeConsts.DefaultIndexOriginOffset);

                Result.SetAbsoluteOriginPosition((uint)(originX & ~SubGridTreeConsts.SubGridLocalKeyMask),
                                                 (uint)(originY & ~SubGridTreeConsts.SubGridLocalKeyMask));
                Result.CalculateWorldOrigin(out double WorldOriginX, out double WorldOriginY);

                if (Design.InterpolateHeights(Result.Cells, WorldOriginX, WorldOriginY, cellSize, offset))
                {
                    CalcResult = DesignProfilerRequestResult.OK;
                }
                else
                {
                    CalcResult = DesignProfilerRequestResult.NoElevationsInRequestedPatch;
                }

                return Result;
            }
            finally
            {
                Designs.UnLock(referenceDesignUID, Design);
            }
        }

        /// <summary>
        /// Performs execution business logic for this executor
        /// </summary>
        /// <returns></returns>
        public IClientHeightLeafSubGrid Execute(Guid projectUID, Guid referenceDesignUID, double cellSize, uint originX, uint originY, double offset)
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
                    IClientHeightLeafSubGrid result = Calc(projectUID, referenceDesignUID, cellSize, originX, originY, offset, out DesignProfilerRequestResult CalcResult);

                    if (result == null)
                    {
                        // TODO: Handle failure to calculate a design elevation patch result
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
                Log.LogError("Execute: Exception: ", E);
                return null;
            }
        }
    }
}
