using Microsoft.Extensions.Logging;
using VSS.Serilog.Extensions;
using VSS.TRex.Common.Interfaces.Interfaces;
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
        private static readonly ILogger _log = Logging.Logger.CreateLogger<CalculateDesignElevationPatch>();

        private readonly bool _isTraceLoggingEnabled = _log.IsTraceEnabled();

        private IDesignFiles _designs;

        private IDesignFiles Designs => _designs ??= DIContext.ObtainRequired<IDesignFiles>();

        /// <summary>
        /// Default no-args constructor
        /// </summary>
        public CalculateDesignElevationPatch()
        {
        }

        /// <summary>
        /// Performs the donkey work of the elevation patch calculation
        /// </summary>
        private IClientHeightLeafSubGrid Calc(ISiteModelBase siteModel, DesignOffset referenceDesign, double cellSize, int originX, int originY,
          out DesignProfilerRequestResult calcResult)
        {
            calcResult = DesignProfilerRequestResult.UnknownError;

            if (_isTraceLoggingEnabled)
              _log.LogTrace("About to lock design");

            var design = Designs.Lock(referenceDesign.DesignID, siteModel, cellSize, out var lockResult);

            if (design == null)
            {
                _log.LogWarning($"Failed to read design file for design {referenceDesign.DesignID}");

                calcResult = lockResult == DesignLoadResult.DesignDoesNotExist
                ? DesignProfilerRequestResult.DesignDoesNotExist
                : DesignProfilerRequestResult.FailedToLoadDesignFile;

                return null;
            }

            try
            {
                if (_isTraceLoggingEnabled)
                  _log.LogTrace("Computing sub grid elevation patch");

                // Check to see if this sub grid has any design surface underlying it
                // from which to calculate an elevation patch. If not, don't bother...
                if (!design.HasElevationDataForSubGridPatch(originX >> SubGridTreeConsts.SubGridIndexBitsPerLevel,
                                                            originY >> SubGridTreeConsts.SubGridIndexBitsPerLevel))
                {
                    calcResult = DesignProfilerRequestResult.NoElevationsInRequestedPatch;
                    return null;
                }

                var result = new ClientHeightLeafSubGrid(null, null, SubGridTreeConsts.SubGridTreeLevels, cellSize, SubGridTreeConsts.DefaultIndexOriginOffset);

                result.SetAbsoluteOriginPosition(originX & ~SubGridTreeConsts.SubGridLocalKeyMask,
                                                 originY & ~SubGridTreeConsts.SubGridLocalKeyMask);
                result.CalculateWorldOrigin(out var worldOriginX, out var worldOriginY);

                calcResult = design.InterpolateHeights(result.Cells, worldOriginX, worldOriginY, cellSize, referenceDesign.Offset) 
                  ? DesignProfilerRequestResult.OK 
                  : DesignProfilerRequestResult.NoElevationsInRequestedPatch;

                if (_isTraceLoggingEnabled)
                  _log.LogTrace("Computed sub grid elevation patch");

                return result;
            }
            finally
            {
                if (_isTraceLoggingEnabled)
                  _log.LogTrace("Unlocking design");

                Designs.UnLock(referenceDesign.DesignID, design);

                if (_isTraceLoggingEnabled)
                  _log.LogTrace("Completed calculating design elevations");
            }
        }

        /// <summary>
        /// Performs execution business logic for this executor
        /// </summary>
        public IClientHeightLeafSubGrid Execute(ISiteModelBase siteModel, DesignOffset referenceDesign, double cellSize, int originX, int originY, out DesignProfilerRequestResult calcResult)
        {
            // Perform the design elevation patch calculation
            try
            {
                /* Test code to force all sub grids to have 0 elevations from a design
                ClientHeightLeafSubGrid test = new ClientHeightLeafSubGrid(null, null, 6, 0.34, SubGridTreeConsts.DefaultIndexOriginOffset);
                test.SetToZeroHeight();
                return test;
                */

                // Calculate the patch of elevations and return it
                return Calc(siteModel, referenceDesign, cellSize, originX, originY, out calcResult);
            }
            finally
            {
                //if Debug_PerformDPServiceRequestHighRateLogging then
                //Log.LogInformation($"#Out# {nameof(CalculateDesignElevationPatch)}.Execute #Result# {calcResult}");
            }
        }
    }
}
