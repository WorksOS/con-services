using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.TRex.Geometry
{
    /// <summary>
    /// Implements a very simple triangle class that uses three XYZ 3D points to concretely describe the vertices and 
    /// provides a small number of geometry operations against that triangle geometry
    /// </summary>
    public class SimpleTriangle
    {
        /// <summary>
        /// First vertex
        /// </summary>
        public XYZ V1 = XYZ.Null;

        /// <summary>
        /// Second vertex
        /// </summary>
        public XYZ V2 = XYZ.Null;

        /// <summary>
        /// Third vertex
        /// </summary>
        public XYZ V3 = XYZ.Null;

        /// <summary>
        /// Constructor taking the three vertices as XYZ 3D points
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="v3"></param>
        public SimpleTriangle(XYZ v1, XYZ v2, XYZ v3)
        {
            V1 = v1;
            V2 = v2;
            V3 = v3;
        }

        /// <summary>
        /// Calculate the area in square meters
        /// </summary>
        public double Area => XYZ.GetTriArea(V1, V2, V3);

        /// <summary>
        /// Determines if this triangle includes the given point
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public bool IncludesPoint(double x, double y) => XYZ.PointInTriangle(V1, V2, V3, x, y);

        /// <summary>
        /// Determine the elevation on the triangle at the location given by x and y. If the location is not
        /// within the trianlge this will return Consts.NullDouble
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public double InterpolateHeight(double x, double y) => XYZ.GetTriangleHeight(V1, V2, V3, x, y);

        /// <summary>
        /// Updates the vertices of the triangle with the given three vertex parameters
        /// </summary>
        /// <param name="V1"></param>
        /// <param name="V2"></param>
        /// <param name="V3"></param>
        public void SetVertices(XYZ V1, XYZ V2, XYZ V3)
        {
            this.V1 = V1;
            this.V2 = V2;
            this.V3 = V3;
        }
    }
}
