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

        private static IDesignFiles designs;

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
      /// <param name="CalcResult"></param>
      /// <param name="projectUID"></param>
      /// <param name="referenceDesign"></param>
      /// <param name="cellSize"></param>
      /// <param name="originX"></param>
      /// <param name="originY"></param>
      /// <returns></returns>
      private IClientHeightLeafSubGrid Calc(Guid projectUID, DesignOffset referenceDesign, double cellSize, int originX, int originY,
          out DesignProfilerRequestResult CalcResult)
        {
            CalcResult = DesignProfilerRequestResult.UnknownError;

            IDesignBase Design = Designs.Lock(referenceDesign.DesignID, projectUID, cellSize, out DesignLoadResult LockResult);

            if (Design == null)
            {
                Log.LogWarning($"Failed to read design file for design {referenceDesign.DesignID}");

                CalcResult = LockResult == DesignLoadResult.DesignDoesNotExist 
                ? DesignProfilerRequestResult.DesignDoesNotExist
                : DesignProfilerRequestResult.FailedToLoadDesignFile;

                return null;
            }

            try
            {
                // Check to see if this sub grid has any design surface underlying it
                // from which to calculate an elevation patch. If not, don't bother...
                if (!Design.HasElevationDataForSubGridPatch(originX >> SubGridTreeConsts.SubGridIndexBitsPerLevel,
                                                            originY >> SubGridTreeConsts.SubGridIndexBitsPerLevel))
                {
                    CalcResult = DesignProfilerRequestResult.NoElevationsInRequestedPatch;
                    return null;
                }

                IClientHeightLeafSubGrid Result = new ClientHeightLeafSubGrid(null, null, SubGridTreeConsts.SubGridTreeLevels, cellSize, SubGridTreeConsts.DefaultIndexOriginOffset);

                Result.SetAbsoluteOriginPosition(originX & ~SubGridTreeConsts.SubGridLocalKeyMask,
                                                 originY & ~SubGridTreeConsts.SubGridLocalKeyMask);
                Result.CalculateWorldOrigin(out double WorldOriginX, out double WorldOriginY);

                CalcResult = Design.InterpolateHeights(Result.Cells, WorldOriginX, WorldOriginY, cellSize, referenceDesign.Offset) 
                  ? DesignProfilerRequestResult.OK 
                  : DesignProfilerRequestResult.NoElevationsInRequestedPatch;

                return Result;
            }
            finally
            {
                Designs.UnLock(referenceDesign.DesignID, Design);
            }
        }

        /// <summary>
        /// Performs execution business logic for this executor
        /// </summary>
        /// <returns></returns>
        public IClientHeightLeafSubGrid Execute(Guid projectUID, DesignOffset referenceDesign, double cellSize, int originX, int originY, out DesignProfilerRequestResult calcResult)
        {
            calcResult = DesignProfilerRequestResult.UnknownError;

            // Perform the design profile calculation
            try
            {
                /* Test code to force all sub grids to have 0 elevations from a design
                ClientHeightLeafSubGrid test = new ClientHeightLeafSubGrid(null, null, 6, 0.34, SubGridTreeConsts.DefaultIndexOriginOffset);
                test.SetToZeroHeight();
                return test;
                */

                // Calculate the patch of elevations and return it
                return Calc(projectUID, referenceDesign, cellSize, originX, originY, out calcResult);
            }
            finally
            {
                //if Debug_PerformDPServiceRequestHighRateLogging then
                //Log.LogInformation($"#Out# {nameof(CalculateDesignElevationPatch)}.Execute #Result# {calcResult}");
            }
        }
    }
}
