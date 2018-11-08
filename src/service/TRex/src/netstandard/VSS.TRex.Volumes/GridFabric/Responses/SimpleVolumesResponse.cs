using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.GridFabric.Responses;

namespace VSS.TRex.Volumes.GridFabric.Responses
{
    /// <summary>
    /// Describes the result of a simple volumes computation in terms of cut, fill and total volumes plus coverage areas
    /// </summary>
    public class SimpleVolumesResponse : SubGridRequestsResponse, IAggregateWith<SimpleVolumesResponse>
    {
        /// <summary>
        /// Cut volume, expressed in cubic meters
        /// </summary>
        public double? Cut;

        /// <summary>
        /// Fill volume, expressed in cubic meters
        /// </summary>
        public double? Fill;

        /// <summary>
        /// Total area coverged by the volume computation, expressed in square meters
        /// </summary>
        public double? TotalCoverageArea;

        /// <summary>
        /// Total area coverged by the volume computation that produced cut volume, expressed in square meters
        /// </summary>
        public double? CutArea;

        /// <summary>
        /// Total area coverged by the volume computation that produced fill volume, expressed in square meters
        /// </summary>
        public double? FillArea;

        /// <summary>
        /// The bounding extent of the area covered by the volume computation expressed in the project site calibration/grid coordinate system
        /// </summary>
        public BoundingWorldExtent3D BoundingExtentGrid = BoundingWorldExtent3D.Null();

        /// <summary>
        /// The bounding extent of the area covered by the volume computation expressed in the WGS84 datum
        /// </summary>
        public BoundingWorldExtent3D BoundingExtentLLH = BoundingWorldExtent3D.Null();

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public SimpleVolumesResponse() 
        {
        }

        /// <summary>
        /// Add two nullable numbers together and return a nullable result. 
        /// The logic here permits null + number to return a number rather than the default double? semantic of returning null
        /// </summary>
        /// <param name="thisVal"></param>
        /// <param name="otherVal"></param>
        /// <returns></returns>
        private double? AggregateValue(double? thisVal, double? otherVal)
        {
            return thisVal.HasValue ? thisVal + (otherVal ?? 0) : otherVal;
        }

        /// <summary>
        /// Combine this simple volumes response with another simple volumes response and store the result in this response
        /// </summary>
        /// <param name="other"></param>
        public SimpleVolumesResponse AggregateWith(SimpleVolumesResponse other)
        {
            Cut = AggregateValue(Cut, other.Cut);
            Fill = AggregateValue(Fill, other.Fill);
            TotalCoverageArea = AggregateValue(TotalCoverageArea, other.TotalCoverageArea);
            CutArea = AggregateValue(CutArea, other.CutArea);
            FillArea = AggregateValue(FillArea, other.FillArea);

            BoundingExtentGrid.Include(other.BoundingExtentGrid);

            // Note: WGS84 bounding rectangle is not enlarged - it is computed after all aggregations have occurred.

            return this;
        }

        /// <summary>
        /// Simple textual represenation of the information in a simple volumes response
        /// </summary>
        public override string ToString() => $"Cut:{Cut}, Fill:{Fill}, Cut Area:{CutArea}, FillArea: {FillArea}, Total Area:{TotalCoverageArea}, BoundingGrid:{BoundingExtentGrid}, BoundingLLH:{BoundingExtentLLH}";
    }
}
