using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Common;
using VSS.VisionLink.Raptor.Geometry;
using VSS.VisionLink.Raptor.GridFabric.Requests.Interfaces;

namespace VSS.VisionLink.Raptor.Volumes.GridFabric.Responses
{
    /// <summary>
    /// Describes the result of a simple volumes computation in terms of cut, fill and total volumes plus coverage areas
    /// </summary>
    public class SimpleVolumesResponse : IResponseAggregateWith<SimpleVolumesResponse>
    {
        /// <summary>
        /// Cut volume, expressed in cubic meters
        /// </summary>
        public double Cut = Consts.NullDouble;

        /// <summary>
        /// Fill volume, expressed in cubic meters
        /// </summary>
        public double Fill = Consts.NullDouble;

        /// <summary>
        /// Total area coverged by the volume computation, expressed in square meters
        /// </summary>
        public double TotalCoverageArea = Consts.NullDouble;

        /// <summary>
        /// Total area coverged by the volume computation that produced cut volume, expressed in square meters
        /// </summary>
        public double CutArea = Consts.NullDouble;

        /// <summary>
        /// Total area coverged by the volume computation that produced fill volume, expressed in square meters
        /// </summary>
        public double FillArea = Consts.NullDouble;

        /// <summary>
        /// The bounding extent of the area covered by the volume computation expressed in the project site calibration/grid coordinate system
        /// </summary>
        public BoundingWorldExtent3D BoundingExtentGrid { get; set; } = BoundingWorldExtent3D.Null();

        /// <summary>
        /// The bounding extent of the area covered by the volume computation expressed in the WGS84 datum
        /// </summary>
        public BoundingWorldExtent3D BoundingExtentLLH { get; set; } = BoundingWorldExtent3D.Null();

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public SimpleVolumesResponse()
        {

        }

        private double AggregateValue(ref double thisVal, double otherVal)
        {
            return thisVal == Consts.NullDouble ? otherVal : thisVal + (otherVal == Consts.NullDouble ? 0 : otherVal);
        }

        /// <summary>
        /// Combine this simple volumes response with another simple volumes response and store the result in this response
        /// </summary>
        /// <param name="other"></param>
        public SimpleVolumesResponse AggregateWith(SimpleVolumesResponse other)
        {
            AggregateValue(ref Cut, other.Cut);
            AggregateValue(ref Fill, other.Fill);
            AggregateValue(ref TotalCoverageArea, other.TotalCoverageArea);
            AggregateValue(ref CutArea, other.CutArea);
            AggregateValue(ref FillArea, other.FillArea);

            BoundingExtentGrid.Include(other.BoundingExtentGrid);

            return this;
        }
    }
}
