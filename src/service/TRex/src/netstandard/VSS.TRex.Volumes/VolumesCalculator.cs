using Microsoft.Extensions.Logging;
using VSS.TRex.Common;
using VSS.TRex.Types;
using VSS.TRex.Common.Exceptions;

namespace VSS.TRex.Volumes
{
    /// <summary>
    /// VolumesCalculator extends VolumesCalculatorBase to include volume
    /// accumulation state tracking for simple volumes calculations.
    /// </summary>
    public class VolumesCalculator : VolumesCalculatorBase
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger<VolumesCalculator>();
      
        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public VolumesCalculator()
        {
        }

        private void ApplyFilterAndSubsetBoundariesToExtents()
        {
            /*
            if VLPDSvcLocations.Debug_ExtremeLogSwitchM then
               begin
                 SIGLogMessage.Publish(Self, Format('TICVolumesCalculator DEBUG  BaseFilter Dates : %s  %s ',[DateTimeToStr(FBaseFilter.StartTime), DateTimeToStr(FBaseFilter.EndTime)]), slmcDebug);
                 SIGLogMessage.Publish(Self, Format('TICVolumesCalculator DEBUG  TopFilter Dates : %s  %s ',[DateTimeToStr(FTopFilter.StartTime), DateTimeToStr(FTopFilter.EndTime)]), slmcDebug);
               end;
            */

            if (FromSelectionType == ProdReportSelectionType.Filter)
                BaseFilter.SpatialFilter.CalculateIntersectionWithExtents(Extents);

            if (ToSelectionType == ProdReportSelectionType.Filter)
                TopFilter.SpatialFilter.CalculateIntersectionWithExtents(Extents);
        }

        public override bool ComputeVolumeInformation()
        {
            if (VolumeType == VolumeComputationType.None)
                throw new TRexException("No report type supplied to ComputeVolumeInformation");

            if (FromSelectionType == ProdReportSelectionType.Surface)
            {
                if (RefOriginal == null)
                {
                  Log.LogError("No RefOriginal surface supplied");
                  return false;
                }
            }

            if (ToSelectionType == ProdReportSelectionType.Surface)
            {
                if (RefDesign == null)
                {
                  Log.LogError("No RefDesign surface supplied");
                  return false;
                }
            }

            // Adjust the extents we have been given to encompass the spatial extent
            // of the supplied filters (if any);
            ApplyFilterAndSubsetBoundariesToExtents();

            BaseFilter.AttributeFilter.ReturnEarliestFilteredCellPass = UseEarliestData;

            // Compute the volume as required
            return ExecutePipeline() == RequestErrorStatus.OK;
        }
    }
}
