using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.Volumes
{
    /// <summary>
    /// VolumesCalculator extends VolumesCalculatorBase to include volume
    /// accumulation state tracking for simple volums calculations.
    /// </summary>
    public class VolumesCalculator : VolumesCalculatorBase
    {
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
            {
                Extents = BaseFilter.SpatialFilter.CalculateIntersectionWithExtents(Extents);
            }

            if (ToSelectionType == ProdReportSelectionType.Filter)
            {
                Extents = TopFilter.SpatialFilter.CalculateIntersectionWithExtents(Extents);
            }
        }

        public override bool ComputeVolumeInformation()
        {
            if (VolumeType == VolumeComputationType.None)
            {
                // TODO Readd when logging available
                //SIGLogMessage.Publish(Self, 'No report type supplied to TICVolumesCalculator.ComputeVolumeInformation', slmcError);
                return false;
            }

            if (FromSelectionType == ProdReportSelectionType.Surface)
            {
                if (RefOriginal == null)
                {
                    // TODO Readd when logging available
                    // SIGLogMessage.Publish(Self, 'No RefOriginal surface supplied', slmcError);
                    return false;
                }
            }

            if (ToSelectionType == ProdReportSelectionType.Surface)
            {
                if (RefDesign == null)
                {
                    // TODO: Readd when logging available
                    //SIGLogMessage.Publish(Self, 'No RefDesign surface supplied', slmcError);
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
